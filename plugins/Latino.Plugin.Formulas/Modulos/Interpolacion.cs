using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class InterpolacionFormulas
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private static double GetDouble(LatinoObject obj) =>
        obj is LatinoNumber n ? n.Value
            : throw new RuntimeError(null!, "Se esperaba un número.");

    /// <summary>
    /// Extrae dos vectores paralelos xs[] e ys[] desde una LatinoList de pares
    /// [[x0,y0],[x1,y1],...] o desde dos LatinoList separadas según el modo.
    /// Modo "pares"  → args[0] = lista de [x, y]
    /// Modo "doble"  → args[0] = lista xs, args[1] = lista ys
    /// </summary>
    private static (double[] xs, double[] ys) ExtraerPuntos(
        LatinoList listaX, LatinoList listaY)
    {
        int n = listaX.Elements.Count;
        if (n != listaY.Elements.Count)
            throw new RuntimeError(null!, "Las listas xs e ys deben tener la misma longitud.");
        if (n < 2)
            throw new RuntimeError(null!, "Se necesitan al menos 2 puntos.");

        var xs = new double[n];
        var ys = new double[n];
        for (int i = 0; i < n; i++)
        {
            xs[i] = GetDouble(listaX.Elements[i]);
            ys[i] = GetDouble(listaY.Elements[i]);
        }
        return (xs, ys);
    }

    /// <summary>Verifica que los xs sean estrictamente crecientes (sin duplicados).</summary>
    private static void ValidarXsUnicos(double[] xs)
    {
        for (int i = 1; i < xs.Length; i++)
            if (xs[i] <= xs[i - 1])
                throw new RuntimeError(null!,
                    $"Los valores x deben ser estrictamente crecientes. " +
                    $"xs[{i-1}] = {xs[i-1]:G}, xs[{i}] = {xs[i]:G}.");
    }

    /// <summary>Empaqueta un double[] como LatinoList de LatinoNumber.</summary>
    private static LatinoList EmpacarLista(double[] v)
    {
        var lista = new LatinoList(new List<LatinoObject>());
        foreach (var d in v)
            lista.Elements.Add(new LatinoNumber(d));
        return lista;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 0. Interpolación Lineal Simple (2D)
    // ─────────────────────────────────────────────────────────────────────────
    [LatinoExtension(
        "interpolacion.lineal",
        Args = "x0, y0, x1, y1, x",
        Help  = "Interpola (o extrapola) linealmente el valor de 'y' para un punto 'x' dado,\n" +
                "basado en dos puntos conocidos (x0, y0) y (x1, y1).\n\n" +
                "Fórmula: y = y0 + (x - x0) * (y1 - y0) / (x1 - x0)")]
    public static LatinoObject Lineal(List<LatinoObject> args)
    {
        if (args.Count < 5)
            throw new RuntimeError(null!, "interpolacion.lineal requiere 5 argumentos: x0, y0, x1, y1, x.");

        double x0 = GetDouble(args[0]);
        double y0 = GetDouble(args[1]);
        double x1 = GetDouble(args[2]);
        double y1 = GetDouble(args[3]);
        double x  = GetDouble(args[4]);

        if (Math.Abs(x1 - x0) < 1e-15)
            throw new RuntimeError(null!, "x0 y x1 no pueden ser iguales (división por cero al calcular la pendiente).");

        double y = y0 + (x - x0) * (y1 - y0) / (x1 - x0);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), new LatinoNumber(y) },
            { new LatinoString("formula"),   new LatinoString("y = y0 + (x - x0) * (y1 - y0) / (x1 - x0)") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Interpolación de Lagrange
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.lagrange",
        Args = "lista_xs, lista_ys, x",
        Help  = "Interpola el valor y en x usando el polinomio de Lagrange de grado n-1.\n" +
                "lista_xs: valores x conocidos (deben ser distintos).\n" +
                "lista_ys: valores y = f(x) correspondientes.\n" +
                "x: punto a evaluar.\n\n" +
                "Fórmula: P(x) = Σ yᵢ · ∏ (x-xⱼ)/(xᵢ-xⱼ), j≠i\n" +
                "ADVERTENCIA: Con muchos puntos (n > 15) puede oscilar (Fenómeno de Runge). " +
                "Prefiere interpolacion.splineCubico para conjuntos grandes.")]
    public static LatinoObject Lagrange(List<LatinoObject> args)
    {
        if (args.Count < 3 || args[0] is not LatinoList lx || args[1] is not LatinoList ly)
            throw new RuntimeError(null!, "interpolacion.lagrange requiere: lista_xs, lista_ys, x.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        ValidarXsUnicos(xs);

        double x = GetDouble(args[2]);
        int    n = xs.Length;
        bool   extrapolando = x < xs[0] || x > xs[n - 1];

        // Evaluación con forma baricéntrica ponderada (más estable numéricamente)
        double P = 0.0;
        for (int i = 0; i < n; i++)
        {
            double Li = 1.0;
            for (int j = 0; j < n; j++)
                if (j != i) Li *= (x - xs[j]) / (xs[i] - xs[j]);
            P += ys[i] * Li;
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(P) },
            { new LatinoString("grado_polinomio"), new LatinoNumber(n - 1) },
            { new LatinoString("es_extrapolacion"),new LatinoBoolean(extrapolando) },
            { new LatinoString("formula"),
              new LatinoString("P(x) = Σ yᵢ·∏(x-xⱼ)/(xᵢ-xⱼ)") }
        };
        if (extrapolando)
            dict[new LatinoString("advertencia")] =
                new LatinoString("x está fuera del rango de los datos — resultado es extrapolación.");

        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Diferencias Divididas de Newton (tabla + evaluación)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.newtonDivididas",
        Args = "lista_xs, lista_ys, x",
        Help  = "Interpola en x usando el polinomio de Newton con Diferencias Divididas.\n" +
                "Más eficiente que Lagrange para agregar puntos incrementalmente.\n" +
                "lista_xs: valores x conocidos (estrictamente crecientes).\n" +
                "lista_ys: valores f(x) correspondientes.\n\n" +
                "Fórmula: P(x) = f[x₀] + f[x₀,x₁](x-x₀) + f[x₀,x₁,x₂](x-x₀)(x-x₁) + ...")]
    public static LatinoObject NewtonDivididas(List<LatinoObject> args)
    {
        if (args.Count < 3 || args[0] is not LatinoList lx || args[1] is not LatinoList ly)
            throw new RuntimeError(null!, "interpolacion.newtonDivididas requiere: lista_xs, lista_ys, x.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        ValidarXsUnicos(xs);

        double x = GetDouble(args[2]);
        int    n = xs.Length;

        // Construir tabla de diferencias divididas in-place
        var dd = new double[n];
        Array.Copy(ys, dd, n);

        for (int j = 1; j < n; j++)
            for (int i = n - 1; i >= j; i--)
                dd[i] = (dd[i] - dd[i - 1]) / (xs[i] - xs[i - j]);

        // Evaluar el polinomio (forma de Horner)
        double P = dd[n - 1];
        for (int i = n - 2; i >= 0; i--)
            P = P * (x - xs[i]) + dd[i];

        bool extrapolando = x < xs[0] || x > xs[n - 1];

        // Exponer los coeficientes (dd[0]..dd[n-1]) como lista para uso avanzado
        var coefs = EmpacarLista(dd);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),         new LatinoNumber(P) },
            { new LatinoString("coeficientes_dd"),   coefs },
            { new LatinoString("grado_polinomio"),   new LatinoNumber(n - 1) },
            { new LatinoString("es_extrapolacion"),  new LatinoBoolean(extrapolando) },
            { new LatinoString("formula"),
              new LatinoString("P(x) = f[x₀] + f[x₀,x₁](x-x₀) + f[x₀,x₁,x₂](x-x₀)(x-x₁) + ...") }
        };
        if (extrapolando)
            dict[new LatinoString("advertencia")] =
                new LatinoString("x está fuera del rango de los datos — resultado es extrapolación.");

        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Spline Cúbico Natural
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.splineCubico",
        Args = "lista_xs, lista_ys, x",
        Help  = "Interpola en x usando un Spline Cúbico Natural.\n" +
                "Construye polinomios cúbicos entre cada par de puntos, garantizando\n" +
                "continuidad de la función, la primera y la segunda derivada.\n" +
                "Condición natural: f''(x₀) = f''(xₙ) = 0.\n\n" +
                "Es el método recomendado para datos experimentales (sin Runge).\n" +
                "Fórmula por tramo: S_i(x) = aᵢ + bᵢ(x-xᵢ) + cᵢ(x-xᵢ)² + dᵢ(x-xᵢ)³")]
    public static LatinoObject SplineCubico(List<LatinoObject> args)
    {
        if (args.Count < 3 || args[0] is not LatinoList lx || args[1] is not LatinoList ly)
            throw new RuntimeError(null!, "interpolacion.splineCubico requiere: lista_xs, lista_ys, x.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        ValidarXsUnicos(xs);

        double x = GetDouble(args[2]);
        int    n = xs.Length;
        int    m = n - 1; // número de tramos

        if (n < 3)
            throw new RuntimeError(null!, "El spline cúbico requiere al menos 3 puntos.");

        // ── Paso 1: calcular h[i] = xs[i+1] - xs[i] ──
        var h = new double[m];
        for (int i = 0; i < m; i++)
            h[i] = xs[i + 1] - xs[i];

        // ── Paso 2: sistema tridiagonal para los momentos M[i] (segundas derivadas) ──
        // Condición natural: M[0] = M[n-1] = 0
        var alpha = new double[m];
        for (int i = 1; i < m; i++)
            alpha[i] = 3.0 / h[i] * (ys[i + 1] - ys[i])
                     - 3.0 / h[i - 1] * (ys[i] - ys[i - 1]);

        // Thomas algorithm (TDMA) para sistema tridiagonal
        var l  = new double[n];
        var mu = new double[n];
        var z  = new double[n];
        var M  = new double[n]; // momentos (segundas derivadas en cada nodo)

        l[0] = 1.0; mu[0] = 0.0; z[0] = 0.0;

        for (int i = 1; i < m; i++)
        {
            l[i]  = 2.0 * (xs[i + 1] - xs[i - 1]) - h[i - 1] * mu[i - 1];
            if (Math.Abs(l[i]) < 1e-15)
                throw new RuntimeError(null!, $"Sistema singular en nodo {i}. Revisa los puntos xs.");
            mu[i] = h[i] / l[i];
            z[i]  = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }

        l[m] = 1.0; z[m] = 0.0; M[m] = 0.0;

        for (int j = m - 1; j >= 0; j--)
            M[j] = z[j] - mu[j] * M[j + 1];

        // ── Paso 3: localizar el tramo y evaluar ──
        // Clamp fuera del dominio al tramo más cercano (extrapolación lineal)
        int seg = m - 1;
        for (int i = 0; i < m; i++)
        {
            if (x <= xs[i + 1]) { seg = i; break; }
        }
        if (x < xs[0]) seg = 0;

        double t  = x - xs[seg];
        double hi = h[seg];

        double a = ys[seg];
        double b = (ys[seg + 1] - ys[seg]) / hi
                 - hi * (2.0 * M[seg] + M[seg + 1]) / 6.0;
        double c = M[seg] / 2.0;
        double d = (M[seg + 1] - M[seg]) / (6.0 * hi);

        double S = a + b * t + c * t * t + d * t * t * t;

        // Primera derivada en x (pendiente del spline)
        double Sp = b + 2.0 * c * t + 3.0 * d * t * t;

        bool extrapolando = x < xs[0] || x > xs[n - 1];

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(S) },
            { new LatinoString("derivada_en_x"),   new LatinoNumber(Sp) },
            { new LatinoString("tramo_usado"),      new LatinoNumber(seg) },
            { new LatinoString("es_extrapolacion"), new LatinoBoolean(extrapolando) },
            { new LatinoString("formula"),
              new LatinoString("S_i(x) = aᵢ + bᵢ(x-xᵢ) + cᵢ(x-xᵢ)² + dᵢ(x-xᵢ)³") }
        };
        if (extrapolando)
            dict[new LatinoString("advertencia")] =
                new LatinoString("x está fuera del rango — se extrapoló con el tramo extremo.");

        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. Spline Cúbico — evaluación en múltiples puntos (batch)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.splineCubicoLista",
        Args = "lista_xs, lista_ys, lista_x_evaluar",
        Help  = "Igual que interpolacion.splineCubico pero evalúa el spline en una lista\n" +
                "de puntos de una sola vez. Útil para graficar o tabular resultados.\n\n" +
                "Retorna una lista de valores y correspondientes a cada x en lista_x_evaluar.")]
    public static LatinoObject SplineCubicoLista(List<LatinoObject> args)
    {
        if (args.Count < 3
            || args[0] is not LatinoList lx
            || args[1] is not LatinoList ly
            || args[2] is not LatinoList lxEval)
            throw new RuntimeError(null!,
                "interpolacion.splineCubicoLista requiere: lista_xs, lista_ys, lista_x_evaluar.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        ValidarXsUnicos(xs);

        int n = xs.Length;
        int m = n - 1;

        if (n < 3)
            throw new RuntimeError(null!, "El spline cúbico requiere al menos 3 puntos.");

        // ── Calcular momentos M[] (igual que SplineCubico) ──
        var h = new double[m];
        for (int i = 0; i < m; i++) h[i] = xs[i + 1] - xs[i];

        var alpha = new double[m];
        for (int i = 1; i < m; i++)
            alpha[i] = 3.0 / h[i]     * (ys[i + 1] - ys[i])
                     - 3.0 / h[i - 1] * (ys[i]     - ys[i - 1]);

        var l  = new double[n]; var mu = new double[n];
        var z  = new double[n]; var M  = new double[n];
        l[0] = 1.0;
        for (int i = 1; i < m; i++)
        {
            l[i]  = 2.0 * (xs[i + 1] - xs[i - 1]) - h[i - 1] * mu[i - 1];
            if (Math.Abs(l[i]) < 1e-15)
                throw new RuntimeError(null!, $"Sistema singular en nodo {i}.");
            mu[i] = h[i] / l[i];
            z[i]  = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }
        l[m] = 1.0; M[m] = 0.0;
        for (int j = m - 1; j >= 0; j--)
            M[j] = z[j] - mu[j] * M[j + 1];

        // ── Evaluar para cada x en lxEval ──
        var resultados = new LatinoList(new List<LatinoObject>());
        foreach (var elem in lxEval.Elements)
        {
            double x   = GetDouble(elem);
            int    seg = m - 1;
            for (int i = 0; i < m; i++) { if (x <= xs[i + 1]) { seg = i; break; } }
            if (x < xs[0]) seg = 0;

            double t  = x - xs[seg];
            double hi = h[seg];
            double a  = ys[seg];
            double b  = (ys[seg + 1] - ys[seg]) / hi - hi * (2.0 * M[seg] + M[seg + 1]) / 6.0;
            double c  = M[seg] / 2.0;
            double d  = (M[seg + 1] - M[seg]) / (6.0 * hi);
            resultados.Elements.Add(new LatinoNumber(a + b * t + c * t * t + d * t * t * t));
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("valores_y"),  resultados },
            { new LatinoString("n_puntos"),   new LatinoNumber(lxEval.Elements.Count) },
            { new LatinoString("formula"),
              new LatinoString("S_i(x) = aᵢ + bᵢ(x-xᵢ) + cᵢ(x-xᵢ)² + dᵢ(x-xᵢ)³ (Spline Natural)") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. Interpolación de Hermite
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.hermite",
        Args = "lista_xs, lista_ys, lista_dys, x",
        Help  = "Interpolación de Hermite cúbica por tramos.\n" +
                "Requiere los valores de la función Y y de su derivada DY en cada nodo.\n" +
                "Garantiza continuidad C¹ (función y primera derivada continuas).\n" +
                "lista_xs:  valores x conocidos (estrictamente crecientes).\n" +
                "lista_ys:  valores f(x)  en cada nodo.\n" +
                "lista_dys: valores f'(x) en cada nodo.\n\n" +
                "Fórmula por tramo: H(x) = h₀₀·y₀ + h₁₀·(x-x₀)·y'₀ + h₀₁·y₁ + h₁₁·(x-x₁)·y'₁\n" +
                "Ideal cuando conoces la pendiente en los nodos (ej. datos de sensores + velocidad).")]
    public static LatinoObject Hermite(List<LatinoObject> args)
    {
        if (args.Count < 4
            || args[0] is not LatinoList lx
            || args[1] is not LatinoList ly
            || args[2] is not LatinoList ldy)
            throw new RuntimeError(null!,
                "interpolacion.hermite requiere: lista_xs, lista_ys, lista_dys, x.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        ValidarXsUnicos(xs);

        int n = xs.Length;
        if (ldy.Elements.Count != n)
            throw new RuntimeError(null!, "lista_dys debe tener la misma longitud que lista_xs.");

        var dys = new double[n];
        for (int i = 0; i < n; i++) dys[i] = GetDouble(ldy.Elements[i]);

        double x = GetDouble(args[3]);
        int    m = n - 1;

        // Localizar tramo
        int seg = m - 1;
        for (int i = 0; i < m; i++) { if (x <= xs[i + 1]) { seg = i; break; } }
        if (x < xs[0]) seg = 0;

        double x0 = xs[seg], x1 = xs[seg + 1];
        double y0 = ys[seg], y1 = ys[seg + 1];
        double d0 = dys[seg], d1 = dys[seg + 1];
        double hi = x1 - x0;

        // Parámetro local t ∈ [0, 1]
        double t  = (x - x0) / hi;
        double t2 = t * t;
        double t3 = t2 * t;

        // Polinomios base de Hermite
        double h00 =  2*t3 - 3*t2 + 1;
        double h10 =     t3 - 2*t2 + t;
        double h01 = -2*t3 + 3*t2;
        double h11 =     t3 -   t2;

        double H = h00 * y0 + h10 * hi * d0 + h01 * y1 + h11 * hi * d1;

        // Primera derivada
        double dh00 = ( 6*t2 - 6*t) / hi;
        double dh10 = ( 3*t2 - 4*t + 1);
        double dh01 = (-6*t2 + 6*t) / hi;
        double dh11 = ( 3*t2 - 2*t);
        double Hp   = dh00 * y0 + dh10 * d0 + dh01 * y1 + dh11 * d1;

        bool extrapolando = x < xs[0] || x > xs[n - 1];

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(H) },
            { new LatinoString("derivada_en_x"),   new LatinoNumber(Hp) },
            { new LatinoString("tramo_usado"),      new LatinoNumber(seg) },
            { new LatinoString("es_extrapolacion"), new LatinoBoolean(extrapolando) },
            { new LatinoString("formula"),
              new LatinoString("H(x) = h₀₀·y₀ + h₁₀·Δx·y'₀ + h₀₁·y₁ + h₁₁·Δx·y'₁") }
        };
        if (extrapolando)
            dict[new LatinoString("advertencia")] =
                new LatinoString("x está fuera del rango — se extrapoló con el tramo extremo.");

        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. Interpolación Bilineal (2D)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.bilineal",
        Args = "x0, x1, y0, y1, f00, f10, f01, f11, x, y",
        Help  = "Interpolación bilineal en una celda rectangular 2D.\n" +
                "Dados los valores f en las 4 esquinas de la celda, estima f(x, y) interior.\n\n" +
                "  (x0,y1)─f01────f11─(x1,y1)\n" +
                "      │              │\n" +
                "  (x0,y0)─f00────f10─(x1,y0)\n\n" +
                "f00 = f(x0,y0), f10 = f(x1,y0), f01 = f(x0,y1), f11 = f(x1,y1)\n\n" +
                "Fórmula: f(x,y) = (1-t)(1-u)f00 + t(1-u)f10 + (1-t)u·f01 + tu·f11\n" +
                "donde t = (x-x0)/(x1-x0), u = (y-y0)/(y1-y0)")]
    public static LatinoObject Bilineal(List<LatinoObject> args)
    {
        if (args.Count < 10)
            throw new RuntimeError(null!,
                "interpolacion.bilineal requiere: x0, x1, y0, y1, f00, f10, f01, f11, x, y.");

        double x0  = GetDouble(args[0]);
        double x1  = GetDouble(args[1]);
        double y0  = GetDouble(args[2]);
        double y1  = GetDouble(args[3]);
        double f00 = GetDouble(args[4]);
        double f10 = GetDouble(args[5]);
        double f01 = GetDouble(args[6]);
        double f11 = GetDouble(args[7]);
        double x   = GetDouble(args[8]);
        double y   = GetDouble(args[9]);

        if (x1 <= x0) throw new RuntimeError(null!, "Se requiere x1 > x0.");
        if (y1 <= y0) throw new RuntimeError(null!, "Se requiere y1 > y0.");

        double t = (x - x0) / (x1 - x0);
        double u = (y - y0) / (y1 - y0);

        bool fuera = t < 0 || t > 1 || u < 0 || u > 1;

        double fxy = (1 - t) * (1 - u) * f00
                   + t       * (1 - u) * f10
                   + (1 - t) * u       * f01
                   + t       * u       * f11;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),       new LatinoNumber(fxy) },
            { new LatinoString("t"),               new LatinoNumber(t) },
            { new LatinoString("u"),               new LatinoNumber(u) },
            { new LatinoString("es_extrapolacion"), new LatinoBoolean(fuera) },
            { new LatinoString("formula"),
              new LatinoString("f(x,y) = (1-t)(1-u)f₀₀ + t(1-u)f₁₀ + (1-t)u·f₀₁ + t·u·f₁₁") }
        };
        if (fuera)
            dict[new LatinoString("advertencia")] =
                new LatinoString("El punto (x,y) está fuera de la celda — resultado es extrapolación.");

        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. Regresión Lineal por Mínimos Cuadrados
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.regresionLineal",
        Args = "lista_xs, lista_ys",
        Help  = "Ajuste de la recta y = a + b·x por el método de Mínimos Cuadrados.\n" +
                "Minimiza la suma de cuadrados de los residuos Σ(yᵢ - ŷᵢ)².\n\n" +
                "Retorna: pendiente (b), intercepto (a), R² (coef. de determinación),\n" +
                "error_estandar y residuos.\n\n" +
                "Fórmulas: b = [n·Σxy - Σx·Σy] / [n·Σx² - (Σx)²] | a = ȳ - b·x̄")]
    public static LatinoObject RegresionLineal(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lx || args[1] is not LatinoList ly)
            throw new RuntimeError(null!, "interpolacion.regresionLineal requiere: lista_xs, lista_ys.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        int n = xs.Length;

        double sumX  = 0, sumY  = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            sumX  += xs[i];
            sumY  += ys[i];
            sumXY += xs[i] * ys[i];
            sumX2 += xs[i] * xs[i];
        }

        double denom = n * sumX2 - sumX * sumX;
        if (Math.Abs(denom) < 1e-15)
            throw new RuntimeError(null!,
                "Todos los valores x son iguales — la recta de regresión es vertical.");

        double b    = (n * sumXY - sumX * sumY) / denom;
        double a    = (sumY - b * sumX) / n;
        double yBar = sumY / n;

        // R² y error estándar
        double ssTot = 0, ssRes = 0;
        var residuos = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < n; i++)
        {
            double yHat = a + b * xs[i];
            double res  = ys[i] - yHat;
            ssTot += (ys[i] - yBar) * (ys[i] - yBar);
            ssRes += res * res;
            residuos.Elements.Add(new LatinoNumber(res));
        }

        double r2  = ssTot > 1e-15 ? 1.0 - ssRes / ssTot : 1.0;
        double sse = n > 2 ? Math.Sqrt(ssRes / (n - 2)) : 0.0;

        string calidad = r2 > 0.99 ? "Ajuste excelente" :
                         r2 > 0.95 ? "Ajuste muy bueno" :
                         r2 > 0.80 ? "Ajuste aceptable" :
                                     "Ajuste pobre — considera modelo no lineal";

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("pendiente"),      new LatinoNumber(b) },
            { new LatinoString("intercepto"),     new LatinoNumber(a) },
            { new LatinoString("r_cuadrado"),     new LatinoNumber(r2) },
            { new LatinoString("error_estandar"), new LatinoNumber(sse) },
            { new LatinoString("residuos"),       residuos },
            { new LatinoString("calidad"),        new LatinoString(calidad) },
            { new LatinoString("formula"),
              new LatinoString("ŷ = a + b·x | b = (n·Σxy - Σx·Σy)/(n·Σx² - (Σx)²)") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 8. Regresión Polinomial por Mínimos Cuadrados
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.regresionPolinomial",
        Args = "lista_xs, lista_ys, grado",
        Help  = "Ajuste de un polinomio de grado k: y = c₀ + c₁x + c₂x² + ... + cₖxᵏ.\n" +
                "Usa el método de Mínimos Cuadrados via eliminación gaussiana con pivoteo parcial.\n" +
                "Retorna coeficientes [c₀, c₁, ..., cₖ] y R².\n\n" +
                "ADVERTENCIA: Grados altos (k > 8) pueden ser inestables numéricamente.\n" +
                "Para evaluar el polinomio en un punto usa interpolacion.evaluarPolinomio.")]
    public static LatinoObject RegresionPolinomial(List<LatinoObject> args)
    {
        if (args.Count < 3 || args[0] is not LatinoList lx || args[1] is not LatinoList ly)
            throw new RuntimeError(null!,
                "interpolacion.regresionPolinomial requiere: lista_xs, lista_ys, grado.");

        var (xs, ys) = ExtraerPuntos(lx, ly);
        int n = xs.Length;
        int k = (int)GetDouble(args[2]);

        if (k < 1)  throw new RuntimeError(null!, "El grado debe ser al menos 1.");
        if (k > 10) throw new RuntimeError(null!, "Grado máximo permitido: 10 (estabilidad numérica).");
        if (k >= n) throw new RuntimeError(null!, "El grado debe ser menor que el número de puntos.");

        int p = k + 1; // número de coeficientes

        // Construir matriz normal A (p×p) y vector b (p×1)
        var A = new double[p, p];
        var B = new double[p];

        for (int i = 0; i < p; i++)
        {
            for (int j = 0; j < p; j++)
            {
                double s = 0;
                for (int r = 0; r < n; r++) s += Math.Pow(xs[r], i + j);
                A[i, j] = s;
            }
            double sb = 0;
            for (int r = 0; r < n; r++) sb += Math.Pow(xs[r], i) * ys[r];
            B[i] = sb;
        }

        // Eliminación gaussiana con pivoteo parcial
        var coefs = GaussPivoteo(A, B, p);

        // Calcular R²
        double yBar = 0;
        for (int i = 0; i < n; i++) yBar += ys[i];
        yBar /= n;

        double ssTot = 0, ssRes = 0;
        for (int i = 0; i < n; i++)
        {
            double yHat = 0;
            for (int j = 0; j < p; j++) yHat += coefs[j] * Math.Pow(xs[i], j);
            ssTot += (ys[i] - yBar) * (ys[i] - yBar);
            ssRes += (ys[i] - yHat) * (ys[i] - yHat);
        }
        double r2 = ssTot > 1e-15 ? 1.0 - ssRes / ssTot : 1.0;

        var listaCoefs = EmpacarLista(coefs);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("coeficientes"),  listaCoefs },
            { new LatinoString("grado"),         new LatinoNumber(k) },
            { new LatinoString("r_cuadrado"),    new LatinoNumber(r2) },
            { new LatinoString("formula"),
              new LatinoString("ŷ = c₀ + c₁x + c₂x² + ... (Mínimos Cuadrados)") },
            { new LatinoString("nota"),
              new LatinoString("Evalúa el polinomio con interpolacion.evaluarPolinomio(coefs, x).") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 9. Evaluar Polinomio (Horner) — helper para regresionPolinomial
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "interpolacion.evaluarPolinomio",
        Args = "lista_coeficientes, x",
        Help  = "Evalúa un polinomio en x usando el Esquema de Horner (O(n), numéricamente estable).\n" +
                "lista_coeficientes: [c₀, c₁, c₂, ...] donde cᵢ es el coeficiente de xⁱ.\n" +
                "(Este formato es el que devuelve interpolacion.regresionPolinomial.)\n\n" +
                "Fórmula: P(x) = c₀ + c₁x + c₂x² + ... evaluado como c₀ + x(c₁ + x(c₂ + ...))")]
    public static LatinoObject EvaluarPolinomio(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lc)
            throw new RuntimeError(null!, "interpolacion.evaluarPolinomio requiere: lista_coeficientes, x.");

        int nc = lc.Elements.Count;
        if (nc < 1) throw new RuntimeError(null!, "La lista de coeficientes no puede estar vacía.");

        var coefs = new double[nc];
        for (int i = 0; i < nc; i++) coefs[i] = GetDouble(lc.Elements[i]);

        double x = GetDouble(args[1]);

        // Horner: evaluar desde el coeficiente de mayor grado hacia el menor
        double P = coefs[nc - 1];
        for (int i = nc - 2; i >= 0; i--)
            P = P * x + coefs[i];

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), new LatinoNumber(P) },
            { new LatinoString("grado"),     new LatinoNumber(nc - 1) },
            { new LatinoString("formula"),   new LatinoString("Esquema de Horner: P(x) = c₀ + x(c₁ + x(c₂ + ...))") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers privados de álgebra lineal (solo para este módulo)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Eliminación gaussiana con pivoteo parcial.
    /// Resuelve A·x = b. Modifica A y b in-place.
    /// </summary>
    private static double[] GaussPivoteo(double[,] A, double[] b, int n)
    {
        // Forward elimination
        for (int col = 0; col < n; col++)
        {
            // Pivoteo parcial
            int maxRow = col;
            double maxVal = Math.Abs(A[col, col]);
            for (int row = col + 1; row < n; row++)
                if (Math.Abs(A[row, col]) > maxVal) { maxVal = Math.Abs(A[row, col]); maxRow = row; }

            if (maxVal < 1e-15)
                throw new RuntimeError(null!,
                    "Sistema singular — los puntos xs podrían ser demasiado próximos o colineales.");

            // Intercambiar filas
            for (int j = col; j < n; j++) (A[col, j], A[maxRow, j]) = (A[maxRow, j], A[col, j]);
            (b[col], b[maxRow]) = (b[maxRow], b[col]);

            // Eliminación
            for (int row = col + 1; row < n; row++)
            {
                double factor = A[row, col] / A[col, col];
                for (int j = col; j < n; j++) A[row, j] -= factor * A[col, j];
                b[row] -= factor * b[col];
            }
        }

        // Back substitution
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = b[i];
            for (int j = i + 1; j < n; j++) x[i] -= A[i, j] * x[j];
            x[i] /= A[i, i];
        }
        return x;
    }
}
