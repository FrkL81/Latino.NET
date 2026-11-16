using System;
using System.IO;
using System.Collections.Generic;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class StdLibLoader
{
    // Rastreador de archivos en proceso de carga (evita ciclos A -> B -> A)
    private static readonly HashSet<string> _activeFiles = new();

    public static void LoadAll(Interpreter interpreter)
    {
        // Registrar librerías existentes
        MathLib.Register(interpreter);
        StringLib.Register(interpreter);
        SysLib.Register(interpreter);
        FileLib.Register(interpreter);
        WebLib.Register(interpreter);
        ListLib.Register(interpreter);
        DictLib.Register(interpreter);
        TupleLib.Register(interpreter);
        SetLib.Register(interpreter);
        StackLib.Register(interpreter);
        QueueLib.Register(interpreter);
        ValidationLib.Register(interpreter);

        // --- CONVERSIÓN DE TIPOS ---
        interpreter.Globals.Define("acadena", new NativeFunction((interp, args) => {
            if (args.Count == 0) return new LatinoString("");
            var val = args[0];
            // Manejo explícito para tipos Latino (asegura 'verdadero/falso/nulo')
            if (val is LatinoBoolean b) return new LatinoString(b.Value ? "cierto" : "falso");
            if (val is LatinoNull) return new LatinoString("nulo");
            return new LatinoString(val.ToString());
        }, 1).WithDocs("Convierte un valor a su representación en cadena", "valor"));
        interpreter.Globals.Define("alogico", new NativeFunction((interp, args) => {
            var val = args[0];
            
            // =====================================================================
            // CONVERSIÓN INTELIGENTE A LÓGICO
            // =====================================================================
            
            // 1. Nulo siempre es falso
            if (val is LatinoNull) 
                return new LatinoBoolean(false);
            
            // 2. Booleano conserva su valor
            if (val is LatinoBoolean b) 
                return b;
            
            // 3. Números: 0 y NaN son falsos, resto verdadero
            if (val is LatinoNumber n) 
                return new LatinoBoolean(n.Value != 0 && !double.IsNaN(n.Value));
                        // 4. Cadenas: Solo la cadena vacía se considera falsa
                // (Filosofía Latino.NET: si existe y no está vacía, es verdadero)
                if (val is LatinoString s)
                    return new LatinoBoolean(s.Value.Length > 0);
    
            // 5. Colecciones: objeto existente = verdadero, independiente de si está vacío
            // Una lista vacía [] sigue siendo un objeto lista válido.
            // Para verificar si está vacía, el usuario usa lista.longitud() == 0.
            if (val is LatinoList || val is LatinoDictionary || val is LatinoTuple ||
                val is LatinoSet  || val is LatinoStack      || val is LatinoQueue)
                return new LatinoBoolean(true);
            
            // 6. Por defecto: objetos existentes son verdaderos
            return new LatinoBoolean(true);
            
        }, 1).WithDocs(
            "Convierte cualquier valor a lógico con semántica inteligente. " +
            "Reconoce '0', 'falso', 'no', 'nulo' y cadenas vacías como falso.",
            "valor"
        ));
        interpreter.Globals.Define("anumero", new NativeFunction((interp, args) => {
             var val = args[0];
             if (val is LatinoNumber) return val;
             if (val is LatinoBoolean b) return new LatinoNumber(b.Value ? 1 : 0);
             if (val is LatinoString s)
             {
                 string texto = s.Value.Trim();
                 // Si contiene una única coma y ningún punto, normalizar coma a punto decimal
                 if (texto.Count(c => c == ',') == 1 && !texto.Contains('.'))
                 {
                     texto = texto.Replace(',', '.');
                 }

                 if (double.TryParse(texto, 
                     System.Globalization.NumberStyles.Float, 
                     System.Globalization.CultureInfo.InvariantCulture, 
                     out double d))
                 {
                     return new LatinoNumber(d);
                 }
             }
             return LatinoNull.Instance;
        }, 1).WithDocs("Convierte cadenas y lógicos a número, devuelve nulo si no es posible", "valor"));

        // ═════════════════════════════════════════════════════════════════
        // LANZAR EXCEPCIONES PERSONALIZADAS
        // ═════════════════════════════════════════════════════════════════
        //
        // PROPÓSITO:
        //   Permitir que el código Latino lance errores personalizados para
        //   validaciones de lógica de negocio (ej: fondos insuficientes,
        //   datos inválidos, permisos denegados).
        //
        // USO TÍPICO:
        //   funcion retirar(cuenta, monto)
        //       si monto > cuenta.saldo
        //           lanzar("Fondos insuficientes")
        //       fin
        //   fin
        //
        //   intentar
        //       retirar(miCuenta, 1000)
        //   atrapar (error)
        //       escribir("Error: " .. error)
        //   fin
        //
        // NOTA: RuntimeError lanzado por lanzar() es capturado por bloques
        //       'intentar/atrapar' de la misma forma que errores del sistema.
        // ═════════════════════════════════════════════════════════════════

        interpreter.Globals.Define("lanzar", new NativeFunction((interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "Error desconocido (sin mensaje)");
            
            // Construir mensaje de error desde los argumentos
            string mensaje = string.Join(" ", args.Select(a => a.ToString()));
            
            throw new RuntimeError(null!, mensaje);
        }, -1).WithDocs(
            "Lanza una excepción personalizada con el mensaje especificado. " +
            "Útil para validaciones de lógica de negocio. Se captura con 'intentar/atrapar'.",
            "mensaje"
        ));

        // =====================================================================
        // EVALUACIÓN DINÁMICA (eval)
        // =====================================================================
        
        // evaluar("expresion") -> valor
        var evalFunction = new NativeFunction((interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "evaluar() requiere un argumento: el código a evaluar.");
            
            string codigo = args[0].ToString();
            
            // Usar el nuevo método RunEval que retorna valores
            return interp.RunEval(codigo);
            
        }, 1).WithDocs("Evalúa código Latino dinámicamente y retorna el resultado de la última expresión.", "codigo");
        
        interpreter.Globals.Define("evaluar", evalFunction);
        interpreter.Globals.Define("eval", evalFunction);  // Alias

        // =====================================================================
        // MANIPULACIÓN DE ESTRUCTURAS DE DATOS (SWAP IN-PLACE)
        // =====================================================================
        interpreter.Globals.Define("trocar", new NativeFunction((interp, args) => {
            if (args.Count < 3) throw new RuntimeError(null!, "trocar() requiere 3 argumentos: coleccion, a, b.");

            var col = args[0];
            var a = args[1];
            var b = args[2];

            if (col is LatinoList list) {
                if (a is not LatinoNumber numA || b is not LatinoNumber numB) 
                    throw new RuntimeError(null!, "Para listas, los índices a trocar deben ser numéricos.");
                
                int idxA = (int)numA.Value;
                int idxB = (int)numB.Value;
                
                // Soporte Python-style para índices negativos
                if (idxA < 0) idxA += list.Elements.Count;
                if (idxB < 0) idxB += list.Elements.Count;

                if (idxA < 0 || idxA >= list.Elements.Count || idxB < 0 || idxB >= list.Elements.Count)
                    throw new RuntimeError(null!, "Índices fuera de rango en trocar().");

                var temp = list.Elements[idxA];
                list.Elements[idxA] = list.Elements[idxB];
                list.Elements[idxB] = temp;

                return list;
            } 
            else if (col is LatinoDictionary dict) {
                if (dict.IsReadOnly) throw new RuntimeError(null!, "El diccionario es de solo lectura.");
                
                if (!dict.Pairs.ContainsKey(a) || !dict.Pairs.ContainsKey(b))
                    throw new RuntimeError(null!, "Una o ambas claves no existen en el diccionario.");

                var temp = dict.Pairs[a];
                dict.Pairs[a] = dict.Pairs[b];
                dict.Pairs[b] = temp;

                return dict;
            }

            throw new RuntimeError(null!, "trocar() solo soporta listas y diccionarios.");
        }, 3).WithDocs("Intercambia los valores de dos posiciones/claves dentro de una lista o diccionario.", "coleccion, a, b"));

        // --- FUNCIÓN TIPO ---
        interpreter.Globals.Define("tipo", new NativeFunction((interp, args) => {
            if (args.Count < 1) return new LatinoString("nulo");
            return new LatinoString(args[0].TypeName);
        }, 1).WithDocs("Devuelve el tipo de dato del valor como cadena", "valor"));

        // --- FUNCIÓN CLONAR (DEEP COPY PROTEGIDA CONTRA CICLOS) ---
        interpreter.Globals.Define("clonar", new NativeFunction((interp, args) => {
            if (args.Count == 0) return LatinoNull.Instance;

            // ReferenceEqualityComparer compara direcciones de memoria (identidad del objeto C#),
            // no igualdad semántica. Esto permite detectar cuando un objeto ya fue visitado
            // durante la recursión, cortando los ciclos antes del StackOverflow.
            var visitados = new Dictionary<object, LatinoObject>(ReferenceEqualityComparer.Instance);

            return ClonarObjeto(args[0], visitados);

            LatinoObject ClonarObjeto(LatinoObject original, Dictionary<object, LatinoObject> cache)
            {
                // Tipos primitivos son inmutables — no necesitan clonación ni rastreo
                if (original is LatinoNumber || original is LatinoString ||
                    original is LatinoBoolean || original is LatinoNull)
                    return original;

                // Prevención de ciclos: Si ya empezamos a clonar este objeto, devolver
                // la referencia del clon en construcción (cierra el ciclo correctamente)
                if (cache.TryGetValue(original, out var clonExistente))
                    return clonExistente;

                if (original is LatinoList list)
                {
                    // Registrar en caché ANTES de iterar para manejar auto-referencias
                    var nuevaLista = new LatinoList(new List<LatinoObject>(list.Elements.Count));
                    cache[original] = nuevaLista;

                    foreach (var elem in list.Elements)
                        nuevaLista.Elements.Add(ClonarObjeto(elem, cache));

                    return nuevaLista;
                }

                if (original is LatinoDictionary dict)
                {
                    var nuevoDict = new LatinoDictionary(
                        new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
                    cache[original] = nuevoDict; // Registrar antes de iterar

                    foreach (var kvp in dict.Pairs)
                        nuevoDict.Pairs[ClonarObjeto(kvp.Key, cache)] = ClonarObjeto(kvp.Value, cache);

                    return nuevoDict;
                }

                if (original is LatinoTuple tuple)
                {
                    // Las tuplas son inmutables en estructura pero pueden contener listas mutables.
                    // Construimos el wrapper primero para poder registrarlo en caché antes de iterar.
                    var tempElements = new List<LatinoObject>(tuple.Elements.Count);
                    var nuevaTupla = new LatinoTuple(tempElements);
                    cache[original] = nuevaTupla;

                    foreach (var elem in tuple.Elements)
                        tempElements.Add(ClonarObjeto(elem, cache));

                    return nuevaTupla;
                }

                // Funciones, clases e instancias: shallow copy (no es seguro clonar closures)
                return original;
            }
        }, 1).WithDocs("Realiza una copia profunda (Deep Copy) segura contra ciclos de listas, diccionarios y tuplas.", "objeto"));
        
        // --- FUNCIÓN Ç (COMPOSITOR) ---
        interpreter.Globals.Define("ç", new NativeFunction(StringLib.Incrustar, 1)
            .WithDocs("El Compositor: Incrusta variables en texto (Atajo: cc).", "plantilla"));

        // --- FUNCIONES DE SALIDA CON SOPORTE EMOJI ---
        var escribirFn = new NativeFunction((interp, args) => {
            string texto = SysLib.EmojiHelper.NormalizarArgumentos(args);
            interp.Out.WriteLine(texto);
            return LatinoNull.Instance;
        }, -1).WithDocs("Escribe texto en la consola con salto de línea.", "texto...");

        interpreter.Globals.Define("escribir", escribirFn);
        interpreter.Globals.Define("imprimir", escribirFn);
        interpreter.Globals.Define("poner", escribirFn);

        // =============================================================================
        // SISTEMA DE FORMATEO TRIPLE
        // =============================================================================

        // 1. FORMATO NATIVO .NET (escribirf/ponerf)
        var escribirfFn = new NativeFunction((interp, args) => {
            if (args.Count < 1) return LatinoNull.Instance;
            string fmt = args[0].ToString();
            var restArgs = args.GetRange(1, args.Count - 1);
            
            string output = StringLib.FormatNative(fmt, restArgs);
            output = SysLib.EmojiHelper.Normalizar(output); // SOPORTE EMOJI
            
            interp.Out.Write(output);
            return LatinoNull.Instance;
        }, -1).WithDocs(
            "Escribe con formato .NET ({0}, {1:C}, {2:X}). Sin salto de línea.", 
            "formato, valores..."
        );

        interpreter.Globals.Define("escribirf", escribirfFn);
        interpreter.Globals.Define("ponerf", escribirfFn);

        // 2. FORMATO C-STYLE TRANSPILADO (imprimirf)
        interpreter.Globals.Define("imprimirf", new NativeFunction((interp, args) => {
            if (args.Count < 1) return LatinoNull.Instance;
            string fmt = args[0].ToString();
            var restArgs = args.GetRange(1, args.Count - 1);
            
            string output = StringLib.FormatCStyle(fmt, restArgs);
            output = SysLib.EmojiHelper.Normalizar(output); // SOPORTE EMOJI
            
            interp.Out.Write(output);
            return LatinoNull.Instance;
        }, -1).WithDocs(
            "Escribe con formato C (%s, %.2f, %x). Sin salto de línea.", 
            "formato, valores..."
        ));

        // --- FUNCIÓN DE LECTURA CON PROMPT OPCIONAL ---
        // leer() -> Espera input
        // leer("Nombre: ") -> Imprime prompt y espera input
        var leerFn = new NativeFunction((interp, args) => {
            if (args.Count > 0)
            {
                interp.Out.Write(args[0].ToString()); // Prompt
            }
            
            string? line = interp.In.ReadLine();
            
            return line != null ? new LatinoString(line) : LatinoNull.Instance;
        }, -1); // Variadic (-1) para aceptar 0 o 1 argumento

        // DOCUMENTACIÓN PARA INTELLISENSE
        leerFn.WithDocs(
            "Lee una línea de texto desde la consola. Detiene la ejecución hasta que el usuario presione Enter.", 
            "mensaje_opcional"
        );

        interpreter.Globals.Define("leer", leerFn);
        interpreter.Globals.Define("pedir", leerFn); // El alias hereda la doc porque es el mismo objeto

        // =============================================================================
        // FUNCIÓN LIMPIAR CONSOLA
        // =============================================================================
        
        interpreter.Globals.Define("limpiar", new NativeFunction((interp, args) => {
            // Determinar modo de limpieza
            bool resetearCursor = true;
            bool intentarScrollback = false;
            
            // Si hay argumento, procesarlo
            if (args.Count > 0)
            {
                string modo = args[0].ToString().ToLower().Trim();
                
                switch (modo)
                {
                    case "pantalla":
                    case "visible":
                        // Solo limpia lo visible (comportamiento default de Console.Clear)
                        resetearCursor = true;
                        intentarScrollback = false;
                        break;
                        
                    case "todo":
                    case "completo":
                    case "scrollback":
                        // Intenta limpiar también el historial de scrollback
                        resetearCursor = true;
                        intentarScrollback = true;
                        break;
                        
                    case "cursor":
                        // Solo resetea cursor sin limpiar
                        try { Console.SetCursorPosition(0, 0); } catch { }
                        return LatinoNull.Instance;
                        
                    default:
                        throw new RuntimeError(null!, 
                            $"Modo de limpieza desconocido: '{modo}'. " +
                            "Valores válidos: 'pantalla' (default), 'todo', 'cursor'");
                }
            }
            
            try
            {
                // Método 1: Limpieza estándar con Console.Clear()
                Console.Clear();
                
                // Método 2: Si se solicita limpiar scrollback, usar secuencias ANSI
                if (intentarScrollback)
                {
                    // Secuencia ANSI que limpia pantalla + scrollback en terminales modernas
                    // \x1b[3J = Limpia scrollback buffer (VT100)
                    // \x1b[2J = Limpia pantalla
                    // \x1b[H  = Mueve cursor a home (0,0)
                    Console.Write("\x1b[3J\x1b[2J\x1b[H");
                }
                
                // Método 3: Asegurar que cursor está en (0,0)
                if (resetearCursor)
                {
                    try { Console.SetCursorPosition(0, 0); } catch { }
                }
            }
            catch
            {
                // Si falla (no hay consola, entorno no interactivo, etc.)
                // Intentar al menos imprimir saltos de línea para "simular" limpieza
                try
                {
                    for (int i = 0; i < 50; i++)
                        interp.Out.WriteLine();
                }
                catch { /* Silenciosamente ignorar si ni siquiera esto funciona */ }
            }
            
            return LatinoNull.Instance;
            
        }, -1).WithDocs(
            "Limpia la pantalla de la consola. Modos: 'pantalla' (default), 'todo' (incluye scrollback), 'cursor' (solo resetea posición).",
            "[modo]"
        ));

        // --- FUNCIÓN RANGO ---
        // rango(fin) -> [0, 1, ... fin-1]
        // rango(inicio, fin) -> [inicio, ... fin-1]
        // rango(inicio, fin, paso)
        interpreter.Globals.Define("rango", new NativeFunction((interp, args) => {
            // ========================================================================
            // VALIDACIÓN DE ARIDAD
            // ========================================================================
            if (args.Count == 0 || args.Count > 3)
                throw new RuntimeError(null!, "rango() requiere 1, 2 o 3 argumentos.");

            // ========================================================================
            // MODO 1: DESCOMPOSICIÓN DE CADENA → Lista de Caracteres
            // ========================================================================
            if (args[0] is LatinoString str)
            {
                // Validación: Solo acepta 1 argumento cuando es cadena
                if (args.Count > 1)
                    throw new RuntimeError(null!, 
                        "rango(cadena) no acepta argumentos adicionales. " +
                        "Uso: rango(\"texto\") devuelve lista de caracteres.");

                // Procesamiento seguro de grafemas Unicode
                var si = new System.Globalization.StringInfo(str.Value);
                var charList = new List<LatinoObject>(si.LengthInTextElements);
                
                for (int i = 0; i < si.LengthInTextElements; i++)
                {
                    charList.Add(new LatinoString(si.SubstringByTextElements(i, 1)));
                }
                
                return new LatinoList(charList);
            }

            // ========================================================================
            // MODO 2: GENERACIÓN NUMÉRICA (Lógica Original Preservada)
            // ========================================================================
            double inicio = 0;
            double fin;
            double paso = 1;

            // DESPUÉS (pattern matching explícito — consistente con el resto de la librería)
            if (args[0] is not LatinoNumber n0)
                throw new RuntimeError(null!,
                    "rango() espera argumentos numéricos. " +
                    "Uso: rango(fin) | rango(inicio, fin) | rango(inicio, fin, paso) | rango(\"cadena\")");

            fin = n0.Value;

            if (args.Count >= 2)
            {
                if (args[1] is not LatinoNumber n1)
                    throw new RuntimeError(null!, "El segundo argumento de rango() debe ser un número.");
                inicio = fin; // El primer arg era inicio en realidad
                fin = n1.Value;
            }

            if (args.Count == 3)
            {
                if (args[2] is not LatinoNumber n2)
                    throw new RuntimeError(null!, "El paso de rango() debe ser un número.");
                paso = n2.Value;
            }

            // Validación de paso
            if (paso == 0) 
                throw new RuntimeError(null!, "El paso de rango() no puede ser 0.");

            // Optimización de capacidad
            int capacidad = (int)Math.Ceiling(Math.Abs((fin - inicio) / paso));
            var lista = new List<LatinoObject>(Math.Max(0, capacidad));
            
            // Generación del rango
            if (paso > 0)
            {
                for (double i = inicio; i < fin; i += paso) 
                    lista.Add(new LatinoNumber(i));
            }
            else
            {
                for (double i = inicio; i > fin; i += paso) 
                    lista.Add(new LatinoNumber(i));
            }

            return new LatinoList(lista);
            
        }, -1).WithDocs(
            "Genera lista de números o desglosa cadena en caracteres (soporta emojis).",
            "inicio, [fin], [paso]"
        ));

        // --- COMANDO INCLUIR (INTELIGENTE) ---
        interpreter.Globals.Define("incluir", new NativeFunction((interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "incluir() requiere el nombre del módulo o librería a cargar.");

            string rawName = args[0].ToString();
            string? finalPath = null;
            bool isDll = false;

            // Lista de candidatos a probar (en orden de prioridad)
            var candidates = new List<string>();
            
            // 1. Si tiene extensión explícita, probar esa primero
            if (Path.HasExtension(rawName))
            {
                candidates.Add(rawName);
            }
            else
            {
                // 2. Si no tiene, probar .lat y luego .dll
                candidates.Add(rawName + ".lat");
                candidates.Add(rawName + ".dll");
            }

            // Rutas base donde buscar
            var searchPaths = new List<string>
            {
                Directory.GetCurrentDirectory(), // 1. Carpeta actual
                AppDomain.CurrentDomain.BaseDirectory, // 2. Carpeta del ejecutable (para libs globales)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib") // 3. Subcarpeta lib/ (opcional, buena práctica)
            };

            // Algoritmo de Búsqueda
            foreach (var basePath in searchPaths)
            {
                foreach (var candidate in candidates)
                {
                    string fullPath = Path.Combine(basePath, candidate);
                    if (File.Exists(fullPath))
                    {
                        finalPath = fullPath;
                        if (candidate.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) isDll = true;
                        goto Found; // Salir de ambos bucles
                    }
                }
            }

            Found:

            if (finalPath == null)
            {
                throw new RuntimeError(null!, $"No se encontró el módulo o librería '{rawName}'. Se buscaron extensiones .lat/.dll en rutas locales y del sistema.");
            }

            // --- LÓGICA DE CARGA (Existente, adaptada) ---
            
            if (isDll)
            {
                PluginLoader.Load(interp, finalPath);
            }
            else
            {
                // Protección de ciclos
                if (_activeFiles.Contains(finalPath))
                    throw new RuntimeError(null!, $"Dependencia circular detectada: '{Path.GetFileName(finalPath)}' ya se está cargando.");

                try
                {
                    _activeFiles.Add(finalPath);
                    string source = File.ReadAllText(finalPath);
                    var parser = new Parser(new Lexer(source).Tokenize());
                    var statements = parser.Parse();
                    if (statements != null) interp.Interpret(statements);
                }
                finally
                {
                    _activeFiles.Remove(finalPath);
                }
            }

            return LatinoNull.Instance;
        }).WithDocs("Carga un script o plugin. Busca .lat y .dll automáticamente.", "nombre_modulo"));
    }
}