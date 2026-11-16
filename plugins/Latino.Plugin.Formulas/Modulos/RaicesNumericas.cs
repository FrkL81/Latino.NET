using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class RaicesNumericas
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private static double GetDouble(LatinoObject obj) =>
        obj is LatinoNumber n ? n.Value
            : throw new RuntimeError(null!, "Se esperaba un número.");

    /// <summary>
    /// Evalúa la función de usuario con Object Pooling de argumentos.
    /// El mismo List&lt;LatinoObject&gt; se reutiliza en cada llamada para
    /// evitar presión innecesaria sobre el GC en bucles de miles de iteraciones.
    /// </summary>
    private static double Evaluar(
        ICallable func,
        Interpreter interp,
        List<LatinoObject> pool,
        double x)
    {
        pool[0] = new LatinoNumber(x);
        var res = func.Call(interp, pool);
        double val = GetDouble(res);
        if (double.IsNaN(val) || double.IsInfinity(val))
            throw new RuntimeError(null!, $"La función retornó un valor no finito en x = {x:G}.");
        return val;
    }

    /// <summary>
    /// Construye el diccionario de retorno estándar para métodos iterativos.
    /// Estructura garantizada:
    ///   resultado, convergio, iteraciones, error_estimado, formula, [extras...]
    /// </summary>
    private static LatinoDictionary RetornoIterativo(
        double resultado,
        bool convergio,
        int iteraciones,
        double errorEstimado,
        string formula,
        LatinoDictionary? extras = null)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),      new LatinoNumber(resultado) },
            { new LatinoString("convergio"),      new LatinoBoolean(convergio) },
            { new LatinoString("iteraciones"),    new LatinoNumber(iteraciones) },
            { new LatinoString("error_estimado"), new LatinoNumber(errorEstimado) },
            { new LatinoString("formula"),        new LatinoString(formula) }
        };
        if (extras != null)
            foreach (var kvp in extras.Pairs)
                dict[kvp.Key] = kvp.Value;
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Bisección
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.biseccion",
        Args = "funcion, a, b, [tolerancia], [maxIter]",
        Help  = "Encuentra una raíz de f(x) en el intervalo [a, b] por el método de Bisección.\n" +
                "Requiere que f(a) y f(b) tengan signos opuestos (Teorema de Bolzano).\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-9), maxIter (defecto 10 000).\n\n" +
                "Fórmula: c = (a + b) / 2 — convergencia garantizada O(log₂((b-a)/tol)).")]
    public static LatinoObject Biseccion(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "raices.biseccion requiere: funcion, a, b. " +
                                          "Opcionalmente: tolerancia, max_iter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a   = GetDouble(args[1]);
        double b   = GetDouble(args[2]);
        double tol = args.Count > 3 ? GetDouble(args[3]) : 1e-9;
        int maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000;

        if (tol <= 0)
            throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1)
            throw new RuntimeError(null!, "max_iter debe ser al menos 1.");
        if (a >= b)
            throw new RuntimeError(null!, "El intervalo requiere a < b.");

        // Object pool — se reutiliza en cada llamada a Evaluar()
        var pool = new List<LatinoObject> { null! };

        double fa = Evaluar(func, interp, pool, a);
        double fb = Evaluar(func, interp, pool, b);

        if (fa * fb > 0)
            throw new RuntimeError(null!,
                $"f(a) = {fa:G} y f(b) = {fb:G} tienen el mismo signo. " +
                "Bolzano no garantiza raíz en [a, b]. Ajusta el intervalo.");

        // Caso exacto en los extremos
        if (Math.Abs(fa) < tol)
            return RetornoIterativo(a, true, 0, Math.Abs(fa), "c = (a+b)/2");
        if (Math.Abs(fb) < tol)
            return RetornoIterativo(b, true, 0, Math.Abs(fb), "c = (a+b)/2");

        double c = a, fc = fa;
        int iter = 0;
        double error = b - a;

        while (iter < maxIter)
        {
            error = (b - a) / 2.0;
            c  = a + error;
            fc = Evaluar(func, interp, pool, c);
            iter++;

            if (Math.Abs(error) < tol || Math.Abs(fc) < tol)
                return RetornoIterativo(c, true, iter, Math.Abs(error), "c = (a+b)/2");

            if (fa * fc < 0) { b = c; fb = fc; }
            else             { a = c; fa = fc; }
        }

        // maxIter alcanzado sin convergencia
        return RetornoIterativo(c, false, iter, Math.Abs(error), "c = (a+b)/2",
            new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("advertencia"),
                  new LatinoString($"Se alcanzó max_iter = {maxIter} sin converger. " +
                                   "El resultado es la mejor aproximación disponible.") }
            }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Newton-Raphson con callback (versión iterativa completa)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.newtonRaphson",
        Args = "funcionF, funcionDf, x0, [tolerancia], [maxIter]",
        Help  = "Encuentra una raíz de f(x) por el método de Newton-Raphson.\n" +
                "Recibe DOS funciones Latino: f(x) y su derivada f'(x).\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-12), maxIter (defecto 10 000).\n" +
                "ADVERTENCIA: Si f'(x₀) ≈ 0 el método diverge — elige otro x₀.\n\n" +
                "Fórmula: xₙ₊₁ = xₙ - f(xₙ)/f'(xₙ) — convergencia cuadrática cerca de la raíz.")]
    public static LatinoObject NewtonRaphson(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "raices.newtonRaphson requiere: funcionF, funcionDf, x0. " +
                                          "Opcionalmente: tolerancia, maxIter.");

        var f  = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento (f) debe ser una función Latino.");
        var df = args[1] as ICallable
            ?? throw new RuntimeError(null!, "El segundo argumento (f') debe ser una función Latino.");

        double x0     = GetDouble(args[2]);
        double tol    = args.Count > 3 ? GetDouble(args[3]) : 1e-12;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000;

        if (tol <= 0)  throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "maxIter debe ser al menos 1.");

        var poolF  = new List<LatinoObject> { null! };
        var poolDf = new List<LatinoObject> { null! };

        double x    = x0;
        double error = double.MaxValue;
        int    iter  = 0;

        while (iter < maxIter)
        {
            double fx  = Evaluar(f,  interp, poolF,  x);
            double dfx = Evaluar(df, interp, poolDf, x);

            if (Math.Abs(dfx) < 1e-15)
                throw new RuntimeError(null!,
                    $"f'(x) ≈ 0 en x = {x:G} (punto de inflexión o cima). " +
                    "El método Newton-Raphson diverge. Elige otro x₀.");

            double xNew = x - fx / dfx;
            error = Math.Abs(xNew - x);
            iter++;
            x = xNew;

            if (error < tol && Math.Abs(fx) < tol)
                return RetornoIterativo(x, true, iter, error,
                    "xₙ₊₁ = xₙ - f(xₙ)/f'(xₙ)");
        }

        return RetornoIterativo(x, false, iter, error, "xₙ₊₁ = xₙ - f(xₙ)/f'(xₙ)",
            new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("advertencia"),
                  new LatinoString($"Se alcanzó maxIter = {maxIter}. Verifica convergencia o usa otro x₀.") }
            }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Secante (Newton sin derivada analítica)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.secante",
        Args = "funcion, x0, x1, [tolerancia], [maxIter]",
        Help  = "Encuentra una raíz de f(x) por el método de la Secante.\n" +
                "No requiere la derivada analítica — la aproxima con dos puntos iniciales.\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-10), maxIter (defecto 10 000).\n\n" +
                "Fórmula: xₙ₊₁ = xₙ - f(xₙ)·(xₙ - xₙ₋₁) / (f(xₙ) - f(xₙ₋₁))\n" +
                "Convergencia superlineal (~orden 1.618). Puede divergir si f(xₙ) ≈ f(xₙ₋₁).")]
    public static LatinoObject Secante(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "raices.secante requiere: funcion, x0, x1. " +
                                          "Opcionalmente: tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double x0     = GetDouble(args[1]);
        double x1     = GetDouble(args[2]);
        double tol    = args.Count > 3 ? GetDouble(args[3]) : 1e-10;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000;

        if (tol <= 0)  throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "maxIter debe ser al menos 1.");
        if (Math.Abs(x1 - x0) < 1e-15)
            throw new RuntimeError(null!, "x0 y x1 no pueden ser iguales.");

        var pool = new List<LatinoObject> { null! };

        double f0    = Evaluar(func, interp, pool, x0);
        double f1    = Evaluar(func, interp, pool, x1);
        double error = double.MaxValue;
        int    iter  = 0;

        while (iter < maxIter)
        {
            double df = f1 - f0;
            if (Math.Abs(df) < 1e-15)
                throw new RuntimeError(null!,
                    $"f(x1) - f(x0) ≈ 0 en iteración {iter}. " +
                    "La secante es casi horizontal — el método diverge. Cambia los puntos iniciales.");

            double x2 = x1 - f1 * (x1 - x0) / df;
            double f2 = Evaluar(func, interp, pool, x2);
            error = Math.Abs(x2 - x1);
            iter++;

            if (error < tol && Math.Abs(f2) < tol)
                return RetornoIterativo(x2, true, iter, error,
                    "xₙ₊₁ = xₙ - f(xₙ)·(xₙ-xₙ₋₁)/(f(xₙ)-f(xₙ₋₁))");

            x0 = x1; f0 = f1;
            x1 = x2; f1 = f2;
        }

        return RetornoIterativo(x1, false, iter, error,
            "xₙ₊₁ = xₙ - f(xₙ)·(xₙ-xₙ₋₁)/(f(xₙ)-f(xₙ₋₁))",
            new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("advertencia"),
                  new LatinoString($"Se alcanzó maxIter = {maxIter}. Verifica la función o cambia los puntos iniciales.") }
            }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. Brent (Brenth) — el más robusto
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.brent",
        Args = "funcion, a, b, [tolerancia], [maxIter]",
        Help  = "Encuentra una raíz de f(x) en [a, b] por el método de Brent.\n" +
                "Combina Bisección (garantía) + Interpolación cuadrática inversa (velocidad).\n" +
                "Es el método recomendado cuando no tienes la derivada analítica.\n" +
                "Requiere f(a)·f(b) < 0 (Teorema de Bolzano).\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-12), maxIter (defecto 10 000).\n\n" +
                "Convergencia: superlineal garantizada, cuadrática en la práctica.")]
    public static LatinoObject Brent(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "raices.brent requiere: funcion, a, b. " +
                                          "Opcionalmente: tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a      = GetDouble(args[1]);
        double b      = GetDouble(args[2]);
        double tol    = args.Count > 3 ? GetDouble(args[3]) : 1e-12;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000;

        if (tol <= 0)  throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "maxIter debe ser al menos 1.");
        if (a >= b)    throw new RuntimeError(null!, "El intervalo requiere a < b.");

        var pool = new List<LatinoObject> { null! };

        double fa = Evaluar(func, interp, pool, a);
        double fb = Evaluar(func, interp, pool, b);

        if (fa * fb > 0)
            throw new RuntimeError(null!,
                $"f(a) = {fa:G} y f(b) = {fb:G} tienen el mismo signo. " +
                "Bolzano no garantiza raíz en [a, b].");

        // Asegurar que |f(b)| ≤ |f(a)| (b es siempre la mejor aproximación)
        if (Math.Abs(fa) < Math.Abs(fb)) { (a, b) = (b, a); (fa, fb) = (fb, fa); }

        double c  = a, fc = fa;
        double d  = 0, s = 0, fs = 0;
        bool   mFlag = true;
        double error  = Math.Abs(b - a);
        int    iter   = 0;

        while (iter < maxIter && Math.Abs(fb) > tol && Math.Abs(b - a) > tol)
        {
            if (Math.Abs(fa - fc) > 1e-15 && Math.Abs(fb - fc) > 1e-15)
            {
                // Interpolación cuadrática inversa (IQI)
                s = a * fb * fc / ((fa - fb) * (fa - fc))
                  + b * fa * fc / ((fb - fa) * (fb - fc))
                  + c * fa * fb / ((fc - fa) * (fc - fb));
            }
            else
            {
                // Secante
                s = b - fb * (b - a) / (fb - fa);
            }

            // Condiciones para rechazar s y usar bisección en su lugar
            double mid = (a + b) / 2.0;
            bool cond1 = s < (3 * a + b) / 4.0 || s > b;
            bool cond2 = mFlag  && Math.Abs(s - b) >= Math.Abs(b - c) / 2.0;
            bool cond3 = !mFlag && Math.Abs(s - b) >= Math.Abs(c - d) / 2.0;
            bool cond4 = mFlag  && Math.Abs(b - c) < tol;
            bool cond5 = !mFlag && Math.Abs(c - d) < tol;

            if (cond1 || cond2 || cond3 || cond4 || cond5)
            {
                s     = mid;
                mFlag = true;
            }
            else
            {
                mFlag = false;
            }

            fs = Evaluar(func, interp, pool, s);
            d  = c;
            c  = b; fc = fb;

            if (fa * fs < 0) { b = s; fb = fs; }
            else             { a = s; fa = fs; }

            if (Math.Abs(fa) < Math.Abs(fb)) { (a, b) = (b, a); (fa, fb) = (fb, fa); }

            error = Math.Abs(b - a);
            iter++;
        }

        bool convergio = Math.Abs(fb) <= tol || Math.Abs(b - a) <= tol;
        var  extra     = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("f_en_raiz"), new LatinoNumber(fb) }
        });
        if (!convergio)
            extra.Pairs[new LatinoString("advertencia")] =
                new LatinoString($"Se alcanzó max_iter = {maxIter} sin converger.");

        return RetornoIterativo(b, convergio, iter, error,
            "IQI / Secante + Bisección (Brent)", extra);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. Punto Fijo (Iteración de punto fijo)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.puntoFijo",
        Args = "funcionG, x0, [tolerancia], [maxIter]",
        Help  = "Calcula un punto fijo x* tal que g(x*) = x*, por iteración x_{n+1} = g(x_n).\n" +
                "Para encontrar raíces de f(x), reescribe como g(x) = x - f(x) (u otra forma).\n" +
                "CONDICIÓN DE CONVERGENCIA: |g'(x*)| < 1 cerca del punto fijo.\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-10), maxIter (defecto 10 000).\n\n" +
                "Fórmula: xₙ₊₁ = g(xₙ) — convergencia lineal.")]
    public static LatinoObject PuntoFijo(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "raices.punto_fijo requiere: funcion_g, x0. " +
                                          "Opcionalmente: tolerancia, max_iter.");

        var g = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino g(x).");

        double x0     = GetDouble(args[1]);
        double tol    = args.Count > 2 ? GetDouble(args[2]) : 1e-10;
        int    maxIter = args.Count > 3 ? (int)GetDouble(args[3]) : 10_000;

        if (tol <= 0)    throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "max_iter debe ser al menos 1.");

        var pool  = new List<LatinoObject> { null! };
        double x  = x0;
        double error = double.MaxValue;
        int    iter  = 0;

        while (iter < maxIter)
        {
            double xNew = Evaluar(g, interp, pool, x);
            error = Math.Abs(xNew - x);
            iter++;
            x = xNew;

            if (error < tol)
                return RetornoIterativo(x, true, iter, error, "xₙ₊₁ = g(xₙ)");
        }

        return RetornoIterativo(x, false, iter, error, "xₙ₊₁ = g(xₙ)",
            new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("advertencia"),
                  new LatinoString($"Se alcanzó max_iter = {maxIter}. " +
                                   "Verifica que |g'(x*)| < 1 (condición de convergencia).") }
            }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. Regula Falsi (Falsa Posición)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.regulaFalsi",
        Args = "funcion, a, b, [tolerancia], [maxIter]",
        Help  = "Encuentra una raíz de f(x) en [a, b] por el método de Regula Falsi.\n" +
                "Similar a Bisección pero usa la cuerda entre (a, f(a)) y (b, f(b)) en lugar del punto medio.\n" +
                "Requiere f(a)·f(b) < 0 (Teorema de Bolzano).\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-10), maxIter (defecto 10 000).\n\n" +
                "Fórmula: c = a - f(a)·(b-a)/(f(b)-f(a))")]
    public static LatinoObject RegulaFalsi(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "raices.regula_falsi requiere: funcion, a, b. " +
                                          "Opcionalmente: tolerancia, max_iter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a      = GetDouble(args[1]);
        double b      = GetDouble(args[2]);
        double tol    = args.Count > 3 ? GetDouble(args[3]) : 1e-10;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000;

        if (tol <= 0)  throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "max_iter debe ser al menos 1.");
        if (a >= b)    throw new RuntimeError(null!, "El intervalo requiere a < b.");

        var pool = new List<LatinoObject> { null! };

        double fa = Evaluar(func, interp, pool, a);
        double fb = Evaluar(func, interp, pool, b);

        if (fa * fb > 0)
            throw new RuntimeError(null!,
                $"f(a) = {fa:G} y f(b) = {fb:G} tienen el mismo signo. " +
                "Bolzano no garantiza raíz en [a, b].");

        double c = a, fc = fa;
        double error = b - a;
        int    iter  = 0;

        while (iter < maxIter)
        {
            double denom = fb - fa;
            if (Math.Abs(denom) < 1e-15)
                throw new RuntimeError(null!,
                    "f(b) - f(a) ≈ 0. La función es casi constante en el intervalo.");

            c     = a - fa * (b - a) / denom;
            fc    = Evaluar(func, interp, pool, c);
            error = Math.Abs(fc);
            iter++;

            if (error < tol || Math.Abs(b - a) < tol)
                return RetornoIterativo(c, true, iter, Math.Abs(b - a),
                    "c = a - f(a)·(b-a)/(f(b)-f(a))");

            if (fa * fc < 0) { b = c; fb = fc; }
            else             { a = c; fa = fc; }
        }

        return RetornoIterativo(c, false, iter, Math.Abs(b - a),
            "c = a - f(a)·(b-a)/(f(b)-f(a))",
            new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("advertencia"),
                  new LatinoString($"Se alcanzó max_iter = {maxIter}. " +
                                   "Regula Falsi puede converger lentamente en funciones muy curvas.") }
            }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. Múltiples raíces (barrido de intervalo)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "raices.buscarIntervalos",
        Args = "funcion, a, b, numSubintervalos",
        Help  = "Escanea [a, b] dividiendo en N subintervalos y detecta dónde f cambia de signo.\n" +
                "Devuelve una lista de pares [a_i, b_i] donde probablemente existe una raíz.\n" +
                "Usa estos pares como entrada para raices.biseccion o raices.brent.\n\n" +
                "Útil para funciones con múltiples raíces — no converge, solo detecta.")]
    public static LatinoObject BuscarIntervalos(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!, "raices.buscar_intervalos requiere: funcion, a, b, num_subintervalos.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a  = GetDouble(args[1]);
        double b  = GetDouble(args[2]);
        int    n  = (int)GetDouble(args[3]);

        if (a >= b) throw new RuntimeError(null!, "El intervalo requiere a < b.");
        if (n < 2)  throw new RuntimeError(null!, "num_subintervalos debe ser al menos 2.");
        if (n > 100_000) throw new RuntimeError(null!, "num_subintervalos no debe superar 100 000.");

        var pool   = new List<LatinoObject> { null! };
        var result = new LatinoList(new List<LatinoObject>());
        double h   = (b - a) / n;
        double xPrev  = a;
        double fPrev  = Evaluar(func, interp, pool, xPrev);

        for (int i = 1; i <= n; i++)
        {
            double xCurr = a + i * h;
            double fCurr = Evaluar(func, interp, pool, xCurr);

            if (fPrev * fCurr <= 0)   // cambio de signo (o cero exacto)
            {
                var par = new LatinoList(new List<LatinoObject>());
                par.Elements.Add(new LatinoNumber(xPrev));
                par.Elements.Add(new LatinoNumber(xCurr));
                result.Elements.Add(par);
            }

            xPrev = xCurr;
            fPrev = fCurr;
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("intervalos"),          result },
            { new LatinoString("num_raices_posibles"), new LatinoNumber(result.Elements.Count) },
            { new LatinoString("formula"),
              new LatinoString("Barrido de signo en N subintervalos de [a, b]") },
            { new LatinoString("nota"),
              new LatinoString("Cada par [a_i, b_i] contiene al menos una raíz. " +
                               "Pasa cada par a raices.brent o raices.biseccion para precisión.") }
        };
        return new LatinoDictionary(dict);
    }
}
