using System;
using System.Collections.Generic;
using System.IO;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Plugin.Env;

public static class EnvPlugin
{
    // =========================================================================
    // env.leer(nombre, [defecto])
    // =========================================================================

    [LatinoExtension("env.leer",
        Args = "nombre, [defecto]",
        Help = "Lee una variable de entorno del proceso actual. " +
               "Retorna el valor por defecto (o nulo) si la variable no existe.")]
    public static LatinoObject Leer(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "env.leer requiere el nombre de la variable.");

        if (args[0] is LatinoNull)
            throw new RuntimeError(null!, "env.leer: El nombre de la variable no puede ser nulo.");

        string nombre = args[0].ToString();

        if (string.IsNullOrWhiteSpace(nombre))
            throw new RuntimeError(null!, "env.leer: El nombre de la variable no puede estar vacío.");

        try
        {
            string? valor = Environment.GetEnvironmentVariable(nombre);

            if (valor != null)
                return new LatinoString(valor);

            // Si hay valor por defecto, retornarlo; sino retornar nulo
            return args.Count > 1 ? args[1] : LatinoNull.Instance;
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"env.leer: {ex.Message}");
        }
    }

    // =========================================================================
    // env.existe(nombre)
    // =========================================================================

    [LatinoExtension("env.existe",
        Args = "nombre",
        Help = "Retorna verdadero si la variable de entorno existe en el proceso actual, " +
               "falso en caso contrario.")]
    public static LatinoObject Existe(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "env.existe requiere el nombre de la variable.");

        if (args[0] is LatinoNull)
            throw new RuntimeError(null!, "env.existe: El nombre de la variable no puede ser nulo.");

        string nombre = args[0].ToString();

        if (string.IsNullOrWhiteSpace(nombre))
            throw new RuntimeError(null!, "env.existe: El nombre de la variable no puede estar vacío.");

        try
        {
            return new LatinoBoolean(Environment.GetEnvironmentVariable(nombre) != null);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"env.existe: {ex.Message}");
        }
    }

    // =========================================================================
    // env.escribir(nombre, valor)
    // =========================================================================

    [LatinoExtension("env.escribir",
        Args = "nombre, valor",
        Help = "Define una variable de entorno para el proceso actual. Si el valor es 'nulo', la variable se elimina.")]
    public static LatinoObject Escribir(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "env.escribir requiere nombre y valor.");

        if (args[0] is LatinoNull)
            throw new RuntimeError(null!, "env.escribir: El nombre de la variable no puede ser nulo.");

        string nombre = args[0].ToString();

        if (string.IsNullOrWhiteSpace(nombre))
            throw new RuntimeError(null!, "env.escribir: El nombre de la variable no puede estar vacío.");

        if (nombre.Contains('='))
            throw new RuntimeError(null!, "env.escribir: El nombre de la variable no puede contener el carácter '='.");

        try
        {
            // Si el usuario envía 'nulo', pasamos 'null' a .NET para ELIMINAR la variable (Multiplataforma)
            string? valor = args[1] is LatinoNull ? null : args[1].ToString();
            
            Environment.SetEnvironmentVariable(nombre, valor);
            return new LatinoBoolean(true);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"env.escribir: {ex.Message}");
        }
    }

    // =========================================================================
    // env.todas()
    // =========================================================================

    [LatinoExtension("env.todas",
        Help = "Retorna un diccionario con todas las variables de entorno del proceso actual.")]
    public static LatinoObject Todas(List<LatinoObject> args)
    {
        try
        {
            var pares = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);

            foreach (System.Collections.DictionaryEntry entrada in Environment.GetEnvironmentVariables())
            {
                string clave = entrada.Key?.ToString() ?? "";
                string valor = entrada.Value?.ToString() ?? "";
                pares[new LatinoString(clave)] = new LatinoString(valor);
            }

            return new LatinoDictionary(pares);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"env.todas: {ex.Message}");
        }
    }

    // =========================================================================
    // env.leerArchivo(ruta, [prefijo])
    // =========================================================================

    [LatinoExtension("env.leerArchivo",
        Args = "ruta, [prefijo]",
        Help = "Parsea un archivo .env y carga sus variables en el proceso actual. " +
               "Si se especifica un prefijo, todas las variables se cargan con ese prefijo antepuesto. " +
               "Retorna un diccionario con las variables cargadas. " +
               "Soporta: comentarios (#), valores con comillas, '=' en valores, export de bash.")]
    public static LatinoObject LeerArchivo(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "env.leerArchivo requiere la ruta del archivo .env.");

        string ruta    = args[0].ToString();
        string prefijo = args.Count > 1 ? args[1].ToString() : "";

        if (!File.Exists(ruta))
            throw new RuntimeError(null!, $"env.leerArchivo: El archivo no existe: '{ruta}'");

        try
        {
            var cargadas = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            string[] lineas = File.ReadAllLines(ruta);

            foreach (string lineaOriginal in lineas)
            {
                string linea = lineaOriginal.Trim();

                // Ignorar líneas vacías y comentarios de línea completa
                if (string.IsNullOrEmpty(linea) || linea.StartsWith('#'))
                    continue;

                // Compatibilidad con bash: quitar 'export ' al inicio
                if (linea.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                    linea = linea.Substring(7).TrimStart();

                // Dividir SOLO en el primer '=' para preservar '=' en valores (URLs, etc.)
                int posIgual = linea.IndexOf('=');
                if (posIgual <= 0)
                    continue; // Línea sin '=' o clave vacía — ignorar

                string clave = linea.Substring(0, posIgual).Trim();
                string valor = linea.Substring(posIgual + 1).Trim();

                // Ignorar claves vacías o que contengan espacios (formato inválido)
                if (string.IsNullOrEmpty(clave) || clave.Contains(' '))
                    continue;

                // Eliminar comentario inline: buscar ' #' fuera de comillas
                valor = EliminarComentarioInline(valor);

                // Quitar comillas externas si las hay (simples o dobles)
                valor = DescomillarValor(valor);

                // Cargar en el proceso con el prefijo indicado
                string nombreFinal = prefijo + clave;
                Environment.SetEnvironmentVariable(nombreFinal, valor);

                cargadas[new LatinoString(nombreFinal)] = new LatinoString(valor);
            }

            return new LatinoDictionary(cargadas);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"env.leerArchivo: Error leyendo '{ruta}': {ex.Message}");
        }
    }

    // =========================================================================
    // Helpers privados
    // =========================================================================

    /// <summary>
    /// Elimina comentarios inline del tipo "valor  # comentario".
    /// Respeta comentarios dentro de cadenas entre comillas.
    /// </summary>
    private static string EliminarComentarioInline(string valor)
    {
        // Si el valor está entre comillas, no tocar nada
        if ((valor.StartsWith('"') && valor.EndsWith('"')) ||
            (valor.StartsWith('\'') && valor.EndsWith('\'')))
            return valor;

        // Buscar ' #' fuera de comillas (comentario inline con espacio previo)
        int posHash = valor.IndexOf(" #", StringComparison.Ordinal);
        if (posHash >= 0)
            return valor.Substring(0, posHash).TrimEnd();

        return valor;
    }

    /// <summary>
    /// Quita las comillas externas de un valor si las tiene.
    /// "valor con espacios" → valor con espacios
    /// 'valor con espacios' → valor con espacios
    /// valor_sin_comillas   → valor_sin_comillas (sin cambios)
    /// </summary>
    private static string DescomillarValor(string valor)
    {
        if (valor.Length >= 2)
        {
            if (valor.StartsWith('"') && valor.EndsWith('"'))
                return valor.Substring(1, valor.Length - 2);

            if (valor.StartsWith('\'') && valor.EndsWith('\''))
                return valor.Substring(1, valor.Length - 2);
        }
        return valor;
    }
}
