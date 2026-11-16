using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class CalculoNumericoFormulas
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private static double GetDouble(LatinoObject obj) =>
        obj is LatinoNumber n ? n.Value
            : throw new RuntimeError(null!, "Se esperaba un número.");

    /// <summary>
    /// Evalúa la función de usuario con Object Pooling.
    /// El mismo List se reutiliza en cada llamada para evitar presión sobre el GC.
    /// </summary>
    private static double Eval(
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
                $"La función retornó un valor no finito en x = {x:G}. " +
                "Verifica singularidades o divisiones por cero.");
        return val;
    }

    /// <summary>Empaqueta un double[] como LatinoList.</summary>
    private static LatinoList EmpacarLista(double[] v)
    {
        var lista = new LatinoList(new List<LatinoObject>());
        foreach (var d in v) lista.Elements.Add(new LatinoNumber(d));
        return lista;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INTEGRACIÓN NUMÉRICA
    // ─────────────────────────────────────────────────────────────────────────

    // 1. Regla del Trapecio compuesta ────────────────────────────────────────

    [LatinoExtension(
        "calculo.trapecio",
        Args = "funcion, a, b, n",
        Help  = "Integración numérica por la Regla del Trapecio Compuesta.\n" +
                "Divide [a,b] en n subintervalos iguales de ancho h = (b-a)/n.\n" +
                "n par no es obligatorio, pero más n → mayor precisión.\n\n" +
                "Fórmula: ∫ ≈ (h/2)·[f(a) + 2·Σf(xᵢ) + f(b)]\n" +
                "Error global: O(h²) — duplicar n reduce el error ~4 veces.")]
    public static LatinoObject Trapecio(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!, "calculo.trapecio requiere: funcion, a, b, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a   = GetDouble(args[1]);
        double b   = GetDouble(args[2]);
        int    n   = (int)GetDouble(args[3]);

        if (n < 1) throw new RuntimeError(null!, "n debe ser al menos 1.");
        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        var pool = new List<LatinoObject> { null! };
        double h  = (b - a) / n;
        double fa = Eval(func, interp, pool, a);
        double fb = Eval(func, interp, pool, b);
        double suma = fa + fb;

        for (int i = 1; i < n; i++)
            suma += 2.0 * Eval(func, interp, pool, a + i * h);

        double resultado = (h / 2.0) * suma;

        // Estimación del error por regla de Richardson (un paso de refinamiento)
        double h2    = (b - a) / (2 * n);
        double suma2 = fa + fb;
        for (int i = 1; i < 2 * n; i++)
            suma2 += 2.0 * Eval(func, interp, pool, a + i * h2);
        double res2 = (h2 / 2.0) * suma2;
        double errorEst = Math.Abs(res2 - resultado) / 3.0;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),      new LatinoNumber(resultado) },
            { new LatinoString("errorEstimado"), new LatinoNumber(errorEst) },
            { new LatinoString("nSubintervalos"),new LatinoNumber(n) },
            { new LatinoString("h"),              new LatinoNumber(h) },
            { new LatinoString("formula"),
              new LatinoString("∫ ≈ (h/2)·[f(a) + 2·Σf(xᵢ) + f(b)]") }
        };
        return new LatinoDictionary(dict);
    }

    // 2. Regla de Simpson 1/3 Compuesta ─────────────────────────────────────

    [LatinoExtension(
        "calculo.simpson13",
        Args = "funcion, a, b, n",
        Help  = "Regla de Simpson 1/3 compuesta. Integra f(x) en [a, b].\n" +
                "Requiere 'n' par (número de intervalos). n debe ser al menos 2.\n\n" +
                "Fórmula: (h/3)·[f(a) + 4Σf(impar) + 2Σf(par) + f(b)]\n" +
                "Error global: O(h⁴) — mucho más preciso que Trapecio con el mismo n.")]
    public static LatinoObject Simpson(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!, "calculo.simpson13 requiere: funcion, a, b, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a = GetDouble(args[1]);
        double b = GetDouble(args[2]);
        int    n = (int)GetDouble(args[3]);

        if (n < 2) throw new RuntimeError(null!, "n debe ser al menos 2.");
        if (n % 2 != 0)
            throw new RuntimeError(null!, $"n debe ser par para Simpson 1/3 (recibido: {n}). Usa n = {n + 1}.");
        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        var    pool = new List<LatinoObject> { null! };
        double h    = (b - a) / n;
        double suma = Eval(func, interp, pool, a) + Eval(func, interp, pool, b);

        for (int i = 1; i < n; i++)
            suma += (i % 2 == 0 ? 2.0 : 4.0) * Eval(func, interp, pool, a + i * h);

        double resultado = (h / 3.0) * suma;

        // Estimación del error: comparar con n*2
        double h2    = (b - a) / (2 * n);
        double suma2 = Eval(func, interp, pool, a) + Eval(func, interp, pool, b);
        for (int i = 1; i < 2 * n; i++)
            suma2 += (i % 2 == 0 ? 2.0 : 4.0) * Eval(func, interp, pool, a + i * h2);
        double res2      = (h2 / 3.0) * suma2;
        double errorEst  = Math.Abs(res2 - resultado) / 15.0; // factor 15 para Simpson O(h⁴)

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(resultado) },
            { new LatinoString("errorEstimado"),  new LatinoNumber(errorEst) },
            { new LatinoString("nSubintervalos"), new LatinoNumber(n) },
            { new LatinoString("h"),               new LatinoNumber(h) },
            { new LatinoString("formula"),
              new LatinoString("∫ ≈ (h/3)·[f(a) + 4f(x₁) + 2f(x₂) + 4f(x₃) + ... + f(b)]") }
        };
        return new LatinoDictionary(dict);
    }

    // 3. Regla de Simpson 3/8 ────────────────────────────────────────────────

    [LatinoExtension(
        "calculo.simpson38",
        Args = "funcion, a, b, n",
        Help  = "Regla de Simpson 3/8 compuesta. Integra f(x) en [a, b].\n" +
                "Requiere 'n' múltiplo de 3. n debe ser al menos 3.\n\n" +
                "Fórmula: (3h/8)·[f(x₀) + 3f(x₁) + 3f(x₂) + 2f(x₃) + ... + f(xₙ)]\n" +
                "Útil cuando n es múltiplo de 3 y no de 2 (complementa a simpson).")]
    public static LatinoObject Simpson38(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!, "calculo.simpson38 requiere: funcion, a, b, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a = GetDouble(args[1]);
        double b = GetDouble(args[2]);
        int    n = (int)GetDouble(args[3]);

        if (n < 3) throw new RuntimeError(null!, "n debe ser al menos 3.");
        if (n % 3 != 0)
            throw new RuntimeError(null!, $"n debe ser múltiplo de 3 para Simpson 3/8 (recibido: {n}).");
        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        var    pool = new List<LatinoObject> { null! };
        double h    = (b - a) / n;
        double suma = Eval(func, interp, pool, a) + Eval(func, interp, pool, b);

        for (int i = 1; i < n; i++)
            suma += (i % 3 == 0 ? 2.0 : 3.0) * Eval(func, interp, pool, a + i * h);

        double resultado = (3.0 * h / 8.0) * suma;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(resultado) },
            { new LatinoString("nSubintervalos"), new LatinoNumber(n) },
            { new LatinoString("h"),               new LatinoNumber(h) },
            { new LatinoString("formula"),
              new LatinoString("∫ ≈ (3h/8)·[f(x₀) + 3f(x₁) + 3f(x₂) + 2f(x₃) + ...]") }
        };
        return new LatinoDictionary(dict);
    }

    // 4. Cuadratura de Gauss-Legendre ────────────────────────────────────────

    [LatinoExtension(
        "calculo.cuadraturaGauss",
        Args = "funcion, a, b, [puntos]",
        Help  = "Integración numérica por Cuadratura de Gauss-Legendre.\n" +
                "Muy precisa con pocos puntos para funciones suaves.\n" +
                "puntos: número de puntos de cuadratura (2, 3, 4 o 5 — defecto: 5).\n\n" +
                "Fórmula: ∫ₐᵇ f(x)dx ≈ (b-a)/2 · Σ wᵢ·f(xᵢ)\n" +
                "Exacta para polinomios de grado ≤ 2n-1. Error O(h^(2n)).")]
    public static LatinoObject GaussLegendre(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "calculo.cuadraturaGauss requiere: funcion, a, b. Opcionalmente: puntos (2-5).");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a      = GetDouble(args[1]);
        double b      = GetDouble(args[2]);
        int    puntos = args.Count > 3 ? (int)GetDouble(args[3]) : 5;

        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        // Nodos y pesos de Gauss-Legendre en [-1, 1]
        double[] t, w;
        switch (puntos)
        {
            case 2:
                t = new[] { -0.5773502691896257, 0.5773502691896257 };
                w = new[] {  1.0,                 1.0 };
                break;
            case 3:
                t = new[] { -0.7745966692414834, 0.0, 0.7745966692414834 };
                w = new[] {  0.5555555555555556, 0.8888888888888888, 0.5555555555555556 };
                break;
            case 4:
                t = new[] { -0.8611363115940526, -0.3399810435848563,
                             0.3399810435848563,  0.8611363115940526 };
                w = new[] {  0.3478548451374538,  0.6521451548625461,
                             0.6521451548625461,  0.3478548451374538 };
                break;
            case 5:
                t = new[] { -0.9061798459386640, -0.5384693101056831, 0.0,
                             0.5384693101056831,  0.9061798459386640 };
                w = new[] {  0.2369268850561891,  0.4786286704993665, 0.5688888888888889,
                             0.4786286704993665,  0.2369268850561891 };
                break;
            default:
                throw new RuntimeError(null!, "Puntos de cuadratura válidos: 2, 3, 4 o 5.");
        }

        var    pool      = new List<LatinoObject> { null! };
        double mid       = (a + b) / 2.0;
        double halfRange = (b - a) / 2.0;
        double suma      = 0.0;

        for (int i = 0; i < puntos; i++)
        {
            double xi = mid + halfRange * t[i];
            suma += w[i] * Eval(func, interp, pool, xi);
        }

        double resultado = halfRange * suma;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),        new LatinoNumber(resultado) },
            { new LatinoString("puntosUsados"),    new LatinoNumber(puntos) },
            { new LatinoString("exactoHastaGrado"), new LatinoNumber(2 * puntos - 1) },
            { new LatinoString("formula"),
              new LatinoString("∫ ≈ (b-a)/2 · Σ wᵢ·f((b+a)/2 + (b-a)/2·tᵢ)") }
        };
        return new LatinoDictionary(dict);
    }

    // 5. Integración por Monte Carlo ─────────────────────────────────────────

    [LatinoExtension(
        "calculo.monteCarlo",
        Args = "funcion, a, b, nMuestras, [semilla]",
        Help  = "Estimación de ∫ₐᵇ f(x)dx por el Método de Monte Carlo.\n" +
                "Muestrea n puntos aleatorios en [a,b] y promedia los valores de f.\n" +
                "Error estadístico: O(1/√n) — para precisión 1e-3 se necesitan ~10⁶ muestras.\n" +
                "semilla: entero opcional para reproducibilidad (defecto: aleatorio).\n\n" +
                "Fórmula: ∫ ≈ (b-a) · (1/n) · Σ f(xᵢ), xᵢ ~ U(a,b)\n" +
                "Ventaja clave: funciona bien en dominios irregulares e integrales múltiples.")]
    public static LatinoObject MonteCarlo(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!, "calculo.monteCarlo requiere: funcion, a, b, nMuestras. Opcionalmente: semilla.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a  = GetDouble(args[1]);
        double b  = GetDouble(args[2]);
        int    n  = (int)GetDouble(args[3]);

        if (n < 10)   throw new RuntimeError(null!, "nMuestras debe ser al menos 10.");
        if (n > 10_000_000) throw new RuntimeError(null!, "nMuestras no debe superar 10 000 000 (rendimiento).");
        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        var rng = args.Count > 4
            ? new Random((int)GetDouble(args[4]))
            : new Random();

        var    pool  = new List<LatinoObject> { null! };
        double suma  = 0.0;
        double suma2 = 0.0; // para varianza

        for (int i = 0; i < n; i++)
        {
            double x  = a + (b - a) * rng.NextDouble();
            double fx = Eval(func, interp, pool, x);
            suma  += fx;
            suma2 += fx * fx;
        }

        double media    = suma / n;
        double varianza = (suma2 / n) - (media * media);
        double resultado = (b - a) * media;

        // Error estándar de Monte Carlo: σ_MC = (b-a) · σ / √n
        double sigmaFx  = varianza > 0 ? Math.Sqrt(varianza) : 0;
        double errorEst = (b - a) * sigmaFx / Math.Sqrt(n);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(resultado) },
            { new LatinoString("errorEstimado"),  new LatinoNumber(errorEst) },
            { new LatinoString("nMuestras"),      new LatinoNumber(n) },
            { new LatinoString("intervaloConfianza95"),
              new LatinoString($"[{resultado - 1.96*errorEst:G6}, {resultado + 1.96*errorEst:G6}]") },
            { new LatinoString("formula"),
              new LatinoString("∫ ≈ (b-a)·(1/n)·Σf(xᵢ), xᵢ ~ U(a,b)") }
        };
        return new LatinoDictionary(dict);
    }

    // 6. Integración adaptativa (Romberg) ────────────────────────────────────

    [LatinoExtension(
        "calculo.romberg",
        Args = "funcion, a, b, [maxNivel], [tolerancia]",
        Help  = "Integración numérica por el Método de Romberg.\n" +
                "Aplica extrapolación de Richardson sobre la Regla del Trapecio para\n" +
                "acelerar la convergencia automáticamente.\n" +
                "maxNivel: niveles de refinamiento (defecto: 10, máx: 15).\n" +
                "tolerancia: criterio de parada (defecto: 1e-10).\n\n" +
                "Fórmula: R(n,m) = [4ᵐ·R(n,m-1) - R(n-1,m-1)] / (4ᵐ - 1)\n" +
                "Convergencia: O(h^(2m)) — extremadamente rápido para funciones suaves.")]
    public static LatinoObject Romberg(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "calculo.romberg requiere: funcion, a, b. Opcionalmente: maxNivel, tolerancia.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a        = GetDouble(args[1]);
        double b        = GetDouble(args[2]);
        int    maxNivel = args.Count > 3 ? (int)GetDouble(args[3]) : 10;
        double tol      = args.Count > 4 ? GetDouble(args[4]) : 1e-10;

        if (maxNivel < 2 || maxNivel > 15)
            throw new RuntimeError(null!, "maxNivel debe estar entre 2 y 15.");
        if (tol <= 0) throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        var pool = new List<LatinoObject> { null! };
        var R    = new double[maxNivel, maxNivel];

        // R(0,0) — trapecio con 1 intervalo
        R[0, 0] = 0.5 * (b - a) * (Eval(func, interp, pool, a) + Eval(func, interp, pool, b));

        int nivelAlcanzado = 1;
        double errorEst    = double.MaxValue;

        for (int i = 1; i < maxNivel; i++)
        {
            // Refinamiento trapecio
            int    m    = 1 << i; // 2^i subintervalos
            double h    = (b - a) / m;
            double suma = 0.0;
            for (int k = 1; k <= m / 2; k++)
                suma += Eval(func, interp, pool, a + (2 * k - 1) * h);

            R[i, 0] = 0.5 * R[i - 1, 0] + h * suma;

            // Extrapolación de Richardson
            for (int j = 1; j <= i; j++)
            {
                double factor = Math.Pow(4.0, j);
                R[i, j] = (factor * R[i, j - 1] - R[i - 1, j - 1]) / (factor - 1.0);
            }

            errorEst = Math.Abs(R[i, i] - R[i - 1, i - 1]);
            nivelAlcanzado = i + 1;

            if (errorEst < tol) break;
        }

        int nFinal = nivelAlcanzado - 1;
        bool convergio = errorEst < tol;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(R[nFinal, nFinal]) },
            { new LatinoString("convergio"),       new LatinoBoolean(convergio) },
            { new LatinoString("errorEstimado"),  new LatinoNumber(errorEst) },
            { new LatinoString("nivelesUsados"),  new LatinoNumber(nivelAlcanzado) },
            { new LatinoString("evaluacionesF"),  new LatinoNumber((1 << nivelAlcanzado) + 1) },
            { new LatinoString("formula"),
              new LatinoString("R(n,m) = [4ᵐ·R(n,m-1) - R(n-1,m-1)] / (4ᵐ-1)") }
        };
        if (!convergio)
            dict[new LatinoString("advertencia")] =
                new LatinoString($"No convergió en {nivelAlcanzado} niveles. " +
                                 "La función puede tener singularidades o ser muy oscilatoria.");
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DIFERENCIACIÓN NUMÉRICA
    // ─────────────────────────────────────────────────────────────────────────

    // 7. Diferencias Finitas (primera derivada) ──────────────────────────────

    [LatinoExtension(
        "calculo.derivada",
        Args = "funcion, x, [h], [orden]",
        Help  = "Aproxima la primera derivada f'(x) por Diferencias Finitas.\n" +
                "orden: 'adelante' (O(h)), 'atras' (O(h)) o 'central' (O(h²), defecto).\n" +
                "h: paso (defecto: 1e-5 — óptimo para aritmética de doble precisión).\n\n" +
                "Fórmulas:\n" +
                "  Central:  f'(x) ≈ [f(x+h) - f(x-h)] / (2h)          O(h²)\n" +
                "  Adelante: f'(x) ≈ [f(x+h) - f(x)] / h                O(h)\n" +
                "  Atrás:    f'(x) ≈ [f(x) - f(x-h)] / h                O(h)")]
    public static LatinoObject Derivada(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "calculo.derivada requiere: funcion, x. Opcionalmente: h, orden.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double x      = GetDouble(args[1]);
        double h      = args.Count > 2 ? GetDouble(args[2]) : 1e-5;
        string orden  = args.Count > 3 ? args[3].ToString().ToLower().Trim() : "central";

        if (h <= 0) throw new RuntimeError(null!, "El paso h debe ser mayor a cero.");

        var pool = new List<LatinoObject> { null! };
        double deriv, errorOrden;
        string formula;

        switch (orden)
        {
            case "adelante":
                deriv = (Eval(func, interp, pool, x + h) - Eval(func, interp, pool, x)) / h;
                errorOrden = h;
                formula = "f'(x) ≈ [f(x+h) - f(x)] / h";
                break;
            case "atras":
                deriv = (Eval(func, interp, pool, x) - Eval(func, interp, pool, x - h)) / h;
                errorOrden = h;
                formula = "f'(x) ≈ [f(x) - f(x-h)] / h";
                break;
            case "central":
                deriv = (Eval(func, interp, pool, x + h) - Eval(func, interp, pool, x - h)) / (2.0 * h);
                errorOrden = h * h;
                formula = "f'(x) ≈ [f(x+h) - f(x-h)] / (2h)";
                break;
            default:
                throw new RuntimeError(null!, "orden debe ser 'adelante', 'atras' o 'central'.");
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),    new LatinoNumber(deriv) },
            { new LatinoString("hUsado"),      new LatinoNumber(h) },
            { new LatinoString("ordenError"),  new LatinoString($"O(h) ≈ O({errorOrden:G2})") },
            { new LatinoString("metodo"),       new LatinoString(orden) },
            { new LatinoString("formula"),      new LatinoString(formula) }
        };
        return new LatinoDictionary(dict);
    }

    // 8. Segunda derivada numérica ───────────────────────────────────────────

    [LatinoExtension(
        "calculo.segundaDerivada",
        Args = "funcion, x, [h]",
        Help  = "Aproxima la segunda derivada f''(x) por Diferencias Finitas Centrales.\n" +
                "h: paso (defecto: 1e-4 — ligeramente mayor que para primera derivada).\n\n" +
                "Fórmula: f''(x) ≈ [f(x+h) - 2f(x) + f(x-h)] / h²     O(h²)")]
    public static LatinoObject SegundaDerivada(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "calculo.segundaDerivada requiere: funcion, x. Opcionalmente: h.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double x = GetDouble(args[1]);
        double h = args.Count > 2 ? GetDouble(args[2]) : 1e-4;

        if (h <= 0) throw new RuntimeError(null!, "El paso h debe ser mayor a cero.");

        var pool = new List<LatinoObject> { null! };
        double fxph  = Eval(func, interp, pool, x + h);
        double fx    = Eval(func, interp, pool, x);
        double fxmh  = Eval(func, interp, pool, x - h);
        double deriv = (fxph - 2.0 * fx + fxmh) / (h * h);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),   new LatinoNumber(deriv) },
            { new LatinoString("hUsado"),     new LatinoNumber(h) },
            { new LatinoString("ordenError"), new LatinoString($"O(h²) ≈ O({h*h:G2})") },
            { new LatinoString("formula"),     new LatinoString("f''(x) ≈ [f(x+h) - 2f(x) + f(x-h)] / h²") }
        };
        return new LatinoDictionary(dict);
    }

    // 9. Extrapolación de Richardson para derivada ───────────────────────────

    [LatinoExtension(
        "calculo.derivadaRichardson",
        Args = "funcion, x, [h], [niveles]",
        Help  = "Aproxima f'(x) con alta precisión usando Extrapolación de Richardson.\n" +
                "Combina múltiples diferencias centrales con h, h/2, h/4... y las combina\n" +
                "mediante la tabla de Richardson para cancelar términos de error.\n" +
                "niveles: profundidad de extrapolación (defecto: 4, máx: 8).\n\n" +
                "Fórmula base: D(h) = [f(x+h) - f(x-h)] / (2h)\n" +
                "Extrapolación: D(n,m) = [4ᵐ·D(n,m-1) - D(n-1,m-1)] / (4ᵐ-1)\n" +
                "Converge a O(h^(2·niveles)) — extremadamente preciso.")]
    public static LatinoObject DerivadaRichardson(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!,
                "calculo.derivadaRichardson requiere: funcion, x. Opcionalmente: h, niveles.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double x       = GetDouble(args[1]);
        double h0      = args.Count > 2 ? GetDouble(args[2]) : 0.1;
        int    niveles = args.Count > 3 ? (int)GetDouble(args[3]) : 4;

        if (h0 <= 0)    throw new RuntimeError(null!, "El paso h debe ser mayor a cero.");
        if (niveles < 1 || niveles > 8) throw new RuntimeError(null!, "niveles debe estar entre 1 y 8.");

        var pool = new List<LatinoObject> { null! };
        var D    = new double[niveles, niveles];

        // Primera columna: diferencia central con h reducido por mitades
        for (int i = 0; i < niveles; i++)
        {
            double hi = h0 / Math.Pow(2.0, i);
            D[i, 0]   = (Eval(func, interp, pool, x + hi) - Eval(func, interp, pool, x - hi)) / (2.0 * hi);
        }

        // Tabla de Richardson
        for (int j = 1; j < niveles; j++)
        {
            for (int i = j; i < niveles; i++)
            {
                double factor = Math.Pow(4.0, j);
                D[i, j] = (factor * D[i, j - 1] - D[i - 1, j - 1]) / (factor - 1.0);
            }
        }

        double resultado = D[niveles - 1, niveles - 1];
        double errorEst  = Math.Abs(D[niveles - 1, niveles - 1] - D[niveles - 2, niveles - 2]);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),      new LatinoNumber(resultado) },
            { new LatinoString("errorEstimado"), new LatinoNumber(errorEst) },
            { new LatinoString("nivelesUsados"), new LatinoNumber(niveles) },
            { new LatinoString("formula"),
              new LatinoString("D(n,m) = [4ᵐ·D(n,m-1) - D(n-1,m-1)] / (4ᵐ-1)") }
        };
        return new LatinoDictionary(dict);
    }

    // 10. Gradiente numérico (función de varias variables) ───────────────────

    [LatinoExtension(
        "calculo.gradiente",
        Args = "funcionNVar, listaPunto, [h]",
        Help  = "Calcula el gradiente numérico ∇f en un punto dado.\n" +
                "funcionNVar: función Latino que recibe UNA lista de n valores y retorna un escalar.\n" +
                "listaPunto: punto x = [x₁, x₂, ..., xₙ] donde evaluar el gradiente.\n" +
                "h: paso para diferencias centrales (defecto: 1e-5).\n\n" +
                "Fórmula: ∂f/∂xᵢ ≈ [f(x + h·eᵢ) - f(x - h·eᵢ)] / (2h)\n" +
                "Retorna lista con n componentes del gradiente.")]
    public static LatinoObject Gradiente(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2 || args[1] is not LatinoList lPunto)
            throw new RuntimeError(null!,
                "calculo.gradiente requiere: funcionNVar, listaPunto. Opcionalmente: h.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double h = args.Count > 2 ? GetDouble(args[2]) : 1e-5;
        if (h <= 0) throw new RuntimeError(null!, "El paso h debe ser mayor a cero.");

        int n = lPunto.Elements.Count;
        if (n < 1) throw new RuntimeError(null!, "listaPunto no puede estar vacía.");

        // Convertir el punto a double[]
        var xBase = new double[n];
        for (int i = 0; i < n; i++) xBase[i] = GetDouble(lPunto.Elements[i]);

        // La función recibe una LatinoList completa (no un escalar)
        // Se construye una pool de LatinoList que se muta en cada llamada
        var xList  = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) xList.Elements.Add(new LatinoNumber(xBase[i]));
        var poolG  = new List<LatinoObject> { xList };

        double EvalMultivar(double[] punto)
        {
            for (int i = 0; i < n; i++) xList.Elements[i] = new LatinoNumber(punto[i]);
            var res = func.Call(interp, poolG);
            double val = GetDouble(res);
            if (double.IsNaN(val) || double.IsInfinity(val))
                throw new RuntimeError(null!, "La función retornó un valor no finito.");
            return val;
        }

        var grad = new double[n];
        for (int i = 0; i < n; i++)
        {
            var xPlus  = (double[])xBase.Clone(); xPlus[i]  += h;
            var xMinus = (double[])xBase.Clone(); xMinus[i] -= h;
            grad[i] = (EvalMultivar(xPlus) - EvalMultivar(xMinus)) / (2.0 * h);
        }

        // Calcular la norma del gradiente
        double norma = 0;
        foreach (var g in grad) norma += g * g;
        norma = Math.Sqrt(norma);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("gradiente"),    EmpacarLista(grad) },
            { new LatinoString("norma"),        new LatinoNumber(norma) },
            { new LatinoString("nVariables"),  new LatinoNumber(n) },
            { new LatinoString("hUsado"),      new LatinoNumber(h) },
            { new LatinoString("formula"),
              new LatinoString("∂f/∂xᵢ ≈ [f(x+h·eᵢ) - f(x-h·eᵢ)] / (2h)") }
        };
        return new LatinoDictionary(dict);
    }

    // 11. Jacobiano numérico ──────────────────────────────────────────────────

    [LatinoExtension(
        "calculo.jacobiana",
        Args = "funcionVectorial, listaX, [h]",
        Help  = "Calcula la matriz Jacobiana J de una función vectorial F: ℝⁿ → ℝᵐ en el punto X.\n" +
                "funcionVectorial: f(X) donde X es lista, debe retornar una lista [f₁, f₂, ..., fₘ].\n" +
                "listaX: punto de evaluación [x₁, x₂, ..., xₙ].\n" +
                "h: paso de diferenciación (defecto: 1e-5).\n\n" +
                "Retorna: matriz (lista de listas) donde J_ij = ∂f_i/∂x_j.")]
    public static LatinoObject Jacobiano(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 2 || args[1] is not LatinoList lPunto)
            throw new RuntimeError(null!,
                "calculo.jacobiano requiere: funcion_vectorial, lista_punto. Opcionalmente: h.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino (vectorial).");

        double h = args.Count > 2 ? GetDouble(args[2]) : 1e-5;
        if (h <= 0) throw new RuntimeError(null!, "El paso h debe ser mayor a cero.");

        int n = lPunto.Elements.Count;
        if (n < 1) throw new RuntimeError(null!, "lista_punto no puede estar vacía.");

        var xBase = new double[n];
        for (int i = 0; i < n; i++) xBase[i] = GetDouble(lPunto.Elements[i]);

        // Función auxiliar: llama a func con un punto double[] y devuelve double[]
        var xList = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++) xList.Elements.Add(new LatinoNumber(xBase[i]));
        var poolJ = new List<LatinoObject> { xList };

        double[] EvalVec(double[] punto)
        {
            for (int i = 0; i < n; i++) xList.Elements[i] = new LatinoNumber(punto[i]);
            var res = func.Call(interp, poolJ);
            if (res is not LatinoList rl)
                throw new RuntimeError(null!, "La función vectorial debe retornar una lista.");
            var v = new double[rl.Elements.Count];
            for (int i = 0; i < v.Length; i++) v[i] = GetDouble(rl.Elements[i]);
            return v;
        }

        // Determinar m (número de salidas) evaluando en el punto base
        double[] f0 = EvalVec(xBase);
        int m = f0.Length;

        // Construir Jacobiana m×n
        var J = new double[m, n];
        for (int j = 0; j < n; j++)
        {
            var xPlus  = (double[])xBase.Clone(); xPlus[j]  += h;
            var xMinus = (double[])xBase.Clone(); xMinus[j] -= h;
            double[] fp = EvalVec(xPlus);
            double[] fm = EvalVec(xMinus);
            for (int i = 0; i < m; i++)
                J[i, j] = (fp[i] - fm[i]) / (2.0 * h);
        }

        // Empacar la Jacobiana como lista de listas
        var jLista = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < m; i++)
        {
            var fila = new LatinoList(new List<LatinoObject>());
            for (int j = 0; j < n; j++) fila.Elements.Add(new LatinoNumber(J[i, j]));
            jLista.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("jacobiana"),   jLista },
            { new LatinoString("filas_m"),     new LatinoNumber(m) },
            { new LatinoString("columnas_n"),  new LatinoNumber(n) },
            { new LatinoString("h_usado"),     new LatinoNumber(h) },
            { new LatinoString("formula"),
              new LatinoString("Jᵢⱼ = ∂fᵢ/∂xⱼ ≈ [fᵢ(x+h·eⱼ) - fᵢ(x-h·eⱼ)] / (2h)") }
        };
        return new LatinoDictionary(dict);
    }

    // 12. Longitud de arco numérica ──────────────────────────────────────────

    [LatinoExtension(
        "calculo.longitudArco",
        Args = "funcion, a, b, n",
        Help  = "Calcula la longitud de arco de y = f(x) en [a, b] numéricamente.\n" +
                "Aproxima ∫√(1 + f'(x)²) dx usando diferencias centrales + Simpson.\n\n" +
                "Fórmula: L = ∫ₐᵇ √(1 + [f'(x)]²) dx\n" +
                "n: número de subintervalos para Simpson (debe ser par, recomendado ≥ 100).")]
    public static LatinoObject LongitudArco(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!, "calculo.longitudArco requiere: funcion, a, b, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino.");

        double a = GetDouble(args[1]);
        double b = GetDouble(args[2]);
        int    n = (int)GetDouble(args[3]);

        if (n < 2) throw new RuntimeError(null!, "n debe ser al menos 2.");
        if (n % 2 != 0) n++; // forzar par para Simpson
        if (Math.Abs(b - a) < 1e-15) throw new RuntimeError(null!, "a y b no pueden ser iguales.");

        var    pool = new List<LatinoObject> { null! };
        double h    = (b - a) / n;
        double hD   = 1e-5; // paso para derivada numérica

        // Integrar sqrt(1 + f'(x)²) con Simpson
        double Integrando(double x)
        {
            double fpx = (Eval(func, interp, pool, x + hD) - Eval(func, interp, pool, x - hD)) / (2.0 * hD);
            return Math.Sqrt(1.0 + fpx * fpx);
        }

        double suma = Integrando(a) + Integrando(b);
        for (int i = 1; i < n; i++)
            suma += (i % 2 == 0 ? 2.0 : 4.0) * Integrando(a + i * h);

        double L = (h / 3.0) * suma;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(L) },
            { new LatinoString("n_subintervalos"), new LatinoNumber(n) },
            { new LatinoString("formula"),
              new LatinoString("L = ∫√(1 + [f'(x)]²) dx  (Simpson + diferencias centrales)") }
        };
        return new LatinoDictionary(dict);
    }
}
