// =============================================================================
// build_release.cs
// -----------------------------------------------------------------------------
// Herramienta de empaquetado de Latino.NET.
//
// Convertido de dotnet-script (.csx) a un "file-based app" nativo de .NET 10:
// un único archivo .cs, sin .csproj, ejecutable directamente con:
//
//     dotnet run build_release.cs
//
// o con el atajo:
//
//     dotnet build_release.cs
//
// Cambios respecto a la versión .csx original:
//   1. El directorio "#r nuget" se reemplazó por la directiva "#:package".
//   2. Se corrigió el build del plugin "Formulas": el script original lo
//      compilaba, pero si el .csproj no se encontraba en la ruta esperada
//      (p. ej. por un cambio de directorio de trabajo al pasar de script a
//      ejecutable compilado), el paso se saltaba EN SILENCIO -exactamente el
//      síntoma de un plugin "faltante" sin ningún mensaje de error-. Además,
//      a diferencia de SQLite, no copiaba las dependencias propias del
//      plugin (p. ej. librerías de cálculo numérico que puedan usar sus
//      módulos de Álgebra Lineal, EDOs, Optimización, etc.), por lo que aun
//      compilando podía fallar en tiempo de ejecución por un ensamblado
//      ausente.
//   3. Se unificó la compilación de TODOS los plugins (los que ya declaran su
//      propio <AssemblyName> y los que necesitan renombrado + copia de
//      dependencias, como SQLite y Formulas) en una sola función de datos
//      (BuildPlugin) recorrida por una lista, en vez de bloques repetidos
//      y casos especiales. Así, agregar un plugin nuevo es una línea, y
//      ninguno puede volver a "desaparecer" sin avisar.
//   4. Se desactivó por defecto la generación automática de documentación
//      (tools/generar_inventario.csx, generar_snippets.csx,
//      generar_referencias.csx): son herramientas destructivas y con fallos
//      conocidos, y su carpeta fue movida deliberadamente fuera de la ruta
//      esperada para que no se ejecuten por accidente. Antes, el script
//      original las invocaba sin condición alguna; con RunCommand abortando
//      el proceso completo (Environment.Exit(1)) ante cualquier fallo, ese
//      paso habría tumbado TODO el release en cuanto 'tools/' no apareciera
//      en la ruta esperada. Ahora es un paso explícito y opcional
//      (GenerarDocumentacionSiCorresponde), apagado por defecto, que nunca
//      aborta el build si está deshabilitado o si la carpeta no existe.
// =============================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

// ============================================================================
// CONFIGURACIÓN DE RUTAS
// ============================================================================

// Proyectos
const string PATH_CORE = "src/Latino.Core/Latino.Core.csproj";
const string PATH_APP = "src/Latino.App/Latino.App.csproj";
const string PATH_LSP = "src/Latino.LanguageServer/Latino.LanguageServer.csproj";
const string PATH_PACKAGE_JSON = "extension/package.json";
const string PATH_EXAMPLES = "ejemplos";

// Directorios de salida
const string DIST_DIR = "dist";
string TEMP_DIR = Path.Combine(DIST_DIR, "temp");
string FINAL_DIR = Path.Combine(DIST_DIR, "final");

// Plugins cuyo .csproj ya define <AssemblyName> igual al alias del lenguaje
// (ej. "texto", "log") y por eso no requieren renombrado tras publicar.
const string PATH_PLUGIN_LAPSOS = "plugins/Latino.Plugin.Lapsos/Latino.Plugin.Lapsos.csproj";
const string PATH_PLUGIN_HILOS = "plugins/Latino.Plugin.Hilos/Latino.Plugin.Hilos.csproj";

// Generación automática de documentación (tools/*.csx): DESACTIVADA por
// defecto — herramientas destructivas y con fallos conocidos; la carpeta
// 'tools' fue reubicada deliberadamente para que no corran por accidente.
// Cambia esto a 'true' y ajusta la ruta SOLO cuando esas herramientas estén
// corregidas y quieras reintegrarlas al pipeline de release.
const bool EJECUTAR_DOCGEN_AUTOMATICA = false;
const string PATH_TOOLS_DOCGEN = "tools";

// ============================================================================
// FUNCIONES AUXILIARES
// ============================================================================

string GetVersionFromJson()
{
    Console.WriteLine("🔍 Buscando versión en extension/package.json...");

    if (!File.Exists(PATH_PACKAGE_JSON))
    {
        Console.WriteLine($"⚠️ No se encontró {PATH_PACKAGE_JSON}. Usando versión por defecto.");
        return "1.0.0";
    }

    try
    {
        string jsonContent = File.ReadAllText(PATH_PACKAGE_JSON);
        using JsonDocument doc = JsonDocument.Parse(jsonContent);

        if (doc.RootElement.TryGetProperty("version", out JsonElement versionElement))
        {
            string version = versionElement.GetString() ?? "1.0.0";
            Console.WriteLine($"📌 Versión detectada: {version}");
            return version;
        }

        Console.WriteLine("⚠️ No se encontró propiedad 'version'. Usando versión por defecto.");
        return "1.0.0";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error leyendo versión: {ex.Message}");
        Environment.Exit(1);
        return "1.0.0"; // Nunca se alcanza
    }
}

void RunCommand(string command, string? workingDir = null)
{
    Console.WriteLine($"🚀 [CMD] {command}");

    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            // No redirigir para permitir que la salida se muestre en tiempo real en la consola
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            Console.WriteLine("❌ Error: No se pudo iniciar el proceso.");
            Environment.Exit(1);
            return;
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"❌ Error ejecutando comando (código: {process.ExitCode}).");
            Environment.Exit(1);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Excepción ejecutando comando: {ex.Message}");
        Environment.Exit(1);
    }
}

void Clean()
{
    Console.WriteLine("🧹 Limpiando directorio dist/...");

    if (Directory.Exists(DIST_DIR))
    {
        try
        {
            Directory.Delete(DIST_DIR, recursive: true);
        }
        catch
        {
            // Fallback forzado para Windows si hay bloqueo de archivos
            RunCommand($"if exist {DIST_DIR} rmdir /s /q {DIST_DIR}");
        }
    }

    Directory.CreateDirectory(TEMP_DIR);
    Directory.CreateDirectory(FINAL_DIR);
}

// ----------------------------------------------------------------------------
// Construcción de un plugin individual.
//
// Caso A (renombrarA == null): el .csproj ya define su propio <AssemblyName>
// igual al alias del lenguaje (Lapsos, Hilos, Zip, Cripto, Env, Log, Proceso,
// Texto, TablaPeriodica) -> se publica directamente en la carpeta común de
// plugins, sin pasos extra.
//
// Caso B (renombrarA != null): el ensamblado conserva el nombre por defecto
// del proyecto (Latino.Plugin.SQLite, Latino.Plugin.Formulas) -> se publica
// en una carpeta AISLADA (para no mezclar sus dependencias con las de otros
// plugins), se renombra el .dll principal al alias esperado por el
// intérprete, y se copian también sus dependencias propias (drivers nativos,
// librerías de cálculo numérico, etc.), excluyendo el núcleo y los plugins
// base para no duplicarlos.
//
// A diferencia del script original, esta función SIEMPRE deja constancia en
// consola cuando un plugin no se pudo compilar o no generó su .dll -antes,
// si el .csproj no aparecía en la ruta esperada, el paso se saltaba sin
// ningún mensaje, lo que hacía parecer que el plugin simplemente "no existía"-.
// ----------------------------------------------------------------------------
void BuildPlugin(string nombre, string csprojPath, string pluginOutDir, string? renombrarA = null)
{
    if (!File.Exists(csprojPath))
    {
        Console.WriteLine($"   ⚠️ Plugin '{nombre}' OMITIDO: no se encontró {csprojPath}");
        return;
    }

    Console.WriteLine($"🔨 Compilando Plugin: {nombre}...");

    if (renombrarA == null)
    {
        RunCommand($"dotnet publish {csprojPath} -c Release -o {pluginOutDir}");
        return;
    }

    string carpetaAislada = Path.Combine(TEMP_DIR, $"lib_raw_{renombrarA}");
    RunCommand($"dotnet publish {csprojPath} -c Release -o {carpetaAislada}");

    string nombreDllOriginal = Path.GetFileNameWithoutExtension(csprojPath) + ".dll";
    string dllOrigen = Path.Combine(carpetaAislada, nombreDllOriginal);
    string dllDestino = Path.Combine(pluginOutDir, $"{renombrarA}.dll");

    if (!File.Exists(dllOrigen))
    {
        Console.WriteLine($"   ⚠️ No se generó {nombreDllOriginal}; revisa el build del plugin '{nombre}'.");
        return;
    }

    File.Copy(dllOrigen, dllDestino, overwrite: true);

    // Copiar dependencias propias del plugin (ej. driver nativo de SQLite,
    // o librerías de cálculo numérico de Formulas) sin duplicar el núcleo.
    foreach (string dllPath in Directory.GetFiles(carpetaAislada, "*.dll"))
    {
        string archivo = Path.GetFileName(dllPath);
        bool esNucleoOBase = archivo == nombreDllOriginal
            || archivo == "Latino.Core.dll"
            || archivo == "Latino.Plugin.Hilos.dll"
            || archivo == "Latino.Plugin.Lapsos.dll";

        if (!esNucleoOBase)
        {
            File.Copy(dllPath, Path.Combine(pluginOutDir, archivo), overwrite: true);
        }
    }
}

void BuildSdkAndPlugins()
{
    Console.WriteLine("📦 Compilando SDK y Plugins...");

    // 1. SDK (Latino.Core)
    string sdkOut = Path.Combine(TEMP_DIR, "sdk");
    RunCommand($"dotnet publish {PATH_CORE} -c Release -o {sdkOut}");

    // 2. Plugins (lista única y explícita — agregar uno nuevo es una línea)
    string pluginOut = Path.Combine(TEMP_DIR, "lib_raw");

    var plugins = new (string Nombre, string CsProj, string? RenombrarA)[]
    {
        ("Lapsos",          PATH_PLUGIN_LAPSOS, null),
        ("Hilos",           PATH_PLUGIN_HILOS, null),
        ("Zip",             "plugins/Latino.Plugin.Zip/Latino.Plugin.Zip.csproj", null),
        ("Cripto",          "plugins/Latino.Plugin.Cripto/Latino.Plugin.Cripto.csproj", null),
        ("Env",             "plugins/Latino.Plugin.Env/Latino.Plugin.Env.csproj", null),
        ("Log",             "plugins/Latino.Plugin.Log/Latino.Plugin.Log.csproj", null),
        ("Proceso",         "plugins/Latino.Plugin.Proceso/Latino.Plugin.Proceso.csproj", null),
        ("Texto",           "plugins/Latino.Plugin.Texto/Latino.Plugin.Texto.csproj", null),
        ("Tabla Periodica", "plugins/Latino.Plugin.TablaPeriodica/Latino.Plugin.TablaPeriodica.csproj", null),
        ("SQLite",          "plugins/Latino.Plugin.SQLite/Latino.Plugin.SQLite.csproj", "sqlite"),
        ("Formulas",        "plugins/Latino.Plugin.Formulas/Latino.Plugin.Formulas.csproj", "formulas"),
    };

    foreach (var p in plugins)
    {
        BuildPlugin(p.Nombre, p.CsProj, pluginOut, p.RenombrarA);
    }
}

void BuildExecutables()
{
    Console.WriteLine("🔨 Compilando Ejecutables (SingleFile)...");

    // Flags para generar un solo archivo limpio
    string flags = "-p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true";

    // Windows x64
    RunCommand($"dotnet publish {PATH_APP} -c Release -r win-x64 --self-contained {flags} -o {Path.Combine(TEMP_DIR, "win")}");

    // Linux x64
    RunCommand($"dotnet publish {PATH_APP} -c Release -r linux-x64 --self-contained {flags} -o {Path.Combine(TEMP_DIR, "linux")}");
}

void AssemblePackages(string version)
{
    Console.WriteLine("📦 Ensamblando carpetas finales (Librerías y Ejemplos)...");

    string pathManual = $"docs/Manual_De_Latino.NET_v{version}.pdf";

    foreach (string osName in new[] { "win", "linux" })
    {
        string basePath = Path.Combine(TEMP_DIR, osName);
        string libPath = Path.Combine(basePath, "lib");
        string examplesPath = Path.Combine(basePath, "ejemplos");

        Directory.CreateDirectory(libPath);

        // 1. Copiar SDK
        string sdkFile = Path.Combine(TEMP_DIR, "sdk", "Latino.Core.dll");
        if (File.Exists(sdkFile))
        {
            File.Copy(sdkFile, Path.Combine(libPath, "Latino.Core.dll"), overwrite: true);
        }

        // Copiar todo el contenido de lib_raw a lib
        string rawLibs = Path.Combine(TEMP_DIR, "lib_raw");
        if (Directory.Exists(rawLibs))
        {
            CopyDirectory(rawLibs, libPath);
        }

        // 5. Copiar la dependencia nativa de SQLite directamente a la raíz del ejecutable
        string nativeSource = osName == "win"
            ? Path.Combine(TEMP_DIR, "lib_raw_sqlite", "runtimes", "win-x64", "native", "e_sqlite3.dll")
            : Path.Combine(TEMP_DIR, "lib_raw_sqlite", "runtimes", "linux-x64", "native", "libe_sqlite3.so");

        string nativeDest = osName == "win"
            ? Path.Combine(basePath, "e_sqlite3.dll")
            : Path.Combine(basePath, "libe_sqlite3.so");

        if (File.Exists(nativeSource))
        {
            File.Copy(nativeSource, nativeDest, overwrite: true);
            Console.WriteLine($"   ⚙️ Dependencia nativa SQLite copiada a la raíz de {osName}");
        }

        // 3. Copiar Carpeta de Ejemplos
        if (Directory.Exists(PATH_EXAMPLES))
        {
            if (Directory.Exists(examplesPath))
                Directory.Delete(examplesPath, recursive: true);

            CopyDirectory(PATH_EXAMPLES, examplesPath);
            Console.WriteLine($"   📂 Ejemplos copiados a {osName}");
        }
        else
        {
            Console.WriteLine("   ⚠️ No se encontró la carpeta 'ejemplos' en la raíz.");
        }

        // 4. Copiar Manual PDF
        if (File.Exists(pathManual))
        {
            File.Copy(pathManual, Path.Combine(basePath, Path.GetFileName(pathManual)), overwrite: true);
            Console.WriteLine($"   📄 Manual copiado a {osName}");
        }
        else
        {
            Console.WriteLine($"   ⚠️ No se encontró el manual en: {pathManual}");
        }
    }
}

void CopyDirectory(string sourceDir, string destDir)
{
    Directory.CreateDirectory(destDir);

    foreach (string file in Directory.GetFiles(sourceDir))
    {
        string destFile = Path.Combine(destDir, Path.GetFileName(file));
        File.Copy(file, destFile, overwrite: true);
    }

    foreach (string dir in Directory.GetDirectories(sourceDir))
    {
        string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
        CopyDirectory(dir, destSubDir);
    }
}

void BuildVsix(string version)
{
    Console.WriteLine($"🧩 Empaquetando Extensión VS Code v{version}...");

    // Actualizar servidor LSP
    string serverOut = "extension/server";
    if (Directory.Exists(serverOut))
    {
        try
        {
            Directory.Delete(serverOut, recursive: true);
        }
        catch
        {
            RunCommand($"if exist {serverOut} rmdir /s /q {serverOut}");
        }
    }

    RunCommand($"dotnet publish {PATH_LSP} -c Release -o {serverOut}");

    // Empaquetar VSIX
    RunCommand("npm install", workingDir: "extension");
    RunCommand("npm run compile", workingDir: "extension");

    string vsixName = $"latino-net-vscode-{version}.vsix";
    string vsixPath = Path.Combine("..", FINAL_DIR, vsixName);

    RunCommand($"npx vsce package -o {vsixPath} --allow-missing-repository", workingDir: "extension");
}

void CreateZips(string version)
{
    Console.WriteLine($"📦 Comprimiendo entregables v{version} (Excluyendo PDBs)...");

    foreach (string osTarget in new[] { "win", "linux" })
    {
        string zipPath = Path.Combine(FINAL_DIR, $"latino-{osTarget}-v{version}.zip");
        string folderToZip = Path.Combine(TEMP_DIR, osTarget);

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (string file in Directory.GetFiles(folderToZip, "*", SearchOption.AllDirectories))
            {
                // Excluir archivos no deseados
                if (file.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase))
                    continue;

                string entryName = Path.GetRelativePath(folderToZip, file);
                // Normalizar el separador: en Windows Path.GetRelativePath devuelve
                // '\' y el cliente zip lo graba literal. Infozip/unzip de Linux lo trata
                // como carácter de escape y aborta la extracción ("appears to use
                // backslashes as path separators"). El estándar ZIP exige '/'.
                entryName = entryName.Replace('\\', '/');
                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }
        }

        Console.WriteLine($"   ✅ Creado: {zipPath}");
    }
}

void CreateSublimeZip()
{
    Console.WriteLine("📦 Empaquetando sintaxis para Sublime Text...");

    string sourceDir = "Sublime-Text-support";
    string zipPath = Path.Combine(FINAL_DIR, "SublimeTextLatSyntax.zip");

    if (File.Exists(zipPath))
        File.Delete(zipPath);

    if (!Directory.Exists(sourceDir))
    {
        Console.WriteLine($"⚠️ No se encontró el directorio '{sourceDir}', omitiendo empaquetado de Sublime Text.");
        return;
    }

    try
    {
        ZipFile.CreateFromDirectory(sourceDir, zipPath, CompressionLevel.Optimal, false);
        Console.WriteLine($"   ✅ Creado: {zipPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al crear zip de Sublime Text: {ex.Message}");
    }
}

// ----------------------------------------------------------------------------
// Generación automática de documentación (tools/*.csx).
//
// Apagada por defecto (EJECUTAR_DOCGEN_AUTOMATICA = false) porque estas
// herramientas resultaron ser destructivas y con fallos conocidos. Si están
// desactivadas o la carpeta no existe, este paso NUNCA aborta el build —solo
// informa y sigue— a diferencia del comportamiento anterior, donde un fallo
// aquí mataba todo el proceso de release vía Environment.Exit(1).
// ----------------------------------------------------------------------------
void GenerarDocumentacionSiCorresponde()
{
    if (!EJECUTAR_DOCGEN_AUTOMATICA)
    {
        Console.WriteLine("⏭️  Generación automática de documentación DESACTIVADA (tools/*.csx deshabilitados a propósito).");
        return;
    }

    if (!Directory.Exists(PATH_TOOLS_DOCGEN))
    {
        Console.WriteLine($"   ⚠️ No se encontró la carpeta '{PATH_TOOLS_DOCGEN}'; se omite la generación de documentación.");
        return;
    }

    Console.WriteLine("📖 Auto-generando Documentación (Librerías, Snippets y Referencias)...");
    RunCommand($"dotnet script {PATH_TOOLS_DOCGEN}/generar_inventario.csx");
    RunCommand($"dotnet script {PATH_TOOLS_DOCGEN}/generar_snippets.csx");
    RunCommand($"dotnet script {PATH_TOOLS_DOCGEN}/generar_referencias.csx");
}

// ============================================================================
// EJECUCIÓN PRINCIPAL
// ============================================================================

string VERSION = GetVersionFromJson();

Clean();

GenerarDocumentacionSiCorresponde();

BuildSdkAndPlugins();
BuildExecutables();
AssemblePackages(VERSION);
CreateSublimeZip();
BuildVsix(VERSION);
CreateZips(VERSION);

Console.WriteLine($"\n✅ PROCESO COMPLETADO. Archivos listos en: {FINAL_DIR}");
return 0;
