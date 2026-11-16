using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using Latino.Core.Types;

namespace Latino.LanguageServer;

/// <summary>
/// Gestiona la carga dinámica de plugins (.dll) para el Language Server.
/// Escanea archivos .lat buscando incluir("nombre") y carga los metadatos.
/// </summary>
public class PluginManager
{
    private readonly ILogger<PluginManager> _logger;
    
    // Cache de plugins cargados: Ruta completa de DLL -> Metadatos
    private readonly ConcurrentDictionary<string, PluginMetadata> _loadedPlugins = new();
    
    // Cache de funciones globales y módulos descubiertos
    private readonly ConcurrentDictionary<string, FunctionMetadata> _globalFunctions = new();
    private readonly ConcurrentDictionary<string, ModuleMetadata> _modules = new();

    public string? GlobalPath { get; set; }

    public PluginManager(ILogger<PluginManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Escanea el código fuente buscando declaraciones incluir("archivo") 
    /// y carga los plugins encontrados.
    /// </summary>
    public void ScanAndLoadPlugins(DocumentUri documentUri, string sourceCode)
    {
        try
        {
            // FIX 1: Regex relajada (cualquier contenido en comillas)
            var pattern = @"incluir\s*\(\s*[""']([^""']+)[""']\s*\)";
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

    /// <summary>
    /// Resuelve la ruta completa de una DLL basándose en la ubicación del documento actual y rutas comunes.
    /// </summary>
    public string ResolveDllPath(DocumentUri documentUri, string rawName)
    {
        try
        {
            // Obtener el directorio del archivo .lat actual
            var filePath = documentUri.GetFileSystemPath();
            var scriptDir = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(scriptDir)) return "";

            // 1. Candidatos de nombre (con y sin .dll)
            var candidates = new List<string>();
            if (rawName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(rawName);
            }
            else
            {
                candidates.Add(rawName + ".dll");
                candidates.Add(rawName); // Por si acaso es un path relativo completo sin ext
            }

            // 2. Rutas de búsqueda (Inteligencia relativa)
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
                    try 
                    {
                        string fullPath = Path.GetFullPath(Path.Combine(basePath, candidate));
                        if (File.Exists(fullPath)) return fullPath;
                    }
                    catch { continue; } // Ignorar rutas inválidas
                }
            }

            return ""; // No encontrado
        }
        catch 
        {
            return "";
        }
    }

    /// <summary>
    /// Carga una DLL usando reflexión y extrae todos los metadatos de [LatinoExtension].
    /// </summary>
    private void LoadPlugin(string dllPath)
    {
        // Evitar recargar el mismo plugin
        if (_loadedPlugins.ContainsKey(dllPath))
        {
            return;
        }

        try
        {
            _logger.LogInformation($"[PluginManager] Cargando plugin: {dllPath}");
            
            var assembly = Assembly.LoadFrom(Path.GetFullPath(dllPath));
            var pluginMetadata = new PluginMetadata { Path = dllPath };

            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
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

                        _logger.LogDebug($"[PluginManager] Función descubierta: {funcMetadata.Name}");
                    }
                }
            }

            _loadedPlugins[dllPath] = pluginMetadata;
            _logger.LogInformation($"[PluginManager] Plugin cargado exitosamente: {dllPath} ({pluginMetadata.Functions.Count} funciones)");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PluginManager] Error al cargar plugin '{dllPath}': {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene todas las funciones globales descubiertas en los plugins.
    /// </summary>
    public IEnumerable<FunctionMetadata> GetGlobalFunctions() => _globalFunctions.Values;

    /// <summary>
    /// Obtiene todos los módulos (librerías) descubiertos en los plugins.
    /// </summary>
    public IEnumerable<ModuleMetadata> GetModules() => _modules.Values;

    /// <summary>
    /// Busca una función específica por nombre (soporta notación punto: "modulo.metodo").
    /// </summary>
    public FunctionMetadata? FindFunction(string fullName)
    {
        if (_globalFunctions.TryGetValue(fullName, out var func))
            return func;

        // Buscar en módulos
        if (fullName.Contains('.'))
        {
            var parts = fullName.Split(new[] { '.' }, 2);
            if (_modules.TryGetValue(parts[0], out var module))
            {
                if (module.Methods.TryGetValue(parts[1], out var method))
                    return method;
            }
        }

        return null;
    }

    /// <summary>
    /// Limpia todos los plugins cargados (útil para reiniciar el servidor).
    /// </summary>
    public void Clear()
    {
        _loadedPlugins.Clear();
        _globalFunctions.Clear();
        _modules.Clear();
        _logger.LogInformation("[PluginManager] Cache de plugins limpiada.");
    }
}

// --- CLASES DE METADATOS ---

public class PluginMetadata
{
    public string Path { get; set; } = string.Empty;
    public List<FunctionMetadata> Functions { get; set; } = new();
}

public class FunctionMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Help { get; set; } = string.Empty;
    public string Args { get; set; } = string.Empty;
    public string Returns { get; set; } = string.Empty;
}

public class ModuleMetadata
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, FunctionMetadata> Methods { get; set; } = new();
}
