using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Necesario para Select
using System.Text.Json;
using System.Text;
using Latino.Core.Runtime;
using Latino.Core.Types;
using Latino.Core.Analysis;

namespace Latino.Core.StdLib;

public static class FileLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        void Def(string name, Func<Interpreter, List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc, pams);
            methods[name] = func;
        }

        // --- LECTURA/ESCRITURA BÁSICA ---
        Def("leer", (i, args) => {
            return ExecuteSafe(() => {
                string ruta = args[0].ToString();
                using var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, detectEncodingFromByteOrderMarks: true);
                return new LatinoString(reader.ReadToEnd());
            });
        }, 1, "Lee el contenido completo de un archivo de texto.", "ruta");

        Def("escribir", (i, args) => {
            ExecuteSafe(() => File.WriteAllText(args[0].ToString(), args[1].ToString()));
            return LatinoNull.Instance;
        }, 2, "Escribe texto en un archivo (sobrescribe si existe).", "ruta, contenido");

        Def("anexar", (i, args) => {
            ExecuteSafe(() => File.AppendAllText(args[0].ToString(), args[1].ToString()));
            return LatinoNull.Instance;
        }, 2, "Añade texto al final del archivo.", "ruta, contenido");

        // --- GESTIÓN DE ARCHIVOS ---
        Def("existe", (i, args) => {
            string path = args[0].ToString();
            return new LatinoBoolean(File.Exists(path) || Directory.Exists(path));
        }, 1, "Verifica si existe un archivo o directorio.", "ruta");

        Def("borrar", (i, args) => {
            ExecuteSafe(() => {
                string path = args[0].ToString();
                if (Directory.Exists(path))
                    Directory.Delete(path, true); // Recursivo
                else
                    File.Delete(path);
            });
            return LatinoNull.Instance;
        }, 1, "Elimina un archivo o directorio (incluyendo contenido).", "ruta");
        
        Def("eliminar", (i, args) => methods["borrar"].Call(i, args), 1, "Alias de borrar.", "ruta");

        // --- AVANZADO ARCHIVOS ---
        Def("lineas", (i, args) => {
            return ExecuteSafe(() => {
                string ruta = args[0].ToString();
                using var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, detectEncodingFromByteOrderMarks: true);
                
                var list = new List<LatinoObject>();
                string? line;
                while ((line = reader.ReadLine()) != null) 
                {
                    list.Add(new LatinoString(line));
                }
                return new LatinoList(list);
            });
        }, 1, "Lee un archivo y retorna lista de líneas.", "ruta");

        Def("duplicar", (i, args) => {
            ExecuteSafe(() => File.Copy(args[0].ToString(), args[1].ToString(), true));
            return LatinoNull.Instance;
        }, 2, "Copia un archivo (sobrescribe si existe).", "origen, destino");

        Def("renombrar", (i, args) => {
            ExecuteSafe(() => {
                string origen = args[0].ToString();
                string destino = args[1].ToString();
                
                // Validación: no se puede renombrar a algo que ya existe
                if (File.Exists(destino) || Directory.Exists(destino))
                    throw new IOException("El destino ya existe.");
                
                if (File.Exists(origen))
                    File.Move(origen, destino);
                else if (Directory.Exists(origen))
                    Directory.Move(origen, destino);
                else
                    throw new FileNotFoundException("El archivo o directorio origen no existe.");
            });
            return LatinoNull.Instance;
        }, 2, "Mueve o renombra archivo/directorio.", "origen, destino");

        // --- GESTIÓN DE DIRECTORIOS ---
        Def("crear", (i, args) => {
            ExecuteSafe(() => {
                string path = args[0].ToString();
                
                // Validación: no sobrescribir archivos existentes
                if (File.Exists(path))
                    throw new IOException($"Ya existe un archivo con ese nombre: '{path}'");
                
                File.Create(path).Close();
            });
            return LatinoNull.Instance;
        }, 1, "Crea un archivo vacío (falla si ya existe).", "ruta");

        Def("crearCarpeta", (i, args) => {
            ExecuteSafe(() => Directory.CreateDirectory(args[0].ToString()));
            return LatinoNull.Instance;
        }, 1, "Crea un directorio (y directorios padres si faltan).", "ruta");

        Def("listar", (i, args) => {
            return ExecuteSafe(() => {
                var entries = Directory.GetFileSystemEntries(args[0].ToString());
                var list = new List<LatinoObject>();
                foreach (var e in entries)
                    list.Add(new LatinoString(Path.GetFileName(e)));
                return new LatinoList(list);
            });
        }, 1, "Lista archivos y directorios en una ruta.", "ruta");

        Def("esCarpeta", (i, args) => {
            try {
                var attr = File.GetAttributes(args[0].ToString());
                return new LatinoBoolean(attr.HasFlag(FileAttributes.Directory));
            } catch {
                return new LatinoBoolean(false);
            }
        }, 1, "Verifica si la ruta es un directorio.", "ruta");

        // --- MANIPULACIÓN DE RUTAS ---
        Def("extension", (i, args) => {
            return new LatinoString(Path.GetExtension(args[0].ToString()));
        }, 1, "Obtiene la extensión del archivo (ej: '.txt').", "ruta");
        
        Def("nombre", (i, args) => {
            return new LatinoString(Path.GetFileName(args[0].ToString()));
        }, 1, "Obtiene el nombre del archivo o directorio.", "ruta");

        // --- INFORMACIÓN DE ARCHIVOS (NUEVO) ---
        Def("tamaño", (i, args) => {
            return ExecuteSafe(() => {
                string path = args[0].ToString();
                
                if (Directory.Exists(path))
                    throw new RuntimeError(null!, "No se puede obtener tamaño de un directorio. Usa archivo.tamaño() solo con archivos.");
                
                var fileInfo = new FileInfo(path);
                return new LatinoNumber(fileInfo.Length);
            });
        }, 1, "Obtiene el tamaño del archivo en bytes.", "ruta");
        
        Def("fechaModificacion", (i, args) => {
            return ExecuteSafe(() => {
                string path = args[0].ToString();
                DateTime fecha;
                
                if (File.Exists(path))
                    fecha = File.GetLastWriteTime(path);
                else if (Directory.Exists(path))
                    fecha = Directory.GetLastWriteTime(path);
                else
                    throw new FileNotFoundException();
                
                return new LatinoString(fecha.ToString("yyyy-MM-dd HH:mm:ss"));
            });
        }, 1, "Obtiene la fecha de última modificación.", "ruta");
        
        Def("esArchivo", (i, args) => {
            try {
                string path = args[0].ToString();
                return new LatinoBoolean(File.Exists(path) && !Directory.Exists(path));
            } catch {
                return new LatinoBoolean(false);
            }
        }, 1, "Verifica si la ruta es un archivo (no directorio).", "ruta");

        Def("rutaAbsoluta", (i, args) => {
            return ExecuteSafe(() => new LatinoString(Path.GetFullPath(args[0].ToString())));
        }, 1, "Convierte ruta relativa a absoluta.", "ruta");

        // =====================================================================
        // SOPORTE PARA FORMATO CSV (Datos Estructurados)
        // =====================================================================

        Def("leerCsv", (interp, args) => {
            string ruta = args[0].ToString();
            string sep = args.Count >= 2 ? args[1].ToString() : ",";

            return ExecuteSafe(() => {
                if (!File.Exists(ruta)) throw new FileNotFoundException();

                var lineas = File.ReadAllLines(ruta);
                if (lineas.Length < 1) return new LatinoList(new List<LatinoObject>());

                // 1. Obtener encabezados
                var headers = lineas[0].Split(new[] { sep }, StringSplitOptions.None);
                var resultado = new List<LatinoObject>();

                // 2. Procesar filas
                for (int i = 1; i < lineas.Length; i++) {
                    if (string.IsNullOrWhiteSpace(lineas[i])) continue;
                    var campos = lineas[i].Split(new[] { sep }, StringSplitOptions.None);
                    var rowDict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
                    
                    for (int j = 0; j < headers.Length; j++) {
                        string key = headers[j].Trim();
                        string val = j < campos.Length ? campos[j].Trim() : "";
                        
                        // Intento de conversión numérica automática
                        LatinoObject objVal;
                        if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                            objVal = new LatinoNumber(d);
                        else
                            objVal = new LatinoString(val);

                        rowDict[new LatinoString(key)] = objVal;
                    }
                    resultado.Add(new LatinoDictionary(rowDict));
                }
                return new LatinoList(resultado);
            });
        }, -1, "Lee un archivo CSV y retorna una lista de diccionarios.", "ruta, [separador]");

        Def("escribirCsv", (interp, args) => {
            if (args.Count < 2) throw new RuntimeError(null!, "archivo.escribirCsv() requiere ruta y lista.");
            string ruta = args[0].ToString();
            if (args[1] is not LatinoList list) throw new RuntimeError(null!, "Se esperaba una lista de diccionarios.");
            string sep = args.Count >= 3 ? args[2].ToString() : ",";

            if (list.Elements.Count == 0) return LatinoNull.Instance;

            return ExecuteSafe(() => {
                // 1. Extraer encabezados del primer diccionario
                if (list.Elements[0] is not LatinoDictionary firstDict) 
                    throw new RuntimeError(null!, "La lista debe contener diccionarios.");

                var headers = firstDict.Pairs.Keys.Select(k => k.ToString()).ToList();
                var sb = new StringBuilder();

                // 2. Escribir encabezados
                sb.AppendLine(string.Join(sep, headers));

                // 3. Escribir filas
                foreach (var item in list.Elements) {
                    if (item is LatinoDictionary dict) {
                        var rowValues = new List<string>();
                        foreach (var h in headers) {
                            var key = new LatinoString(h);
                            string val = dict.Pairs.TryGetValue(key, out var v) ? v.ToString() : "";
                            rowValues.Add(val);
                        }
                        sb.AppendLine(string.Join(sep, rowValues));
                    }
                }

                File.WriteAllText(ruta, sb.ToString());
                return LatinoNull.Instance;
            });
        }, -1, "Escribe una lista de diccionarios en formato CSV.", "ruta, lista_dicts, [separador]");

        // --- EJECUTAR (CON PROTECCIÓN) ---
        Def("ejecutar", (i, args) => {
            string path = args[0].ToString();
            
            return ExecuteSafe(() => {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Script no encontrado: '{path}'");
                
                // Protección: Verificar extensión
                if (!path.EndsWith(".lat", StringComparison.OrdinalIgnoreCase))
                    throw new RuntimeError(null!, "Solo se pueden ejecutar archivos .lat");
                
                var isolatedInterp = new Interpreter();
                StdLibLoader.LoadAll(isolatedInterp);
                isolatedInterp.Out = i.Out;
                isolatedInterp.In = i.In;
                
                // Heredar límite de recursión del intérprete padre
                isolatedInterp.MaxDepth = i.MaxDepth;

                string source = File.ReadAllText(path);
                var parser = new Parser(new Lexer(source).Tokenize());
                isolatedInterp.Interpret(parser.Parse());
                
                return LatinoNull.Instance;
            });
        }, 1, "Ejecuta un script .lat en un entorno aislado.", "ruta");

        // --- BÚSQUEDA Y JSON (NUEVO) ---
        Def("buscar", (i, args) => {
            string directorio = args[0].ToString();
            string patron = args[1].ToString();
            
            if (!Directory.Exists(directorio))
                throw new RuntimeError(null!, $"El directorio no existe: '{directorio}'");
            
            if (string.IsNullOrWhiteSpace(patron))
                throw new RuntimeError(null!, "El patrón de búsqueda no puede estar vacío.");
            
            try
            {
                var archivos = Directory.GetFiles(directorio, patron, SearchOption.AllDirectories);
                var lista = new List<LatinoObject>();
                foreach (var archivo in archivos)
                {
                    lista.Add(new LatinoString(Path.GetFullPath(archivo)));
                }
                return new LatinoList(lista);
            }
            catch (UnauthorizedAccessException)
            {
                throw new RuntimeError(null!, $"Acceso denegado al buscar en: '{directorio}'.");
            }
            catch (Exception ex)
            {
                throw new RuntimeError(null!, $"Error al buscar archivos: {ex.Message}");
            }
        }, 2, "Busca archivos recursivamente usando patrón glob (*.lat, test_*.txt, etc).", "directorio, patron");

        Def("leerJson", (i, args) => {
            string ruta = args[0].ToString();
            return ExecuteSafe(() => {
                using var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, detectEncodingFromByteOrderMarks: true);
                string jsonText = reader.ReadToEnd();
                
                try
                {
                    using var doc = JsonDocument.Parse(jsonText);
                    return ConvertirJsonALatino(doc.RootElement);
                }
                catch (JsonException ex)
                {
                    throw new RuntimeError(null!, $"Error al parsear JSON: {ex.Message}");
                }
            });
        }, 1, "Lee un archivo JSON y retorna diccionario o lista.", "ruta");

        Def("escribirJson", (i, args) => {
            if (args.Count < 2)
                throw new RuntimeError(null!, 
                    "archivo.escribirJson() requiere al menos 2 argumentos: ruta y datos.");
            
            string ruta = args[0].ToString();
            var datos = args[1];
            bool identado = args.Count >= 3 && args[2] is LatinoBoolean b ? b.Value : true;
            
            if (datos is not LatinoDictionary && datos is not LatinoList)
                throw new RuntimeError(null!, "archivo.escribirJson() requiere un diccionario o lista.");
            
            return ExecuteSafe(() => {
                var options = new JsonWriterOptions { 
                    Indented = identado,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, options);
                EscribirObjetoLatino(writer, datos);
                writer.Flush();
                
                string jsonText = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(ruta, jsonText);
                return LatinoNull.Instance;
            });
        }, -1, "Escribe un diccionario o lista como JSON.", "ruta, datos, [identado]");

        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("archivo", modulo);
    }

    /// <summary>
    /// Helper para ejecutar operaciones de archivo con errores localizados en español.
    /// </summary>
    private static void ExecuteSafe(Action action)
    {
        try
        {
            action();
        }
        catch (FileNotFoundException ex)
        {
            string filename = Path.GetFileName(ex.FileName ?? "desconocido");
            throw new RuntimeError(null!, $"Archivo no encontrado: '{filename}'");
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new RuntimeError(null!, $"Directorio no encontrado: '{ex.Message}'");
        }
        catch (UnauthorizedAccessException)
        {
            throw new RuntimeError(null!, "Acceso denegado. Verifica que tienes permisos para esta operación.");
        }
        catch (PathTooLongException)
        {
            throw new RuntimeError(null!, "La ruta del archivo es demasiado larga (máximo 260 caracteres en Windows).");
        }
        catch (NotSupportedException ex)
        {
            throw new RuntimeError(null!, $"Nombre de archivo inválido (caracteres prohibidos o nombres reservados como CON, PRN): {ex.Message}");
        }
        catch (IOException ex) when (ex.Message.Contains("being used"))
        {
            throw new RuntimeError(null!, "El archivo está siendo usado por otro programa. Ciérralo e intenta nuevamente.");
        }
        catch (IOException ex) when (ex.Message.Contains("already exists"))
        {
            throw new RuntimeError(null!, "El archivo o directorio ya existe.");
        }
        catch (IOException ex) when (ex.Message.Contains("not enough space") || ex.Message.Contains("disk full"))
        {
            throw new RuntimeError(null!, "No hay espacio suficiente en el disco.");
        }
        catch (IOException ex)
        {
            throw new RuntimeError(null!, $"Error de entrada/salida: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null!, $"Error inesperado del sistema: {ex.Message}");
        }
    }

    /// <summary>
    /// Sobrecarga para operaciones que retornan valores.
    /// </summary>
    private static T ExecuteSafe<T>(Func<T> func)
    {
        T result = default!;
        ExecuteSafe(() => { result = func(); });
        return result!;
    }

    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }

    internal static LatinoObject ConvertirJsonALatino(JsonElement elemento)
    {
        switch (elemento.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
                foreach (var prop in elemento.EnumerateObject())
                    dict[new LatinoString(prop.Name)] = ConvertirJsonALatino(prop.Value);
                return new LatinoDictionary(dict);
            
            case JsonValueKind.Array:
                var lista = new List<LatinoObject>();
                foreach (var item in elemento.EnumerateArray())
                    lista.Add(ConvertirJsonALatino(item));
                return new LatinoList(lista);
            
            case JsonValueKind.String:
                return new LatinoString(elemento.GetString() ?? "");
            
            case JsonValueKind.Number:
                return new LatinoNumber(elemento.GetDouble());
            
            case JsonValueKind.True: return new LatinoBoolean(true);
            case JsonValueKind.False: return new LatinoBoolean(false);
            case JsonValueKind.Null: return LatinoNull.Instance;
            default: return LatinoNull.Instance;
        }
    }

    private static void EscribirObjetoLatino(Utf8JsonWriter writer, LatinoObject obj)
    {
        switch (obj)
        {
            case LatinoDictionary dict:
                writer.WriteStartObject();
                foreach (var kvp in dict.Pairs)
                {
                    writer.WritePropertyName(kvp.Key.ToString());
                    EscribirObjetoLatino(writer, kvp.Value);
                }
                writer.WriteEndObject();
                break;
            
            case LatinoList lista:
                writer.WriteStartArray();
                foreach (var elem in lista.Elements)
                    EscribirObjetoLatino(writer, elem);
                writer.WriteEndArray();
                break;
            
            case LatinoNumber num: writer.WriteNumberValue(num.Value); break;
            case LatinoString str: writer.WriteStringValue(str.Value); break;
            case LatinoBoolean b: writer.WriteBooleanValue(b.Value); break;
            case LatinoNull: writer.WriteNullValue(); break;
            default: writer.WriteStringValue(obj.ToString()); break;
        }
    }
}
