using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class PluginLoader
{
    public static void Load(Interpreter interpreter, string path)
    {
        if (!File.Exists(path)) throw new RuntimeError(null!, $"No se encuentra el plugin: {path}");

        try
        {
            var assembly = Assembly.LoadFrom(Path.GetFullPath(path));
            
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = method.GetCustomAttribute<LatinoExtensionAttribute>();
                    if (attr != null)
                    {
                        // Verificamos la firma del método para seguridad
                        // Debe ser: static LatinoObject Nombre(Interpreter, List<LatinoObject>)
                        // O simplificado: static LatinoObject Nombre(List<LatinoObject>)
                        
                        NativeFunction? nativeFunc = null;
                        var parameters = method.GetParameters();

                        if (parameters.Length == 2 && 
                            parameters[0].ParameterType == typeof(Interpreter) &&
                            parameters[1].ParameterType == typeof(List<LatinoObject>))
                        {
                            // Firma Completa
                            nativeFunc = new NativeFunction((i, args) => {
                                try {
                                    return (LatinoObject)method.Invoke(null, new object[] { i, args })!;
                                } catch (TargetInvocationException ex) {
                                    if (ex.InnerException is RuntimeError re) throw re;
                                    // Traducimos el error clásico de índice a un mensaje amigable
                                    if (ex.InnerException is ArgumentOutOfRangeException) 
                                        throw new RuntimeError(null!, $"Faltan argumentos requeridos para ejecutar '{attr.FunctionName}'.");
                                    
                                    throw new RuntimeError(null!, $"Error en plugin '{attr.FunctionName}': {ex.InnerException?.Message ?? ex.Message}");
                                }
                            });
                        }
                        else if (parameters.Length == 1 && 
                                 parameters[0].ParameterType == typeof(List<LatinoObject>))
                        {
                            // Firma Simple (solo args)
                            nativeFunc = new NativeFunction((i, args) => {
                                try {
                                    return (LatinoObject)method.Invoke(null, new object[] { args })!;
                                } catch (TargetInvocationException ex) {
                                    if (ex.InnerException is RuntimeError re) throw re;
                                    // Traducimos el error clásico de índice a un mensaje amigable
                                    if (ex.InnerException is ArgumentOutOfRangeException) 
                                        throw new RuntimeError(null!, $"Faltan argumentos requeridos para ejecutar '{attr.FunctionName}'.");
                                    
                                    throw new RuntimeError(null!, $"Error en plugin '{attr.FunctionName}': {ex.InnerException?.Message ?? ex.Message}");
                                }
                            });
                        }

                        if (nativeFunc != null)
                        {
                            string name = attr.FunctionName;

                            if (name.Contains('.'))
                            {
                                // Lógica de Namespace (ej: "lapsos.ahora")
                                var parts = name.Split(new[] { '.' }, 2);
                                var moduleName = parts[0];
                                var methodName = parts[1];

                                LatinoDictionary moduleDict;

                                // 1. Verificar si el módulo ya existe
                                var existing = interpreter.Globals.GetAt(moduleName);
                                
                                if (existing is LatinoDictionary dic)
                                {
                                    moduleDict = dic;
                                }
                                else if (existing == null)
                                {
                                    // 2. Si no existe, crearlo
                                    moduleDict = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
                                    interpreter.Globals.Define(moduleName, moduleDict);
                                }
                                else
                                {
                                    // Conflicto: existe pero no es un diccionario
                                    throw new RuntimeError(null!, $"Conflicto de nombres: '{moduleName}' ya existe y no es un módulo.");
                                }

                                // 3. Agregar la función al módulo
                                moduleDict.Pairs[new LatinoString(methodName)] = nativeFunc;
                            }
                            else
                            {
                                // Lógica Global (Comportamiento clásico)
                                interpreter.Globals.Define(name, nativeFunc);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null!, $"Error al cargar plugin '{path}': {ex.Message}");
        }
    }
}
