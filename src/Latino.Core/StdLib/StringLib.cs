using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Latino.Core.Analysis;
using Latino.Core.Runtime;
using Latino.Core.Types;
using System.Linq;

namespace Latino.Core.StdLib;

public static class StringLib
{
    public static LatinoObject Incrustar(Interpreter interp, List<LatinoObject> args)
    {
        string plantilla = args[0].ToString();
        // Regex ampliada para soportar llamadas a funciones básicas dentro de | |
        var regex = new System.Text.RegularExpressions.Regex(@"\|([a-zA-Z_]\w*(?:[\.\[\(].*?)?)\|");

        string resultado = regex.Replace(plantilla, match => {
            try
            {
                var valor = ResolverExpresion(interp, match.Groups[1].Value);
                return valor != null ? valor.ToString() : match.Value;
            }
            catch { return match.Value; }
        });

        return new LatinoString(resultado);
    }

    // --- MINI PARSER INTERNO ---
    
    private enum TipoSegmento { Variable, Propiedad, Indice }
    private class Segmento { public TipoSegmento Tipo; public string Valor = string.Empty; }

    private static LatinoObject? ResolverExpresion(Interpreter interp, string expresion)
    {
        var segmentos = ParsearExpresion(expresion);
        if (segmentos.Count == 0) return null;

        // Paso 1: Variable Base
        LatinoObject? objetoActual = interp.GetVariable(segmentos[0].Valor);
        if (objetoActual == null) return null;

        // Paso 2: Drill-down
        for (int i = 1; i < segmentos.Count; i++)
        {
            var seg = segmentos[i];
            if (seg.Tipo == TipoSegmento.Propiedad)
            {
                if (objetoActual is LatinoInstance inst) 
                    objetoActual = inst.Get(new Token(TokenType.Identifier, seg.Valor, null!, 0, 0));
                else if (objetoActual is LatinoDictionary dic) // Soporte opcional dic.clave
                    objetoActual = dic.Pairs.TryGetValue(new LatinoString(seg.Valor), out var v) ? v : null;
                else return null;
            }
            else if (seg.Tipo == TipoSegmento.Indice)
            {
                var indice = EvaluarIndice(seg.Valor);
                if (indice == null) return null;

                if (objetoActual is LatinoList list && indice is LatinoNumber n)
                {
                    int idx = (int)n.Value;
                    if (idx >= 0 && idx < list.Elements.Count) objetoActual = list.Elements[idx];
                    else return null;
                }
                else if (objetoActual is LatinoDictionary dic)
                {
                    if (dic.Pairs.TryGetValue(indice, out var val)) objetoActual = val;
                    else return null;
                }
                else if (objetoActual is LatinoString str && indice is LatinoNumber sn)
                {
                    int idx = (int)sn.Value;
                    if (idx >= 0 && idx < str.Value.Length) 
                        objetoActual = new LatinoString(str.Value[idx].ToString());
                    else return null;
                }
                else return null;
            }
            if (objetoActual == null) return null;
        }
        return objetoActual;
    }

    private static List<Segmento> ParsearExpresion(string expresion)
    {
        var segmentos = new List<Segmento>();
        int pos = 0;
        
        // Leer Variable Base
        int start = pos;
        while (pos < expresion.Length && (char.IsLetterOrDigit(expresion[pos]) || expresion[pos] == '_')) pos++;
        if (pos == start) return segmentos;
        segmentos.Add(new Segmento { Tipo = TipoSegmento.Variable, Valor = expresion[start..pos] });

        while (pos < expresion.Length)
        {
            if (expresion[pos] == '.')
            {
                pos++;
                start = pos;
                while (pos < expresion.Length && (char.IsLetterOrDigit(expresion[pos]) || expresion[pos] == '_')) pos++;
                if (pos > start) segmentos.Add(new Segmento { Tipo = TipoSegmento.Propiedad, Valor = expresion[start..pos] });
                else return new List<Segmento>(); // Error sintaxis
            }
            else if (expresion[pos] == '[')
            {
                pos++;
                start = pos;
                int depth = 1;
                while (pos < expresion.Length && depth > 0)
                {
                    if (expresion[pos] == '[') depth++;
                    else if (expresion[pos] == ']') depth--;
                    if (depth > 0) pos++;
                }
                if (depth == 0) {
                    segmentos.Add(new Segmento { Tipo = TipoSegmento.Indice, Valor = expresion[start..pos].Trim() });
                    pos++;
                } else return new List<Segmento>(); // Corchete sin cerrar
            }
            else break;
        }
        return segmentos;
    }

    private static LatinoObject? EvaluarIndice(string s)
    {
        if ((s.StartsWith('"') && s.EndsWith('"')) || (s.StartsWith('\'') && s.EndsWith('\'')))
            return new LatinoString(s.Substring(1, s.Length - 2));
        
        if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
            return new LatinoNumber(d);
        
        if (s == "verdadero" || s == "cierto") return new LatinoBoolean(true);
        if (s == "falso") return new LatinoBoolean(false);
        
        return null; // No soportamos variables dentro de índices aún por complejidad
    }


    // =========================================================================
    // SISTEMA DE TRANSFORMACIÓN DE ESTILOS
    // =========================================================================
    
    /// <summary>
    /// Helper: Descompone cualquier string (CamelCase, snake_case, kebab-case, etc.) 
    /// en una lista de palabras individuales limpias.
    /// </summary>
    private static string[] ToWords(string text)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<string>();
        
        // Regex que separa por:
        // - Espacios/tabs (\s+)
        // - Transiciones lowercase→UPPERCASE (camelCase)
        // - Transiciones UPPERCASE→Uppercase (HTMLParser → HTML Parser)
        // - Delimitadores: guiones, barras, puntos, underscores
        var regex = new Regex(
            @"\s+|(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|[-_/\\\.\s]+",
            RegexOptions.Compiled
        );
        
        return regex.Split(text)
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToArray();
    }

    /// <summary>
    /// Transforma texto a diferentes estilos/convenciones de nombres.
    /// </summary>
    public static LatinoObject CambiarEstilo(Interpreter i, List<LatinoObject> args)
    {
        // VALIDACIÓN: Prevenir crash por argumentos faltantes
        if (args.Count < 2)
            throw new RuntimeError(null!, 
                "cadena.estilo() requiere 2 argumentos: (texto, modo). " +
                "Ejemplo: cadena.estilo(\"Hola Mundo\", \"camello\")");

        string text = args[0].ToString();
        string style = args[1].ToString().ToLower().Trim();

        // Validación: Texto vacío retorna vacío
        if (string.IsNullOrEmpty(text)) 
            return new LatinoString("");

        // =====================================================================
        // 1. TRANSFORMACIONES DIRECTAS (No requieren separar palabras)
        // =====================================================================
        
        switch (style)
        {
            case "mayusculas":
            case "mayúsculas":
                return new LatinoString(text.ToUpper());
                
            case "minusculas":
            case "minúsculas":
                return new LatinoString(text.ToLower());
                
            case "invertir":
            {
                var si = new System.Globalization.StringInfo(text);
                int len = si.LengthInTextElements;
                var elementos = new string[len];
                for (int k = 0; k < len; k++)
                    elementos[k] = si.SubstringByTextElements(k, 1);
                Array.Reverse(elementos);
                return new LatinoString(string.Concat(elementos));
            }
                
            case "alternar":
            case "swap":
                // Invierte mayúsculas↔minúsculas (SwapCase)
                return new LatinoString(string.Concat(
                    text.Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c))
                ));
                
            case "capitalizar":
                // Primera letra mayúscula, resto SIN cambios
                if (text.Length == 0) return new LatinoString("");
                return new LatinoString(char.ToUpper(text[0]) + text.Substring(1));
        }

        // =====================================================================
        // 2. TRANSFORMACIONES ESTRUCTURALES (Requieren separar palabras)
        // =====================================================================
        
        var words = ToWords(text);
        if (words.Length == 0) return new LatinoString("");
        
        var sb = new System.Text.StringBuilder();
        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;

        switch (style)
        {
            case "camello":
            case "camel":
                // holaMundoCruel (primera palabra minúscula, resto PascalCase)
                for (int k = 0; k < words.Length; k++)
                    sb.Append(k == 0 
                        ? words[k].ToLower() 
                        : textInfo.ToTitleCase(words[k].ToLower()));
                break;
                
            case "pascal":
                // HolaMundoCruel (todas las palabras capitalizadas)
                foreach (var w in words) 
                    sb.Append(textInfo.ToTitleCase(w.ToLower()));
                break;
                
            case "serpiente":
            case "snake":
                // hola_mundo_cruel
                sb.Append(string.Join("_", words.Select(w => w.ToLower())));
                break;
                
            case "camel_serpiente":
            case "camel_snake":
                // hola_Mundo_Cruel (híbrido: snake con PascalCase excepto primera)
                for (int k = 0; k < words.Length; k++)
                {
                    if (k > 0) sb.Append('_');
                    sb.Append(k == 0 
                        ? words[k].ToLower() 
                        : textInfo.ToTitleCase(words[k].ToLower()));
                }
                break;
                
            case "kebab":
                // hola-mundo-cruel
                sb.Append(string.Join("-", words.Select(w => w.ToLower())));
                break;
                
            case "tren":
            case "train":
                // Hola-Mundo-Cruel (kebab con mayúsculas iniciales)
                sb.Append(string.Join("-", words.Select(w => textInfo.ToTitleCase(w.ToLower()))));
                break;
                
            case "constante":
            case "screaming":
                // HOLA_MUNDO_CRUEL
                sb.Append(string.Join("_", words.Select(w => w.ToUpper())));
                break;
                
            case "titulo":
            case "title":
                // Hola Mundo Cruel (espacios, mayúsculas iniciales)
                foreach (var w in words)
                {
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(textInfo.ToTitleCase(w.ToLower()));
                }
                break;

            case "oracion":
            case "sentence":
                // Hola mundo cruel (solo primera letra mayúscula)
                string sentence = string.Join(" ", words.Select(w => w.ToLower()));
                if (sentence.Length > 0)
                    sb.Append(char.ToUpper(sentence[0]) + sentence.Substring(1));
                break;
                
            case "ruta":
            case "path":
                // INTELIGENTE: Detecta sistema operativo y normaliza TODAS las barras
                char targetSep = Environment.OSVersion.Platform == PlatformID.Win32NT ? '\\' : '/';
                
                string normalized = string.Join(targetSep.ToString(), 
                    words.Select(w => w.ToLower()));
                
                sb.Append(normalized);
                break;
                
            case "ruta_unix":
            case "path_unix":
                // Forzar estilo Unix: convertir TODAS las barras a /
                string unixPath = string.Join("/", words.Select(w => w.ToLower()));
                sb.Append(unixPath);
                break;
                
            case "ruta_windows":
            case "path_windows":
                // Forzar estilo Windows: convertir TODAS las barras a \
                string winPath = string.Join("\\", words.Select(w => w.ToLower()));
                sb.Append(winPath);
                break;
                
            case "punto":
            case "dot":
                // hola.mundo.cruel (notación de namespaces)
                sb.Append(string.Join(".", words.Select(w => w.ToLower())));
                break;

            default:
                throw new RuntimeError(null!, 
                    $"Estilo desconocido: '{style}'. Estilos válidos:\n" +
                    "  Directos: mayusculas, minusculas, invertir, alternar, capitalizar\n" +
                    "  Estructurales: camello, pascal, serpiente, camel_serpiente, kebab, tren,\n" +
                    "                 constante, titulo, oracion, ruta, punto\n" +
                    "  Rutas: ruta (auto), ruta_unix, ruta_windows");
        }

        return new LatinoString(sb.ToString());
    }



    // =====================================================================
    // COMPARACIÓN DE CADENAS (Mejorada vs Latino Clásico)
    // =====================================================================

    // cadena.es_igual(str1, str2, [modo]) -> logico
    public static LatinoObject EsIgual(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "cadena.es_igual() requiere al menos 2 argumentos.");

        string str1 = args[0].ToString();
        string str2 = args[1].ToString();

        // Modo por defecto: exacta (case-sensitive) - compatible con Latino Clásico
        string modo = args.Count >= 3 ? args[2].ToString().ToLower() : "exacta";

        switch (modo)
        {
            case "exacta":
            case "exact":
                // Comparación exacta (Latino Clásico)
                return new LatinoBoolean(str1 == str2);

            case "ignora_caso":
            case "ignore_case":
            case "insensible":
                // Case-insensitive
                return new LatinoBoolean(
                    string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase)
                );

            case "normalizada":
            case "normalized":
                // Normalizar acentos + case-insensitive
                string norm1 = RemoverAcentos(str1).ToLowerInvariant();
                string norm2 = RemoverAcentos(str2).ToLowerInvariant();
                return new LatinoBoolean(norm1 == norm2);

            case "comodin":
            case "wildcard":
            case "patron":
                // Comparación con comodines (* = cualquier secuencia, ? = un carácter)
                return new LatinoBoolean(CompararConComodines(str1, str2));

            case "comodin_insensible":
            case "wildcard_ignore_case":
                // Comodines + case-insensitive
                return new LatinoBoolean(
                    CompararConComodines(str1.ToLowerInvariant(), str2.ToLowerInvariant())
                );

            default:
                throw new RuntimeError(null!, 
                    $"Modo '{modo}' no válido. Usa: 'exacta', 'ignora_caso', 'normalizada', 'comodin', o 'comodin_insensible'.");
        }
    }

    // =====================================================================
    // FUNCIÓN AUXILIAR: Remover Acentos
    // =====================================================================

    private static string RemoverAcentos(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        // Normalizar a NFD (Normalization Form Decomposed)
        var normalizado = texto.Normalize(System.Text.NormalizationForm.FormD);
        var resultado = new System.Text.StringBuilder();

        foreach (char c in normalizado)
        {
            // Filtrar marcas diacríticas (acentos)
            var categoria = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(c);
            }
        }

        // Volver a normalizar a NFC (Normalization Form Composed)
        return resultado.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    // =====================================================================
    // FUNCIÓN AUXILIAR: Comparar con Comodines
    // =====================================================================

    private static bool CompararConComodines(string texto, string patron)
    {
        // Usar StringInfo para tratar emojis y caracteres compuestos como un solo elemento
        var siTexto = new System.Globalization.StringInfo(texto);
        var siPatron = new System.Globalization.StringInfo(patron);

        int lenTexto = siTexto.LengthInTextElements;
        int lenPatron = siPatron.LengthInTextElements;

        int textoIdx = 0;
        int patronIdx = 0;
        int ultimoAsterisco = -1;
        int textoBacktrack = 0;

        while (textoIdx < lenTexto)
        {
            string tChar = siTexto.SubstringByTextElements(textoIdx, 1);
            string pChar = patronIdx < lenPatron ? siPatron.SubstringByTextElements(patronIdx, 1) : "";

            if (patronIdx < lenPatron && (pChar == "?" || pChar == tChar))
            {
                textoIdx++;
                patronIdx++;
            }
            else if (patronIdx < lenPatron && pChar == "*")
            {
                ultimoAsterisco = patronIdx;
                textoBacktrack = textoIdx;
                patronIdx++;
            }
            else if (ultimoAsterisco != -1)
            {
                patronIdx = ultimoAsterisco + 1;
                textoBacktrack++;
                textoIdx = textoBacktrack;
            }
            else
            {
                return false;
            }
        }

        // Consumir asteriscos sobrantes al final del patrón
        while (patronIdx < lenPatron && siPatron.SubstringByTextElements(patronIdx, 1) == "*")
            patronIdx++;

        return patronIdx == lenPatron;
    }

    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();

        // Actualizamos Def para aceptar docs opcionales
        void Def(string name, Func<List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc, pams);
            methods[name] = func;
        }

        // --- BÁSICAS ---
        Def("longitud", args => {
            // Contamos grafemas (Text Elements), no chars de 16 bits.
            // Esto asegura que un emoji cuente como 1, no como 2.
            var si = new System.Globalization.StringInfo(Str(args[0]));
            return new LatinoNumber(si.LengthInTextElements);
        }, 1, "Devuelve el número de caracteres (grafemas) en la cadena. Los emojis cuentan como 1.", "cadena");
        Def("concatenar", args => new LatinoString(Str(args[0]) + Str(args[1])), 2, "Une dos cadenas.", "cadena1, cadena2");
        Def("contiene", args => new LatinoBoolean(Str(args[0]).Contains(Str(args[1]))), 2, "Verifica si una cadena contiene otra.", "cadena, subcadena");
        Def("termina_con", args => new LatinoBoolean(Str(args[0]).EndsWith(Str(args[1]))), 2, "Verifica si la cadena termina con el texto especificado.", "cadena, sufijo");
        Def("inicia_con", args => new LatinoBoolean(Str(args[0]).StartsWith(Str(args[1]))), 2, "Verifica si la cadena inicia con el texto especificado.", "cadena, prefijo");
        Def("indice", args => {
            // Retorna posición en grafemas, consistente con cadena.longitud y cadena.subcadena
            string texto = Str(args[0]);
            string buscar = Str(args[1]);
            
            int charIdx = texto.IndexOf(buscar, StringComparison.Ordinal);
            if (charIdx < 0) return new LatinoNumber(-1);
            
            // Convertir posición de char a posición de grafema
            var si = new System.Globalization.StringInfo(texto.Substring(0, charIdx));
            return new LatinoNumber(si.LengthInTextElements);
        }, 2, "Encuentra la posición en grafemas de una subcadena (-1 si no existe).", "cadena, subcadena");

        Def("encontrar", args => {
            string texto = Str(args[0]);
            string buscar = Str(args[1]);
            
            int charIdx = texto.IndexOf(buscar, StringComparison.Ordinal);
            if (charIdx < 0) return new LatinoNumber(-1);
            
            var si = new System.Globalization.StringInfo(texto.Substring(0, charIdx));
            return new LatinoNumber(si.LengthInTextElements);
        }, 2, "Encuentra la posición en grafemas de una subcadena (-1 si no existe).", "cadena, subcadena");

        // --- TRANSFORMACIÓN ---
        Def("mayusculas", args => new LatinoString(Str(args[0]).ToUpper()), 1, "Convierte la cadena a MAYÚSCULAS.", "cadena");
        Def("minusculas", args => new LatinoString(Str(args[0]).ToLower()), 1, "Convierte la cadena a minúsculas.", "cadena");
        Def("recortar", args => new LatinoString(Str(args[0]).Trim()), 1, "Elimina espacios al inicio y final de la cadena.", "cadena");
        Def("invertir", args => {
            string s = Str(args[0]);
            var si = new System.Globalization.StringInfo(s);
            int len = si.LengthInTextElements;
            var elementos = new string[len];
            for (int k = 0; k < len; k++)
                elementos[k] = si.SubstringByTextElements(k, 1);
            Array.Reverse(elementos);
            return new LatinoString(string.Concat(elementos));
        }, 1, "Invierte el orden de los caracteres sin corromper emojis o caracteres especiales.", "cadena");

        // --- TRANSFORMACIÓN AVANZADA ---
        Def("estilo", args => CambiarEstilo(null!, args), 2, 
            "Transforma texto a diferentes estilos (camello, pascal, serpiente, kebab, etc.).", 
            "texto, estilo");

        // --- SUBSTRINGS ---
        Def("subcadena", args => {
            string s = Str(args[0]);
            int start = (int)Num(args[1]);
            int length = (int)Num(args[2]);

            var si = new System.Globalization.StringInfo(s);
            int totalLen = si.LengthInTextElements;

            if (start < 0) start = 0;
            if (start >= totalLen) return new LatinoString("");
            if (start + length > totalLen) length = totalLen - start;

            // SubstringByTextElements garantiza que los emojis no se corten a la mitad
            return new LatinoString(si.SubstringByTextElements(start, length));
        }, 3, "Extrae una parte de la cadena sin corromper emojis o caracteres especiales.", "cadena, inicio, longitud");

        // Regex lógico (Blindado)
        Def("regexl", args => {
            try { 
                return new LatinoBoolean(Regex.IsMatch(Str(args[0]), Str(args[1]), RegexOptions.None, SysLib.RegexTimeout)); 
            }
            catch (RegexMatchTimeoutException) { 
                throw new RuntimeError(null!, $"Regex timeout ({SysLib.RegexTimeoutSegundos}s). Patrón complejo. Ajusta sis.config('regex_timeout', N)"); 
            }
            catch (Exception ex) { throw new RuntimeError(null!, $"Error Regex: {ex.Message}"); }
        }, 2, "Verifica si la cadena coincide con una expresión regular.", "cadena, patron");

        // Regex extracción (Blindado)
        Def("regex", args => {
            try {
                var matches = Regex.Matches(Str(args[0]), Str(args[1]), RegexOptions.None, SysLib.RegexTimeout);
                var list = new List<LatinoObject>();
                foreach (Match m in matches) list.Add(new LatinoString(m.Value));
                return new LatinoList(list);
            }
            catch (RegexMatchTimeoutException) { 
                throw new RuntimeError(null!, $"Regex timeout ({SysLib.RegexTimeoutSegundos}s). Patrón complejo. Ajusta sis.config('regex_timeout', N)"); 
            }
            catch (Exception ex) { throw new RuntimeError(null!, $"Error Regex: {ex.Message}"); }
        }, 2, "Extrae todas las coincidencias de una expresión regular.", "cadena, patron");

        // Regex reemplazo avanzado (Soporta Lambdas con grupos, Límite opcional)
        Def("reemplazarRegex", args => {
            if (args.Count < 3) 
                throw new RuntimeError(null!, 
                    "cadena.reemplazarRegex requiere 3 argumentos: texto, patron, reemplazo. " +
                    "Opcionalmente un 4to argumento: limite (número entero).");
            
            string texto   = Str(args[0]);
            string patron  = Str(args[1]);
            var reemplazo  = args[2];
            
            int limite = -1; // -1 = sin límite (reemplazar todas las ocurrencias)
            if (args.Count > 3 && args[3] is LatinoNumber nLimite)
                limite = (int)nLimite.Value;

            try {
                var regex = new Regex(patron, RegexOptions.None, SysLib.RegexTimeout);
                
                // ── Modo Dinámico: el reemplazo es una función (lambda) ────────────
                if (reemplazo is ICallable func)
                {
                    MatchEvaluator evaluador = (Match match) =>
                    {
                        // Construir la lista de argumentos para la lambda de Latino.
                        // Firma: (matchCompleto, grupo1, grupo2, ..., grupoN, indice, textoOriginal)
                        // Esto replica la API de JS String.prototype.replace con función,
                        // permitiendo usar grupos de captura dentro de la lógica de transformación.
                        var funcArgs = new List<LatinoObject>();

                        // Arg 0: texto completo del match
                        funcArgs.Add(new LatinoString(match.Value));

                        // Args 1..N: grupos de captura posicionales (sin el grupo 0 que es el match completo)
                        for (int g = 1; g < match.Groups.Count; g++)
                            funcArgs.Add(new LatinoString(match.Groups[g].Value));

                        // Arg N+1: posición del match en el texto original (índice de char)
                        funcArgs.Add(new LatinoNumber(match.Index));

                        // Arg N+2: texto original completo
                        funcArgs.Add(new LatinoString(texto));

                        // FIX ARQUITECTÓNICO: Latino.NET tiene validación estricta de aridad.
                        // No podemos enviar más argumentos de los que la función declaró,
                        // o LatinoFunction lanzará un RuntimeError. Recortamos la lista:
                        if (func.Arity >= 0 && funcArgs.Count > func.Arity)
                        {
                            funcArgs = funcArgs.GetRange(0, func.Arity);
                        }

                        var resultado = func.Call(interpreter, funcArgs);
                        return resultado is LatinoNull ? "" : resultado.ToString();
                    };
                    
                    return new LatinoString(limite >= 0 
                        ? regex.Replace(texto, evaluador, limite) 
                        : regex.Replace(texto, evaluador));
                }
                // ── Modo Estático: el reemplazo es una cadena ─────────────────────
                // .NET soporta referencias a grupos de captura nativamente:
                //   $1, $2 ... $N  → grupos posicionales
                //   ${nombre}      → grupos con nombre
                //   $&             → match completo
                //   $`             → texto antes del match
                //   $'             → texto después del match
                else
                {
                    string reemplazoStr = Str(reemplazo);
                    return new LatinoString(limite >= 0 
                        ? regex.Replace(texto, reemplazoStr, limite) 
                        : regex.Replace(texto, reemplazoStr));
                }
            }
            catch (RegexMatchTimeoutException) { 
                throw new RuntimeError(null!, 
                    $"Regex timeout ({SysLib.RegexTimeoutSegundos}s). Patrón complejo. " +
                    "Ajusta sis.config('regex_timeout', N);"); 
            }
            catch (ArgumentException ex) {
                // Patrón regex inválido — captura distinta a timeout para dar mejor mensaje
                throw new RuntimeError(null!, $"Patrón regex inválido: {ex.Message}");
            }
            catch (Exception ex) { 
                throw new RuntimeError(null!, $"Error en reemplazarRegex: {ex.Message}"); 
            }
        }, -1, 
        "Reemplaza coincidencias de una expresión regular. El reemplazo puede ser " +
        "texto (con $1, $2 para grupos) o una función(match, grupo1..N, indice, original). " +
        "Límite opcional como 4to argumento.",
        "texto, patron, reemplazo_o_funcion, [limite]");

        // --- UTILIDADES ---
        Def("esta_vacia", args => new LatinoBoolean(string.IsNullOrEmpty(Str(args[0]))), 1, "Verifica si la cadena está vacía o es nula.", "cadena");
        Def("es_vacia", args => new LatinoBoolean(string.IsNullOrEmpty(Str(args[0]))), 1, "Verifica si la cadena está vacía o es nula.", "cadena");

        Def("rellenar_izquierda", args => {
            string s = Str(args[0]);
            char relleno = Str(args[1])[0];
            int longitud = (int)Num(args[2]);
            // Usar grafemas para calcular el relleno visual correcto
            var si = new System.Globalization.StringInfo(s);
            int faltantes = longitud - si.LengthInTextElements;
            if (faltantes <= 0) return new LatinoString(s);
            return new LatinoString(new string(relleno, faltantes) + s);
        }, 3, "Rellena la cadena por la izquierda hasta alcanzar la longitud en grafemas especificada.", "cadena, caracter, longitud");

        Def("rellenar_derecha", args => {
            string s = Str(args[0]);
            char relleno = Str(args[1])[0];
            int longitud = (int)Num(args[2]);
            var si = new System.Globalization.StringInfo(s);
            int faltantes = longitud - si.LengthInTextElements;
            if (faltantes <= 0) return new LatinoString(s);
            return new LatinoString(s + new string(relleno, faltantes));
        }, 3, "Rellena la cadena por la derecha hasta alcanzar la longitud en grafemas especificada.", "cadena, caracter, longitud");

        // Formato estilo C (Transpilado a .NET)
        Def("formato", args => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "cadena.formato() requiere al menos la plantilla de formato.");

            string fmt = Str(args[0]);
            var restArgs = args.GetRange(1, args.Count - 1);
            return new LatinoString(FormatCStyle(fmt, restArgs));
        }, -1, "Genera una cadena con formato estilo printf (transpilado a .NET).", "formato, valores...");

        // --- PARIDAD CLÁSICA (FALTANTES) ---

        Def("separar", args => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "cadena.separar() requiere al menos la cadena a separar.");
            
            string s = Str(args[0]);
            string sep = args.Count > 1 ? Str(args[1]) : " "; // Default espacio
            var parts = s.Split(new[] { sep }, StringSplitOptions.None);
            var list = new List<LatinoObject>();
            foreach(var p in parts) list.Add(new LatinoString(p));
            return new LatinoList(list);
        }, -1, "Divide la cadena en una lista usando un separador.", "cadena, [separador]");

        Def("reemplazar", args => {
            string s = Str(args[0]);
            string oldVal = Str(args[1]);
            string newVal = Str(args[2]);
            return new LatinoString(s.Replace(oldVal, newVal));
        }, 3, "Reemplaza todas las ocurrencias de un texto.", "cadena, buscar, reemplazo");

        Def("ultimo_indice", args => {
            string texto = Str(args[0]);
            string buscar = Str(args[1]);
            
            int charIdx = texto.LastIndexOf(buscar, StringComparison.Ordinal);
            if (charIdx < 0) return new LatinoNumber(-1);
            
            // Convertir posición de char a posición de grafema
            var si = new System.Globalization.StringInfo(texto.Substring(0, charIdx));
            return new LatinoNumber(si.LengthInTextElements);
        }, 2, "Encuentra la última posición en grafemas de una subcadena (-1 si no existe).", "cadena, subcadena");

        Def("comparar", args => {
            string s1 = Str(args[0]);
            string s2 = Str(args[1]);
            // CompareOrdinal realiza una comparación byte a byte (ASCII/Unicode)
            // y devuelve la diferencia numérica exacta (ej: -20), replicando C.
            return new LatinoNumber(string.CompareOrdinal(s1, s2));
        }, 2, "Compara dos cadenas y devuelve la diferencia numérica de caracteres (0 si son iguales).", "cadena1, cadena2");

        Def("insertar", args => {
            string s = Str(args[0]);
            string sub = Str(args[1]);
            int pos = (int)Num(args[2]);

            var si = new System.Globalization.StringInfo(s);
            int totalLen = si.LengthInTextElements;

            if (pos < 0) pos = 0;
            if (pos > totalLen) pos = totalLen;

            // SubstringByTextElements requiere dos argumentos: inicio y longitud.
            // La sobrecarga de un solo argumento NO existe en .NET.
            string left  = pos > 0 ? si.SubstringByTextElements(0, pos) : "";
            string right = pos < totalLen ? si.SubstringByTextElements(pos, totalLen - pos) : "";

            return new LatinoString(left + sub + right);
        }, 3, "Inserta una subcadena en la posición en grafemas especificada respetando emojis y caracteres Unicode.", "cadena, subcadena, posicion");

        Def("eliminar", args => {
            string s = Str(args[0]);
            string sub = Str(args[1]);

            // Optimización: retornar inmediatamente si la subcadena a eliminar está vacía
            if (string.IsNullOrEmpty(sub)) return new LatinoString(s);

            // StringComparison.Ordinal garantiza coincidencia exacta de bytes,
            // lo cual es correcto para emojis y secuencias ZWJ
            int index = s.IndexOf(sub, StringComparison.Ordinal);
            if (index < 0) return new LatinoString(s);

            return new LatinoString(s.Remove(index, sub.Length));
        }, 2, "Elimina la primera ocurrencia de una subcadena.", "cadena, subcadena");

        Def("es_numero", args => {
            return new LatinoBoolean(double.TryParse(Str(args[0]), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _));
        }, 1, "Verifica si la cadena es un número válido.", "cadena");

        Def("es_numerico", args => {
            return new LatinoBoolean(double.TryParse(Str(args[0]), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _));
        }, 1, "Verifica si la cadena es un número válido (alias de es_numero).", "cadena");

        Def("es_alfa", args => {
            return new LatinoBoolean(Str(args[0]).All(char.IsLetterOrDigit));
        }, 1, "Verifica si la cadena contiene solo letras o números.", "cadena");

        Def("bytes", args => {
            var list = new List<LatinoObject>();
            foreach(char c in Str(args[0])) list.Add(new LatinoNumber(c));
            return new LatinoList(list);
        }, 1, "Convierte cadena a lista de códigos UTF-16. NOTA: Emojis y caracteres fuera del BMP generan 2 códigos (par subrogado).", "cadena");

        Def("char", args => {
            if (args[0] is LatinoList l)
            {
                string res = "";
                foreach(var item in l.Elements) res += (char)((LatinoNumber)item).Value;
                return new LatinoString(res);
            }
            if (args[0] is LatinoNumber n) return new LatinoString(((char)n.Value).ToString());
            return LatinoNull.Instance;
        }, 1, "Convierte lista de ASCII o número a carácter.", "lista_o_numero");

        // =====================================================================
        // EJECUCIÓN DINÁMICA (Legacy - Retrocompatibilidad con Latino Clásico)
        // =====================================================================
        
        // cadena.ejecutar("codigo") -> nulo (solo ejecuta, NO retorna valor)
        Def("ejecutar", args => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "cadena.ejecutar() requiere un argumento: el código a ejecutar.");
            
            string codigo = args[0].ToString();
            
            try 
            {
                // Compilar código
                var lexer = new Lexer(codigo);
                var parser = new Parser(lexer.Tokenize());
                var statements = parser.Parse();
                
                // Ejecutar sin capturar retorno (legacy)
                if (statements != null)
                {
                    interpreter.Interpret(statements);
                }
                
                // SIEMPRE retorna nulo (comportamiento legacy)
                return LatinoNull.Instance;
            }
            catch (Exception ex)
            {
                throw new RuntimeError(null!, $"Error al ejecutar cadena: {ex.Message}");
            }
            
        }, 1, "Ejecuta código Latino contenido en una cadena (legacy, no retorna valor).", "codigo");

        // =====================================================================
        // COMPARACIÓN AVANZADA
        // =====================================================================
        
        Def("es_igual", args => EsIgual(interpreter, args), -1, 
            "Compara dos cadenas con diferentes modos (exacta, ignora_caso, normalizada, comodin).", 
            "cadena1, cadena2, [modo]");

        // =====================================================================
        // ANÁLISIS Y CONTEO
        // =====================================================================

        Def("contar", args => {
            string texto = Str(args[0]);
            string subcadena = Str(args[1]);

            if (string.IsNullOrEmpty(subcadena))
                return new LatinoNumber(0);

            int count = 0;
            int pos = 0;

            // Buscar sobre chars es correcto aquí (buscamos el string completo, no un índice)
            // pero avanzamos subcadena.Length chars para evitar solapamiento correctamente
            while ((pos = texto.IndexOf(subcadena, pos, StringComparison.Ordinal)) != -1)
            {
                count++;
                pos += subcadena.Length;
            }

            return new LatinoNumber(count);
        }, 2, "Cuenta cuántas veces aparece la subcadena (sin solapamiento).", "texto, subcadena");

        Def("contarPalabras", args => {
            string texto = Str(args[0]);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoNumber(0);
            
            char[] separadores = { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '!', '?', 
                                  '(', ')', '[', ']', '{', '}', '"', '\'', '-', '–', '—' };
            
            var palabras = texto.Split(separadores, StringSplitOptions.RemoveEmptyEntries);
            
            return new LatinoNumber(palabras.Length);
        }, 1, "Retorna el número de palabras en el texto.", "texto");

        // =====================================================================
        // MANIPULACIÓN
        // =====================================================================

        Def("partir", args => {
            string texto = Str(args[0]);
            string separador = Str(args[1]);
            int limite = (int)Num(args[2]);
            
            if (limite <= 0)
                throw new RuntimeError(null!, "El límite debe ser un número entero positivo.");
            
            if (string.IsNullOrEmpty(separador))
                throw new RuntimeError(null!, "El separador no puede estar vacío.");
            
            var partes = texto.Split(new[] { separador }, limite, StringSplitOptions.None);
            var lista = new List<LatinoObject>();
            
            foreach (var parte in partes)
                lista.Add(new LatinoString(parte));
            
            return new LatinoList(lista);
        }, 3, "Divide el texto con límite máximo de divisiones.", "texto, separador, limite");

        Def("centrar", args => {
            string texto = Str(args[0]);
            int ancho = (int)Num(args[1]);
            char relleno = args.Count >= 3 && !string.IsNullOrEmpty(Str(args[2]))
                ? Str(args[2])[0]
                : ' ';

            if (ancho <= 0)
                throw new RuntimeError(null!, "El ancho debe ser un número entero positivo.");

            // Usar longitud en grafemas para calcular el centrado visual correcto
            var si = new System.Globalization.StringInfo(texto);
            int longGrafemas = si.LengthInTextElements;

            if (longGrafemas >= ancho)
                return new LatinoString(texto);

            int espacioTotal = ancho - longGrafemas;
            int izquierda = espacioTotal / 2;
            int derecha = espacioTotal - izquierda;

            return new LatinoString(
                new string(relleno, izquierda) + texto + new string(relleno, derecha)
            );
        }, -1, "Centra el texto en un campo de ancho especificado usando conteo de grafemas. Relleno opcional.", "texto, ancho, [relleno]");

        // =====================================================================
        // EXTRACCIÓN (Modos Predefinidos)
        // =====================================================================

        Def("extraer", args => {
            string texto = Str(args[0]);
            string modo = Str(args[1]).ToLower().Trim();
            
            var patrones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "digitos", @"\d+" },
                { "letras", @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+" },
                { "decimales", @"\d+\.\d+" },
                { "emails", @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}" },
                { "urls", @"https?://[^\s]+" },
                { "telefonos", @"\+?\d[\d\s\-\(\)]{7,}\d" },
                { "hashtags", @"#[a-zA-Z0-9_]+" },
                { "menciones", @"@[a-zA-Z0-9_]+" },
                { "monedas", @"[$€£¥]\s*\d+(?:\.\d{2})?" },
                { "fechas", @"\d{1,2}[-/]\d{1,2}[-/]\d{2,4}|\d{4}-\d{2}-\d{2}" }
            };
            
            if (!patrones.ContainsKey(modo))
            {
                var modosValidos = string.Join(", ", patrones.Keys);
                throw new RuntimeError(null!, 
                    $"Modo de extracción desconocido: '{modo}'. " +
                    $"Modos válidos: {modosValidos}");
            }
            
            try
            {
                var matches = Regex.Matches(texto, patrones[modo], 
                    RegexOptions.None, SysLib.RegexTimeout);
                
                var lista = new List<LatinoObject>();
                foreach (Match m in matches)
                    lista.Add(new LatinoString(m.Value));
                
                return new LatinoList(lista);
            }
            catch (RegexMatchTimeoutException)
            {
                throw new RuntimeError(null!, $"Tiempo de extracción excedido (timeout de {SysLib.RegexTimeoutSegundos} segundos).");
            }
            catch (Exception ex)
            {
                throw new RuntimeError(null!, $"Error en extracción: {ex.Message}");
            }
        }, 2, "Extrae patrones del texto. Modos: digitos, letras, decimales, emails, urls, telefonos, hashtags, menciones, monedas, fechas", "texto, modo");

        // =====================================================================
        // CONVERSIÓN DE NÚMEROS A TEXTO
        // =====================================================================

        Def("nombrarNumero", args => {
            if (args[0] is not LatinoNumber num)
                throw new RuntimeError(null!, 
                    "cadena.nombrarNumero() requiere un número como argumento.");
            
            // Truncar a entero (ignorar decimales)
            long entero = (long)Math.Truncate(num.Value);
            
            // Validar rango soportado
            const long LIMITE_SUPERIOR = 999_999_999_999_999_999; // ~10^18 (un trillón)
            const long LIMITE_INFERIOR = -LIMITE_SUPERIOR;
            
            if (entero > LIMITE_SUPERIOR || entero < LIMITE_INFERIOR)
            {
                throw new RuntimeError(null!, 
                    $"cadena.nombrarNumero() soporta números entre {LIMITE_INFERIOR:N0} y {LIMITE_SUPERIOR:N0}. " +
                    $"Número recibido: {entero:N0}");
            }
            
            return new LatinoString(NumerosEnEspanol.Convertir(entero));
            
        }, 1, "Convierte un número a su representación cardinal en español (ignora decimales).", "numero");

        // =====================================================================
        // CRIPTOGRAFÍA Y SEGURIDAD
        // =====================================================================

        Def("hash", args => {
            string texto = Str(args[0]);
            string algoritmo = args.Count >= 2 ? Str(args[1]).ToUpper().Trim() : "SHA256";

            byte[] data = System.Text.Encoding.UTF8.GetBytes(texto);
            byte[] hashBytes;

            using (System.Security.Cryptography.HashAlgorithm hashAlg = algoritmo switch {
                "MD5" => System.Security.Cryptography.MD5.Create(),
                "SHA1" => System.Security.Cryptography.SHA1.Create(),
                "SHA256" => System.Security.Cryptography.SHA256.Create(),
                "SHA512" => System.Security.Cryptography.SHA512.Create(),
                _ => throw new RuntimeError(null!, $"Algoritmo de hash no soportado: '{algoritmo}'. Usa: MD5, SHA1, SHA256, SHA512.")
            })
            {
                hashBytes = hashAlg.ComputeHash(data);
            }

            return new LatinoString(BitConverter.ToString(hashBytes).Replace("-", "").ToLower());
        }, -1, "Calcula el hash de una cadena. Algoritmos: MD5, SHA1, SHA256 (default), SHA512.", "texto, [algoritmo]");

        Def("codificarBase64", args => {
            string texto = Str(args[0]);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(texto);
            return new LatinoString(Convert.ToBase64String(bytes));
        }, 1, "Codifica una cadena de texto a Base64.", "texto");

        Def("decodificarBase64", args => {
            string base64 = Str(args[0]);
            try {
                byte[] bytes = Convert.FromBase64String(base64);
                return new LatinoString(System.Text.Encoding.UTF8.GetString(bytes));
            } catch (Exception) {
                throw new RuntimeError(null!, "La cadena no tiene un formato Base64 válido.");
            }
        }, 1, "Decodifica una cadena en formato Base64 a texto.", "base64");

        interpreter.Globals.Define("cadena", new LatinoDictionary(ConvertMap(methods), true));
    }

    // =============================================================================
    // SISTEMA DE FORMATEO HÍBRIDO
    // =============================================================================

    /// <summary>
    /// Formato nativo .NET para escribirf/ponerf
    /// Sintaxis: "Hola {0}, precio {1:C}, hex {2:X}"
    /// </summary>
    public static string FormatNative(string format, List<LatinoObject> args)
    {
        if (string.IsNullOrEmpty(format)) return format;
        
        try
        {
            // Convertir LatinoObjects a tipos primitivos que .NET entiende
            var rawArgs = new object[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                
                // Manejar números especialmente para formato hex/enteros
                if (arg is LatinoNumber num)
                {
                    // Si es entero matemático, convertir a long para soportar :X
                    if (num.Value % 1 == 0 && num.Value >= long.MinValue && num.Value <= long.MaxValue)
                        rawArgs[i] = (long)num.Value;
                    else
                        rawArgs[i] = num.Value;
                }
                else if (arg is LatinoString str)
                    rawArgs[i] = str.Value;
                else if (arg is LatinoBoolean b)
                    rawArgs[i] = b.Value;
                else if (arg is LatinoNull)
                    rawArgs[i] = "nulo";
                else
                    rawArgs[i] = arg.ToString();
            }
            
            return string.Format(format, rawArgs);
        }
        catch (FormatException ex)
        {
            throw new RuntimeError(null!, $"Error de formato: {ex.Message}");
        }
    }

    /// <summary>
    /// Transpilador C-style → .NET para imprimirf
    /// Convierte: "Pi=%.2f hex=%x" → "Pi={0:F2} hex={1:X}"
    /// </summary>
    public static string FormatCStyle(string format, List<LatinoObject> args)
    {
        if (string.IsNullOrEmpty(format)) return format;

        int argIndex = 0;
        
        // Regex mejorada: captura flags, width, precision y type
        // Patrón: %(flags)(width)(.precision)(type)
        var regex = new Regex(@"%(?:([-+0 #]+)?(\d+)?(\.(\d+))?)?([sdifoxXegc%])");

        try
        {
            string netFormat = regex.Replace(format, match => 
            {
                string type = match.Groups[5].Value;
                
                // %% → % literal
                if (type == "%") return "%";

                // Sin más argumentos → mantener original como literal
                if (argIndex >= args.Count) 
                    return match.Value;

                string flags = match.Groups[1].Value;
                string width = match.Groups[2].Value;
                string precision = match.Groups[4].Value;

                // Construir {index[,alignment][:format]}
                var parts = new List<string> { argIndex.ToString() };

                // ALINEACIÓN: .NET usa negativo=izquierda, positivo=derecha
                if (!string.IsNullOrEmpty(width))
                {
                    bool leftAlign = flags.Contains('-');
                    parts.Add("," + (leftAlign ? "-" : "") + width);
                }

                // ESPECIFICADOR DE FORMATO
                string fmtSpec = type switch
                {
                    "f" => "F" + (!string.IsNullOrEmpty(precision) ? precision : "6"), // Default 6 decimales (estándar C)
                    "e" => "e" + (!string.IsNullOrEmpty(precision) ? precision : "6"),
                    "g" => "G" + (!string.IsNullOrEmpty(precision) ? precision : ""),  // General
                    "d" or "i" => "D",               // Decimal (entero)
                    "x" => "x" + (!string.IsNullOrEmpty(precision) ? precision : ""),  // Hex minúsculas
                    "X" => "X" + (!string.IsNullOrEmpty(precision) ? precision : ""),  // Hex mayúsculas
                    "o" => "",                       // Octal (manejado después)
                    "c" => "",                       // Carácter (sin formato)
                    "s" => "",                       // String (sin formato)
                    _ => ""
                };

                if (!string.IsNullOrEmpty(fmtSpec))
                    parts.Add(":" + fmtSpec);

                string placeholder = "{" + string.Join("", parts) + "}";
                
                argIndex++;
                return placeholder;
            });

            // MANEJO ESPECIAL: Octal (%o) no existe en .NET
            // Procesamos los args antes de Format
            var processedArgs = new List<object>();
            argIndex = 0;

            // Re-parsear el formato ORIGINAL para detectar %o
            var octalMatches = new Regex(@"%(?:[-+0 #]+)?(?:\d+)?(?:\.\d+)?o").Matches(format);
            var needsOctalConversion = new HashSet<int>();
            
            foreach (Match m in octalMatches)
            {
                // Calcular qué índice de argumento corresponde a este %o
                int position = format.Substring(0, m.Index).Count(c => c == '%') - 
                               format.Substring(0, m.Index).Count(s => s == '%' && format[format.IndexOf(s) + 1] == '%');
                needsOctalConversion.Add(position);
            }

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                // Conversión especial para Octal
                if (needsOctalConversion.Contains(i) && arg is LatinoNumber num)
                {
                    processedArgs.Add(Convert.ToString((long)num.Value, 8));
                    continue;
                }

                // Conversión estándar
                if (arg is LatinoNumber n)
                {
                    // Enteros para :D, :X
                    if (n.Value % 1 == 0 && n.Value >= long.MinValue && n.Value <= long.MaxValue)
                        processedArgs.Add((long)n.Value);
                    else
                        processedArgs.Add(n.Value);
                }
                else if (arg is LatinoString s)
                    processedArgs.Add(s.Value);
                else if (arg is LatinoBoolean b)
                    processedArgs.Add(b.Value ? "verdadero" : "falso");
                else if (arg is LatinoNull)
                    processedArgs.Add("nulo");
                else
                    processedArgs.Add(arg.ToString());
            }

            return string.Format(netFormat, processedArgs.ToArray());
        }
        catch (FormatException ex)
        {
            throw new RuntimeError(null!, $"Error en formato C-style: {ex.Message}");
        }
    }

    private static string Str(LatinoObject obj) => obj.ToString();
    private static double Num(LatinoObject obj) => ((LatinoNumber)obj).Value;

    // =========================================================================
    // CLASE AUXILIAR: CONVERSIÓN DE NÚMEROS A TEXTO EN ESPAÑOL
    // =========================================================================

    /// <summary>
    /// Convierte números enteros a su representación cardinal en español.
    /// Sigue las reglas de la RAE y usa escala larga (millón = 10^6, billón = 10^12).
    /// </summary>
    private static class NumerosEnEspanol
    {
        // =====================================================================
        // TABLAS LÉXICAS ESTÁTICAS (Basadas en normas de la RAE)
        // =====================================================================
        
        private static readonly string[] Unidades = 
            { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
        
        private static readonly string[] Especiales0_15 = 
            { "cero", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", 
              "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince" };
        
        private static readonly string[] Decenas = 
            { "", "", "veinte", "treinta", "cuarenta", "cincuenta", 
              "sesenta", "setenta", "ochenta", "noventa" };
        
        // Formas especiales de 16 a 19
        private static readonly string[] Dieci16_19 = 
            { "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
        
        // Formas especiales de 21 a 29
        private static readonly string[] Veinti21_29 = 
            { "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco", 
              "veintiséis", "veintisiete", "veintiocho", "veintinueve" };
        
        // Centenas (índice 1-9)
        private static readonly string[] Centenas = 
            { "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos", 
              "seiscientos", "setecientos", "ochocientos", "novecientos" };
        
        // Escalas (escala larga): mil, millón, billón, trillón...
        // Índice par = singular, impar = plural
        private static readonly string[] Escala =
        {
            "",                              // 10^0  (unidades, no se nombra)
            "mil",                           // 10^3  (siempre singular)
            "millón",        "millones",     // 10^6
            "billón",        "billones",     // 10^12
            "trillón",       "trillones",    // 10^18
            "cuatrillón",    "cuatrillones", // 10^24
            "quintillón",    "quintillones", // 10^30
            "sextillón",     "sextillones",  // 10^36
            "septillón",     "septillones",  // 10^42
            "octillón",      "octillones",   // 10^48
            "nonillón",      "nonillones",   // 10^54
            "decillón",      "decillones",   // 10^60
            "undecillón",    "undecillones", // 10^66
            "duodecillón",   "duodecillones",// 10^72
            "tredecillón",   "tredecillones",// 10^78
            "cuatrodecillón","cuatrodecillones", // 10^84
            "quindecillón",  "quindecillones",   // 10^90
            "sexdecillón",   "sexdecillones",    // 10^96
            "septendecillón","septendecillones", // 10^102
            "octodecillón",  "octodecillones",   // 10^108
            "novendecillón", "novendecillones",  // 10^114
            "vigintillón",   "vigintillones"     // 10^120
        };
        
        // =====================================================================
        // CACHÉ DE NÚMEROS FRECUENTES (0-1000) - OPTIMIZACIÓN CRÍTICA
        // =====================================================================
        
        private static readonly Dictionary<long, string> CacheNumerosComunes;
        
        static NumerosEnEspanol()
        {
            // Precalcular los primeros 1001 números (0-1000)
            CacheNumerosComunes = new Dictionary<long, string>(1001);
            for (long i = 0; i <= 1000; i++)
            {
                CacheNumerosComunes[i] = ConvertirSinCache(i);
            }
        }
        
        // =====================================================================
        // MÉTODO PÚBLICO PRINCIPAL
        // =====================================================================
        
        /// <summary>
        /// Convierte un número a su representación cardinal en español.
        /// Ejemplos: 
        ///   4421 → "cuatro mil cuatrocientos veintiuno"
        ///   -31 → "menos treinta y uno"
        /// </summary>
        public static string Convertir(long n)
        {
            // Optimización: usar caché para números frecuentes
            if (n >= 0 && n <= 1000)
                return CacheNumerosComunes[n];
            
            if (n < 0 && n >= -1000)
                return "menos " + CacheNumerosComunes[-n];
            
            return ConvertirSinCache(n);
        }
        
        // =====================================================================
        // LÓGICA DE CONVERSIÓN PRINCIPAL
        // =====================================================================
        
        private static string ConvertirSinCache(long n)
        {
            if (n == 0) return "cero";
            
            // Manejo de negativos
            if (n < 0)
                return "menos " + ConvertirSinCache(-n);
            
            // Dividir en grupos de 3 dígitos (de derecha a izquierda)
            var grupos = new List<int>();
            long temp = n;
            while (temp > 0)
            {
                grupos.Add((int)(temp % 1000));
                temp /= 1000;
            }
            
            // Optimización: usar StringBuilder para números grandes
            var sb = new System.Text.StringBuilder(100);
            bool esPrimero = true;
            
            // Procesar desde el grupo más significativo hacia abajo
            for (int i = grupos.Count - 1; i >= 0; i--)
            {
                int valorGrupo = grupos[i];
                if (valorGrupo == 0) continue;
                
                if (!esPrimero) sb.Append(' ');
                esPrimero = false;
                
                // --- Caso especial: miles (i == 1) ---
                if (i == 1)
                {
                    if (valorGrupo == 1)
                    {
                        sb.Append("mil");
                    }
                    else
                    {
                        sb.Append(ConvertirGrupo(valorGrupo, esUltimo: false));
                        sb.Append(" mil");
                    }
                    continue;
                }
                
                // --- Caso: unidades (i == 0) ---
                if (i == 0)
                {
                    sb.Append(ConvertirGrupo(valorGrupo, esUltimo: true));
                    continue;
                }
                
                // --- Caso: millones y superiores (i >= 2) ---
                int escalaIndex = i * 2 - 2;
                
                // Protección contra índice fuera de rango
                if (escalaIndex >= Escala.Length - 1)
                {
                    // Número demasiado grande, retornar notación científica
                    return $"[número fuera de rango: ~10^{i * 3}]";
                }
                
                string nombreEscala = (valorGrupo == 1) 
                    ? Escala[escalaIndex]      // singular
                    : Escala[escalaIndex + 1]; // plural
                
                string grupoStr = (valorGrupo == 1) 
                    ? "un" 
                    : ConvertirGrupo(valorGrupo, esUltimo: false);
                
                sb.Append(grupoStr);
                sb.Append(' ');
                sb.Append(nombreEscala);
            }
            
            return sb.ToString();
        }
        
        // =====================================================================
        // CONVERSIÓN DE GRUPOS (0-999)
        // =====================================================================
        
        /// <summary>
        /// Convierte un número entre 0 y 999 a texto.
        /// </summary>
        /// <param name="n">Número entre 0 y 999</param>
        /// <param name="esUltimo">
        /// true = forma completa (veintiuno, treinta y uno)
        /// false = con apócope (veintiún, treinta y un)
        /// </param>
        private static string ConvertirGrupo(int n, bool esUltimo)
        {
            // Casos base
            if (n == 0) return "";
            if (n < 16) return Especiales0_15[n];
            if (n < 20) return Dieci16_19[n - 16];
            
            // 20-29: casos especiales
            if (n < 30)
            {
                if (n == 20) return "veinte";
                
                int idx = n - 21;
                string forma = Veinti21_29[idx];
                
                // Aplicar apócope solo al 21
                if (n == 21)
                    return esUltimo ? "veintiuno" : "veintiún";
                
                return forma;
            }
            
            // 30-99
            if (n < 100)
            {
                int decena = n / 10;
                int unidad = n % 10;
                
                if (unidad == 0)
                    return Decenas[decena];
                
                string unidadStr = ConvertirUnidad(unidad, esUltimo);
                return Decenas[decena] + " y " + unidadStr;
            }
            
            // 100-999
            int centena = n / 100;
            int resto = n % 100;
            
            if (resto == 0)
            {
                // 100 → "cien", 200 → "doscientos"
                return (n == 100) ? "cien" : Centenas[centena];
            }
            
            // 101-999: "ciento", "doscientos", etc.
            string centenaStr = (centena == 1) ? "ciento" : Centenas[centena];
            return centenaStr + " " + ConvertirGrupo(resto, esUltimo);
        }
        
        // =====================================================================
        // CONVERSIÓN DE UNIDAD CON APÓCOPE
        // =====================================================================
        
        /// <summary>
        /// Convierte una unidad (1-9) aplicando apócope cuando corresponde.
        /// </summary>
        private static string ConvertirUnidad(int u, bool esUltimo)
        {
            if (u == 0) return "";
            if (u == 1) return esUltimo ? "uno" : "un";
            return Unidades[u];
        }
    }

    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>();
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
