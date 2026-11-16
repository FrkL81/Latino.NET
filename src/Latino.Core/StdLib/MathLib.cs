using System;
using System.Collections.Generic;
using System.Linq;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class MathLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();

        // Helper para definir funciones con metadatos
        void Def(string name, Func<Interpreter, List<LatinoObject>, LatinoObject> logic, int arity, string? desc = null, string? pams = null) 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc!, pams ?? "");
            methods[name] = func;
        }

        // Helper para obtener double seguro
        double Get(LatinoObject obj)
        {
            if (obj is LatinoNumber n) return n.Value;
            throw new RuntimeError(null!, $"Se esperaba un número, se recibió: {obj.GetType().Name}");
        }

        // Helper para extraer la colección de números (sea de args o de una lista)
        IEnumerable<double> ExtractNumbers(List<LatinoObject> args)
        {
            // Caso 1: Una sola lista como argumento
            if (args.Count == 1 && args[0] is LatinoList list)
            {
                if (list.Elements.Count == 0) 
                    throw new RuntimeError(null!, "La lista está vacía.");
                return list.Elements.Select(Get);
            }
            
            // Caso 2: Argumentos sueltos
            if (args.Count == 0) 
                throw new RuntimeError(null!, "Se requiere al menos un argumento.");
            return args.Select(Get);
        }

        // Helper privado: Algoritmo de Euclides
        long CalcularMCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        // --- CONSTANTES ---
        var constMap = new Dictionary<string, LatinoObject> {
            { "pi", new LatinoNumber(Math.PI) },
            { "e", new LatinoNumber(Math.E) },
            { "tau", new LatinoNumber(Math.Tau) }
        };

        // --- ARITMÉTICA BÁSICA ---
        Def("abs", (i, args) => new LatinoNumber(Math.Abs(Get(args[0]))), 1, "Valor absoluto de un número.", "numero");
        Def("raiz", (i, args) => {
            double res = Math.Sqrt(Get(args[0]));
            return new LatinoNumber(PolinomioSolver.CorregirErrorNumerico(res));
        }, 1, "Raíz cuadrada.", "numero");

        Def("raizc", (i, args) => {
            double res = Math.Cbrt(Get(args[0]));
            return new LatinoNumber(PolinomioSolver.CorregirErrorNumerico(res));
        }, 1, "Raíz cúbica.", "numero");

        Def("pot", (i, args) => new LatinoNumber(Math.Pow(Get(args[0]), Get(args[1]))), 2, "Eleva una base a un exponente.", "base, exponente");

        Def("exp", (i, args) => {
            double x = Get(args[0]);
            double res = Math.Exp(x);
            // Identidad: exp(0) = 1
            if (Math.Abs(x) < 1e-12) res = 1.0;
            return new LatinoNumber(res);
        }, 1, "Exponencial (e^x).", "numero");
        
        Def("max", (i, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "mate.max() requiere al menos un número o una lista de números.");
            return new LatinoNumber(ExtractNumbers(args).Max());
        }, -1, "Devuelve el valor máximo de una lista de números o argumentos sueltos.", "lista | numeros...");
            
        Def("min", (i, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "mate.min() requiere al menos un número o una lista de números.");
            return new LatinoNumber(ExtractNumbers(args).Min());
        }, -1, "Devuelve el valor mínimo de una lista de números o argumentos sueltos.", "lista | numeros...");

        // --- REDONDEO ---
        Def("piso", (i, args) => new LatinoNumber(Math.Floor(Get(args[0]))), 1, "Redondea hacia abajo al entero más cercano.", "numero");
        Def("techo", (i, args) => new LatinoNumber(Math.Ceiling(Get(args[0]))), 1, "Redondea hacia arriba al entero más cercano.", "numero");
        Def("trunc", (i, args) => new LatinoNumber(Math.Truncate(Get(args[0]))), 1, "Trunca la parte decimal.", "numero");
        
        Def("redondear", (i, args) => {
            if (args.Count == 0) throw new RuntimeError(null!, "mate.redondear() requiere al menos un número.");
            double valor = Get(args[0]);
            int decimales = 0;
            
            if (args.Count > 1) {
                decimales = (int)Get(args[1]);
                if (decimales < 0 || decimales > 15) 
                    throw new RuntimeError(null!, "La cantidad de decimales debe estar entre 0 y 15.");
            }
            
            // Usamos AwayFromZero para que 2.5 redondee a 3, y -2.5 a -3 (Redondeo matemático tradicional)
            return new LatinoNumber(Math.Round(valor, decimales, MidpointRounding.AwayFromZero));
        }, -1, "Redondea al entero más cercano o a los decimales especificados.", "numero, [decimales]");
        
        Def("rnd", (i, args) => methods["redondear"].Call(i, args), -1, "Alias de redondear.", "numero, [decimales]");

        // --- TRIGONOMETRÍA ---
        Def("sen", (i, args) => {
            double res = Math.Sin(Get(args[0]));
            return new LatinoNumber(PolinomioSolver.CorregirErrorNumerico(res, 1e-12));
        }, 1, "Seno de un ángulo (radianes).", "rad");

        Def("cos", (i, args) => {
            double res = Math.Cos(Get(args[0]));
            return new LatinoNumber(PolinomioSolver.CorregirErrorNumerico(res, 1e-12));
        }, 1, "Coseno de un ángulo (radianes).", "rad");

        Def("tan", (i, args) => {
            double res = Math.Tan(Get(args[0]));
            return new LatinoNumber(PolinomioSolver.CorregirErrorNumerico(res, 1e-12));
        }, 1, "Tangente de un ángulo (radianes).", "rad");
        
        Def("asen", (i, args) => new LatinoNumber(Math.Asin(Get(args[0]))), 1, "Arcoseno.", "num");
        Def("asin", (i, args) => new LatinoNumber(Math.Asin(Get(args[0]))), 1, "Alias de asen.", "num");
        Def("acos", (i, args) => new LatinoNumber(Math.Acos(Get(args[0]))), 1, "Arcocoseno.", "num");
        Def("arco", (i, args) => new LatinoNumber(Math.Acos(Get(args[0]))), 1, "Alias de acos.", "num");
        Def("atan", (i, args) => new LatinoNumber(Math.Atan(Get(args[0]))), 1, "Arcotangente.", "num");
        Def("atan2", (i, args) => new LatinoNumber(Math.Atan2(Get(args[0]), Get(args[1]))), 2, "Devuelve el ángulo (radianes) entre el eje X positivo y el punto (y, x).", "y, x");

        // --- HIPERBÓLICAS ---
        Def("senh", (i, args) => new LatinoNumber(Math.Sinh(Get(args[0]))), 1, "Seno hiperbólico.", "num");
        Def("cosh", (i, args) => new LatinoNumber(Math.Cosh(Get(args[0]))), 1, "Coseno hiperbólico.", "num");
        Def("tanh", (i, args) => new LatinoNumber(Math.Tanh(Get(args[0]))), 1, "Tangente hiperbólica.", "num");
        Def("asenh", (i, args) => new LatinoNumber(Math.Asinh(Get(args[0]))), 1, "Arcoseno hiperbólico.", "num");
        Def("acosh", (i, args) => new LatinoNumber(Math.Acosh(Get(args[0]))), 1, "Arcocoseno hiperbólico.", "num");
        Def("atanh", (i, args) => new LatinoNumber(Math.Atanh(Get(args[0]))), 1, "Arcotangente hiperbólica.", "num");

        // --- LOGARITMOS ---
        Def("log", (i, args) => {
            double x = Get(args[0]);
            double res = Math.Log(x);
            // Identidades: log(e)=1, log(1)=0
            if (Math.Abs(x - Math.E) < 1e-12) res = 1.0;
            else if (Math.Abs(x - 1.0) < 1e-12) res = 0.0;
            return new LatinoNumber(res);
        }, 1, "Logaritmo natural.", "numero");

        Def("log10", (i, args) => {
            double x = Get(args[0]);
            double res = Math.Log10(x);
            // Identidades: log10(10)=1, log10(1)=0
            if (Math.Abs(x - 10.0) < 1e-12) res = 1.0;
            else if (Math.Abs(x - 1.0) < 1e-12) res = 0.0;
            return new LatinoNumber(res);
        }, 1, "Logaritmo base 10.", "numero");

        // --- MANIPULACIÓN BITS/FLOTANTE ---
        Def("ldexp", (i, args) => new LatinoNumber(Get(args[0]) * Math.Pow(2, Get(args[1]))), 2, "Carga exponente: x * 2^exp.", "x, exp");
        
        Def("frexp", (i, args) => {
            double d = Get(args[0]);
            int exp = (d == 0) ? 0 : (int)Math.Floor(Math.Log(Math.Abs(d), 2)) + 1;
            double mantissa = (d == 0) ? 0 : d * Math.Pow(2, -exp);
            return new LatinoList(new List<LatinoObject> { new LatinoNumber(mantissa), new LatinoNumber(exp) });
        }, 1, "Descompone en [mantisa, exponente].", "num");

        // --- PORCENTAJES ---
        // porc(parte, total) -> porcentaje (ej: 20, 100 -> 0.2)
        Def("porc", (i, args) => new LatinoNumber(Get(args[0]) / Get(args[1])), 2, "Obtiene la proporción decimal.", "parte, total");
        Def("porcentaje", (i, args) => new LatinoNumber(Get(args[0]) / Get(args[1])), 2, "Alias de porc.", "parte, total");
        
        // parte(total, porc) -> valor (ej: 200, 50 -> 100)
        Def("parte", (i, args) => new LatinoNumber(Get(args[0]) * (Get(args[1]) / 100.0)), 2, "Calcula la parte de un total dado un % entero.", "total, porc_entero");
        
        // base(parte, porc) -> total (ej: 50, 10 -> 500)
        Def("base", (i, args) => new LatinoNumber((Get(args[0]) * 100.0) / Get(args[1])), 2, "Calcula el total dado una parte y su %.", "parte, porc_entero");


        // --- ALEATORIO ---
        Def("aleatorio", (i, args) => {
            var r = Random.Shared;
            // 0 args: Rango[0.0, 1.0)
            if (args.Count == 0) return new LatinoNumber(r.NextDouble());
            
            // 1 arg: Rango [0, Max] inclusivo
            if (args.Count == 1) return new LatinoNumber(r.Next(0, (int)Get(args[0]) + 1));
            
            // 2 args: Rango [Min, Max] inclusivo
            return new LatinoNumber(r.Next((int)Get(args[0]), (int)Get(args[1]) + 1));
        }, -1, "Genera número aleatorio. 0 args: 0.0-1.0; 1 arg: 0-Max (Inclusivo); 2 args: Min-Max (Inclusivo).", "min?, max?");
        
        Def("alt", (i, args) => methods["aleatorio"].Call(i, args), -1, "Alias de aleatorio.", "min?, max?");

        // =====================================================================
        // FUNCIONES MATEMÁTICAS AVANZADAS
        // =====================================================================
        
        // -------------------------
        // 1. mate.limitar (Clamp)
        // -------------------------
        Def("limitar", (interp, args) => {
            if (args.Count != 3)
                throw new RuntimeError(null!, "mate.limitar() requiere 3 argumentos: valor, mínimo, máximo.");
            
            double valor = Get(args[0]);
            double min = Get(args[1]);
            double max = Get(args[2]);
            
            // Validación: min debe ser menor o igual a max
            if (min > max)
                throw new RuntimeError(null!, 
                    $"mate.limitar(): El mínimo ({min}) no puede ser mayor que el máximo ({max}).");
            
            // Clamp: limitar el valor al rango [min, max]
            double resultado = Math.Max(min, Math.Min(max, valor));
            
            return new LatinoNumber(resultado);
            
        }, 3, "Limita un valor al rango [mínimo, máximo] (clamp).", "valor, minimo, maximo");

        // -------------------------
        // mate.signo(numero) -> -1, 0, o 1
        // -------------------------
        Def("signo", (interp, args) => {
            if (args.Count != 1)
                throw new RuntimeError(null!, "mate.signo() requiere 1 argumento: número.");
            
            double valor = Get(args[0]);
            
            if (valor > 0) return new LatinoNumber(1);
            if (valor < 0) return new LatinoNumber(-1);
            return new LatinoNumber(0);
            
        }, 1, "Retorna el signo de un número: -1 (negativo), 0 (cero), 1 (positivo).", "numero");

        
        
        // -------------------------
        // 2. mate.interpolar (Lerp)
        // -------------------------
        Def("interpolar", (interp, args) => {
            if (args.Count != 3)
                throw new RuntimeError(null!, "mate.interpolar() requiere 3 argumentos: inicio, fin, factor.");
            
            double inicio = Get(args[0]);
            double fin = Get(args[1]);
            double factor = Get(args[2]);
            
            // Lerp: inicio + (fin - inicio) * factor
            // factor = 0 → inicio
            // factor = 1 → fin
            // factor = 0.5 → punto medio
            double resultado = inicio + (fin - inicio) * factor;
            
            return new LatinoNumber(resultado);
            
        }, 3, "Interpolación lineal entre dos valores. Factor 0 = inicio, 1 = fin.", "inicio, fin, factor");
        
        
        // -------------------------
        // 3. mate.factorial
        // -------------------------
        Def("factorial", (interp, args) => {
            if (args.Count != 1)
                throw new RuntimeError(null!, "mate.factorial() requiere 1 argumento: número entero positivo.");
            
            double valor = Get(args[0]);
            
            // Validación: debe ser entero no negativo
            if (valor < 0)
                throw new RuntimeError(null!, 
                    $"mate.factorial(): No se puede calcular factorial de número negativo ({valor}).");
            
            if (Math.Abs(valor % 1) > double.Epsilon)
                throw new RuntimeError(null!, 
                    $"mate.factorial(): El argumento debe ser un número entero, no {valor}.");
            
            int n = (int)valor;
            
            // Límite de seguridad (171! causa overflow en double)
            if (n > 170)
                throw new RuntimeError(null!, 
                    $"mate.factorial(): El valor {n} es demasiado grande (máximo: 170).");
            
            // Cálculo del factorial
            double resultado = 1;
            for (int i = 2; i <= n; i++)
            {
                resultado *= i;
            }
            
            return new LatinoNumber(resultado);
            
        }, 1, "Calcula el factorial de un número entero (n!). Máximo: 170.", "numero");
        
        // ═════════════════════════════════════════════════════════════════
        // ÉPSILON DE MÁQUINA
        // ═════════════════════════════════════════════════════════════════
        Def("epsilonMaquina", (interp, args) => {
            // Algoritmo: Encontrar el menor ε tal que 1.0 + ε > 1.0
            double epsilon = 1.0;
            
            while ((1.0 + epsilon / 2.0) > 1.0)
            {
                epsilon /= 2.0;
            }
            
            return new LatinoNumber(epsilon);
            
        }, 0, 
        "Calcula el épsilon de máquina (la menor diferencia representable en punto flotante). " +
        "Es el número positivo más pequeño ε tal que 1.0 + ε ≠ 1.0 en aritmética de la máquina.", 
        "");
        
        
        // -------------------------
        // 4. mate.pitagorica
        // -------------------------
        Def("pitagorica", (interp, args) => {
            if (args.Count != 3)
                throw new RuntimeError(null!, 
                    "mate.pitagorica() requiere 3 argumentos: cateto_a, cateto_b, hipotenusa_c. " +
                    "Usa string para el lado a calcular (ej: 'a', 'b', 'c' o 'h').");
            
            // Identificar cuál parámetro es el string (incógnita)
            int incognitaPos = -1;
            string incognitaNombre = "";
            double[] valores = new double[3];
            int numerosEncontrados = 0;
            
            for (int i = 0; i < 3; i++)
            {
                if (args[i] is LatinoString str)
                {
                    if (incognitaPos != -1)
                        throw new RuntimeError(null!, 
                            "mate.pitagorica(): Solo puede haber UN parámetro string (el lado a calcular). " +
                            "Los otros dos deben ser números.");
                    
                    incognitaPos = i;
                    incognitaNombre = str.Value.ToLower().Trim();
                    
                    // Validar nombre de incógnita
                    if (incognitaNombre != "a" && incognitaNombre != "b" && 
                        incognitaNombre != "c" && incognitaNombre != "h")
                    {
                        throw new RuntimeError(null!, 
                            $"mate.pitagorica(): Nombre de incógnita inválido '{incognitaNombre}'. " +
                            "Usa 'a' (cateto), 'b' (cateto), 'c' o 'h' (hipotenusa).");
                    }
                }
                else
                {
                    try
                    {
                        valores[i] = Get(args[i]);
                        numerosEncontrados++;
                        
                        // Validar que sea positivo
                        if (valores[i] <= 0)
                            throw new RuntimeError(null!, 
                                $"mate.pitagorica(): Los lados deben ser números positivos. Recibido: {valores[i]}");
                    }
                    catch (Exception ex) when (!(ex is RuntimeError))
                    {
                        throw new RuntimeError(null!, 
                            $"mate.pitagorica(): El argumento en posición {i + 1} debe ser un número o string.");
                    }
                }
            }
            
            // Validación final
            if (incognitaPos == -1)
                throw new RuntimeError(null!, 
                    "mate.pitagorica(): Debes especificar qué lado calcular usando un string ('a', 'b', 'c' o 'h').");
            
            if (numerosEncontrados != 2)
                throw new RuntimeError(null!, 
                    "mate.pitagorica(): Se requieren exactamente 2 valores numéricos y 1 string.");
            
            // Normalizar nombre de incógnita (c y h son lo mismo)
            if (incognitaNombre == "h") incognitaNombre = "c";
            
            // Calcular según la posición de la incógnita
            double resultado;
            string claveResultado = incognitaNombre;
            
            if (incognitaPos == 2) // Calcular hipotenusa c
            {
                // c² = a² + b²
                double a = valores[0];
                double b = valores[1];
                resultado = Math.Sqrt(a * a + b * b);
                claveResultado = "c";
            }
            else if (incognitaPos == 0) // Calcular cateto a
            {
                // a² = c² - b²
                double b = valores[1];
                double c = valores[2];
                
                if (c <= b)
                    throw new RuntimeError(null!, 
                        $"mate.pitagorica(): La hipotenusa ({c}) debe ser mayor que el cateto ({b}).");
                
                resultado = Math.Sqrt(c * c - b * b);
            }
            else // incognitaPos == 1, Calcular cateto b
            {
                // b² = c² - a²
                double a = valores[0];
                double c = valores[2];
                
                if (c <= a)
                    throw new RuntimeError(null!, 
                        $"mate.pitagorica(): La hipotenusa ({c}) debe ser mayor que el cateto ({a}).");
                
                resultado = Math.Sqrt(c * c - a * a);
            }
            
            // Retornar diccionario con el resultado
            var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
            {
                { new LatinoString(claveResultado), new LatinoNumber(resultado) }
            };
            
            return new LatinoDictionary(dict);
            
        }, 3, 
        "Calcula cualquier lado de un triángulo rectángulo (Teorema de Pitágoras). " +
        "Usa string para el lado a calcular: 'a', 'b' (catetos), 'c' o 'h' (hipotenusa).", 
        "cateto_a, cateto_b, hipotenusa_c");

        // -------------------------
        // mate.compuesto - Fórmula Universal de Crecimiento/Decrecimiento
        // -------------------------
        Def("compuesto", (interp, args) => {
            if (args.Count < 3 || args.Count > 4)
                throw new RuntimeError(null!, 
                    "mate.compuesto() requiere 3 o 4 argumentos: valor_inicial, tasa, periodos, [modo].");
            
            double valorInicial = Get(args[0]);
            double tasa = Get(args[1]);
            double periodos = Get(args[2]);
            
            // Modo opcional: "simple" (default) o "continuo"
            string modo = args.Count == 4 ? args[3].ToString().ToLower() : "simple";
            
            double resultado;
            
            switch (modo)
            {
                case "simple":
                case "discreto":
                    // Fórmula: V = P × (1 + r)^n
                    resultado = valorInicial * Math.Pow(1 + tasa, periodos);
                    break;
                
                case "continuo":
                case "euler":
                    // Fórmula: V = P × e^(r×n)
                    // Útil para procesos naturales continuos
                    resultado = valorInicial * Math.Exp(tasa * periodos);
                    break;
                
                default:
                    throw new RuntimeError(null!, 
                        $"Modo '{modo}' no válido. Usa: 'simple' (default) o 'continuo'.");
            }
            
            return new LatinoNumber(resultado);
            
        }, -1, // Aridad variable (3 o 4 args)
        "Calcula crecimiento/decrecimiento compuesto: V = P×(1+r)^n. " +
        "Modos: 'simple' (discreto, default) o 'continuo' (e^(r×n)).", 
        "valor_inicial, tasa, periodos, [modo]");

        // =====================================================================
        // TEORÍA DE NÚMEROS
        // =====================================================================

        Def("esPrimo", (interp, args) => {
            double val = Get(args[0]);
            
            if (Math.Abs(val % 1) > double.Epsilon)
                throw new RuntimeError(null!, 
                    $"mate.esPrimo() requiere un número entero, no {val}");
            
            if (val < 2)
                return new LatinoBoolean(false);
            
            if (val > 1e12)
                throw new RuntimeError(null!, 
                    "mate.esPrimo() soporta números hasta 10^12 (1 billón)");
            
            long n = (long)val;
            
            if (n == 2 || n == 3) return new LatinoBoolean(true);
            if (n % 2 == 0 || n % 3 == 0) return new LatinoBoolean(false);
            
            long limite = (long)Math.Sqrt(n);
            for (long i = 5; i <= limite; i += 6)
            {
                if (n % i == 0 || n % (i + 2) == 0)
                    return new LatinoBoolean(false);
            }
            
            return new LatinoBoolean(true);
        }, 1, "Retorna verdadero si el número es primo. Máximo: 10^12", "numero");

        Def("mcd", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "mate.mcd() requiere al menos dos números o una lista.");
            var numeros = ExtractNumbers(args).Select(n => {
                if (Math.Abs(n % 1) > double.Epsilon)
                    throw new RuntimeError(null!, 
                        $"mate.mcd() requiere enteros, no {n}");
                return (long)n;
            }).ToList();
            
            if (numeros.Count < 2)
                throw new RuntimeError(null!, 
                    "mate.mcd() requiere al menos 2 números.");
            
            long resultado = numeros[0];
            for (int i = 1; i < numeros.Count; i++)
            {
                resultado = CalcularMCD(resultado, numeros[i]);
                if (resultado == 1) break;
            }
            
            return new LatinoNumber(resultado);
        }, -1, "Calcula el Máximo Común Divisor. Acepta números o lista.", "a, b, ... | lista");

        Def("mcm", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "mate.mcm() requiere al menos dos números o una lista.");
            var numeros = ExtractNumbers(args).Select(n => {
                if (Math.Abs(n % 1) > double.Epsilon)
                    throw new RuntimeError(null!, 
                        $"mate.mcm() requiere enteros, no {n}");
                return (long)n;
            }).ToList();
            
            if (numeros.Count < 2)
                throw new RuntimeError(null!, 
                    "mate.mcm() requiere al menos 2 números.");
            
            long resultado = numeros[0];
            
            try
            {
                for (int i = 1; i < numeros.Count; i++)
                {
                    long mcdValue = CalcularMCD(resultado, numeros[i]);
                    checked
                    {
                        resultado = Math.Abs(resultado * numeros[i]) / mcdValue;
                    }
                }
            }
            catch (OverflowException)
            {
                throw new RuntimeError(null!, 
                    "Desbordamiento numérico en mate.mcm(). Los números son demasiado grandes.");
            }
            
            return new LatinoNumber(resultado);
        }, -1, "Calcula el Mínimo Común Múltiplo. Acepta números o lista.", "a, b, ... | lista");

        // =====================================================================
        // ESTADÍSTICA BÁSICA
        // =====================================================================

        Def("mediana", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "mate.mediana() requiere una lista de números.");
            if (args[0] is not LatinoList list)
                throw new RuntimeError(null!, 
                    "mate.mediana() requiere una lista como argumento.");
            
            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, 
                    "No se puede calcular la mediana de una lista vacía.");
            
            var numeros = new List<double>();
            for (int i = 0; i < list.Elements.Count; i++)
            {
                if (list.Elements[i] is not LatinoNumber num)
                    throw new RuntimeError(null!, 
                        $"El elemento en la posición {i} no es un número.");
                numeros.Add(num.Value);
            }
            
            numeros.Sort();
            
            int nCount = numeros.Count;
            int mitad = nCount / 2;
            
            if (nCount % 2 == 1)
            {
                return new LatinoNumber(numeros[mitad]);
            }
            else
            {
                return new LatinoNumber((numeros[mitad - 1] + numeros[mitad]) / 2.0);
            }
        }, -1, "Calcula la mediana de una lista numérica (valor central).", "lista");

        Def("moda", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "mate.moda() requiere una lista de números.");
            if (args[0] is not LatinoList list)
                throw new RuntimeError(null!, 
                    "mate.moda() requiere una lista como argumento.");
            
            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, 
                    "No se puede calcular la moda de una lista vacía.");
            
            var frecuencias = new Dictionary<double, int>();
            
            foreach (var elem in list.Elements)
            {
                if (elem is not LatinoNumber num)
                    throw new RuntimeError(null!, 
                        "mate.moda() solo acepta listas de números.");
                
                if (!frecuencias.ContainsKey(num.Value))
                    frecuencias[num.Value] = 0;
                
                frecuencias[num.Value]++;
            }
            
            double modaValue = 0;
            int maxFrecuencia = 0;
            
            foreach (var kvp in frecuencias)
            {
                if (kvp.Value > maxFrecuencia)
                {
                    maxFrecuencia = kvp.Value;
                    modaValue = kvp.Key;
                }
            }
            
            return new LatinoNumber(modaValue);
        }, -1, "Retorna el valor que más se repite en la lista.", "lista");

        // =====================================================================
        // OPERACIONES DE CONJUNTO NUMÉRICO
        // =====================================================================

        Def("sumatoria", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "mate.sumatoria() requiere una lista de números.");
            if (args[0] is not LatinoList list)
                throw new RuntimeError(null!, 
                    "mate.sumatoria() requiere una lista como argumento.");
            
            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, 
                    "No se puede calcular la sumatoria de una lista vacía.");
            
            double suma = 0;
            for (int i = 0; i < list.Elements.Count; i++)
            {
                if (list.Elements[i] is not LatinoNumber num)
                    throw new RuntimeError(null!, 
                        $"El elemento en la posición {i} no es un número.");
                suma += num.Value;
            }
            
            return new LatinoNumber(suma);
        }, -1, "Suma todos los elementos de la lista (Σ).", "lista");

        Def("productoria", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, "mate.productoria() requiere una lista de números.");
            if (args[0] is not LatinoList list)
                throw new RuntimeError(null!, 
                    "mate.productoria() requiere una lista como argumento.");
            
            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, 
                    "No se puede calcular la productoria de una lista vacía.");
            
            double producto = 1.0;
            
            for (int i = 0; i < list.Elements.Count; i++)
            {
                if (list.Elements[i] is not LatinoNumber num)
                    throw new RuntimeError(null!, 
                        $"El elemento en la posición {i} no es un número.");
                
                if (num.Value == 0)
                    return new LatinoNumber(0);
                
                producto *= num.Value;
                
                if (double.IsInfinity(producto))
                    throw new RuntimeError(null!, 
                        "Desbordamiento numérico en mate.productoria(). El resultado es demasiado grande.");
            }
            
            return new LatinoNumber(producto);
        }, -1, "Multiplica todos los elementos de la lista (∏).", "lista");

        // =====================================================================
        // PARSING INTELIGENTE DE NÚMEROS
        // =====================================================================

        Def("parsearNumero", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "mate.parsearNumero() requiere al menos un argumento: el valor a parsear.");
            
            string? separadorMiles = args.Count >= 2 ? args[1].ToString() : null;
            bool esNegativo = false;
            string textoNormalizado;
            double valorEntero;
            int valorDecimal;
            
            if (args[0] is LatinoNumber num)
            {
                esNegativo = num.Value < 0;
                double valorAbs = Math.Abs(num.Value);
                valorEntero = Math.Floor(valorAbs);
                double parteDecimal = valorAbs - valorEntero;
                
                // Extraer parte decimal de forma segura usando strings para evitar precisión/overflow
                string decPartStr = parteDecimal.ToString("F10", System.Globalization.CultureInfo.InvariantCulture);
                string decimalStr = decPartStr.Contains(".") ? decPartStr.Split('.')[1].TrimEnd('0') : "";
                
                valorDecimal = string.IsNullOrEmpty(decimalStr) ? 0 : int.Parse(decimalStr);
                
                textoNormalizado = valorEntero.ToString("F0", 
                    System.Globalization.CultureInfo.InvariantCulture);
                
                if (valorDecimal > 0)
                {
                    textoNormalizado += "." + decimalStr;
                }
            }
            else
            {
                string textoOriginal = args[0].ToString().Trim();
                if (string.IsNullOrWhiteSpace(textoOriginal))
                    throw new RuntimeError(null!, 
                        "mate.parsearNumero() no puede parsear texto vacío.");
                
                if (textoOriginal.StartsWith("-"))
                {
                    esNegativo = true;
                    textoOriginal = textoOriginal.Substring(1).Trim();
                }
                else if (textoOriginal.StartsWith("+"))
                {
                    textoOriginal = textoOriginal.Substring(1).Trim();
                }
                
                string textoProcesado = textoOriginal;
                if (!string.IsNullOrEmpty(separadorMiles))
                {
                    if (separadorMiles.Any(char.IsDigit))
                        throw new RuntimeError(null!, 
                            $"El separador de miles no puede contener dígitos: '{separadorMiles}'");
                    textoProcesado = textoOriginal.Replace(separadorMiles, "");
                }
                
                int countPunto = textoProcesado.Count(c => c == '.');
                int countComa = textoProcesado.Count(c => c == ',');
                char separadorDecimal = '.';
                
                if (countPunto == 0 && countComa == 1)
                {
                    separadorDecimal = ',';
                }
                else if (countPunto == 1 && countComa == 0)
                {
                    separadorDecimal = '.';
                }
                else if (countPunto > 0 && countComa > 0)
                {
                    int ultimoPunto = textoProcesado.LastIndexOf('.');
                    int ultimaComa = textoProcesado.LastIndexOf(',');
                    separadorDecimal = (ultimaComa > ultimoPunto) ? ',' : '.';
                    char separadorMilesDetectado = (separadorDecimal == '.') ? ',' : '.';
                    textoProcesado = textoProcesado.Replace(separadorMilesDetectado.ToString(), "");
                }
                else if (countPunto > 1 || countComa > 1)
                {
                    if (countPunto > 1 && countComa == 0)
                    {
                        textoProcesado = textoProcesado.Replace(".", "");
                        separadorDecimal = '\0';
                    }
                    else if (countComa > 1 && countPunto == 0)
                    {
                        textoProcesado = textoProcesado.Replace(",", "");
                        separadorDecimal = '\0';
                    }
                }
                
                if (separadorDecimal == ',')
                {
                    textoProcesado = textoProcesado.Replace(',', '.');
                }
                
                if (!double.TryParse(
                    textoProcesado, 
                    System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double numeroParseado))
                {
                    throw new RuntimeError(null!, 
                        $"No se pudo parsear el número. Texto inválido: '{textoOriginal}'");
                }
                
                double valorAbsoluto = Math.Abs(numeroParseado);
                valorEntero = Math.Floor(valorAbsoluto);
                double parteDecimalDouble = valorAbsoluto - valorEntero;
                string decimalStr = parteDecimalDouble.ToString("F10", 
                    System.Globalization.CultureInfo.InvariantCulture).Split('.')[1].TrimEnd('0');
                valorDecimal = string.IsNullOrEmpty(decimalStr) ? 0 : int.Parse(decimalStr);
                textoNormalizado = valorEntero.ToString("F0", 
                    System.Globalization.CultureInfo.InvariantCulture);
                if (valorDecimal > 0)
                {
                    textoNormalizado += "." + decimalStr;
                }
            }
            
            var resultadoDict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
            {
                { new LatinoString("entero"), new LatinoNumber(valorEntero) },
                { new LatinoString("decimal"), new LatinoNumber(valorDecimal) },
                { new LatinoString("texto"), new LatinoString(textoNormalizado) },
                { new LatinoString("negativo"), new LatinoBoolean(esNegativo) }
            };
            
            return new LatinoDictionary(resultadoDict);
            
        }, -1, "Parsea un número desde texto o valor numérico. Retorna {entero, decimal, texto, negativo}. Soporta múltiples formatos internacionales.", "valor, [separador_miles]");

        Def("reconstruir", (interp, args) => {
            if (args[0] is not LatinoDictionary dict)
                throw new RuntimeError(null!, 
                    "mate.reconstruir() requiere un diccionario como argumento (resultado de parsearNumero).");
            
            var enteroKey = new LatinoString("entero");
            var decimalKey = new LatinoString("decimal");
            var negativoKey = new LatinoString("negativo");
            
            if (!dict.Pairs.ContainsKey(enteroKey))
                throw new RuntimeError(null!, 
                    "mate.reconstruir() requiere que el diccionario tenga la clave 'entero'.");
            
            double entValue = Get(dict.Pairs[enteroKey]);
            double decValue = dict.Pairs.ContainsKey(decimalKey) ? Get(dict.Pairs[decimalKey]) : 0;
            bool isNeg = dict.Pairs.ContainsKey(negativoKey) && 
                            ((LatinoBoolean)dict.Pairs[negativoKey]).Value;
            
            string dStr = decValue.ToString();
            int lDec = dStr.Length;
            double fNum = entValue + (decValue / Math.Pow(10, lDec));
            
            if (isNeg)
                fNum = -fNum;
            
            return new LatinoNumber(fNum);
            
        }, 1, "Reconstruye un número desde un diccionario (inverso de parsearNumero).", "diccionario");

        // ═════════════════════════════════════════════════════════════════
        // ÁLGEBRA: SOLVER DE POLINOMIOS (Grados 1-4)
        // ═════════════════════════════════════════════════════════════════
        // 
        // NOTACIÓN: Los coeficientes se pasan de MAYOR a MENOR grado.
        // Ejemplo: mate.resolverPolinomio(2, -3, 1) → 2x² - 3x + 1 = 0
        //
        // MÉTODOS USADOS:
        // • Grado 1: Fórmula lineal directa
        // • Grado 2: Fórmula cuadrática (discriminante)
        // • Grado 3: Método de Cardano (depresión cúbica + trigonométrica)
        // • Grado 4: Método de Ferrari (cúbica resolvente + factorización)
        //
        // RETORNO: Diccionario con estructura:
        // {
        //   "grado": número entero (1-4),
        //   "coeficientes": lista de números (normalizados),
        //   "descripcion": string explicativo del tipo de raíces,
        //   "x1", "x2", ...: raíces (números reales o strings "a ± bi" para complejas)
        // }
        // ═════════════════════════════════════════════════════════════════

        Def("resolverPolinomio", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "mate.resolverPolinomio() requiere al menos un coeficiente. " +
                    "Uso: mate.resolverPolinomio(a, b, c, ...) donde a es el coeficiente de mayor grado.");
            
            // Convertir argumentos a array de coeficientes
            var coeficientes = new double[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] is not LatinoNumber num)
                    throw new RuntimeError(null!, 
                        $"mate.resolverPolinomio(): Todos los coeficientes deben ser números. " +
                        $"El argumento en posición {i + 1} es de tipo '{args[i].TypeName}'.");
                
                coeficientes[i] = num.Value;
            }
            
            // Resolver usando el solver interno
            Dictionary<string, object> resultado;
            try
            {
                resultado = PolinomioSolver.Resolver(coeficientes);
            }
            catch (RuntimeError)
            {
                throw; // Re-lanzar errores de validación ya traducidos
            }
            catch (Exception ex)
            {
                throw new RuntimeError(null!, 
                    $"Error inesperado al resolver polinomio: {ex.Message}");
            }
            
            // Convertir resultado a diccionario Latino
            var dictLatino = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            // Grado
            dictLatino[new LatinoString("grado")] = new LatinoNumber((int)resultado["grado"]);
            
            // Coeficientes (como lista)
            var coefArray = (double[])resultado["coeficientes"];
            var coefLista = new List<LatinoObject>();
            foreach (var c in coefArray)
                coefLista.Add(new LatinoNumber(c));
            dictLatino[new LatinoString("coeficientes")] = new LatinoList(coefLista);
            
            // Descripción
            dictLatino[new LatinoString("descripcion")] = new LatinoString((string)resultado["descripcion"]);
            
            // Raíces (x1, x2, x3, x4 según grado)
            int grado = (int)resultado["grado"];
            for (int i = 1; i <= grado; i++)
            {
                string key = $"x{i}";
                if (resultado.ContainsKey(key))
                {
                    var raiz = resultado[key];
                    
                    // Convertir a LatinoObject apropiado
                    LatinoObject raizLatino;
                    if (raiz is double d)
                        raizLatino = new LatinoNumber(d);
                    else if (raiz is string s)
                        raizLatino = new LatinoString(s); // Complejo como string "a ± bi"
                    else
                        raizLatino = new LatinoString(raiz.ToString()!);
                    
                    dictLatino[new LatinoString(key)] = raizLatino;
                }
            }
            
            return new LatinoDictionary(dictLatino);
            
        }, -1, 
        "Resuelve polinomios de grado 1-4 usando fórmulas algebraicas exactas. " +
        "Retorna diccionario con: {grado, coeficientes, descripcion, x1, x2, ...}. " +
        "Los coeficientes se pasan de mayor a menor grado: resolverPolinomio(a, b, c) = ax² + bx + c = 0",
        "coeficientes...");

        // Alias corto
        methods["resolverPol"] = methods["resolverPolinomio"];


        // =====================================================================
        // ESTADÍSTICA AVANZADA e INGENIERÍA
        // =====================================================================

        Def("varianza", (interp, args) => {
            var nums = ExtractNumbers(args).ToList();
            if (nums.Count < 2) throw new RuntimeError(null!, "La varianza requiere al menos 2 números.");
            
            double media = nums.Average();
            double sumaCuadrados = nums.Sum(n => Math.Pow(n - media, 2));
            return new LatinoNumber(sumaCuadrados / (nums.Count - 1)); // Varianza muestral
        }, -1, "Calcula la varianza muestral de una lista de números.", "lista | numeros...");

        Def("desviacion", (interp, args) => {
            var nums = ExtractNumbers(args).ToList();
            if (nums.Count < 2) throw new RuntimeError(null!, "La desviación requiere al menos 2 números.");
            
            double media = nums.Average();
            double sumaCuadrados = nums.Sum(n => Math.Pow(n - media, 2));
            double varianza = sumaCuadrados / (nums.Count - 1);
            return new LatinoNumber(Math.Sqrt(varianza));
        }, -1, "Calcula la desviación estándar muestral.", "lista | numeros...");

        Def("regresion", (interp, args) => {
            if (args.Count != 2 || args[0] is not LatinoList xList || args[1] is not LatinoList yList)
                throw new RuntimeError(null!, "mate.regresion() requiere dos listas: x, y.");

            if (xList.Elements.Count != yList.Elements.Count)
                throw new RuntimeError(null!, "Las listas X e Y deben tener la misma longitud.");

            int n = xList.Elements.Count;
            if (n < 2) throw new RuntimeError(null!, "Se requieren al menos 2 puntos para la regresión.");

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int i = 0; i < n; i++) {
                double x = Get(xList.Elements[i]);
                double y = Get(yList.Elements[i]);
                sumX += x; sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double pendiente = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double interseccion = (sumY - pendiente * sumX) / n;

            var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("m"), new LatinoNumber(pendiente) },
                { new LatinoString("b"), new LatinoNumber(interseccion) },
                { new LatinoString("formula"), new LatinoString($"y = {pendiente:F4}x + {interseccion:F4}") }
            };
            return new LatinoDictionary(res);
        }, 2, "Realiza una regresión lineal simple. Retorna {m, b, formula}.", "lista_x, lista_y");

        // =====================================================================
        // CONVERSIÓN DE UNIDADES (Navaja Suiza)
        // =====================================================================

        Def("convertir", (interp, args) => {
            double valor = Get(args[0]);
            string desde = args[1].ToString().ToLower();
            string hacia = args[2].ToString().ToLower();

            // Lógica especial para Temperatura
            if (desde == "celsius" && hacia == "fahrenheit") return new LatinoNumber((valor * 9 / 5) + 32);
            if (desde == "fahrenheit" && hacia == "celsius") return new LatinoNumber((valor - 32) * 5 / 9);
            if (desde == "celsius" && hacia == "kelvin") return new LatinoNumber(valor + 273.15);
            if (desde == "kelvin" && hacia == "celsius") return new LatinoNumber(valor - 273.15);

            // Factores de conversión base (Normalizados a una unidad base por categoría)
            var factores = new Dictionary<string, (string categoria, double factor)> {
                // LONGITUD (Base: metros)
                ["m"] = ("long", 1), ["metros"] = ("long", 1),
                ["km"] = ("long", 1000), ["kilometros"] = ("long", 1000),
                ["cm"] = ("long", 0.01), ["centimetros"] = ("long", 0.01),
                ["mm"] = ("long", 0.001), ["milimetros"] = ("long", 0.001),
                ["pulg"] = ("long", 0.0254), ["pulgadas"] = ("long", 0.0254),
                ["pies"] = ("long", 0.3048), ["pie"] = ("long", 0.3048),
                ["yd"] = ("long", 0.9144), ["yardas"] = ("long", 0.9144),
                ["millas"] = ("long", 1609.34),
                ["nmi"] = ("long", 1852), ["milla_nautica"] = ("long", 1852),
                
                // LONGITUD HISTÓRICA / CULTURAL
                ["vara"] = ("long", 0.8359),
                ["legua"] = ("long", 5572.7),
                ["codo"] = ("long", 0.418),
                ["palmo"] = ("long", 0.209),
                
                // LONGITUD ASTRONÓMICA
                ["ua"] = ("long", 1.496e11), ["unidad_astronomica"] = ("long", 1.496e11),
                ["ly"] = ("long", 9.461e15), ["anio_luz"] = ("long", 9.461e15),
                
                // MASA (Base: kilogramos)
                ["kg"] = ("masa", 1), ["kilogramos"] = ("masa", 1),
                ["g"] = ("masa", 0.001), ["gramos"] = ("masa", 0.001),
                ["mg"] = ("masa", 1e-6), ["miligramos"] = ("masa", 1e-6),
                ["lb"] = ("masa", 0.453592), ["libras"] = ("masa", 0.453592),
                ["oz"] = ("masa", 0.0283495), ["onzas"] = ("masa", 0.0283495),
                ["t"] = ("masa", 1000), ["tonelada"] = ("masa", 1000),
                ["ton_corta"] = ("masa", 907.185),
                
                // MASA HISTÓRICA
                ["arroba"] = ("masa", 11.502),
                ["quintal"] = ("masa", 46.009),
                ["grano"] = ("masa", 0.0000648),
                ["quilate"] = ("masa", 0.0002),
                
                // ÁREA (Base: metros cuadrados)
                ["m2"] = ("area", 1), ["km2"] = ("area", 1e6),
                ["ha"] = ("area", 10000), ["hectarea"] = ("area", 10000),
                ["acre"] = ("area", 4046.86),
                ["fanega"] = ("area", 6459.6),
                
                // VOLUMEN (Base: litros)
                ["l"] = ("vol", 1), ["litro"] = ("vol", 1), ["litros"] = ("vol", 1),
                ["ml"] = ("vol", 0.001), ["mililitros"] = ("vol", 0.001),
                ["gl"] = ("vol", 3.78541), ["galon"] = ("vol", 3.78541),
                ["gl_uk"] = ("vol", 4.54609), ["galon_imperial"] = ("vol", 4.54609),
                ["pinta"] = ("vol", 0.473176),
                ["taza"] = ("vol", 0.24),
                ["barril"] = ("vol", 158.987),
                
                // PRESIÓN (Base: Pascales)
                ["pa"] = ("pres", 1), ["pascal"] = ("pres", 1),
                ["psi"] = ("pres", 6894.76),
                ["bar"] = ("pres", 100000),
                ["atm"] = ("pres", 101325), ["atmosfera"] = ("pres", 101325),
                ["torr"] = ("pres", 133.322),
                
                // VELOCIDAD (Base: metros por segundo)
                ["ms"] = ("vel", 1),
                ["kmh"] = ("vel", 0.277778), ["km_h"] = ("vel", 0.277778),
                ["mph"] = ("vel", 0.44704),
                ["nudo"] = ("vel", 0.514444), ["kt"] = ("vel", 0.514444),
                ["mach"] = ("vel", 340.3),
                
                // ENERGÍA (Base: Joules)
                ["j"] = ("en", 1), ["joule"] = ("en", 1),
                ["cal"] = ("en", 4.184), ["caloria"] = ("en", 4.184),
                ["kcal"] = ("en", 4184),
                ["btu"] = ("en", 1055.06),
                ["ev"] = ("en", 1.60218e-19),
                
                // ÁNGULOS (Base: radianes)
                ["rad"] = ("ang", 1), ["radianes"] = ("ang", 1),
                ["deg"] = ("ang", Math.PI / 180), ["grados"] = ("ang", Math.PI / 180),
                
                // DIGITAL (Base: Bytes)
                ["byte"] = ("dig", 1), ["bytes"] = ("dig", 1),
                ["bit"] = ("dig", 0.125),
                ["kb"] = ("dig", 1024),
                ["mb"] = ("dig", 1048576),
                ["gb"] = ("dig", 1073741824),
                ["tb"] = ("dig", 1099511627776)
            };

            if (factores.TryGetValue(desde, out var f1) && factores.TryGetValue(hacia, out var f2)) {
                if (f1.categoria != f2.categoria)
                    throw new RuntimeError(null!, $"No se puede convertir de {f1.categoria} a {f2.categoria}.");
                return new LatinoNumber(valor * f1.factor / f2.factor);
            }

            throw new RuntimeError(null!, $"Unidades no soportadas o incompatibles: {desde} -> {hacia}");
        }, 3, "Convierte valores entre unidades físicas y temperaturas.", "valor, desde, hacia");

        // Construir y Registrar
        var mateObj = new LatinoDictionary(ConvertMap(methods));
        foreach(var kv in constMap) mateObj.Pairs[new LatinoString(kv.Key)] = kv.Value;
        mateObj.MakeReadOnly();

        interpreter.Globals.Define("mate", mateObj);
    }
    
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }

    private static LatinoNumber Num(double val) => new LatinoNumber(val);
}

// ═════════════════════════════════════════════════════════════════════
// SOLVER DE POLINOMIOS (Grados 1-4)
// ═════════════════════════════════════════════════════════════════════

/// <summary>
/// Resuelve polinomios algebraicos de grado 1 a 4 usando métodos exactos.
/// Basado en fórmulas clásicas: Lineal, Cuadrática, Cardano (cúbica), Ferrari (cuártica).
/// </summary>
internal static class PolinomioSolver
{
    // ═════════════════════════════════════════════════════════════════
    // CORRECCIÓN DE ERRORES NUMÉRICOS
    // ═════════════════════════════════════════════════════════════════

    /// <summary>
    /// Corrige errores de redondeo en punto flotante.
    /// Si un número está "casi" en un entero, lo ajusta a ese entero.
    /// </summary>
    internal static double CorregirErrorNumerico(double valor, double tolerancia = 1e-9)
    {
        if (double.IsNaN(valor) || double.IsInfinity(valor))
            return valor;
        
        double enteroMasCercano = Math.Round(valor);
        double distancia = Math.Abs(valor - enteroMasCercano);
        
        if (distancia < tolerancia)
            return enteroMasCercano;
        
        return valor;
    }

    /// <summary>
    /// Versión para strings que representan números complejos.
    /// </summary>
    internal static string CorregirComplejoString(string complejo, double tolerancia = 1e-9)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            complejo, 
            @"^([-+]?\d+\.?\d*)\s*([+-])\s*(\d+\.?\d*)i$"
        );
        
        if (!match.Success)
            return complejo;
        
        double real = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        string operador = match.Groups[2].Value;
        double imag = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
        
        real = CorregirErrorNumerico(real, tolerancia);
        imag = CorregirErrorNumerico(imag, tolerancia);
        
        if (Math.Abs(imag) < tolerancia) return $"{real:G}";
        
        if (Math.Abs(real) < tolerancia)
        {
            if (Math.Abs(imag - 1.0) < tolerancia) return "i";
            if (Math.Abs(imag + 1.0) < tolerancia) return "-i";
            return $"{imag:G}i";
        }
        
        string signo = imag >= 0 ? "+" : "-";
        imag = Math.Abs(imag);
        
        if (Math.Abs(imag - 1.0) < tolerancia)
            return $"{real:G} {signo} i";
        
        return $"{real:G} {signo} {imag:G}i";
    }

    public static Dictionary<string, object> Resolver(double[] coeficientes)
    {
        // ═════════════════════════════════════════════════════════════
        // VALIDACIÓN DE ENTRADA
        // ═════════════════════════════════════════════════════════════
        if (coeficientes == null || coeficientes.Length == 0)
            throw new RuntimeError(null!, 
                "mate.resolverPolinomio() requiere al menos un coeficiente.");

        // Eliminar ceros a la izquierda (coeficientes no significativos)
        int primerNoNulo = Array.FindIndex(coeficientes, c => c != 0);
        
        if (primerNoNulo == -1)
            throw new RuntimeError(null!, 
                "El polinomio es idénticamente cero (0 = 0). Tiene infinitas soluciones.");

        if (primerNoNulo > 0)
        {
            int nuevoLength = coeficientes.Length - primerNoNulo;
            double[] recortados = new double[nuevoLength];
            Array.Copy(coeficientes, primerNoNulo, recortados, 0, nuevoLength);
            coeficientes = recortados;
        }

        int grado = coeficientes.Length - 1;

        // ═════════════════════════════════════════════════════════════
        // VALIDACIÓN DE GRADO
        // ═════════════════════════════════════════════════════════════
        if (grado > 4)
            throw new RuntimeError(null!, 
                $"Polinomios de grado {grado} no tienen fórmula algebraica general. " +
                "Solo se soportan grados 1-4 (lineal, cuadrática, cúbica, cuártica).");
        
        if (grado < 1)
            throw new RuntimeError(null!, 
                "El polinomio es una constante (grado 0). No tiene raíces.");

        // ═════════════════════════════════════════════════════════════
        // ESTRUCTURA DE RETORNO
        // ═════════════════════════════════════════════════════════════
        var resultado = new Dictionary<string, object>
        {
            ["grado"] = grado,
            ["coeficientes"] = coeficientes
        };

        // Resolver según grado
        switch (grado)
        {
            case 1: ResolverLineal(coeficientes, resultado); break;
            case 2: ResolverCuadratica(coeficientes, resultado); break;
            case 3: ResolverCubica(coeficientes, resultado); break;
            case 4: ResolverCuartica(coeficientes, resultado); break;
        }

        return resultado;
    }

    // ═════════════════════════════════════════════════════════════════
    // GRADO 1: ax + b = 0  →  x = -b/a
    // ═════════════════════════════════════════════════════════════════
    private static void ResolverLineal(double[] coef, Dictionary<string, object> resultado)
    {
        double a = coef[0];
        double b = coef[1];
        double raiz = CorregirErrorNumerico(-b / a);
        
        resultado["x1"] = raiz;
        resultado["descripcion"] = "Raíz real única";
    }

    // ═════════════════════════════════════════════════════════════════
    // GRADO 2: ax² + bx + c = 0
    // Discriminante: Δ = b² - 4ac
    // ═════════════════════════════════════════════════════════════════
    private static void ResolverCuadratica(double[] coef, Dictionary<string, object> resultado)
    {
        double a = coef[0], b = coef[1], c = coef[2];
        double discriminante = b * b - 4 * a * c;

        const double epsilon = 1e-12; // Tolerancia numérica

        if (Math.Abs(discriminante) < epsilon)
        {
            // Raíz doble
            double raiz = CorregirErrorNumerico(-b / (2 * a));
            resultado["x1"] = raiz;
            resultado["x2"] = raiz;
            resultado["descripcion"] = "Raíz real doble (discriminante = 0)";
        }
        else if (discriminante > 0)
        {
            // Dos raíces reales distintas
            double sqrtD = Math.Sqrt(discriminante);
            double r1 = CorregirErrorNumerico((-b - sqrtD) / (2 * a));
            double r2 = CorregirErrorNumerico((-b + sqrtD) / (2 * a));
            
            // Ordenar de menor a mayor
            if (r1 > r2) { double tmp = r1; r1 = r2; r2 = tmp; }
            
            resultado["x1"] = r1;
            resultado["x2"] = r2;
            resultado["descripcion"] = "Raíces reales distintas (discriminante > 0)";
        }
        else
        {
            // Raíces complejas conjugadas: a ± bi
            double real = CorregirErrorNumerico(-b / (2 * a));
            double imag = CorregirErrorNumerico(Math.Sqrt(-discriminante) / (2 * a));
            
            resultado["x1"] = CorregirComplejoString($"{real:G} - {imag:G}i");
            resultado["x2"] = CorregirComplejoString($"{real:G} + {imag:G}i");
            resultado["descripcion"] = "Raíces complejas conjugadas (discriminante < 0)";
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // GRADO 3: ax³ + bx² + cx + d = 0
    // Método de Cardano (con depresión cúbica)
    // ═════════════════════════════════════════════════════════════════
    private static void ResolverCubica(double[] coef, Dictionary<string, object> resultado)
    {
        double a = coef[0], b = coef[1], c = coef[2], d = coef[3];
        
        // Normalizar a forma mónica: x³ + Ax² + Bx + C = 0
        double A = b / a, B = c / a, C = d / a;

        // Sustitución x = y - A/3 (eliminar término cuadrático)
        double p = B - A * A / 3.0;
        double q = (2.0 * A * A * A / 27.0) - (A * B / 3.0) + C;
        
        // Discriminante cúbico
        double discriminante = q * q / 4.0 + p * p * p / 27.0;

        const double epsilon = 1e-12;
        double shift = A / 3.0; // Corrección para volver a x desde y

        // CASO 1: Raíz triple (polinomio casi nulo)
        if (Math.Abs(p) < epsilon && Math.Abs(q) < epsilon)
        {
            double raiz = CorregirErrorNumerico(-shift);
            resultado["x1"] = raiz;
            resultado["x2"] = raiz;
            resultado["x3"] = raiz;
            resultado["descripcion"] = "Raíz real triple";
            return;
        }

        // CASO 2: Discriminante > 0 → 1 raíz real + 2 complejas
        if (discriminante > epsilon)
        {
            double sqrtDisc = Math.Sqrt(discriminante);
            double u = Math.Cbrt(-q / 2.0 + sqrtDisc);
            double v = Math.Cbrt(-q / 2.0 - sqrtDisc);
            double y1 = u + v;
            
            // Raíz real
            resultado["x1"] = CorregirErrorNumerico(y1 - shift);
            
            // Raíces complejas conjugadas
            double realPart = CorregirErrorNumerico(-(u + v) / 2.0 - shift);
            double imagPart = CorregirErrorNumerico(Math.Sqrt(3) / 2.0 * (u - v));
            
            resultado["x2"] = CorregirComplejoString($"{realPart:G} - {imagPart:G}i");
            resultado["x3"] = CorregirComplejoString($"{realPart:G} + {imagPart:G}i");
            resultado["descripcion"] = "Una raíz real y dos complejas conjugadas";
            return;
        }

        // CASO 3: Discriminante < 0 → 3 raíces reales (Método trigonométrico)
        if (discriminante < -epsilon)
        {
            double r = Math.Sqrt(-p * p * p / 27.0);
            double phi = Math.Acos(-q / (2.0 * r));
            double cbrtR = Math.Cbrt(r);
            
            double[] raices = new double[3];
            for (int k = 0; k < 3; k++)
            {
                raices[k] = CorregirErrorNumerico(2.0 * cbrtR * Math.Cos((phi + 2.0 * Math.PI * k) / 3.0) - shift);
            }
            
            Array.Sort(raices); // Ordenar de menor a mayor
            
            resultado["x1"] = raices[0];
            resultado["x2"] = raices[1];
            resultado["x3"] = raices[2];
            resultado["descripcion"] = "Tres raíces reales distintas";
            return;
        }

        // CASO 4: Discriminante ≈ 0 → Raíz doble + raíz simple
        double t = Math.Cbrt(-q / 2.0);
        double y1d = CorregirErrorNumerico(2.0 * t - shift);
        double y2d = CorregirErrorNumerico(-t - shift);
        
        resultado["x1"] = y1d; // Raíz simple
        resultado["x2"] = y2d; // Raíz doble
        resultado["x3"] = y2d;
        resultado["descripcion"] = "Raíz real simple y raíz real doble";
    }

    // ═════════════════════════════════════════════════════════════════
    // GRADO 4: ax⁴ + bx³ + cx² + dx + e = 0
    // Método de Ferrari (reducción a cúbica resolvente)
    // ═════════════════════════════════════════════════════════════════
    private static void ResolverCuartica(double[] coef, Dictionary<string, object> resultado)
    {
        double a = coef[0], b = coef[1], c = coef[2], d = coef[3], e = coef[4];
        
        // Normalizar a forma mónica
        double B = b / a, C = c / a, D = d / a, E = e / a;

        // Sustitución x = y - B/4 (eliminar término cúbico)
        double p = C - 3.0 * B * B / 8.0;
        double q = D - B * C / 2.0 + B * B * B / 8.0;
        double r = E - B * D / 4.0 + B * B * C / 16.0 - 3.0 * B * B * B * B / 256.0;

        const double epsilon = 1e-12;
        double shift = B / 4.0;

        // CASO ESPECIAL: Bicuadrática (q = 0) → y⁴ + py² + r = 0
        if (Math.Abs(q) < epsilon)
        {
            // Resolver como cuadrática en z = y²
            double[] cuadCoef = { 1.0, p, r };
            var tempDict = new Dictionary<string, object>();
            ResolverCuadratica(cuadCoef, tempDict);
            
            var raices = new List<string>();
            
            // Procesar z1
            if (tempDict["x1"] is double z1)
            {
                z1 = CorregirErrorNumerico(z1);
                if (z1 >= 0)
                {
                    double y = Math.Sqrt(z1);
                    raices.Add($"{CorregirErrorNumerico(y - shift):G}");
                    raices.Add($"{CorregirErrorNumerico(-y - shift):G}");
                }
                else
                {
                    double imag = Math.Sqrt(-z1);
                    raices.Add(CorregirComplejoString($"{-shift:G} - {imag:G}i"));
                    raices.Add(CorregirComplejoString($"{-shift:G} + {imag:G}i"));
                }
            }
            else
            {
                // z1 es complejo (string)
                raices.Add(CorregirComplejoString(tempDict["x1"].ToString()!));
            }
            
            // Procesar z2
            if (tempDict["x2"] is double z2)
            {
                z2 = CorregirErrorNumerico(z2);
                if (z2 >= 0)
                {
                    double y = Math.Sqrt(z2);
                    raices.Add($"{CorregirErrorNumerico(y - shift):G}");
                    raices.Add($"{CorregirErrorNumerico(-y - shift):G}");
                }
                else
                {
                    double imag = Math.Sqrt(-z2);
                    raices.Add(CorregirComplejoString($"{-shift:G} - {imag:G}i"));
                    raices.Add(CorregirComplejoString($"{-shift:G} + {imag:G}i"));
                }
            }
            else
            {
                raices.Add(CorregirComplejoString(tempDict["x2"].ToString()!));
            }
            
            resultado["x1"] = raices[0];
            resultado["x2"] = raices[1];
            resultado["x3"] = raices[2];
            resultado["x4"] = raices[3];
            resultado["descripcion"] = "Reducción a forma bicuadrática";
            return;
        }

        // CASO GENERAL: Método de Ferrari
        // Resolver cúbica resolvente: m³ - pm² - 4rm + (4pr - q²) = 0
        double[] cubicaCoef = { 1.0, -p, -4.0 * r, 4.0 * p * r - q * q };
        var cubicaDict = new Dictionary<string, object>();
        ResolverCubica(cubicaCoef, cubicaDict);
        
        // Tomar la primera raíz real (cualquiera funciona)
        double m = cubicaDict["x1"] is double ? (double)cubicaDict["x1"] : 0.0;

        // Factorización: (y² + αy + β)(y² - αy + γ) = 0
        double alpha = Math.Sqrt(Math.Max(0, m - p));
        double beta = (m + q / alpha) / 2.0;
        double gamma = (m - q / alpha) / 2.0;

        // Resolver dos cuadráticas
        var raices1Dict = new Dictionary<string, object>();
        var raices2Dict = new Dictionary<string, object>();
        
        ResolverCuadratica(new[] { 1.0, alpha, beta }, raices1Dict);
        ResolverCuadratica(new[] { 1.0, -alpha, gamma }, raices2Dict);

        // Aplicar shift y recolectar
        var todasRaices = new List<string>();
        
        void AgregarRaiz(object raiz)
        {
            if (raiz is double d)
                todasRaices.Add($"{CorregirErrorNumerico(d - shift):G}");
            else
                todasRaices.Add(CorregirComplejoString(raiz.ToString()!));
        }
        
        AgregarRaiz(raices1Dict["x1"]);
        AgregarRaiz(raices1Dict["x2"]);
        AgregarRaiz(raices2Dict["x1"]);
        AgregarRaiz(raices2Dict["x2"]);

        resultado["x1"] = todasRaices[0];
        resultado["x2"] = todasRaices[1];
        resultado["x3"] = todasRaices[2];
        resultado["x4"] = todasRaices[3];
        resultado["descripcion"] = "Método de Ferrari (reducción a cúbica resolvente)";
    }
}
