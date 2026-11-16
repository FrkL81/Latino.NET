using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class OptimizacionFormulas
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private static double GetDouble(LatinoObject obj) =>
        obj is LatinoNumber n ? n.Value
            : throw new RuntimeError(null!, "Se esperaba un número.");

    /// <summary>
    /// Evalúa f(x) escalar con Object Pooling.
    /// Pool de 1 elemento reutilizado en todo el bucle de optimización.
    /// </summary>
    private static double Eval1D(
        ICallable func,
        Interpreter interp,
        List<LatinoObject> pool,
        double x)
    {
        pool[0] = new LatinoNumber(x);
        var res = func.Call(interp, pool);
        double val = GetDouble(res);
        if (double.IsNaN(val) || double.IsInfinity(val))
            throw new RuntimeError(null!,
                $"f({x:G}) retornó un valor no finito. Verifica singularidades.");
        return val;
    }

    /// <summary>
    /// Evalúa f(X) multivariable con Object Pooling.
    /// Muta una LatinoList existente para evitar allocations en cada llamada.
    /// </summary>
    private static double EvalND(
        ICallable func,
        Interpreter interp,
        List<LatinoObject> poolArgs,
        LatinoList poolX,
        double[] x)
    {
        for (int i = 0; i < x.Length; i++)
            poolX.Elements[i] = new LatinoNumber(x[i]);
        var res = func.Call(interp, poolArgs);
        double val = GetDouble(res);
        if (double.IsNaN(val) || double.IsInfinity(val))
            throw new RuntimeError(null!, "f(X) retornó un valor no finito.");
        return val;
    }

    /// <summary>Gradiente numérico de f en x por diferencias centrales.</summary>
    private static double[] GradND(
        ICallable func,
        Interpreter interp,
        List<LatinoObject> poolArgs,
        LatinoList poolX,
        double[] x,
        double h = 1e-5)
    {
        int    n    = x.Length;
        var    grad = new double[n];
        for (int i = 0; i < n; i++)
        {
            var xp = (double[])x.Clone(); xp[i] += h;
            var xm = (double[])x.Clone(); xm[i] -= h;
            grad[i] = (EvalND(func, interp, poolArgs, poolX, xp)
                     - EvalND(func, interp, poolArgs, poolX, xm)) / (2.0 * h);
        }
        return grad;
    }

    /// <summary>Norma euclidiana de un vector.</summary>
    private static double Norma(double[] v)
    {
        double s = 0; foreach (var vi in v) s += vi * vi; return Math.Sqrt(s);
    }

    /// <summary>double[] → LatinoList de LatinoNumber.</summary>
    private static LatinoList VecALista(double[] v)
    {
        var l = new LatinoList(new List<LatinoObject>());
        foreach (var d in v) l.Elements.Add(new LatinoNumber(d));
        return l;
    }

    /// <summary>Construye el diccionario de retorno estándar para optimización.</summary>
    private static LatinoDictionary RetornoOpt(
        double[] xOpt,
        double   fOpt,
        bool     convergio,
        int      iteraciones,
        double   errorEst,
        string   formula,
        LatinoDictionary? extras = null)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("optimo"),         VecALista(xOpt) },
            { new LatinoString("f_optimo"),       new LatinoNumber(fOpt) },
            { new LatinoString("convergio"),      new LatinoBoolean(convergio) },
            { new LatinoString("iteraciones"),    new LatinoNumber(iteraciones) },
            { new LatinoString("error_estimado"), new LatinoNumber(errorEst) },
            { new LatinoString("formula"),        new LatinoString(formula) }
        };
        if (extras != null)
            foreach (var kvp in extras.Pairs)
                dict[kvp.Key] = kvp.Value;
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // OPTIMIZACIÓN 1D
    // ─────────────────────────────────────────────────────────────────────────

    // 1. Sección Áurea ────────────────────────────────────────────────────────

    [LatinoExtension(
        "optimizacion.seccionAurea",
        Args = "funcion, a, b, [tolerancia], [maxIter]",
        Help  = "Minimiza f(x) en el intervalo [a, b] usando el Método de Sección Áurea.\n" +
                "No requiere derivadas. Garantiza convergencia si f es unimodal en [a, b].\n" +
                "Reduce el intervalo en un factor φ = (√5-1)/2 ≈ 0.618 por iteración.\n\n" +
                "Parámetros opcionales: tolerancia (defecto: 1e-8), maxIter (defecto: 1 000).\n\n" +
                "Fórmula: x₁ = a + (1-φ)(b-a),  x₂ = a + φ(b-a)\n" +
                "         Si f(x₁) > f(x₂): a = x₁  sino: b = x₂\n" +
                "ADVERTENCIA: Solo encuentra mínimos locales. Para máximos pasa -f(x).")]
    public static LatinoObject SeccionAurea(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!,
                "optimizacion.seccionAurea requiere: funcion, a, b. Opcionalmente: tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(x).");

        double a       = GetDouble(args[1]);
        double b       = GetDouble(args[2]);
        double tol     = args.Count > 3 ? GetDouble(args[3]) : 1e-8;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 1_000;

        if (a >= b)   throw new RuntimeError(null!, "El intervalo requiere a < b.");
        if (tol <= 0) throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");

        const double PHI  = 0.6180339887498949; // (√5-1)/2
        const double PHI2 = 0.3819660112501051; // 1 - PHI

        var    pool = new List<LatinoObject> { null! };
        double x1   = a + PHI2 * (b - a);
        double x2   = a + PHI  * (b - a);
        double f1   = Eval1D(func, interp, pool, x1);
        double f2   = Eval1D(func, interp, pool, x2);

        int iter = 0;
        while (iter < maxIter && (b - a) > tol)
        {
            if (f1 > f2)
            {
                a  = x1;
                x1 = x2; f1 = f2;
                x2 = a + PHI * (b - a);
                f2 = Eval1D(func, interp, pool, x2);
            }
            else
            {
                b  = x2;
                x2 = x1; f2 = f1;
                x1 = a + PHI2 * (b - a);
                f1 = Eval1D(func, interp, pool, x1);
            }
            iter++;
        }

        double xOpt  = (a + b) / 2.0;
        double fOpt  = Eval1D(func, interp, pool, xOpt);
        bool   conv  = (b - a) <= tol;

        var res = RetornoOpt(
            new[] { xOpt }, fOpt, conv, iter, b - a,
            "x₁ = a+(1-φ)(b-a), x₂ = a+φ(b-a), φ = (√5-1)/2");
        if (!conv)
            res.Pairs[new LatinoString("advertencia")] =
                new LatinoString($"Se alcanzó maxIter = {maxIter}. La función puede no ser unimodal.");
        return res;
    }

    // 2. Interpolación Cuadrática (Brent 1D) ─────────────────────────────────

    [LatinoExtension(
        "optimizacion.brent1D",
        Args = "funcion, a, b, [tolerancia], [maxIter]",
        Help  = "Minimiza f(x) en [a, b] con el Método de Brent para optimización 1D.\n" +
                "Combina Sección Áurea (robustez) + Interpolación Cuadrática (velocidad).\n" +
                "El método más robusto para minimización 1D sin derivadas.\n\n" +
                "Parámetros opcionales: tolerancia (defecto: 1e-10), maxIter (defecto: 500).\n\n" +
                "Convergencia: superlineal — mucho más rápido que Sección Áurea.\n" +
                "Para máximos pasa -f(x).")]
    public static LatinoObject Brent1D(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!,
                "optimizacion.brent1D requiere: funcion, a, b. Opcionalmente: tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(x).");

        double a       = GetDouble(args[1]);
        double b       = GetDouble(args[2]);
        double tol     = args.Count > 3 ? GetDouble(args[3]) : 1e-10;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 500;

        if (a >= b)   throw new RuntimeError(null!, "El intervalo requiere a < b.");
        if (tol <= 0) throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");

        const double PHI2 = 0.3819660112501051;
        const double CGOLD = PHI2;
        const double ZEPS  = 1e-10;

        var    pool = new List<LatinoObject> { null! };
        double x    = a + CGOLD * (b - a);
        double w    = x, v = x;
        double fx   = Eval1D(func, interp, pool, x);
        double fw   = fx, fv = fx;
        double d    = 0, e = 0;

        int iter = 0;
        while (iter < maxIter)
        {
            double xm   = 0.5 * (a + b);
            double tol1 = tol * Math.Abs(x) + ZEPS;
            double tol2 = 2.0 * tol1;

            if (Math.Abs(x - xm) <= tol2 - 0.5 * (b - a)) break;

            double p = 0, q = 0, r = 0;
            if (Math.Abs(e) > tol1)
            {
                // Intentar interpolación cuadrática
                r = (x - w) * (fx - fv);
                q = (x - v) * (fx - fw);
                p = (x - v) * q - (x - w) * r;
                q = 2.0 * (q - r);
                if (q > 0) p = -p; else q = -q;
                r = e; e = d;

                if (Math.Abs(p) < Math.Abs(0.5 * q * r)
                    && p > q * (a - x)
                    && p < q * (b - x))
                {
                    d = p / q;
                    if ((x + d - a) < tol2 || (b - (x + d)) < tol2)
                        d = x < xm ? tol1 : -tol1;
                }
                else
                {
                    e = x < xm ? b - x : a - x;
                    d = CGOLD * e;
                }
            }
            else
            {
                e = x < xm ? b - x : a - x;
                d = CGOLD * e;
            }

            double step = Math.Abs(d) >= tol1 ? d : (d > 0 ? tol1 : -tol1);
            double u    = x + step;
            double fu   = Eval1D(func, interp, pool, u);
            iter++;

            if (fu <= fx)
            {
                if (u < x) b = x; else a = x;
                v = w; fv = fw; w = x; fw = fx; x = u; fx = fu;
            }
            else
            {
                if (u < x) a = u; else b = u;
                if (fu <= fw || w == x) { v = w; fv = fw; w = u; fw = fu; }
                else if (fu <= fv || v == x || v == w) { v = u; fv = fu; }
            }
        }

        bool conv = Math.Abs(x - (a + b) / 2.0) <= tol || iter < maxIter;

        var res = RetornoOpt(new[] { x }, fx, conv, iter, b - a,
            "Brent 1D: Sección Áurea + Interpolación Cuadrática");
        if (!conv)
            res.Pairs[new LatinoString("advertencia")] =
                new LatinoString($"Se alcanzó maxIter = {maxIter}.");
        return res;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // OPTIMIZACIÓN MULTIDIMENSIONAL — BASADA EN GRADIENTE
    // ─────────────────────────────────────────────────────────────────────────

    // 3. Descenso de Gradiente ────────────────────────────────────────────────

    [LatinoExtension(
        "optimizacion.descensoGradiente",
        Args = "funcion, listaX0, [tasa], [tolerancia], [maxIter]",
        Help  = "Minimiza f(X) con el Método de Descenso de Gradiente (Steepest Descent).\n" +
                "funcion:   función Latino f(X) donde X es una lista de n valores.\n" +
                "listaX0:  punto inicial [x₁, x₂, ..., xₙ].\n" +
                "tasa:      tasa de aprendizaje α (defecto: 0.01).\n" +
                "           Si la función diverge, reduce α.\n" +
                "tolerancia: ‖∇f‖ < tol como criterio de parada (defecto: 1e-6).\n" +
                "maxIter:  (defecto: 10 000).\n\n" +
                "Fórmula: xₙ₊₁ = xₙ - α·∇f(xₙ)\n" +
                "Convergencia lineal cerca del óptimo. Para convergencia cuadrática usa " +
                "optimizacion.bfgs.")]
    public static LatinoObject DescensoGradiente(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2 || args[1] is not LatinoList lX0)
            throw new RuntimeError(null!,
                "optimizacion.descensoGradiente requiere: funcion, listaX0. " +
                "Opcionalmente: tasa, tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(X).");

        double alpha   = args.Count > 2 ? GetDouble(args[2]) : 0.01;
        double tol     = args.Count > 3 ? GetDouble(args[3]) : 1e-6;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000;

        if (alpha <= 0) throw new RuntimeError(null!, "La tasa de aprendizaje debe ser mayor a cero.");
        if (tol   <= 0) throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");

        int n = lX0.Elements.Count;
        if (n < 1) throw new RuntimeError(null!, "listaX0 no puede estar vacía.");

        var x      = new double[n];
        for (int i = 0; i < n; i++) x[i] = GetDouble(lX0.Elements[i]);

        var poolX    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) poolX.Elements.Add(new LatinoNumber(x[i]));
        var poolArgs = new List<LatinoObject> { poolX };

        double fVal   = EvalND(func, interp, poolArgs, poolX, x);
        double error  = double.MaxValue;
        int    iter   = 0;

        while (iter < maxIter)
        {
            var grad = GradND(func, interp, poolArgs, poolX, x);
            error    = Norma(grad);
            if (error < tol) break;

            // Actualizar x
            for (int i = 0; i < n; i++) x[i] -= alpha * grad[i];

            double fNew = EvalND(func, interp, poolArgs, poolX, x);

            // Reducir paso si la función aumentó (backtracking simple)
            if (fNew > fVal)
            {
                // Deshacer y reducir alpha
                for (int i = 0; i < n; i++) x[i] += alpha * grad[i];
                alpha *= 0.5;
                if (alpha < 1e-15)
                    throw new RuntimeError(null!,
                        "La tasa de aprendizaje colapsó a cero. " +
                        "La función puede no tener mínimo en este punto o ser muy plana.");
            }
            else
            {
                fVal = fNew;
                iter++;
            }
        }

        fVal = EvalND(func, interp, poolArgs, poolX, x);
        bool conv = error < tol;

        var res = RetornoOpt(x, fVal, conv, iter, error,
            "xₙ₊₁ = xₙ - α·∇f(xₙ)  (backtracking automático)");
        res.Pairs[new LatinoString("tasa_final")] = new LatinoNumber(alpha);
        if (!conv)
            res.Pairs[new LatinoString("advertencia")] =
                new LatinoString($"Se alcanzó maxIter = {maxIter}. ‖∇f‖ = {error:G3}. " +
                                 "Considera aumentar maxIter o usar optimizacion.bfgs.");
        return res;
    }

    // 4. BFGS (cuasi-Newton) ──────────────────────────────────────────────────

    [LatinoExtension(
        "optimizacion.bfgs",
        Args = "funcion, listaX0, [tolerancia], [maxIter]",
        Help  = "Minimiza f(X) con el método cuasi-Newton BFGS.\n" +
                "Aproxima la Hessiana inversa y la actualiza en cada paso.\n" +
                "Convergencia superlineal — mucho más rápido que Descenso de Gradiente.\n\n" +
                "funcion:   función Latino f(X) donde X es una lista de n valores.\n" +
                "listaX0:  punto inicial.\n" +
                "tolerancia: ‖∇f‖ < tol (defecto: 1e-8).\n" +
                "maxIter:  (defecto: 1 000).\n\n" +
                "Fórmula de actualización (Broyden-Fletcher-Goldfarb-Shanno):\n" +
                "  Hₖ₊₁ = (I - ρ·s·yᵀ)·Hₖ·(I - ρ·y·sᵀ) + ρ·s·sᵀ\n" +
                "  donde s = xₖ₊₁-xₖ, y = ∇fₖ₊₁-∇fₖ, ρ = 1/(yᵀs)\n" +
                "Usa búsqueda de línea de Wolfe para control del paso.")]
    public static LatinoObject BFGS(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2 || args[1] is not LatinoList lX0)
            throw new RuntimeError(null!,
                "optimizacion.bfgs requiere: funcion, listaX0. Opcionalmente: tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(X).");

        double tol     = args.Count > 2 ? GetDouble(args[2]) : 1e-8;
        int    maxIter = args.Count > 3 ? (int)GetDouble(args[3]) : 1_000;

        if (tol   <= 0) throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "maxIter debe ser al menos 1.");

        int n = lX0.Elements.Count;
        if (n < 1) throw new RuntimeError(null!, "listaX0 no puede estar vacía.");

        var x = new double[n];
        for (int i = 0; i < n; i++) x[i] = GetDouble(lX0.Elements[i]);

        var poolX    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) poolX.Elements.Add(new LatinoNumber(x[i]));
        var poolArgs = new List<LatinoObject> { poolX };

        // Hessiana inversa inicial: identidad
        var H = new double[n, n];
        for (int i = 0; i < n; i++) H[i, i] = 1.0;

        double[] g    = GradND(func, interp, poolArgs, poolX, x);
        double   fVal = EvalND(func, interp, poolArgs, poolX, x);
        double   error = Norma(g);
        int      iter  = 0;

        while (iter < maxIter && error > tol)
        {
            // Dirección de descenso: d = -H·g
            var d = new double[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    d[i] -= H[i, j] * g[j];

            // Búsqueda de línea de Armijo (backtracking)
            double alpha = 1.0;
            double c1    = 1e-4;
            double dg    = 0; for (int i = 0; i < n; i++) dg += d[i] * g[i];

            for (int ls = 0; ls < 50; ls++)
            {
                var xNew = new double[n];
                for (int i = 0; i < n; i++) xNew[i] = x[i] + alpha * d[i];
                double fNew = EvalND(func, interp, poolArgs, poolX, xNew);
                if (fNew <= fVal + c1 * alpha * dg) break;
                alpha *= 0.5;
                if (alpha < 1e-15) break;
            }

            // Nuevo punto
            var xPlus = new double[n];
            for (int i = 0; i < n; i++) xPlus[i] = x[i] + alpha * d[i];

            double[]  gPlus  = GradND(func, interp, poolArgs, poolX, xPlus);
            double    fPlus  = EvalND(func, interp, poolArgs, poolX, xPlus);

            // s = x_{k+1} - x_k,  y = g_{k+1} - g_k
            var s = new double[n]; for (int i = 0; i < n; i++) s[i] = xPlus[i] - x[i];
            var y = new double[n]; for (int i = 0; i < n; i++) y[i] = gPlus[i] - g[i];
            double ys = 0; for (int i = 0; i < n; i++) ys += y[i] * s[i];

            // Actualización BFGS de la Hessiana inversa
            if (ys > 1e-12 * Norma(s) * Norma(y))
            {
                double rho = 1.0 / ys;

                // Hₖ₊₁ = (I - ρ·s·yᵀ)·Hₖ·(I - ρ·y·sᵀ) + ρ·s·sᵀ
                // Paso 1: A = H·(I - ρ·y·sᵀ)  → A[i,j] = H[i,j] - ρ·(H·y)[i]·s[j]
                var Hy = new double[n]; for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) Hy[i] += H[i, j] * y[j];
                var Hnew = new double[n, n];
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                    {
                        double A = H[i, j] - rho * Hy[i] * s[j];
                        // Paso 2: Hₙₑᵥ = (I - ρ·s·yᵀ)·A + ρ·s·sᵀ
                        // (I - ρ·s·yᵀ)·A[col j] = A[i,j] - ρ·s[i]·(yᵀ·A_col_j)
                        // Se computa completo abajo como producto
                        Hnew[i, j] = A;
                    }
                // Multiplicar por (I - ρ·s·yᵀ) desde la izquierda
                var Hnew2 = new double[n, n];
                for (int i = 0; i < n; i++)
                {
                    double yHrow = 0; for (int k = 0; k < n; k++) yHrow += y[k] * Hnew[k, i]; // columna i de A
                    for (int j = 0; j < n; j++)
                        Hnew2[j, i] = Hnew[j, i] - rho * s[j] * yHrow;
                }
                // Añadir ρ·s·sᵀ
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        H[i, j] = Hnew2[i, j] + rho * s[i] * s[j];
            }

            x    = xPlus;
            g    = gPlus;
            fVal = fPlus;
            error = Norma(g);
            iter++;
        }

        bool conv = error < tol;
        var  res  = RetornoOpt(x, fVal, conv, iter, error,
            "Hₖ₊₁ = (I-ρsyᵀ)Hₖ(I-ρysᵀ) + ρssᵀ  (BFGS + Armijo)");
        if (!conv)
            res.Pairs[new LatinoString("advertencia")] =
                new LatinoString($"Se alcanzó maxIter = {maxIter}. ‖∇f‖ = {error:G3}.");
        return res;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // OPTIMIZACIÓN SIN GRADIENTE
    // ─────────────────────────────────────────────────────────────────────────

    // 5. Método de Nelder-Mead (Simplex) ─────────────────────────────────────

    [LatinoExtension(
        "optimizacion.nelderMead",
        Args = "funcion, listaX0, [escalaInicial], [tolerancia], [maxIter]",
        Help  = "Minimiza f(X) con el Método de Nelder-Mead (Simplex Downhill).\n" +
                "No usa gradientes — robusto para funciones ruidosas o no diferenciables.\n\n" +
                "funcion:        función Latino f(X) donde X es una lista de n valores.\n" +
                "listaX0:       vértice inicial del simplex.\n" +
                "escalaInicial: tamaño del simplex inicial (defecto: 0.05 para componentes\n" +
                "                no nulas, 0.00025 para nulas).\n" +
                "tolerancia:     ‖desviación de f en el simplex‖ < tol (defecto: 1e-8).\n" +
                "maxIter:       (defecto: 10 000 × n).\n\n" +
                "Operaciones: Reflexión (ρ=1), Expansión (χ=2), Contracción (γ=0.5), Reducción.\n" +
                "Convergencia típica: O(1/k) — lento pero muy robusto.")]
    public static LatinoObject NelderMead(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2 || args[1] is not LatinoList lX0)
            throw new RuntimeError(null!,
                "optimizacion.nelderMead requiere: funcion, listaX0. " +
                "Opcionalmente: escalaInicial, tolerancia, maxIter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(X).");

        int    n       = lX0.Elements.Count;
        double escala  = args.Count > 2 ? GetDouble(args[2]) : 0.0; // 0 = automático
        double tol     = args.Count > 3 ? GetDouble(args[3]) : 1e-8;
        int    maxIter = args.Count > 4 ? (int)GetDouble(args[4]) : 10_000 * n;

        if (n < 1)  throw new RuntimeError(null!, "listaX0 no puede estar vacía.");
        if (tol <= 0) throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");

        var poolX    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) poolX.Elements.Add(new LatinoNumber(0.0));
        var poolArgs = new List<LatinoObject> { poolX };

        // ── Construir el simplex inicial (n+1 vértices) ──
        int   m    = n + 1;
        var   sim  = new double[m][];
        var   fsim = new double[m];

        for (int i = 0; i < n; i++) sim[0] = new double[n]; // aloca vértice 0
        for (int i = 0; i < n; i++) sim[0][i] = GetDouble(lX0.Elements[i]);
        fsim[0] = EvalND(func, interp, poolArgs, poolX, sim[0]);

        for (int j = 1; j <= n; j++)
        {
            sim[j] = (double[])sim[0].Clone();
            double step = escala > 0 ? escala
                         : (Math.Abs(sim[0][j-1]) > 1e-10 ? 0.05 * Math.Abs(sim[0][j-1]) : 0.00025);
            sim[j][j-1] += step;
            fsim[j] = EvalND(func, interp, poolArgs, poolX, sim[j]);
        }

        // Parámetros de NM
        const double ALPHA = 1.0;  // reflexión
        const double GAMMA = 2.0;  // expansión
        const double BETA  = 0.5;  // contracción
        const double SIGMA = 0.5;  // reducción

        int iter = 0;
        while (iter < maxIter)
        {
            // Ordenar vértices por f (mejor primero)
            Array.Sort(fsim, sim);

            // Criterio de convergencia: desviación estándar de f en el simplex
            double fMean = 0; for (int i = 0; i < m; i++) fMean += fsim[i]; fMean /= m;
            double fStd  = 0; for (int i = 0; i < m; i++) fStd += (fsim[i]-fMean)*(fsim[i]-fMean);
            fStd = Math.Sqrt(fStd / m);
            if (fStd < tol) break;

            // Centroide de los n mejores vértices (excluye el peor)
            var xBar = new double[n];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                    xBar[i] += sim[j][i] / n;

            double[] xWorst = sim[n];
            double   fWorst = fsim[n];
            double[] xBest  = sim[0];

            // Reflexión
            var xR = new double[n];
            for (int i = 0; i < n; i++) xR[i] = xBar[i] + ALPHA * (xBar[i] - xWorst[i]);
            double fR = EvalND(func, interp, poolArgs, poolX, xR);

            if (fR < fsim[0])
            {
                // Expansión
                var xE = new double[n];
                for (int i = 0; i < n; i++) xE[i] = xBar[i] + GAMMA * (xR[i] - xBar[i]);
                double fE = EvalND(func, interp, poolArgs, poolX, xE);
                if (fE < fR) { sim[n] = xE; fsim[n] = fE; }
                else         { sim[n] = xR; fsim[n] = fR; }
            }
            else if (fR < fsim[n - 1])
            {
                // Reflexión aceptada directamente
                sim[n] = xR; fsim[n] = fR;
            }
            else
            {
                // Contracción
                bool contraerExterno = fR < fWorst;
                var xC = new double[n];
                if (contraerExterno)
                    for (int i = 0; i < n; i++) xC[i] = xBar[i] + BETA * (xR[i] - xBar[i]);
                else
                    for (int i = 0; i < n; i++) xC[i] = xBar[i] + BETA * (xWorst[i] - xBar[i]);

                double fC = EvalND(func, interp, poolArgs, poolX, xC);

                if (fC < (contraerExterno ? fR : fWorst))
                {
                    sim[n] = xC; fsim[n] = fC;
                }
                else
                {
                    // Reducción del simplex hacia el mejor
                    for (int j = 1; j <= n; j++)
                    {
                        for (int i = 0; i < n; i++)
                            sim[j][i] = xBest[i] + SIGMA * (sim[j][i] - xBest[i]);
                        fsim[j] = EvalND(func, interp, poolArgs, poolX, sim[j]);
                    }
                }
            }

            iter++;
        }

        // Ordenar final
        Array.Sort(fsim, sim);
        double errorFinal = 0;
        for (int i = 1; i < m; i++) errorFinal += (fsim[i]-fsim[0])*(fsim[i]-fsim[0]);
        errorFinal = Math.Sqrt(errorFinal / n);

        bool conv = errorFinal < tol;
        var  res  = RetornoOpt(sim[0], fsim[0], conv, iter, errorFinal,
            "Reflexión(1) + Expansión(2) + Contracción(0.5) + Reducción(0.5)  (Nelder-Mead)");
        if (!conv)
            res.Pairs[new LatinoString("advertencia")] =
                new LatinoString($"Se alcanzó maxIter = {maxIter}. " +
                                 "Aumenta maxIter o verifica que el mínimo exista en la región.");
        return res;
    }

    // 6. Algoritmo Genético (optimización global) ─────────────────────────────

    [LatinoExtension(
        "optimizacion.genetico",
        Args = "funcion, listaLimitesInf, listaLimitesSup, " +
               "[tamPoblacion], [generaciones], [tolerancia], [semilla]",
        Help  = "Minimiza f(X) mediante un Algoritmo Genético de valor real.\n" +
                "Búsqueda global — puede escapar de mínimos locales.\n\n" +
                "funcion:           f(X) donde X es una lista de n valores.\n" +
                "listaLimitesInf: límites inferiores [lb₁, lb₂, ..., lbₙ].\n" +
                "listaLimitesSup: límites superiores [ub₁, ub₂, ..., ubₙ].\n" +
                "Parámetros opcionales:\n" +
                "  tamPoblacion: individuos por generación (defecto: 50).\n" +
                "  generaciones:  número de generaciones (defecto: 200).\n" +
                "  tolerancia:    convergencia de la mejor f (defecto: 1e-8).\n" +
                "  semilla:       semilla aleatoria para reproducibilidad.\n\n" +
                "Operadores: selección por torneo (k=3), cruce aritmético (α∈[0,1]),\n" +
                "cruce BLX-α, mutación gaussiana adaptativa.\n" +
                "Para refinar el resultado usa optimizacion.bfgs o optimizacion.nelderMead.")]
    public static LatinoObject Genetico(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3
            || args[1] is not LatinoList lLB
            || args[2] is not LatinoList lUB)
            throw new RuntimeError(null!,
                "optimizacion.genetico requiere: funcion, listaLimitesInf, listaLimitesSup. " +
                "Opcionalmente: tamPoblacion, generaciones, tolerancia, semilla.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(X).");

        int n = lLB.Elements.Count;
        if (n < 1) throw new RuntimeError(null!, "listaLimitesInf no puede estar vacía.");
        if (lUB.Elements.Count != n)
            throw new RuntimeError(null!, "listaLimitesInf y listaLimitesSup deben tener la misma longitud.");

        var lb = new double[n]; for (int i = 0; i < n; i++) lb[i] = GetDouble(lLB.Elements[i]);
        var ub = new double[n]; for (int i = 0; i < n; i++) ub[i] = GetDouble(lUB.Elements[i]);
        for (int i = 0; i < n; i++)
            if (ub[i] <= lb[i])
                throw new RuntimeError(null!, $"El límite superior debe ser > inferior para la variable {i}.");

        int    popSize = args.Count > 3 ? (int)GetDouble(args[3]) : 50;
        int    nGen    = args.Count > 4 ? (int)GetDouble(args[4]) : 200;
        double tol     = args.Count > 5 ? GetDouble(args[5]) : 1e-8;
        var    rng     = args.Count > 6
            ? new Random((int)GetDouble(args[6]))
            : new Random();

        if (popSize < 4)   throw new RuntimeError(null!, "tamPoblacion debe ser al menos 4.");
        if (nGen     < 1)   throw new RuntimeError(null!, "generaciones debe ser al menos 1.");

        var poolX    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) poolX.Elements.Add(new LatinoNumber(0.0));
        var poolArgs = new List<LatinoObject> { poolX };

        // ── Inicializar población aleatoria ──
        var pop  = new double[popSize][];
        var fits = new double[popSize];
        for (int p = 0; p < popSize; p++)
        {
            pop[p] = new double[n];
            for (int i = 0; i < n; i++)
                pop[p][i] = lb[i] + rng.NextDouble() * (ub[i] - lb[i]);
            fits[p] = EvalND(func, interp, poolArgs, poolX, pop[p]);
        }

        double bestF       = double.MaxValue;
        double[] bestX     = new double[n];
        double prevBestF   = double.MaxValue;
        int    sinMejora   = 0;

        // Selección por torneo de tamaño 3
        int Torneo()
        {
            int a = rng.Next(popSize);
            int b = rng.Next(popSize);
            int c = rng.Next(popSize);
            if (fits[a] <= fits[b] && fits[a] <= fits[c]) return a;
            if (fits[b] <= fits[a] && fits[b] <= fits[c]) return b;
            return c;
        }

        for (int gen = 0; gen < nGen; gen++)
        {
            // Actualizar mejor
            for (int p = 0; p < popSize; p++)
            {
                if (fits[p] < bestF) { bestF = fits[p]; bestX = (double[])pop[p].Clone(); }
            }

            // Criterio de convergencia
            if (Math.Abs(prevBestF - bestF) < tol) { sinMejora++; if (sinMejora > 20) break; }
            else { sinMejora = 0; prevBestF = bestF; }

            // Mutación adaptativa: sigma se reduce conforme avanzan generaciones
            double sigma = 0.3 * (1.0 - (double)gen / nGen);

            var newPop  = new double[popSize][];
            var newFits = new double[popSize];

            // Elitismo: conservar el mejor individuo
            newPop[0]  = (double[])bestX.Clone();
            newFits[0] = bestF;

            for (int p = 1; p < popSize; p++)
            {
                // Selección por torneo
                int p1 = Torneo(), p2 = Torneo();
                double alpha = rng.NextDouble();

                // Cruce aritmético BLX-α
                var hijo = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double d    = Math.Abs(pop[p1][i] - pop[p2][i]);
                    double low  = Math.Min(pop[p1][i], pop[p2][i]) - alpha * d;
                    double high = Math.Max(pop[p1][i], pop[p2][i]) + alpha * d;
                    hijo[i]     = low + rng.NextDouble() * (high - low);

                    // Mutación gaussiana
                    if (rng.NextDouble() < 0.15)
                        hijo[i] += sigma * (ub[i] - lb[i]) * (rng.NextDouble() * 2.0 - 1.0);

                    // Clamp al dominio
                    hijo[i] = Math.Max(lb[i], Math.Min(ub[i], hijo[i]));
                }

                newPop[p]  = hijo;
                newFits[p] = EvalND(func, interp, poolArgs, poolX, hijo);
            }

            pop  = newPop;
            fits = newFits;
        }

        // Actualizar mejor final
        for (int p = 0; p < popSize; p++)
            if (fits[p] < bestF) { bestF = fits[p]; bestX = (double[])pop[p].Clone(); }

        var res = RetornoOpt(bestX, bestF, true, nGen, 0.0,
            "Torneo(3) + BLX-α + Mutación Gaussiana adaptativa  (Algoritmo Genético)");
        res.Pairs[new LatinoString("nota")] =
            new LatinoString("Para mayor precisión, refina el resultado con optimizacion.bfgs " +
                             "usando este óptimo como punto inicial.");
        return res;
    }

    // 7. Recocido Simulado ────────────────────────────────────────────────────

    [LatinoExtension(
        "optimizacion.recocidoSimulado",
        Args = "funcion, listaX0, listaLimitesInf, listaLimitesSup, " +
               "[tempInicial], [tempFinal], [enfriamiento], [maxIter], [semilla]",
        Help  = "Minimiza f(X) con el Método de Recocido Simulado (Simulated Annealing).\n" +
                "Búsqueda global estocástica — acepta soluciones peores con prob. exp(-ΔE/T).\n" +
                "Permite escapar de mínimos locales a altas temperaturas.\n\n" +
                "funcion:      f(X) donde X es una lista de n valores.\n" +
                "listaX0:     punto inicial.\n" +
                "limites:      dominio de búsqueda (listas de n valores).\n" +
                "Parámetros opcionales:\n" +
                "  tempInicial:  temperatura inicial T₀ (defecto: 100.0).\n" +
                "  tempFinal:    temperatura final Tf  (defecto: 1e-6).\n" +
                "  enfriamiento:  factor α de enfriamiento geométrico (defecto: 0.995).\n" +
                "  maxIter:      iteraciones por temperatura (defecto: 100).\n" +
                "  semilla:       semilla aleatoria.\n\n" +
                "Fórmula: P(aceptar peor) = exp(-|ΔE|/T)  |  Tₙ₊₁ = α·Tₙ")]
    public static LatinoObject RecocidoSimulado(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4
            || args[1] is not LatinoList lX0
            || args[2] is not LatinoList lLB
            || args[3] is not LatinoList lUB)
            throw new RuntimeError(null!,
                "optimizacion.recocidoSimulado requiere: funcion, listaX0, " +
                "listaLimitesInf, listaLimitesSup. " +
                "Opcionalmente: tempInicial, tempFinal, enfriamiento, maxIter, semilla.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(X).");

        int n = lX0.Elements.Count;
        if (n < 1) throw new RuntimeError(null!, "listaX0 no puede estar vacía.");

        var x  = new double[n]; for (int i = 0; i < n; i++) x[i]  = GetDouble(lX0.Elements[i]);
        var lb = new double[n]; for (int i = 0; i < n; i++) lb[i] = GetDouble(lLB.Elements[i]);
        var ub = new double[n]; for (int i = 0; i < n; i++) ub[i] = GetDouble(lUB.Elements[i]);
        for (int i = 0; i < n; i++)
            if (ub[i] <= lb[i])
                throw new RuntimeError(null!, $"límite superior debe ser > inferior para variable {i}.");

        double T0     = args.Count > 4 ? GetDouble(args[4]) : 100.0;
        double Tf     = args.Count > 5 ? GetDouble(args[5]) : 1e-6;
        double alpha  = args.Count > 6 ? GetDouble(args[6]) : 0.995;
        int    nInner = args.Count > 7 ? (int)GetDouble(args[7]) : 100;
        var    rng    = args.Count > 8
            ? new Random((int)GetDouble(args[8]))
            : new Random();

        if (T0 <= Tf)      throw new RuntimeError(null!, "tempInicial debe ser mayor que tempFinal.");
        if (alpha <= 0 || alpha >= 1) throw new RuntimeError(null!, "enfriamiento debe estar en (0, 1).");
        if (nInner < 1)    throw new RuntimeError(null!, "maxIter debe ser al menos 1.");

        var poolX    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) poolX.Elements.Add(new LatinoNumber(x[i]));
        var poolArgs = new List<LatinoObject> { poolX };

        double fCur   = EvalND(func, interp, poolArgs, poolX, x);
        double[] xBest = (double[])x.Clone();
        double   fBest = fCur;

        double T       = T0;
        int    totalIt = 0;
        int    aceptados = 0;

        while (T > Tf)
        {
            for (int k = 0; k < nInner; k++)
            {
                // Vecino aleatorio: perturbar una componente aleatoria
                var xNew = (double[])x.Clone();
                int dim  = rng.Next(n);
                xNew[dim] += (rng.NextDouble() * 2.0 - 1.0) * (ub[dim] - lb[dim]) * 0.1;
                xNew[dim]  = Math.Max(lb[dim], Math.Min(ub[dim], xNew[dim]));

                double fNew = EvalND(func, interp, poolArgs, poolX, xNew);
                double dE   = fNew - fCur;

                if (dE < 0 || rng.NextDouble() < Math.Exp(-dE / T))
                {
                    x    = xNew;
                    fCur = fNew;
                    aceptados++;
                    if (fCur < fBest) { fBest = fCur; xBest = (double[])x.Clone(); }
                }
                totalIt++;
            }
            T *= alpha;
        }

        double tasaAcept = (double)aceptados / totalIt;

        var res = RetornoOpt(xBest, fBest, true, totalIt, 0.0,
            "P(aceptar) = exp(-|ΔE|/T)  |  Tₙ₊₁ = α·Tₙ  (Recocido Simulado)");
        res.Pairs[new LatinoString("temperatura_final")] = new LatinoNumber(T);
        res.Pairs[new LatinoString("tasa_aceptacion")]   = new LatinoNumber(tasaAcept);
        res.Pairs[new LatinoString("nota")] =
            new LatinoString("Para mayor precisión refina con optimizacion.bfgs o " +
                             "optimizacion.nelderMead usando este punto como inicio.");
        return res;
    }

    // 8. Programación Lineal (Simplex de Dantzig) ────────────────────────────

    [LatinoExtension(
        "optimizacion.simplexLineal",
        Args = "listaC, matrizA, listaB",
        Help  = "Resuelve un problema de Programación Lineal en forma estándar:\n" +
                "  Minimizar   cᵀx\n" +
                "  Sujeto a    A·x ≤ b,   x ≥ 0\n\n" +
                "lista_b:    lado derecho de las restricciones (m valores, todos ≥ 0).\n\n" +
                "Usa el Método Simplex de Dantzig con variables de holgura.\n" +
                "Para maximizar, pasa -c como función objetivo.\n\n" +
                "Fórmula: pivote sobre elemento básico más negativo en la fila de costo.")]
    public static LatinoObject SimplexLineal(List<LatinoObject> args)
    {
        if (args.Count < 3
            || args[0] is not LatinoList lC
            || args[1] is not LatinoList lA
            || args[2] is not LatinoList lB)
            throw new RuntimeError(null!,
                "optimizacion.simplex_lineal requiere: lista_c, matriz_A, lista_b.");

        int n = lC.Elements.Count; // variables
        int m = lB.Elements.Count; // restricciones

        if (lA.Elements.Count != m)
            throw new RuntimeError(null!, "matriz_A debe tener m filas (igual que lista_b).");

        var c = new double[n];
        for (int j = 0; j < n; j++) c[j] = GetDouble(lC.Elements[j]);

        var A = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            if (lA.Elements[i] is not LatinoList fila || fila.Elements.Count != n)
                throw new RuntimeError(null!,
                    $"Fila {i} de matriz_A debe tener {n} columnas.");
            for (int j = 0; j < n; j++) A[i, j] = GetDouble(fila.Elements[j]);
        }

        var b = new double[m];
        for (int i = 0; i < m; i++)
        {
            b[i] = GetDouble(lB.Elements[i]);
            if (b[i] < 0)
                throw new RuntimeError(null!,
                    $"b[{i}] = {b[i]:G} es negativo. El Simplex estándar requiere b ≥ 0. " +
                    "Multiplica la restricción por -1 para invertirla.");
        }

        // ── Construir tableau aumentado con variables de holgura ──
        // Tableau: (m+1) filas × (n+m+1) columnas
        // Columnas: [x₁..xₙ | s₁..sₘ | b]
        int    cols = n + m + 1;
        var    T    = new double[m + 1, cols];
        var    base_ = new int[m]; // índices de las variables básicas

        // Filas de restricción
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++) T[i, j] = A[i, j];
            T[i, n + i]    = 1.0;   // variable de holgura
            T[i, cols - 1] = b[i];  // RHS
            base_[i]       = n + i;
        }

        // Fila de costo (negativa para minimización)
        for (int j = 0; j < n; j++) T[m, j] = c[j];

        // ── Iteraciones del Simplex ──
        int    maxIt   = 10 * (n + m);
        int    it      = 0;
        string estado  = "optimo";

        while (it < maxIt)
        {
            // Encontrar columna pivote: coeficiente más negativo en la fila de costo
            int pivCol = -1;
            double minCost = -1e-10;
            for (int j = 0; j < n + m; j++)
                if (T[m, j] < minCost) { minCost = T[m, j]; pivCol = j; }

            if (pivCol < 0) break; // todos ≥ 0 → solución óptima

            // Encontrar fila pivote: criterio de la razón mínima
            int    pivRow = -1;
            double minRatio = double.MaxValue;
            for (int i = 0; i < m; i++)
            {
                if (T[i, pivCol] > 1e-10)
                {
                    double ratio = T[i, cols - 1] / T[i, pivCol];
                    if (ratio < minRatio) { minRatio = ratio; pivRow = i; }
                }
            }

            if (pivRow < 0) { estado = "no_acotado"; break; }

            // Pivote
            double pivElem = T[pivRow, pivCol];
            for (int j = 0; j < cols; j++) T[pivRow, j] /= pivElem;
            for (int i = 0; i <= m; i++)
            {
                if (i == pivRow) continue;
                double factor = T[i, pivCol];
                for (int j = 0; j < cols; j++) T[i, j] -= factor * T[pivRow, j];
            }
            base_[pivRow] = pivCol;
            it++;
        }

        if (it >= maxIt) estado = "max_iter";

        // Extraer solución
        var xOpt = new double[n];
        for (int i = 0; i < m; i++)
            if (base_[i] < n) xOpt[base_[i]] = T[i, cols - 1];

        double fOpt = T[m, cols - 1];

        // Valores de las variables de holgura (para análisis de restricciones activas)
        var holguras = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < m; i++)
        {
            double s = 0;
            for (int k = 0; k < m; k++)
                if (base_[k] == n + i) { s = T[k, cols - 1]; break; }
            holguras.Elements.Add(new LatinoNumber(s));
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("solucion"),    VecALista(xOpt) },
            { new LatinoString("f_optimo"),    new LatinoNumber(fOpt) },
            { new LatinoString("estado"),      new LatinoString(estado) },
            { new LatinoString("iteraciones"), new LatinoNumber(it) },
            { new LatinoString("holguras"),    holguras },
            { new LatinoString("formula"),
              new LatinoString("Simplex de Dantzig: pivote en coef. más negativo de la fila de costo") }
        };
        if (estado != "optimo")
            dict[new LatinoString("advertencia")] =
                new LatinoString(estado == "no_acotado"
                    ? "El problema es NO ACOTADO. La función objetivo puede decrecer indefinidamente."
                    : $"Se alcanzó el límite de iteraciones ({maxIt}). Verifica la factibilidad.");
        return new LatinoDictionary(dict);
    }

    // 9. Búsqueda de cuadrícula (Grid Search) ─────────────────────────────────

    [LatinoExtension(
        "optimizacion.busquedaCuadricula",
        Args = "funcion, lista_limites_inf, lista_limites_sup, lista_n_puntos",
        Help  = "Evalúa f(X) en una cuadrícula regular y retorna el mejor punto encontrado.\n" +
                "Útil para exploración inicial del paisaje de la función, ajuste de hiperparámetros\n" +
                "y búsqueda de región aproximada del mínimo global.\n\n" +
                "funcion:           f(X) donde X es una lista de n valores.\n" +
                "lista_limites_inf: límites inferiores por dimensión.\n" +
                "lista_limites_sup: límites superiores por dimensión.\n" +
                "lista_n_puntos:    número de puntos por dimensión (producto total ≤ 1 000 000).\n\n" +
                "ADVERTENCIA: El costo crece exponencialmente con la dimensión (maldición de dim.).\n" +
                "Para n > 4 usa optimizacion.genetico o optimizacion.recocidoSimulado.")]
    public static LatinoObject BusquedaCuadricula(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4
            || args[1] is not LatinoList lLB
            || args[2] is not LatinoList lUB
            || args[3] is not LatinoList lNP)
            throw new RuntimeError(null!,
                "optimizacion.busquedaCuadricula requiere: funcion, lista_limites_inf, " +
                "lista_limites_sup, lista_n_puntos.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(X).");

        int n = lLB.Elements.Count;
        if (lUB.Elements.Count != n || lNP.Elements.Count != n)
            throw new RuntimeError(null!, "Todas las listas deben tener la misma longitud n.");

        var lb  = new double[n]; for (int i = 0; i < n; i++) lb[i]  = GetDouble(lLB.Elements[i]);
        var ub  = new double[n]; for (int i = 0; i < n; i++) ub[i]  = GetDouble(lUB.Elements[i]);
        var nPs = new int[n];    for (int i = 0; i < n; i++) nPs[i] = (int)GetDouble(lNP.Elements[i]);

        for (int i = 0; i < n; i++)
        {
            if (ub[i] <= lb[i]) throw new RuntimeError(null!, $"ub[{i}] debe ser > lb[{i}].");
            if (nPs[i] < 2)     throw new RuntimeError(null!, $"n_puntos[{i}] debe ser al menos 2.");
        }

        // Verificar que el total no explote
        long total = 1;
        for (int i = 0; i < n; i++) { total *= nPs[i]; if (total > 1_000_000) break; }
        if (total > 1_000_000)
            throw new RuntimeError(null!,
                $"El producto total de puntos ({total:N0}) supera 1 000 000. " +
                "Reduce lista_n_puntos o usa optimizacion.genetico.");

        var poolX    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) poolX.Elements.Add(new LatinoNumber(0.0));
        var poolArgs = new List<LatinoObject> { poolX };

        // Iterar sobre la cuadrícula usando índices multi-dimensionales
        var    indices = new int[n];
        double fBest   = double.MaxValue;
        var    xBest   = new double[n];
        long   evalCount = 0;

        while (true)
        {
            // Construir punto actual
            var x = new double[n];
            for (int i = 0; i < n; i++)
                x[i] = nPs[i] == 1
                    ? lb[i]
                    : lb[i] + indices[i] * (ub[i] - lb[i]) / (nPs[i] - 1);

            double fVal = EvalND(func, interp, poolArgs, poolX, x);
            evalCount++;
            if (fVal < fBest) { fBest = fVal; xBest = (double[])x.Clone(); }

            // Avanzar al siguiente multi-índice
            int carry = 1;
            for (int i = n - 1; i >= 0 && carry == 1; i--)
            {
                indices[i] += carry;
                if (indices[i] >= nPs[i]) { indices[i] = 0; carry = 1; }
                else                        carry = 0;
            }
            if (carry == 1) break; // se completó la cuadrícula
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("optimo"),        VecALista(xBest) },
            { new LatinoString("f_optimo"),      new LatinoNumber(fBest) },
            { new LatinoString("evaluaciones"),  new LatinoNumber(evalCount) },
            { new LatinoString("formula"),       new LatinoString("Barrido exhaustivo en cuadrícula regular") },
            { new LatinoString("nota"),
              new LatinoString("Refina con optimizacion.bfgs o optimizacion.nelder_mead " +
                               "usando este punto como inicio.") }
        };
        return new LatinoDictionary(dict);
    }
}
