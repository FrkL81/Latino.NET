using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class WebLib
{
    // =====================================================================
    // CLIENTE HTTP SINGLETON (Reutilizable para mejor rendimiento)
    // =====================================================================
    
    private static readonly HttpClient HttpClientInstance = new HttpClient(
        new HttpClientHandler
        {
            // Seguir redirects automáticamente
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            
            // Comprimir respuestas automáticamente (Brotli no siempre disponible en todos los .NET runtimes, GZip/Deflate sí)
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | 
                                     System.Net.DecompressionMethods.Deflate
        })
    {
        Timeout = TimeSpan.FromSeconds(30) // Timeout por defecto
    };
    
    static WebLib()
    {
        // Configurar User-Agent por defecto
        HttpClientInstance.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Latino.NET/2.0 (HttpClient)"
        );
    }
    
    // =====================================================================
    // REGISTRO DE FUNCIONES
    // =====================================================================
    
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        
        void Def(string name, Func<Interpreter, List<LatinoObject>, LatinoObject> logic, 
                 int arity, string? desc = null, string? pams = null) 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc!, pams ?? "");
            methods[name] = func;
        }
        
        // =================================================================
        // FUNCIONES SIMPLES (Accesos directos)
        // =================================================================
        
        Def("obtener", (i, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "web.obtener() requiere la URL como primer argumento.");
            
            string url = args[0].ToString();
            
            // Segundo argumento opcional: headers
            Dictionary<string, string>? headers = null;
            if (args.Count >= 2 && args[1] is LatinoDictionary headersDict)
            {
                headers = ConvertirHeadersDesdeLatino(headersDict);
            }
            
            return RealizarPeticion("GET", url, null, headers, 30);
        }, -1, "Realiza una petición GET. Opcionalmente acepta diccionario de encabezados.", "url, [encabezados]");
        
        Def("enviar", (i, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "web.enviar() requiere la URL como primer argumento.");
            
            string url = args[0].ToString();
            var datos = args.Count >= 2 ? args[1] : null;
            
            // Tercer argumento opcional: headers
            Dictionary<string, string>? headers = null;
            if (args.Count >= 3 && args[2] is LatinoDictionary headersDict)
            {
                headers = ConvertirHeadersDesdeLatino(headersDict);
            }
            
            return RealizarPeticion("POST", url, datos, headers, 30);
        }, -1, "Realiza una petición POST. Acepta datos (diccionario o string) y encabezados opcionales.", "url, [datos], [encabezados]");
        
        // =================================================================
        // FUNCIÓN COMPLETA (Configuración avanzada)
        // =================================================================
        
        Def("solicitar", (i, args) => {
            if (args[0] is not LatinoDictionary config)
                throw new RuntimeError(null!, 
                    "web.solicitar() requiere un diccionario de configuración.");
            
            // Extraer parámetros
            string url = ObtenerString(config, "url") ?? "";
            if (string.IsNullOrWhiteSpace(url))
                throw new RuntimeError(null!, 
                    "web.solicitar() requiere la clave 'url' en la configuración.");
            
            string metodo = (ObtenerString(config, "metodo", "GET") ?? "GET").ToUpper();
            var datos = ObtenerValor(config, "datos");
            var headersDict = ObtenerDiccionario(config, "encabezados");
            int timeout = ObtenerEntero(config, "timeout", 30);
            
            // Validar timeout
            if (timeout <= 0)
                throw new RuntimeError(null!, 
                    "El timeout debe ser un número positivo (en segundos).");
            
            var headers = headersDict != null 
                ? ConvertirHeadersDesdeLatino(headersDict) 
                : null;
            
            return RealizarPeticion(metodo, url, datos, headers, timeout);
        }, 1, "Realiza una petición HTTP personalizada. Config: {url, metodo, datos, encabezados, timeout}.", "configuracion");
        
        // =================================================================
        // DESCARGA DE ARCHIVOS
        // =================================================================
        
        Def("descargar", (i, args) => {
            string url = args[0].ToString();
            string rutaDestino = args[1].ToString();
            
            // Validar URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                throw new RuntimeError(null!, 
                    $"URL inválida: '{url}'");
            
            try
            {
                // Realizar petición GET
                var tarea = HttpClientInstance.GetAsync(uri);
                tarea.Wait();
                var respuesta = tarea.Result;

                // Validar que la respuesta sea exitosa (200-299) ANTES de escribir en disco.
                // Sin esta validación, se escribiría el HTML de error del servidor como archivo.
                if (!respuesta.IsSuccessStatusCode)
                    throw new RuntimeError(null!,
                        $"Fallo en descarga. El servidor respondió: {(int)respuesta.StatusCode} {respuesta.ReasonPhrase}");

                // Leer contenido en memoria
                var tareaContenido = respuesta.Content.ReadAsByteArrayAsync();
                tareaContenido.Wait();
                byte[] contenido = tareaContenido.Result;

                // Escribir archivo — usar try/finally para limpiar si falla a mitad
                bool escrituraExitosa = false;
                try
                {
                    File.WriteAllBytes(rutaDestino, contenido);
                    escrituraExitosa = true;
                }
                finally
                {
                    // Si la escritura falló y el archivo quedó parcialmente creado, eliminarlo
                    if (!escrituraExitosa && File.Exists(rutaDestino))
                        File.Delete(rutaDestino);
                }

                // Retornar información de descarga exitosa
                var resultado = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
                {
                    { new LatinoString("bytes"),  new LatinoNumber(contenido.Length) },
                    { new LatinoString("ruta"),   new LatinoString(Path.GetFullPath(rutaDestino)) },
                    { new LatinoString("nombre"), new LatinoString(Path.GetFileName(rutaDestino)) }
                };

                return new LatinoDictionary(resultado);
            }
            catch (Exception ex) when (ex is not RuntimeError)
            {
                throw new RuntimeError(null!, $"Error al descargar archivo: {ex.Message}");
            }
        }, 2, "Descarga un archivo desde una URL. Retorna {bytes, ruta, nombre}.", "url, ruta_destino");
        
        // =================================================================
        // UTILIDADES
        // =================================================================
        
        Def("codificarUrl", (i, args) => {
            string texto = args[0].ToString();
            return new LatinoString(Uri.EscapeDataString(texto));
        }, 1, "Codifica un string para uso en URL (URL encoding).", "texto");
        
        Def("decodificarUrl", (i, args) => {
            string texto = args[0].ToString();
            return new LatinoString(Uri.UnescapeDataString(texto));
        }, 1, "Decodifica un string URL-encoded.", "texto");
        
        // =================================================================
        // EXTENSIONES FUTURAS POSIBLES (Esqueletos)
        // =================================================================
        
        /*
        Def("conectarWS", (i, args) => {
            // TODO: Implementar WebSockets
            throw new NotImplementedException("WebSockets aún no implementado.");
        }, 1, "Futura soporte para WebSockets.", "url");

        Def("graphql", (i, args) => {
            // TODO: Implementar GraphQL
            throw new NotImplementedException("GraphQL aún no implementado.");
        }, 2, "Futura soporte para GraphQL.", "url, consulta");
        */

        // Registrar módulo
        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("web", modulo);
    }
    
    // =====================================================================
    // LÓGICA PRINCIPAL DE PETICIÓN HTTP
    // =====================================================================
    
    private static LatinoObject RealizarPeticion(
        string metodo, 
        string url, 
        LatinoObject? datos, 
        Dictionary<string, string>? headers,
        int timeoutSegundos)
    {
        // Validar URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            throw new RuntimeError(null!, $"URL inválida: '{url}'");

        // Validar que el método HTTP sea reconocido.
        // HttpMethod en .NET acepta cualquier string sin validar; la excepción real
        // ocurre tarde (cuando el servidor responde 405) en lugar de lanzarse aquí.
        // Esta lista blanca garantiza un error temprano y claro.
        var metodosPermitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS" };

        if (!metodosPermitidos.Contains(metodo))
            throw new RuntimeError(null!,
                $"Método HTTP no reconocido: '{metodo}'. " +
                $"Métodos válidos: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS.");

        try
        {
            // Crear mensaje HTTP
            var mensaje = new HttpRequestMessage
            {
                Method = new HttpMethod(metodo),
                RequestUri = uri
            };
            
            // Agregar headers personalizados
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    mensaje.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }
            
            // Agregar cuerpo si hay datos
            if (datos != null && metodo != "GET" && metodo != "HEAD")
            {
                mensaje.Content = CrearContenido(datos, headers);
            }
            
            // Configurar timeout específico
            using var cts = new System.Threading.CancellationTokenSource(
                TimeSpan.FromSeconds(timeoutSegundos)
            );
            
            // Realizar petición (sincrónico para compatibilidad con Latino)
            var tarea = HttpClientInstance.SendAsync(mensaje, cts.Token);
            tarea.Wait();
            var respuesta = tarea.Result;
            
            // Leer contenido de respuesta
            var tareaContenido = respuesta.Content.ReadAsStringAsync();
            tareaContenido.Wait();
            string contenidoTexto = tareaContenido.Result;
            
            // Construir diccionario de respuesta
            var resultado = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            resultado[new LatinoString("estado")] = new LatinoNumber((int)respuesta.StatusCode);
            resultado[new LatinoString("ok")] = new LatinoBoolean(respuesta.IsSuccessStatusCode);
            resultado[new LatinoString("texto")] = new LatinoString(contenidoTexto);
            resultado[new LatinoString("url")] = new LatinoString(respuesta.RequestMessage?.RequestUri?.ToString() ?? url);
            
            // Intentar parsear como JSON
            LatinoObject jsonParseado = LatinoNull.Instance;
            if (!string.IsNullOrWhiteSpace(contenidoTexto))
            {
                var contentType = respuesta.Content.Headers.ContentType?.MediaType ?? "";
                if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(contenidoTexto);
                        jsonParseado = FileLib.ConvertirJsonALatino(doc.RootElement);
                    }
                    catch
                    {
                        // Si falla el parsing, dejar como null
                    }
                }
            }
            resultado[new LatinoString("json")] = jsonParseado;
            
            // Headers de respuesta
            var headersRespuesta = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            foreach (var header in respuesta.Headers)
            {
                string valor = string.Join(", ", header.Value);
                headersRespuesta[new LatinoString(header.Key)] = new LatinoString(valor);
            }
            resultado[new LatinoString("encabezados")] = new LatinoDictionary(headersRespuesta);
            
            return new LatinoDictionary(resultado);
        }
        catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
        {
            throw new RuntimeError(null!, 
                $"La petición excedió el timeout de {timeoutSegundos} segundos.");
        }
        catch (HttpRequestException ex)
        {
            throw new RuntimeError(null!, 
                $"Error de red: {ex.Message}");
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, 
                $"Error al realizar petición HTTP: {ex.Message}");
        }
    }
    
    // =====================================================================
    // CREACIÓN DE CONTENIDO HTTP
    // =====================================================================
    
    private static HttpContent CrearContenido(LatinoObject datos, Dictionary<string, string>? headers)
    {
        // Determinar Content-Type (por defecto JSON)
        string contentType = "application/json";
        if (headers != null && headers.ContainsKey("Content-Type"))
        {
            contentType = headers["Content-Type"];
        }
        
        // Caso 1: Datos ya son string (enviar tal cual)
        if (datos is LatinoString str)
        {
            return new StringContent(str.Value, Encoding.UTF8, contentType);
        }
        
        // Caso 2: Diccionario → JSON o Form Data
        if (datos is LatinoDictionary dict)
        {
            if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                // Serializar a JSON
                string json = SerializarAJson(datos);
                return new StringContent(json, Encoding.UTF8, "application/json");
            }
            else if (contentType.Contains("form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                // Serializar a Form Data
                var formData = new Dictionary<string, string>();
                foreach (var kvp in dict.Pairs)
                {
                    formData[kvp.Key.ToString()] = kvp.Value.ToString();
                }
                return new FormUrlEncodedContent(formData);
            }
        }
        
        // Por defecto: intentar serializar a JSON
        string jsonFallback = SerializarAJson(datos);
        return new StringContent(jsonFallback, Encoding.UTF8, "application/json");
    }
    
    // =====================================================================
    // SERIALIZACIÓN JSON
    // =====================================================================
    
    private static string SerializarAJson(LatinoObject obj)
    {
        var options = new JsonWriterOptions 
        { 
            Indented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, options);
        
        EscribirObjetoLatino(writer, obj);
        writer.Flush();
        
        return Encoding.UTF8.GetString(stream.ToArray());
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
                {
                    EscribirObjetoLatino(writer, elem);
                }
                writer.WriteEndArray();
                break;
            
            case LatinoNumber num:
                writer.WriteNumberValue(num.Value);
                break;
            
            case LatinoString str:
                writer.WriteStringValue(str.Value);
                break;
            
            case LatinoBoolean b:
                writer.WriteBooleanValue(b.Value);
                break;
            
            case LatinoNull:
                writer.WriteNullValue();
                break;
            
            default:
                writer.WriteStringValue(obj.ToString());
                break;
        }
    }
    
    // =====================================================================
    // HELPERS
    // =====================================================================
    
    private static Dictionary<string, string> ConvertirHeadersDesdeLatino(LatinoDictionary dict)
    {
        var headers = new Dictionary<string, string>();
        foreach (var kvp in dict.Pairs)
        {
            headers[kvp.Key.ToString()] = kvp.Value.ToString();
        }
        return headers;
    }
    
    private static string? ObtenerString(LatinoDictionary dict, string clave, string? valorPorDefecto = null)
    {
        var key = new LatinoString(clave);
        return dict.Pairs.ContainsKey(key) ? dict.Pairs[key].ToString() : valorPorDefecto;
    }
    
    private static int ObtenerEntero(LatinoDictionary dict, string clave, int valorPorDefecto)
    {
        var key = new LatinoString(clave);
        if (dict.Pairs.ContainsKey(key) && dict.Pairs[key] is LatinoNumber num)
            return (int)num.Value;
        return valorPorDefecto;
    }
    
    private static LatinoObject? ObtenerValor(LatinoDictionary dict, string clave)
    {
        var key = new LatinoString(clave);
        return dict.Pairs.ContainsKey(key) ? dict.Pairs[key] : null;
    }
    
    private static LatinoDictionary? ObtenerDiccionario(LatinoDictionary dict, string clave)
    {
        var key = new LatinoString(clave);
        return dict.Pairs.ContainsKey(key) && dict.Pairs[key] is LatinoDictionary d ? d : null;
    }
    
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) 
    {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) 
            output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
