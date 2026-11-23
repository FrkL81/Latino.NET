# CAPÍTULO 15: Mantenimiento de la Extensión Oficial (VS Code)

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Mantenedores del núcleo, desarrolladores de extensiones de VS Code, ingenieros de herramientas (*Tooling Engineers*) y colaboradores del ecosistema de desarrollo de Latino.NET.  
**Propósito:** Detallar la arquitectura interna de la extensión oficial para Visual Studio Code: implementación del Language Server Protocol (LSP) en C#, sincronización de buffers en memoria, introspección en tiempo real, catálogo de Magic Strings y pipeline de publicación del paquete `.vsix`.

## La Experiencia de Desarrollo y el Principio de un Solo Cerebro

Un lenguaje de programación moderno no triunfa únicamente por la elegancia de su gramática o la velocidad de su máquina virtual; triunfa cuando la **Experiencia del Desarrollador (DX - *Developer Experience*)** es fluida, predecible y estimulante. Escribir código en un editor de texto plano a ciegas —sin detección inmediata de errores, sin documentación al pasar el cursor y sin autocompletado contextual— convierte el desarrollo en una tarea lenta y propensa a fallos.

Para dotar a Latino.NET de un entorno de primera clase, rechazamos desde el primer día la solución rápida adoptada por otros proyectos: escribir expresiones regulares frágiles en JavaScript que adivinan la sintaxis.

En su lugar, diseñamos la extensión oficial sobre el estándar industrial de Microsoft: el **Language Server Protocol (LSP)**, respaldado por una directiva arquitectónica absoluta: **el principio de «Un Solo Cerebro»**.

### El Problema que Resolvemos

En extensiones tradicionales de lenguajes, coexisten **dos parsers independientes**:

```text
┌─────────────────┐         ┌──────────────────┐
│  Intérprete     │         │   Extensión      │
│  (C#/C++)       │         │   (JS/Regex)     │
│                 │         │                  │
│  Parser Real    │    ≠    │  Parser "Falso"  │
│  Ejecuta código │         │  Solo colorea    │
└─────────────────┘         └──────────────────┘
```

**Consecuencia:** Añadir `mate.hipotenusa()` requiere programarlo dos veces. Si olvidas actualizar la extensión, VS Code marca errores en código válido.

### Nuestra Solución: LSP como Puente

Utilizamos el **Language Server Protocol** para que la extensión sea una "ventana transparente" hacia el intérprete real:

```text
┌──────────────┐    LSP     ┌─────────────────┐
│  VS Code     │ ◄────────► │ Latino.Core     │
│  (TypeScript)│   JSON     │ (C# - El Único  │
│  UI Manager  │            │  Cerebro)       │
└──────────────┘            └─────────────────┘
```

- **Latino.Core:** Define gramática, funciones, validación. Es la fuente de verdad.
- **Latino.LanguageServer:** Adaptador que traduce entre VS Code y el Core (stdin/stdout).
- **Cliente VS Code:** Thin client que gestiona UI, configuración y ciclo de vida del proceso.

## 🏗️ Estructura del Repositorio

```text
Latino.NET/
├── src/
│   ├── Latino.Core/              # El Cerebro
│   │   ├── Analysis/             # Lexer, Parser, AST
│   │   ├── Runtime/              # Interpreter, Memory
│   │   └── StdLib/               # Librerías estándar (mate, sis, etc.)
│   │
│   └── Latino.LanguageServer/    # El Adaptador LSP
│       ├── Program.cs            # Punto de entrada
│       ├── BufferManager.cs      # Cache de archivos en memoria
│       ├── PluginManager.cs      # Sistema de plugins (.dll)
│       ├── TextDocumentHandler.cs    # Validación en tiempo real
│       ├── CompletionHandler.cs      # Autocompletado inteligente
│       ├── HoverHandler.cs           # Documentación al pasar el mouse
│       ├── DocumentSymbolHandler.cs  # Símbolos de documento (Outline)
│       ├── MagicStringsCatalog.cs    # Catálogo de strings mágicos (@)
│       └── MarkdownHelper.cs         # Formateador Markdown y LaTeX
│
└── extension/                    # El Cliente de VS Code
    ├── src/
    │   └── extension.ts          # Lógica de activación
    ├── syntaxes/
    │   └── latino.tmLanguage.json # Colorización TextMate
    ├── themes/
    │   ├── Latino.NET-Dark-color-theme.json
    │   └── Latino.NET-Light-color-theme.json
    ├── snippets/
    │   └── latino.code-snippets  # Plantillas de código
    ├── server/                   # ⚠️ IGNORADO EN GIT (binarios)
    │   └── Latino.LanguageServer.dll
    └── package.json              # Manifiesto de la extensión
```

### Carpetas Críticas

| Carpeta | Propósito | ¿En Git? |
| :--- | :--- | :--- |
| `src/Latino.Core` | Fuente de verdad del lenguaje | ✅ Sí |
| `src/Latino.LanguageServer` | Código fuente del LSP | ✅ Sí |
| `extension/server/` | Binarios compilados (.dll) | ✅ Sí (precompilados) |
| `extension/syntaxes/` | Gramática de colores | ✅ Sí |

## 🔄 Ciclo de Vida Completo de una Actualización

### Escenario Real: Añadir `mate.hipotenusa(a, b)`

#### **Paso 1: Programar en el Core**

```csharp
// src/Latino.Core/StdLib/MathLib.cs -> Dentro de Register(Interpreter interpreter)
Def("hipotenusa", (i, args) => {
    double a = Get(args[0]);
    double b = Get(args[1]);
    return new LatinoNumber(Math.Sqrt(a * a + b * b));
}, 2, "Calcula la hipotenusa de un triángulo rectángulo.", "cateto_a, cateto_b");
```

**Estado:** `Latino.Core` ahora sabe que existe, pero nadie más.

#### **Paso 2: Recompilar el Servidor LSP**

```bash
# Desde la raíz del proyecto
dotnet publish src/Latino.LanguageServer/Latino.LanguageServer.csproj \
  -c Release \
  -o extension/server
```

**¿Por qué?** El `.dll` actual en `extension/server/` contiene una versión vieja de `Latino.Core`. Necesitamos sobrescribirlo con la nueva versión que incluye `hipotenusa()`.

**Verificación:**

```bash
ls -lh extension/server/Latino.Core.dll  # Fecha modificada debe ser reciente
```

#### **Paso 3: (Opcional) Actualizar Gramática de Colores**

Si añadiste una **palabra reservada nueva** (ej: `hipotenusa` como keyword), edita:

```json
// extension/syntaxes/latino.tmLanguage.json
{
  "functions": {
    "patterns": [{
      "name": "support.class.latino",
      "match": "\\b(mate|cadena|sis)\\b"  // ← mate ya está aquí
    }]
  }
}
```

**Nota:** Para funciones de librería (`mate.hipotenusa`), no hace falta cambiar nada aquí. La gramática TextMate solo afecta el **colorizado estático**, no la inteligencia (que viene del LSP).

#### **Paso 4: Empaquetar la Extensión**

```bash
cd extension

# 1. Incrementar versión (OBLIGATORIO para que VS Code detecte cambios)
# Editar package.json: "version": "1.0.81" → "1.0.82"

# 2. Compilar TypeScript
npm run compile

# 3. Generar .vsix
npx vsce package
```

**Resultado:** `latino-net-vscode-1.0.82.vsix`

#### **Paso 5: Instalar y Probar**

```bash
# Desinstalar versión antigua
code --uninstall-extension LatinoLang.latino-net-vscode

# Instalar nueva versión
code --install-extension latino-net-vscode-1.0.82.vsix

# Reiniciar VS Code completamente (no solo reload)
```

**Prueba:**

```latino
# test.lat
resultado = mate.hipote  # ← Aquí debería aparecer autocompletado
```

## 📊 Flujo de Datos en Tiempo Real

```text
┌─────────────────────────────────────────────────────────────┐
│ Usuario escribe: mate.se|                                   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
          ┌──────────────────────┐
          │  extension.ts        │
          │  (Cliente TypeScript)│
          └──────────┬───────────┘
                     │ Lanza proceso:
                     │ dotnet Latino.LanguageServer.dll
                     ▼
          ┌──────────────────────┐
          │  Program.cs          │
          │  Configura DI        │
          │  - Interpreter (★)   │
          │  - PluginManager     │
          └──────────┬───────────┘
                     │
        ┌────────────┴────────────┐
        ▼                         ▼
┌───────────────┐       ┌─────────────────┐
│TextDocument   │       │Completion       │
│Handler        │       │Handler          │
│               │       │                 │
│1. Guarda en   │       │1. Lee Globals   │
│   BufferMgr   │       │   del Interpreter│
│2. Parsea con  │       │2. Extrae métodos│
│   Latino.Core │       │   de "mate"     │
│3. Envía ❌    │       │3. Retorna lista │
└───────────────┘       └─────────────────┘
        │                         │
        └────────────┬────────────┘
                     ▼
          ┌──────────────────────┐
          │  VS Code UI          │
          │  - Líneas rojas      │
          │  - Autocompletado:   │
          │    mate.sen()        │
          │    mate.cos()        │
          └──────────────────────┘
```

**★ Punto Clave:** El `Interpreter` es un **Singleton** (una sola instancia compartida). Se crea al iniciar el servidor con todas las librerías estándar pre-cargadas, evitando recargarlas en cada keystroke.

## 🔍 Entendiendo `package.json` (Manifiesto)

```json
{
  "name": "latino-net-vscode",
  "version": "1.0.81",  // ← INCREMENTAR en cada release
  "main": "./dist/extension.js",  // ← Punto de entrada compilado
  
  "contributes": {
    "languages": [{
      "id": "latino",
      "extensions": [".lat"]
    }],
    
    "configuration": {
      "properties": {
        "latino.binaryPath": {  // ← Ruta al ejecutable latino.exe
          "type": "string",
          "default": ""
        }
      }
    },
    
    "commands": [
      {
        "command": "latino.run",
        "title": "Ejecutar Latino",
        "category": "Latino",
        "icon": "$(play)"
      },
      {
        "command": "latino.newFile",
        "title": "Nuevo archivo vacío",
        "category": "Latino"
      },
      {
        "command": "latino.newModule",
        "title": "Nuevo módulo simple",
        "category": "Latino"
      },
      {
        "command": "latino.newClass",
        "title": "Nuevo módulo de clases",
        "category": "Latino"
      },
      {
        "command": "latino.newCrudProject",
        "title": "Nuevo proyecto CRUD CLI (SQLite)",
        "category": "Latino"
      },
      {
        "command": "latino.newProfessionalProject",
        "title": "Nuevo Proyecto Profesional (Estructura Base)",
        "category": "Latino"
      }
    ]
  }
}
```

### ¿Por qué Hay Dos `package.json`?

1. **`/package.json` (Raíz):** Scripts globales del repo (opcional, puede no existir).
2. **`/extension/package.json`:** El manifiesto **real** que VS Code lee.

## ❓ FAQ para Desarrolladores

### P: Modifiqué `MathLib.cs` pero el autocompletado no muestra mi función

**Diagnóstico:**

```bash
# Verificar fecha de Latino.Core.dll en la extensión
ls -lh extension/server/Latino.Core.dll
```

**Solución:**

```bash
# 1. Recompilar servidor
dotnet publish src/Latino.LanguageServer/Latino.LanguageServer.csproj \
    -c Release -o extension/server

# 2. Reiniciar VS Code (no solo reload window)
```

### P: ¿Por qué `extension/server/` está versionado en Git?

**Razón:** la extensión resuelve el servidor en `server/Latino.LanguageServer.dll` y lo lanza como `dotnet <ese dll>`. Esa carpeta es lo que el VSIX empaqueta en runtime, de modo que versionarla permite **empaquetar y probar la extensión sin compilar previamente C#**.

**Consecuencia:** al clonar el repo, la carpeta ya contiene los binarios; no hace falta compilar para probar la extensión en desarrollo.

Es una decisión deliberada: es «ruido esperado» menor que no rompe nada. La solución de raíz —empaquetado del VSIX por plataforma, con el LSP autocontenido— está prevista para la **v1.0.82**, momento en el que se revisará.

### P: Cambié `latino.tmLanguage.json` pero los colores no cambian

**Causas:**

1. VS Code cachea gramáticas agresivamente.
2. No incrementaste la versión en `package.json`.

**Solución:**

```bash
# 1. Editar extension/package.json
"version": "1.0.81"  # ← Cambiar

# 2. Reempaquetar
cd extension && npx vsce package

# 3. Reinstalar VSIX
code --install-extension latino-net-vscode-1.0.82.vsix

# 4. Restart VS Code completamente
```

**Atajo de Desarrollo:** Presionar `F5` en VS Code (con la extensión abierta como carpeta) lanza una ventana de "Extension Development Host" que fuerza la recarga sin reinstalar.

### P: ¿Puedo añadir validación en `extension.ts`?

**Respuesta Corta:** No deberías.

**Explicación:** Si pones lógica de validación en TypeScript, estás rompiendo el principio del "Cerebro Único". El resultado será:

- El intérprete de consola valida diferente al editor.
- Deberás mantener dos implementaciones sincronizadas manualmente.

**Correcto:** Añade la validación en `Latino.Core` (C#), y automáticamente estará disponible en:

- El intérprete de consola (`latino.exe`)
- El servidor LSP (VS Code)
- Cualquier otro frontend futuro

### P: ¿Qué hace exactamente `dotnet publish -o extension/server`?

```bash
dotnet publish src/Latino.LanguageServer/Latino.LanguageServer.csproj \
  -c Release \           # ← Modo Release (optimizado)
  -o extension/server    # ← Output: sobrescribir DLLs aquí
```

**Resultado:**

```text
extension/server/
├── Latino.LanguageServer.dll      ← El servidor LSP
├── Latino.Core.dll                ← ★ El cerebro (con tu nueva función)
├── OmniSharp.Extensions.*.dll     ← Dependencias LSP
└── ... (runtime dependencies)
```

**Crítico:** La nueva versión de `Latino.Core.dll` reemplaza la vieja. Sin este paso, el servidor sigue usando el código obsoleto.

## 🔧 Arquitectura del Servidor (Backend C#)

### Program.cs - El Orquestador

```csharp
static async Task Main(string[] args)
{
    // 1. Parsear argumentos del cliente
    string? globalPathArg = null;
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--global-path" && i + 1 < args.Length)
        {
            globalPathArg = args[i + 1];
        }
    }

    var server = await LanguageServer.From(options =>
        options
            .WithInput(Console.OpenStandardInput())   // ← Lee de VS Code
            .WithOutput(Console.OpenStandardOutput()) // ← Responde a VS Code
            .ConfigureLogging(x => x
                .AddLanguageProtocolLogging()
                .SetMinimumLevel(LogLevel.Debug)
            )
            .WithServices(services =>
            {
                // ★ SINGLETON: Una sola instancia del Interpreter
                services.AddSingleton<Interpreter>(sp => {
                    var interp = new Interpreter();
                        // Carga mate, sis, etc.
                        Latino.Core.StdLib.StdLibLoader.LoadAll(interp);
                    return interp;
                });
                
                services.AddSingleton<PluginManager>(sp => 
                    new PluginManager(sp.GetService<ILogger<PluginManager>>()));
            })
            .WithHandler<TextDocumentHandler>()   // ← Validación
            .WithHandler<CompletionHandler>()     // ← Autocompletado
            .WithHandler<HoverHandler>()          // ← Documentación
            .WithHandler<DocumentSymbolHandler>() // ← Símbolos de documento
            .OnInitialize((server, request, token) =>
            {
                // 2. Inyectar ruta global al PluginManager
                if (!string.IsNullOrEmpty(globalPathArg))
                {
                    var pm = server.Services.GetService<PluginManager>();
                    pm.GlobalPath = globalPathArg;
                }
                return Task.CompletedTask;
            })
    );

    await server.WaitForExit;
}
```

#### **Puntos Clave:**

1. **Comunicación por Pipes Estándar:**
   - `stdin`: Recibe mensajes JSON-RPC de VS Code
   - `stdout`: Envía respuestas JSON-RPC a VS Code
   - `stderr`: Canal de logging (visible en "Output > Latino Language Server")

2. **Inyección de Dependencias:**
   - `Singleton<Interpreter>`: Se crea **una vez** al inicio. Todos los handlers comparten la misma instancia.
   - `Singleton<PluginManager>`: Gestiona la carga de DLLs externas.

3. **Argumentos Ocultos:**
   - `--global-path "C:\Latino"`: Pasado por `extension.ts` desde la configuración `latino.binaryPath`.
   - Permite al servidor buscar plugins en la instalación global del usuario.

### BufferManager.cs - Cache de Archivos

```csharp
public static class BufferManager
{
    private static readonly ConcurrentDictionary<DocumentUri, string> _buffers = new();

    public static void UpdateBuffer(DocumentUri uri, string text) 
        => _buffers[uri] = text;
    
    public static string? GetBuffer(DocumentUri uri) 
        => _buffers.TryGetValue(uri, out var text) ? text : null;
}
```

#### **¿Por Qué Existe?**

VS Code envía el contenido del editor **antes de que el usuario guarde**. Sin este cache, el servidor no podría validar código no guardado.

#### **Flujo:**

```text
Usuario escribe:
    x = 10 + 

TextDocumentHandler:
    BufferManager.UpdateBuffer(uri, "x = 10 + ")
    ValidateTextDocument(uri, "x = 10 + ")  ← Parsea el texto en memoria
    
Resultado:
    Línea roja: "Error de sintaxis: expresión incompleta"
```

### PluginManager.cs - Sistema de Extensibilidad

El componente más complejo. Permite cargar librerías externas (.dll) sin recompilar.

#### **1. Escaneo de Código:**

```csharp
public void ScanAndLoadPlugins(DocumentUri documentUri, string sourceCode)
{
    try
    {
        // FIX 1: Regex relajada (cualquier contenido en comillas)
        var pattern = @"incluir\s*\(\s*[\"\"']([^\"\"']+)[\"\"']\s*\)";
        var matches = Regex.Matches(sourceCode, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string rawName = match.Groups[1].Value;
            
            // Ignoramos scripts .lat, solo buscamos DLLs potenciales
            if (rawName.EndsWith(".lat", StringComparison.OrdinalIgnoreCase)) continue;

            string dllPath = ResolveDllPath(documentUri, rawName);

            if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
            {
                LoadPlugin(dllPath);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"[PluginManager] Error al escanear plugins: {ex.Message}");
    }
}
```

**Ejemplo Detectado:**

```latino
incluir("lapsos")  // ← Plugin externo
incluir("utils.lat") // ← Ignorado (script, no DLL)
```

#### **2. Resolución Inteligente de Rutas:**

```csharp
public string ResolveDllPath(DocumentUri documentUri, string rawName)
{
    var scriptDir = Path.GetDirectoryName(documentUri.GetFileSystemPath());
    
    // Candidatos de nombre
    var candidates = rawName.EndsWith(".dll") 
        ? new[] { rawName } 
        : new[] { rawName + ".dll", rawName };
    
    // Rutas de búsqueda (Inteligencia relativa)
    var searchPaths = new List<string>
    {
        scriptDir,                          // Misma carpeta
        Path.Combine(scriptDir, "lib"),     // Subcarpeta lib
        Path.Combine(scriptDir, "plugins"), // Subcarpeta plugins
        Path.Combine(scriptDir, ".."),      // Carpeta padre (útil en tests)
        Path.Combine(scriptDir, "../lib"),
        Path.Combine(scriptDir, "../plugins")
    };

    // Intentar también LATINO_HOME si existe
    var latinoHome = Environment.GetEnvironmentVariable("LATINO_HOME");
    if (!string.IsNullOrEmpty(latinoHome))
    {
        searchPaths.Add(latinoHome);
        searchPaths.Add(Path.Combine(latinoHome, "lib"));
        searchPaths.Add(Path.Combine(latinoHome, "plugins"));
    }

    // 3. Ruta Global Inyectada (Desde VS Code Config)
    if (!string.IsNullOrEmpty(GlobalPath))
    {
        searchPaths.Add(GlobalPath);
        searchPaths.Add(Path.Combine(GlobalPath, "lib"));
        searchPaths.Add(Path.Combine(GlobalPath, "plugins"));
    }

    foreach (var basePath in searchPaths)
    {
        foreach (var candidate in candidates)
        {
            string fullPath = Path.Combine(basePath, candidate);
            if (File.Exists(fullPath)) return fullPath;
        }
    }
    
    return ""; // No encontrado
}
```

**Ejemplo de Búsqueda:**

```text
Usuario escribe: incluir("lapsos")
Documento actual: C:\Proyectos\MiApp\src\main.lat

Búsqueda:
1. C:\Proyectos\MiApp\src\lapsos.dll         ❌
2. C:\Proyectos\MiApp\src\lib\lapsos.dll     ✅ ENCONTRADO
```

#### **3. Carga con Reflexión:**

```csharp
private void LoadPlugin(string dllPath)
{
    if (_loadedPlugins.ContainsKey(dllPath)) return; // Ya cargado

    try
    {
        _logger.LogInformation($"[PluginManager] Cargando plugin: {dllPath}");
        
        var assembly = Assembly.LoadFrom(Path.GetFullPath(dllPath));
        var pluginMetadata = new PluginMetadata { Path = dllPath };

        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(
                    BindingFlags.Public | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<LatinoExtensionAttribute>();
                if (attr != null)
                {
                    var funcMetadata = new FunctionMetadata
                    {
                        Name = attr.FunctionName,
                        Help = attr.Help ?? string.Empty,
                        Args = attr.Args ?? string.Empty,
                        Returns = attr.Returns ?? string.Empty
                    };

                    pluginMetadata.Functions.Add(funcMetadata);

                    // Clasificar: ¿Es función global o método de módulo?
                    if (funcMetadata.Name.Contains('.'))
                    {
                        var parts = funcMetadata.Name.Split(new[] { '.' }, 2);
                        var moduleName = parts[0];
                        var methodName = parts[1];

                        // Agregar al módulo
                        if (!_modules.TryGetValue(moduleName, out var module))
                        {
                            module = new ModuleMetadata { Name = moduleName };
                            _modules[moduleName] = module;
                        }

                        module.Methods[methodName] = funcMetadata;
                    }
                    else
                    {
                        // Función global
                        _globalFunctions[funcMetadata.Name] = funcMetadata;
                    }

                    _logger.LogDebug(
                        $"[PluginManager] Función descubierta: {funcMetadata.Name}");
                }
            }
        }

        _loadedPlugins[dllPath] = pluginMetadata;
        _logger.LogInformation(
            $"[PluginManager] Plugin cargado exitosamente: {dllPath} " +
            $"({pluginMetadata.Functions.Count} funciones)");
    }
    catch (Exception ex)
    {
        _logger.LogError(
            $"[PluginManager] Error al cargar plugin '{dllPath}': {ex.Message}");
    }
}
```

**Estructura del Atributo en el Plugin:**

```csharp
// Dentro de lapsos.dll
[LatinoExtension("lapsos.ahora", 
    Help = "Obtiene la fecha y hora actual",
    Args = "[formato]",
    Returns = "cadena")]
public static LatinoObject GetNow(List<LatinoObject> args)
{
    string formato = args.Count > 0 ? args[0].ToString() : "yyyy-MM-dd HH:mm:ss";
    return new LatinoString(DateTime.Now.ToString(formato));
}

[LatinoExtension("lapsos.sumarDias",
    Help = "Suma días a una fecha",
    Args = "fecha, dias",
    Returns = "cadena")]
public static LatinoObject AddDays(List<LatinoObject> args)
{
    if (args.Count < 2) 
        throw new RuntimeError(null!, "lapsos.sumarDias requiere fecha y dias.");

    var dt = DateTime.Parse(args[0].ToString());
    double dias = ((LatinoNumber)args[1]).Value;
    return new LatinoString(dt.AddDays(dias).ToString("yyyy-MM-dd"));
}
```

### TextDocumentHandler.cs - El Vigilante

Responsable de la validación en tiempo real.

```csharp
public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken ct)
{
    foreach (var change in request.ContentChanges)
    {
        // 1. Actualizar memoria
        BufferManager.UpdateBuffer(request.TextDocument.Uri, change.Text);
        
        // 2. Re-escanear plugins (por si se añadió/quitó un incluir())
        _pluginManager.ScanAndLoadPlugins(request.TextDocument.Uri, change.Text);
        
        // 3. Validar sintaxis
        ValidateTextDocument(request.TextDocument.Uri, change.Text);
    }
    return Unit.Task;
}

private void ValidateTextDocument(DocumentUri uri, string text)
{
    var diagnostics = new List<Diagnostic>();

    try
    {
        // Análisis Léxico
        var lexer = new Lexer(text);
        var tokens = lexer.Tokenize();
        
        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Error)
            {
                diagnostics.Add(new Diagnostic {
                    Severity = DiagnosticSeverity.Error,
                    Message = $"Carácter inesperado: '{token.Text}'",
                    Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                        new Position(token.Line - 1, token.Column - 1),
                        new Position(token.Line - 1, token.Column)
                    )
                });
            }
        }

        // Análisis Sintáctico
        var parser = new Parser(tokens);
        parser.Parse();
        
        // Recopilar errores del parser
        foreach (var error in parser.Errors)
        {
            diagnostics.Add(new Diagnostic {
                Severity = DiagnosticSeverity.Error,
                Message = error.Message,
                Range = new Range(
                    new Position(error.Token.Line - 1, error.Token.Column - 1),
                    new Position(error.Token.Line - 1, 
                                error.Token.Column + error.Token.Text.Length)
                ),
                Source = "Latino Parser"
            });
        }
    }
    catch (Exception ex)
    {
        _logger.LogCritical($"Excepción: {ex.Message}");
    }

    // Enviar a VS Code
    _router.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams {
        Uri = uri,
        Diagnostics = new Container<Diagnostic>(diagnostics)
    });
}
```

#### **Detalle Crucial: Coordenadas**

VS Code usa **indexación base 0** (línea 0, columna 0).  
Latino.Core usa **indexación base 1** (línea 1, columna 1).

**Conversión:**

```csharp
new Position(error.Token.Line - 1, error.Token.Column - 1)
```

### CompletionHandler.cs - El Sugeridor

Genera autocompletado usando **introspección en tiempo real**.

```csharp
public Task<CompletionList> Handle(CompletionParams request, CancellationToken ct)
{
    var completions = new List<CompletionItem>();

    // --- NUEVO: DETECCIÓN DE CONTEXTO DE CADENA (Magic Strings) ---
    string? buffer = BufferManager.GetBuffer(request.TextDocument.Uri);
    if (buffer != null)
    {
        var lines = buffer.Split('\n');
        int lineIdx = request.Position.Line;
        if (lineIdx < lines.Length)
        {
            string lineText = lines[lineIdx];
            int charIdx = request.Position.Character;
            
            if (charIdx > lineText.Length) charIdx = lineText.Length;
            string textBeforeCursor = lineText.Substring(0, charIdx);
            
            int doubleQuotesCount = textBeforeCursor.Count(c => c == '"');
            int singleQuotesCount = textBeforeCursor.Count(c => c == '\'');
            bool insideDouble = doubleQuotesCount % 2 != 0;
            bool insideSingle = singleQuotesCount % 2 != 0;

            if (insideDouble || insideSingle)
            {
                // Estamos dentro de una cadena -> Ofrecer Catálogo
                var start = charIdx;
                while (start > 0
                    && !char.IsWhiteSpace(lineText[start - 1])
                    && lineText[start - 1] != '"')
                    start--;

                var range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                    new Position(lineIdx, start),
                    new Position(lineIdx, charIdx)
                );

                foreach (var item in MagicStringsCatalog.Items)
                {
                    completions.Add(new CompletionItem {
                        Label = item.Label,
                        InsertText = item.InsertText,
                        FilterText = item.FilterText,
                        Detail = item.Detail,
                        Kind = item.Kind,
                        Preselect = item.Preselect,
                        Documentation = item.Documentation,
                        TextEdit = new TextEdit {
                            NewText = item.InsertText ?? item.Label,
                            Range = range
                        }
                    });
                }
                
                return Task.FromResult(new CompletionList(completions));
            }
        }
    }

    // 1. Palabras Clave (Keywords del Lexer)
    foreach (var kw in Latino.Core.Analysis.Lexer.Keywords.Keys)
    {
        completions.Add(new CompletionItem
            {
                Label = kw,
                Kind = CompletionItemKind.Keyword
            });
    }

    // 2. INTROSPECCIÓN DINÁMICA DEL ENTORNO (Librerías estándar)
    foreach(var key in _interpreter.Globals.GetKeys())
    {
         var obj = _interpreter.Globals.GetAt(key);
         
         if (obj is LatinoDictionary lib)
         {
             // ES UNA LIBRERÍA (ej: "mate", "sis")
             completions.Add(new CompletionItem { 
                 Label = key, 
                 Kind = CompletionItemKind.Module,
                 Detail = "Librería Estándar"
             });

             // Agregar todos sus métodos (ej: "mate.sen")
             foreach(var methodKey in lib.Pairs.Keys)
             {
                 string methodName = methodKey.ToString();
                 var methodObj = lib.Pairs[methodKey];
                 string fullLabel = $"{key}.{methodName}";
                 
                 CompletionItem item;

                 if (methodObj is NativeFunction nf)
                 {
                    string? documentation = !string.IsNullOrEmpty(nf.Description)
                        ? nf.Description
                        : null;
                     string detail = $"Método de {key}";
                     string insertText = fullLabel;
                     InsertTextFormat insertFormat = InsertTextFormat.PlainText;

                     if (!string.IsNullOrEmpty(nf.ParameterNames))
                     {
                         insertFormat = InsertTextFormat.Snippet;
                        var pList = nf.ParameterNames.Split(',')
                            .Select(p => p.Trim())
                            .ToList();
                        var snippetArgs = string.Join(", ",
                            pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                         insertText = $"{fullLabel}({snippetArgs})";
                         detail += $"({nf.ParameterNames})";
                     }

                     item = new CompletionItem { 
                        Label = fullLabel, 
                        Kind = CompletionItemKind.Method,
                        Detail = detail,
                        Documentation = documentation != null
                            ? new StringOrMarkupContent(
                                new MarkupContent
                                {
                                    Kind = MarkupKind.Markdown,
                                    Value = MarkdownHelper.FormatHelp(documentation)
                                })
                            : null,
                        InsertText = insertText,
                        InsertTextFormat = insertFormat
                     };
                 }
                 else
                 {
                     item = new CompletionItem { 
                        Label = fullLabel, 
                        Kind = CompletionItemKind.Method,
                        Detail = $"Método de {key}",
                        InsertText = fullLabel
                     };
                 }

                 completions.Add(item);
             }
         }
         else
         {
             // ES UNA FUNCIÓN GLOBAL (ej: "escribir", "incluir")
             CompletionItem item;

             if (obj is NativeFunction nf)
             {
                string? documentation = !string.IsNullOrEmpty(nf.Description)
                    ? nf.Description
                    : null;
                 string detail = "Función Global";
                 string insertText = key;
                 InsertTextFormat insertFormat = InsertTextFormat.PlainText;

                 if (!string.IsNullOrEmpty(nf.ParameterNames))
                 {
                     insertFormat = InsertTextFormat.Snippet;
                    var pList = nf.ParameterNames.Split(',')
                        .Select(p => p.Trim())
                        .ToList();
                    var snippetArgs = string.Join(", ",
                        pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                     insertText = $"{key}({snippetArgs})";
                     detail += $"({nf.ParameterNames})";
                 }

                 item = new CompletionItem { 
                     Label = key, 
                     Kind = CompletionItemKind.Function,
                     Detail = detail,
                    Documentation = documentation != null
                        ? new StringOrMarkupContent(
                            new MarkupContent
                            {
                                Kind = MarkupKind.Markdown,
                                Value = MarkdownHelper.FormatHelp(documentation)
                            })
                        : null,
                     InsertText = insertText,
                     InsertTextFormat = insertFormat
                 };
             }
             else
             {
                 item = new CompletionItem { 
                     Label = key, 
                     Kind = CompletionItemKind.Function,
                     Detail = "Función Global",
                     InsertText = key
                 };
             }

             completions.Add(item);
         }
    }

    // 3. NUEVO: AGREGAR FUNCIONES DE PLUGINS
    // 3.1 Funciones globales de plugins
    foreach (var func in _pluginManager.GetGlobalFunctions())
    {
        string detail = "Función de Plugin";
        string insertText = func.Name;
        InsertTextFormat insertFormat = InsertTextFormat.PlainText;

        if (!string.IsNullOrEmpty(func.Args))
        {
            insertFormat = InsertTextFormat.Snippet;
            var pList = func.Args.Split(',').Select(p => p.Trim()).ToList();
            var snippetArgs = string.Join(", ",
                pList.Select((p, i) => $"${{{i+1}:{p}}}"));
            insertText = $"{func.Name}({snippetArgs})";
            detail += $"({func.Args})";
        }

        completions.Add(new CompletionItem
        {
            Label = func.Name,
            Kind = CompletionItemKind.Function,
            Detail = detail,
                Documentation = !string.IsNullOrEmpty(func.Help)
                    ? new StringOrMarkupContent(
                        new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = MarkdownHelper.FormatHelp(func.Help)
                        })
                    : null,
            InsertText = insertText,
            InsertTextFormat = insertFormat
        });
    }

    // 3.2 Plugins (ej: "lapsos")
    foreach (var module in _pluginManager.GetModules())
    {
        // Agregar el módulo en sí
        completions.Add(new CompletionItem
        {
            Label = module.Name,
            Kind = CompletionItemKind.Module,
            Detail = "Módulo de Plugin"
        });

        // Agregar todos los métodos del plugin (ej: "lapsos.ahora")
        foreach (var method in module.Methods.Values)
        {
            string detail = $"Método de {module.Name}";
            string insertText = method.Name; // Ya contiene el prefijo completo
            InsertTextFormat insertFormat = InsertTextFormat.PlainText;

            if (!string.IsNullOrEmpty(method.Args))
            {
                insertFormat = InsertTextFormat.Snippet;
                var pList = method.Args.Split(',').Select(p => p.Trim()).ToList();
                var snippetArgs = string.Join(", ",
                    pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                insertText = $"{method.Name}({snippetArgs})";
                detail += $"({method.Args})";
            }

            completions.Add(new CompletionItem
            {
                Label = method.Name,
                Kind = CompletionItemKind.Method,
                Detail = detail,
                    Documentation = !string.IsNullOrEmpty(method.Help)
                        ? new StringOrMarkupContent(
                            new MarkupContent
                            {
                                Kind = MarkupKind.Markdown,
                                Value = MarkdownHelper.FormatHelp(method.Help)
                            })
                        : null,
                InsertText = insertText,
                InsertTextFormat = insertFormat
            });
        }
    }

    return Task.FromResult(new CompletionList(completions));
}
```

🎯 **Ejemplo de Snippet Generado:**

Cuando el usuario escribe `mate.sen` y acepta el autocompletado:

```latino
mate.sen(${1:angulo})  // ← Cursor se posiciona en "angulo", Tab salta al final
```

Esto se logra con:

```csharp
InsertTextFormat = InsertTextFormat.Snippet
InsertText = "mate.sen(${1:angulo})"
```

## 🎨 El Cliente (Frontend TypeScript)

### extension.ts - El Coordinador

#### **1. Inyección de Configuración Global**

```typescript
const config = vscode.workspace.getConfiguration('latino');
let binaryPath = config.get<string>('binaryPath');

if (binaryPath && binaryPath.trim() !== "") {
    let cleanPath = binaryPath.replace(/['"]/g, '').trim();
    let globalDir = cleanPath;

    // Si es un archivo (latino.exe), sacar el directorio
    if (fs.existsSync(cleanPath)) {
        const stat = fs.statSync(cleanPath);
        if (stat.isFile()) {
            globalDir = path.dirname(cleanPath);
        }
    }

    extraArgs.push('--global-path');
    extraArgs.push(globalDir);
}
```

**Propósito:** Permitir que el servidor encuentre plugins en la instalación global del usuario.

**Configuración del Usuario:**

```json
// settings.json
{
  "latino.binaryPath": "C:\\Program Files\\Latino\\latino.exe"
}
```

**Resultado:**

```bash
dotnet Latino.LanguageServer.dll --global-path "C:\Program Files\Latino"
```

#### **2. Detección de Servidor (Producción vs Desarrollo)**

```typescript
const prodServerPath = context.asAbsolutePath(
    path.join('server', 'Latino.LanguageServer.dll')
);
const devServerPath = context.asAbsolutePath(
    path.join('..', 'src', 'Latino.LanguageServer', 'bin', 'Debug', 'net10.0', 
              'Latino.LanguageServer.dll')
);

let serverPath = prodServerPath;
if (!fs.existsSync(serverPath)) {
    if (fs.existsSync(devServerPath)) {
        serverPath = devServerPath;
        outputChannel.appendLine("[Server] Usando modo DEV");
    } else {
        vscode.window.showErrorMessage(
            "Error: No se encuentra Latino.LanguageServer.dll");
        return;
    }
}
```

**Escenarios:**

| Caso | Ruta Usada | Cuándo |
| :--- | :--- | :--- |
| Extensión instalada | `extension/server/Latino.LanguageServer.dll` | Usuario normal |
| Desarrollo (F5) | `src/Latino.LanguageServer/bin/Debug/.../` | Desarrollador |

#### **3. Comando Run (Ejecución Robusta en PowerShell)**

```typescript
let runCommand = vscode.commands.registerCommand('latino.run', () => {
    const editor = vscode.window.activeTextEditor;
    if (editor) {
        const filePath = editor.document.fileName;
        const terminal = vscode.window.createTerminal(`Latino Run`);
        terminal.show();

        let executable = "latino";
        if (binaryPath && binaryPath.trim() !== "") {
            executable = binaryPath.replace(/['"]/g, '').trim();
        }

        if (process.platform === 'win32') {
            // FIX: Operador & para manejar rutas con espacios
            terminal.sendText(`& "${executable}" "${filePath}"`);
        } else {
            terminal.sendText(`"${executable}" "${filePath}"`);
        }
    }
});
```

**Problema en Windows:**

```powershell
# ❌ Falla si la ruta tiene espacios
"C:\Program Files\Latino\latino.exe" "script.lat"

# ✅ Correcto con operador &
& "C:\Program Files\Latino\latino.exe" "script.lat"
```

### Gramática TextMate (latino.tmLanguage.json)

Define el **colorizado estático** (no depende del LSP).

#### **Estructura de un Bloque:**

```json
{
  "name": "meta.block.conditional.latino",
  "begin": "\\b(si)\\b",
  "end": "\\b(fin)\\b",
  "beginCaptures": {
    "1": { "name": "keyword.control.conditional.if.latino" }
  },
  "endCaptures": {
    "0": { "name": "keyword.control.end.latino" }
  },
  "patterns": [
    { "include": "#blocks-internal" },
    { "include": "$self" }
  ]
}
```

**Visualización:**

```latino
si x > 10      ← "si" se colorea con keyword.control.conditional.if
    escribir("Mayor")
fin            ← "fin" se colorea con keyword.control.end
```

#### **Función Especial: Compositor (ç)**

```json
{
  "compositor": {
    "begin": "(ç)(?=\\s*\\()",
    "end": "\\)",
    "patterns": [
      {
        "begin": "\"",
        "end": "\"",
        "patterns": [
          { "include": "#compositor-interpolation" }
        ]
      }
    ]
  },
  "compositor-interpolation": {
    "begin": "\\|",
    "end": "\\|",
    "patterns": [
      { "name": "variable.other.readwrite.latino", 
        "match": "[a-zA-Z_][a-zA-Z0-9_]*" }
    ]
  }
}
```

**Uso:**

```latino
nombre = "Juan"
edad = 30
texto = ç("Hola |nombre|, tienes |edad| años")
```

**Colorización:**

- `ç` → `support.function.builtin`
- `"Hola..."` → `string.quoted.double`
- `|nombre|` → `variable.other.readwrite`

## 🔧 Debugging y Troubleshooting

### Panel de Salida (Output)

VS Code mantiene dos canales de log:

#### **1. Latino Client (TypeScript)**

```text
Ver > Salida > Seleccionar "Latino Client"
```

**Buscar:**

- `Servidor encontrado en...`
- `Argumentos inyectados: ["--global-path", "..."]`
- `Error al iniciar cliente`

**Ejemplo de Log Exitoso:**

```text
[10:30:15] Activando extensión Latino.NET (v1.0.81)...
[10:30:15] [Config] Leído binaryPath: 'C:\Latino\latino.exe'
[10:30:15] [Config] Directorio Global deducido: 'C:\Latino'
[10:30:15] [Config] Argumentos inyectados: ["--global-path","C:\\Latino"]
[10:30:15] [Server] Usando: extension/server/Latino.LanguageServer.dll
[10:30:16] [LSP] Cliente arrancado.
```

#### **2. Latino Language Server (C#)**

```text
Ver > Salida > Seleccionar "Latino Language Server"
```

**Buscar:**

- `[PluginManager] Cargando plugin: C:\...`
- `[LSP] Error Léxico encontrado:`
- `[LSP] Excepción Crítica:`

**Ejemplo de Carga de Plugin:**

```text
[10:30:17] [PluginManager] Cargando plugin: C:\Proyecto\lib\lapsos.dll
[10:30:17] [PluginManager] Función descubierta: lapsos.ahora
[10:30:17] [PluginManager] Función descubierta: lapsos.formatear
[10:30:17] [PluginManager] Plugin cargado exitosamente: lapsos.dll (2 funciones)
```

### Problemas Comunes

#### **Problema 1: "Autocompletado vacío"**

**Síntoma:** Al escribir `mate.`, no aparece nada.

**Diagnóstico:**

1. Abrir "Latino Language Server" en Output
2. Buscar errores de carga del Interpreter

**Causas:**

```text
❌ StdLibLoader.LoadAll() falló silenciosamente
❌ Latino.Core.dll está corrupta
❌ El servidor se crasheó al inicio
```

**Solución:**

```bash
# Recompilar servidor con logs verbosos
dotnet publish src/Latino.LanguageServer/Latino.LanguageServer.csproj -o extension/server

# Reiniciar VS Code
```

#### **Problema 2: "Plugin no se carga"**

**Síntoma:** Escribes `incluir("lapsos")` pero el autocompletado no muestra `lapsos.ahora`.

**Diagnóstico:**

```latino
# Añadir temporalmente
incluir("lapsos")
x = 1  # ← Guardar archivo (Ctrl+S) para forzar re-escaneo
```

**Verificar en Output:**

```text
[PluginManager] Cargando plugin: [RUTA] ← ¿Aparece?
[PluginManager] Error al cargar plugin: [MENSAJE] ← ¿Hay error?
```

**Causas Comunes:**

```text
❌ lapsos.dll no tiene el atributo [LatinoExtension]
❌ La DLL está en una ruta no buscada
❌ latino.binaryPath no está configurado (falta --global-path)
```

**Solución:**

```csharp
// Verificar que el plugin tenga:
[LatinoExtension("lapsos.ahora", Help = "Obtiene fecha actual")]
public static LatinoObject GetNow(List<LatinoObject> args) { ... }
```

#### **Problema 3: "Líneas rojas incorrectas"**

**Síntoma:** Código válido aparece con error.

**Diagnóstico:**

```bash
# Comparar versiones
latino --version        # ← Versión del intérprete
ls -lh extension/server/Latino.Core.dll  # ← Fecha de la DLL
```

**Causa:** Latino.Core.dll desactualizada en la extensión.

**Solución:**

```bash
dotnet publish src/Latino.LanguageServer/Latino.LanguageServer.csproj \
    -c Release -o extension/server
code --install-extension latino-net-vscode-1.0.XX.vsix
```

## 🔨 Casos de Uso Avanzados

### Caso 1: Añadir una Nueva Palabra Reservada

**Objetivo:** Añadir `asincrono` como keyword.

#### **Paso 1: Modificar Lexer**

```csharp
// src/Latino.Core/Analysis/Lexer.cs
public static Dictionary<string, TokenType> Keywords = new()
{
    { "si", TokenType.Keyword },
    { "mientras", TokenType.Keyword },
    { "asincrono", TokenType.Keyword },  // ← Registrar como TokenType.Keyword
    // ...
};
```

#### **Paso 2: Actualizar Gramática**

```json
// extension/syntaxes/latino.tmLanguage.json
{
  "keywords": {
    "patterns": [{
      "name": "keyword.control.latino",
      "match": "\\b(si|mientras|asincrono)\\b"
    }]
  }
}
```

#### **Paso 3: Recompilar y Empaquetar**

```bash
dotnet publish src/Latino.LanguageServer/Latino.LanguageServer.csproj -o extension/server
cd extension && npm run compile && npx vsce package
```

### Caso 2: Crear un Plugin Externo

> ⚠️ **Este tutorial usa nombres ficticios a propósito** (`Latino.MiTiempo`, `mitiempo.dll`).
> El repositorio ya incluye un plugin oficial llamado **`lapsos`** (`Latino.Plugin.Lapsos`, `lapsos.dll`), documentado en el capítulo `9.15`. No compiles este ejemplo con el nombre `lapsos`: sobrescribirías la librería oficial.

**Objetivo:** Crear `mitiempo.dll` con funciones de fecha/hora.

#### **Paso 1: Crear Proyecto**

```bash
dotnet new classlib -n Latino.MiTiempo
dotnet add reference ../Latino.Core/Latino.Core.csproj
```

#### **Paso 2: Implementar Funciones**

```csharp
using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.MiTiempo;

public static class MiTiempoLib
{
    [LatinoExtension("mitiempo.ahora", 
        Help = "Obtiene la fecha y hora actual",
        Args = "[formato]",
        Returns = "cadena")]
    public static LatinoObject GetNow(List<LatinoObject> args)
    {
        string formato = args.Count > 0 ? args[0].ToString() : "yyyy-MM-dd HH:mm:ss";
        return new LatinoString(DateTime.Now.ToString(formato));
    }

    [LatinoExtension("mitiempo.sumarDias",
        Help = "Suma días a una fecha",
        Args = "fecha, dias",
        Returns = "cadena")]
    public static LatinoObject AddDays(List<LatinoObject> args)
    {
        if (args.Count < 2) 
            throw new RuntimeError(null!, "mitiempo.sumarDias requiere fecha y dias.");

        var dt = DateTime.Parse(args[0].ToString());
        double dias = ((LatinoNumber)args[1]).Value;
        return new LatinoString(dt.AddDays(dias).ToString("yyyy-MM-dd"));
    }
}
```

#### **Paso 3: Compilar y Distribuir**

```bash
dotnet build -c Release
cp bin/Release/net10.0/MiTiempo.dll ~/MiProyecto/lib/mitiempo.dll
```

#### **Paso 4: Usar en Latino**

> ⚠️ **Este ejemplo requiere el paso anterior:** la DLL `mitiempo.dll` debe estar compilada y en la ruta de bísqueda, o `incluir` fallará con `No se encontró el módulo o librería 'mitiempo'`.

```latino
incluir("mitiempo")   # el nombre corresponde al DLL: mitiempo.dll

hoy = mitiempo.ahora("dd/MM/yyyy")
escribir(hoy)

manana = mitiempo.sumarDias(hoy, 1)
escribir(manana)
```

**Autocompletado Automático:**
Al escribir `mitiempo.`, VS Code sugerirá:

- `mitiempo.ahora(formato)`
- `mitiempo.sumarDias(fecha, dias)`

> El prefijo del autocompletado lo determina el atributo `[LatinoExtension("mitiempo.ahora")]` del C#, no el nombre del archivo de la DLL.

### Caso 3: Debugging del Servidor LSP

**Objetivo:** Depurar el servidor C# con breakpoints.

#### **Configuración en VS Code:**

```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Debug Language Server",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/Latino.LanguageServer/bin/Debug/net10.0/Latino.LanguageServer.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "console": "internalConsole",
      "stopAtEntry": false
    }
  ]
}
```

#### **Pasos:**

1. Colocar breakpoint en `CompletionHandler.cs`
2. Presionar `F5` (Debug Language Server)
3. Abrir **otra instancia** de VS Code
4. En la segunda instancia, abrir un archivo `.lat`
5. Escribir `mate.` → El breakpoint se activará en la primera instancia

## 📚 Recursos de Referencia

### Documentación Oficial

- **LSP Spec:** [https://microsoft.github.io/language-server-protocol/](https://microsoft.github.io/language-server-protocol/)
- **OmniSharp Extensions:** [https://github.com/OmniSharp/csharp-language-server-protocol](https://github.com/OmniSharp/csharp-language-server-protocol)
- **TextMate Grammars:** [https://macromates.com/manual/en/language_grammars](https://macromates.com/manual/en/language_grammars)

### Estructura de un Mensaje LSP

```json
// Request (stdin del servidor)
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "textDocument/completion",
  "params": {
    "textDocument": { "uri": "file:///test.lat" },
    "position": { "line": 5, "character": 10 }
  }
}

// Response (stdout del servidor)
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "items": [
      {
        "label": "mate.sen",
        "kind": 2,
        "insertText": "mate.sen(${1:angulo})"
      }
    ]
  }
}
```

📋 **Checklist de Desarrollo**

Antes de publicar una nueva versión:

- [ ] Compilar Core: `dotnet build src/Latino.Core`
- [ ] Compilar Servidor: `dotnet publish src/Latino.LanguageServer -o extension/server`
- [ ] Ejecutar tests: `dotnet test`
- [ ] Incrementar versión en `extension/package.json`
- [ ] Compilar cliente: `cd extension && npm run compile`
- [ ] Empaquetar: `npx vsce package`
- [ ] Probar instalación: `code --install-extension latino-net-vscode-X.X.XX.vsix`
- [ ] Verificar autocompletado en archivo `.lat` de prueba
- [ ] Verificar que el botón Run ejecuta correctamente
- [ ] Revisar logs en "Output > Latino Language Server"
- [ ] Publicar: `npx vsce publish`

**Filosofía Final:** El diseño "Cerebro Único" no es solo una técnica, es un compromiso con la mantenibilidad. Cada línea de código que escribes en `Latino.Core` beneficia a todos los frontends simultáneamente.
