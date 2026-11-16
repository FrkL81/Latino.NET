using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class EcuacionesDiferencialesFormulas
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private static double GetDouble(LatinoObject obj) =>
        obj is LatinoNumber n ? n.Value
            : throw new RuntimeError(null!, "Se esperaba un número.");

    /// <summary>
    /// Evalúa f(t, y) con Object Pooling de dos argumentos.
    /// El mismo List&lt;LatinoObject&gt; se reutiliza en cada llamada para
    /// evitar presión sobre el GC en bucles de miles de pasos.
    /// </summary>
    private static double EvalF(
        ICallable func,
        Interpreter interp,
        List<LatinoObject> pool,
        double t,
        double y)
    {
        pool[0] = new LatinoNumber(t);
        pool[1] = new LatinoNumber(y);
        var res  = func.Call(interp, pool);
        double val = GetDouble(res);
        if (double.IsNaN(val) || double.IsInfinity(val))
            throw new RuntimeError(null!,
                $"f(t={t:G}, y={y:G}) retornó un valor no finito. " +
                "Verifica singularidades o divisiones por cero en la EDO.");
        return val;
    }

    /// <summary>
    /// Evalúa f(t, Y) donde Y es un vector de estado (sistema de EDOs).
    /// Construye una LatinoList con el vector y la reutiliza.
    /// </summary>
    private static double[] EvalSistema(
        ICallable func,
        Interpreter interp,
        List<LatinoObject> poolArgs,
        LatinoList poolY,
        double t,
        double[] y)
    {
        poolArgs[0] = new LatinoNumber(t);
        for (int i = 0; i < y.Length; i++)
            poolY.Elements[i] = new LatinoNumber(y[i]);
        poolArgs[1] = poolY;

        var res = func.Call(interp, poolArgs);
        if (res is not LatinoList rl)
            throw new RuntimeError(null!,
                "La función del sistema debe retornar una lista con las derivadas [dy₁/dt, dy₂/dt, ...].");

        var dY = new double[rl.Elements.Count];
        for (int i = 0; i < dY.Length; i++)
        {
            dY[i] = GetDouble(rl.Elements[i]);
            if (double.IsNaN(dY[i]) || double.IsInfinity(dY[i]))
                throw new RuntimeError(null!,
                    $"Componente {i} de f(t={t:G}) retornó un valor no finito.");
        }
        return dY;
    }

    /// <summary>double[] → LatinoList</summary>
    private static LatinoList VecALista(double[] v)
    {
        var l = new LatinoList(new List<LatinoObject>());
        foreach (var d in v) l.Elements.Add(new LatinoNumber(d));
        return l;
    }

    /// <summary>
    /// Construye la LatinoList de resultados (trayectoria completa).
    /// Cada paso se almacena como lista [t, y₀, y₁, ...].
    /// </summary>
    private static LatinoList EmpacarTrayectoria(List<double> ts, List<double[]> ys)
    {
        var tray = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < ts.Count; i++)
        {
            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(ts[i]));
            foreach (var yi in ys[i]) fila.Elements.Add(new LatinoNumber(yi));
            tray.Elements.Add(fila);
        }
        return tray;
    }

    /// <summary>
    /// Suma de dos vectores: a + escalar*b
    /// </summary>
    private static double[] SumaEscalar(double[] a, double escalar, double[] b)
    {
        var c = new double[a.Length];
        for (int i = 0; i < a.Length; i++) c[i] = a[i] + escalar * b[i];
        return c;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MÉTODOS DE PASO ÚNICO (EDO escalar)
    // ─────────────────────────────────────────────────────────────────────────

    // 1. Método de Euler ──────────────────────────────────────────────────────

    [LatinoExtension(
        "edo.euler",
        Args = "funcionF, t0, y0, tf, n",
        Help  = "Resuelve dy/dt = f(t, y) con condición inicial y(t₀) = y₀\n" +
                "usando el Método de Euler explícito (primer orden).\n\n" +
                "funcionF: función Latino f(t, y) que retorna dy/dt.\n" +
                "t0, y0:    condición inicial.\n" +
                "tf:        tiempo final.\n" +
                "n:         número de pasos (más n → mayor precisión).\n\n" +
                "Fórmula: yₙ₊₁ = yₙ + h·f(tₙ, yₙ)   h = (tf-t0)/n\n" +
                "Error global: O(h). Para mayor precisión usa edo.rk4 o edo.rkf45.")]
    public static LatinoObject Euler(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 5)
            throw new RuntimeError(null!, "edo.euler requiere: funcionF, t0, y0, tf, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(t,y).");

        double t0 = GetDouble(args[1]);
        double y0 = GetDouble(args[2]);
        double tf = GetDouble(args[3]);
        int    n  = (int)GetDouble(args[4]);

        if (n < 1)   throw new RuntimeError(null!, "n debe ser al menos 1.");
        if (tf <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        double h     = (tf - t0) / n;
        var    pool  = new List<LatinoObject> { null!, null! };
        var    ts    = new List<double>(n + 1);
        var    ysLat = new List<double>(n + 1);

        double t = t0, y = y0;
        ts.Add(t); ysLat.Add(y);

        for (int i = 0; i < n; i++)
        {
            double k1 = EvalF(func, interp, pool, t, y);
            y += h * k1;
            t += h;
            ts.Add(t); ysLat.Add(y);
        }

        // Trayectoria: lista de [t, y]
        var tray = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i <= n; i++)
        {
            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(ts[i]));
            fila.Elements.Add(new LatinoNumber(ysLat[i]));
            tray.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"),  tray },
            { new LatinoString("y_final"),      new LatinoNumber(y) },
            { new LatinoString("t_final"),      new LatinoNumber(t) },
            { new LatinoString("h"),            new LatinoNumber(h) },
            { new LatinoString("n_pasos"),      new LatinoNumber(n) },
            { new LatinoString("formula"),      new LatinoString("yₙ₊₁ = yₙ + h·f(tₙ, yₙ)") }
        };
        return new LatinoDictionary(dict);
    }

    // 2. Método de Euler Mejorado (Heun) ─────────────────────────────────────

    [LatinoExtension(
        "edo.eulerMejorado",
        Args = "funcionF, t0, y0, tf, n",
        Help  = "Resuelve dy/dt = f(t, y) con el Método de Euler Mejorado (Heun).\n" +
                "Predictor-corrector de segundo orden: predice con Euler, corrige con el promedio.\n\n" +
                "Fórmula:\n" +
                "  Predictor: ỹ = yₙ + h·f(tₙ, yₙ)\n" +
                "  Corrector: yₙ₊₁ = yₙ + (h/2)·[f(tₙ, yₙ) + f(tₙ₊₁, ỹ)]\n" +
                "Error global: O(h²) — doble precisión que Euler con el mismo costo.")]
    public static LatinoObject EulerMejorado(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 5)
            throw new RuntimeError(null!, "edo.eulerMejorado requiere: funcionF, t0, y0, tf, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(t,y).");

        double t0 = GetDouble(args[1]);
        double y0 = GetDouble(args[2]);
        double tf = GetDouble(args[3]);
        int    n  = (int)GetDouble(args[4]);

        if (n < 1)    throw new RuntimeError(null!, "n debe ser al menos 1.");
        if (tf <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        double h    = (tf - t0) / n;
        var    pool = new List<LatinoObject> { null!, null! };
        var    tray = new LatinoList(new List<LatinoObject>());

        double t = t0, y = y0;
        var fila0 = new LatinoList(new List<LatinoObject>());
        fila0.Elements.Add(new LatinoNumber(t)); fila0.Elements.Add(new LatinoNumber(y));
        tray.Elements.Add(fila0);

        for (int i = 0; i < n; i++)
        {
            double k1   = EvalF(func, interp, pool, t, y);
            double yPred = y + h * k1;
            double k2   = EvalF(func, interp, pool, t + h, yPred);
            y += (h / 2.0) * (k1 + k2);
            t += h;

            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(t));
            fila.Elements.Add(new LatinoNumber(y));
            tray.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"), tray },
            { new LatinoString("y_final"),     new LatinoNumber(y) },
            { new LatinoString("t_final"),     new LatinoNumber(t) },
            { new LatinoString("h"),           new LatinoNumber(h) },
            { new LatinoString("n_pasos"),     new LatinoNumber(n) },
            { new LatinoString("formula"),
              new LatinoString("yₙ₊₁ = yₙ + (h/2)·[f(tₙ,yₙ) + f(tₙ₊₁, yₙ+h·f(tₙ,yₙ))]") }
        };
        return new LatinoDictionary(dict);
    }

    // 3. Runge-Kutta 4 (RK4) clásico ─────────────────────────────────────────

    [LatinoExtension(
        "edo.rk4",
        Args = "funcionF, t0, y0, tf, n",
        Help  = "Resuelve dy/dt = f(t, y) con el método clásico Runge-Kutta de orden 4.\n" +
                "El método más usado en la práctica para EDOs escalares con paso fijo.\n\n" +
                "funcionF: función Latino f(t, y) → dy/dt.\n" +
                "t0, y0:    condición inicial.\n" +
                "tf:        tiempo final.\n" +
                "n:         número de pasos.\n\n" +
                "Fórmula:\n" +
                "  k₁ = f(tₙ,      yₙ)\n" +
                "  k₂ = f(tₙ+h/2,  yₙ+h·k₁/2)\n" +
                "  k₃ = f(tₙ+h/2,  yₙ+h·k₂/2)\n" +
                "  k₄ = f(tₙ+h,    yₙ+h·k₃)\n" +
                "  yₙ₊₁ = yₙ + (h/6)·(k₁ + 2k₂ + 2k₃ + k₄)\n" +
                "Error global: O(h⁴). Object Pooling activo — k₁..k₄ reusan el mismo pool.")]
    public static LatinoObject RK4(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 5)
            throw new RuntimeError(null!, "edo.rk4 requiere: funcionF, t0, y0, tf, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(t,y).");

        double t0 = GetDouble(args[1]);
        double y0 = GetDouble(args[2]);
        double tf = GetDouble(args[3]);
        int    n  = (int)GetDouble(args[4]);

        if (n < 1)    throw new RuntimeError(null!, "n debe ser al menos 1.");
        if (tf <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        double h    = (tf - t0) / n;
        var    pool = new List<LatinoObject> { null!, null! }; // pool reutilizado en k₁..k₄
        var    tray = new LatinoList(new List<LatinoObject>());

        double t = t0, y = y0;
        var f0 = new LatinoList(new List<LatinoObject>());
        f0.Elements.Add(new LatinoNumber(t)); f0.Elements.Add(new LatinoNumber(y));
        tray.Elements.Add(f0);

        for (int i = 0; i < n; i++)
        {
            double k1 = EvalF(func, interp, pool, t,           y);
            double k2 = EvalF(func, interp, pool, t + h / 2.0, y + h * k1 / 2.0);
            double k3 = EvalF(func, interp, pool, t + h / 2.0, y + h * k2 / 2.0);
            double k4 = EvalF(func, interp, pool, t + h,       y + h * k3);

            y += (h / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
            t += h;

            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(t));
            fila.Elements.Add(new LatinoNumber(y));
            tray.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"), tray },
            { new LatinoString("y_final"),     new LatinoNumber(y) },
            { new LatinoString("t_final"),     new LatinoNumber(t) },
            { new LatinoString("h"),           new LatinoNumber(h) },
            { new LatinoString("n_pasos"),     new LatinoNumber(n) },
            { new LatinoString("formula"),
              new LatinoString("yₙ₊₁ = yₙ + (h/6)·(k₁ + 2k₂ + 2k₃ + k₄)") }
        };
        return new LatinoDictionary(dict);
    }

    // 4. Runge-Kutta-Fehlberg 4(5) — paso adaptativo ─────────────────────────

    [LatinoExtension(
        "edo.rkf45",
        Args = "funcionF, t0, y0, tf, [tolerancia], [hInicial], [hMin], [hMax]",
        Help  = "Resuelve dy/dt = f(t, y) con el método Runge-Kutta-Fehlberg RKF45.\n" +
                "Control automático del tamaño de paso para mantener el error local\n" +
                "dentro de la tolerancia especificada.\n\n" +
                "Parámetros opcionales:\n" +
                "  tolerancia: error local máximo (defecto: 1e-6)\n" +
                "  hInicial:  paso inicial (defecto: (tf-t0)/100)\n" +
                "  hMin:      paso mínimo (defecto: 1e-10)\n" +
                "  hMax:      paso máximo (defecto: (tf-t0)/10)\n\n" +
                "Usa 6 evaluaciones de f por paso (tablero de Butcher de Fehlberg).\n" +
                "El error se estima comparando soluciones de orden 4 y 5.\n" +
                "Retorna trayectoria con pasos no uniformes y estadísticas de control.")]
    public static LatinoObject RKF45(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4)
            throw new RuntimeError(null!,
                "edo.rkf45 requiere: funcionF, t0, y0, tf. " +
                "Opcionalmente: tolerancia, hInicial, hMin, hMax.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(t,y).");

        double t0  = GetDouble(args[1]);
        double y0  = GetDouble(args[2]);
        double tf  = GetDouble(args[3]);
        double tol = args.Count > 4 ? GetDouble(args[4]) : 1e-6;
        double h   = args.Count > 5 ? GetDouble(args[5]) : (tf - t0) / 100.0;
        double hMin = args.Count > 6 ? GetDouble(args[6]) : 1e-10;
        double hMax = args.Count > 7 ? GetDouble(args[7]) : (tf - t0) / 10.0;

        if (tol  <= 0)  throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (tf   <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");
        if (hMin <= 0)  throw new RuntimeError(null!, "hMin debe ser mayor a cero.");
        if (hMax <= 0)  throw new RuntimeError(null!, "hMax debe ser mayor a cero.");

        // ── Coeficientes de Butcher (Fehlberg) ──
        // Nodos c
        const double c2 = 1.0/4.0,   c3 = 3.0/8.0,   c4 = 12.0/13.0, c5 = 1.0, c6 = 1.0/2.0;
        // Coeficientes a (tabla inferior)
        const double a21 = 1.0/4.0;
        const double a31 = 3.0/32.0,      a32 = 9.0/32.0;
        const double a41 = 1932.0/2197.0, a42 = -7200.0/2197.0, a43 = 7296.0/2197.0;
        const double a51 = 439.0/216.0,   a52 = -8.0,           a53 = 3680.0/513.0,   a54 = -845.0/4104.0;
        const double a61 = -8.0/27.0,     a62 = 2.0,            a63 = -3544.0/2565.0, a64 = 1859.0/4104.0, a65 = -11.0/40.0;
        // Pesos RK4 (orden 4) y RK5 (orden 5)
        const double b1 = 25.0/216.0,  b3 = 1408.0/2565.0, b4 = 2197.0/4104.0, b5 = -1.0/5.0;
        const double e1 = 16.0/135.0,  e3 = 6656.0/12825.0, e4 = 28561.0/56430.0, e5 = -9.0/50.0, e6 = 2.0/55.0;

        var pool = new List<LatinoObject> { null!, null! };
        var tray = new LatinoList(new List<LatinoObject>());
        var f0   = new LatinoList(new List<LatinoObject>());
        f0.Elements.Add(new LatinoNumber(t0)); f0.Elements.Add(new LatinoNumber(y0));
        tray.Elements.Add(f0);

        double t = t0, y = y0;
        int    pasosAceptados   = 0;
        int    pasosRechazados  = 0;
        int    maxPasos         = 100_000;

        while (t < tf && pasosAceptados + pasosRechazados < maxPasos)
        {
            if (t + h > tf) h = tf - t;
            if (h < hMin)   h = hMin;

            double k1 = EvalF(func, interp, pool, t,          y);
            double k2 = EvalF(func, interp, pool, t + c2*h,   y + h*a21*k1);
            double k3 = EvalF(func, interp, pool, t + c3*h,   y + h*(a31*k1 + a32*k2));
            double k4 = EvalF(func, interp, pool, t + c4*h,   y + h*(a41*k1 + a42*k2 + a43*k3));
            double k5 = EvalF(func, interp, pool, t + c5*h,   y + h*(a51*k1 + a52*k2 + a53*k3 + a54*k4));
            double k6 = EvalF(func, interp, pool, t + c6*h,   y + h*(a61*k1 + a62*k2 + a63*k3 + a64*k4 + a65*k5));

            // Solución de orden 4
            double y4 = y + h*(b1*k1 + b3*k3 + b4*k4 + b5*k5);
            // Solución de orden 5
            double y5 = y + h*(e1*k1 + e3*k3 + e4*k4 + e5*k5 + e6*k6);

            // Estimación del error local
            double errorLocal = Math.Abs(y5 - y4);
            double yScale     = Math.Max(1e-10, Math.Max(Math.Abs(y), Math.Abs(y4)));
            double errorRel   = errorLocal / yScale;

            if (errorRel <= tol || h <= hMin)
            {
                // Paso aceptado — avanzar con solución de orden 4
                y = y4;
                t += h;
                pasosAceptados++;

                var fila = new LatinoList(new List<LatinoObject>());
                fila.Elements.Add(new LatinoNumber(t));
                fila.Elements.Add(new LatinoNumber(y));
                tray.Elements.Add(fila);
            }
            else
            {
                pasosRechazados++;
            }

            // Control de paso: factor de seguridad 0.9, límites [0.1, 4.0]
            if (errorRel > 0)
            {
                double factor = 0.9 * Math.Pow(tol / errorRel, 0.2);
                factor = Math.Max(0.1, Math.Min(4.0, factor));
                h = Math.Min(hMax, h * factor);
            }
        }

        bool completo = Math.Abs(t - tf) < 1e-10 * (tf - t0 + 1);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"),      tray },
            { new LatinoString("y_final"),          new LatinoNumber(y) },
            { new LatinoString("t_final"),          new LatinoNumber(t) },
            { new LatinoString("pasos_aceptados"),  new LatinoNumber(pasosAceptados) },
            { new LatinoString("pasos_rechazados"), new LatinoNumber(pasosRechazados) },
            { new LatinoString("completo"),         new LatinoBoolean(completo) },
            { new LatinoString("formula"),
              new LatinoString("RKF45: error = |y₅ - y₄| — paso adaptativo vía Fehlberg") }
        };
        if (!completo)
            dict[new LatinoString("advertencia")] =
                new LatinoString("No se alcanzó tf. Aumenta max_pasos o revisa la EDO.");
        return new LatinoDictionary(dict);
    }

    // 5. Adams-Bashforth-Moulton de orden 4 (predictor-corrector) ────────────

    [LatinoExtension(
        "edo.adams",
        Args = "funcionF, t0, y0, tf, n",
        Help  = "Resuelve dy/dt = f(t, y) con el método Adams-Bashforth-Moulton de orden 4.\n" +
                "Método multipaso: usa los últimos 4 puntos para predecir (Adams-Bashforth)\n" +
                "y corregir (Adams-Moulton). Arranque con RK4.\n\n" +
                "Predictor (Adams-Bashforth 4 pasos):\n" +
                "  ỹₙ₊₁ = yₙ + (h/24)·(55fₙ - 59fₙ₋₁ + 37fₙ₋₂ - 9fₙ₋₃)\n" +
                "Corrector (Adams-Moulton 3 pasos):\n" +
                "  yₙ₊₁ = yₙ + (h/24)·(9f̃ₙ₊₁ + 19fₙ - 5fₙ₋₁ + fₙ₋₂)\n" +
                "Error global: O(h⁴). Más eficiente que RK4 para funciones costosas de evaluar.")]
    public static LatinoObject Adams(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 5)
            throw new RuntimeError(null!, "edo.adams requiere: funcionF, t0, y0, tf, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino f(t,y).");

        double t0 = GetDouble(args[1]);
        double y0 = GetDouble(args[2]);
        double tf = GetDouble(args[3]);
        int    n  = (int)GetDouble(args[4]);

        if (n < 4)    throw new RuntimeError(null!, "Adams requiere n ≥ 4 para el arranque RK4.");
        if (tf <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        double h    = (tf - t0) / n;
        var    pool = new List<LatinoObject> { null!, null! };

        // Arrays de historia
        var t_ = new double[n + 1];
        var y_ = new double[n + 1];
        var f_ = new double[n + 1];

        t_[0] = t0; y_[0] = y0;
        f_[0] = EvalF(func, interp, pool, t0, y0);

        // ── Arranque con RK4 (primeros 3 pasos) ──
        for (int i = 0; i < 3; i++)
        {
            double ti = t_[i], yi = y_[i];
            double k1 = EvalF(func, interp, pool, ti,           yi);
            double k2 = EvalF(func, interp, pool, ti + h/2.0,   yi + h*k1/2.0);
            double k3 = EvalF(func, interp, pool, ti + h/2.0,   yi + h*k2/2.0);
            double k4 = EvalF(func, interp, pool, ti + h,       yi + h*k3);
            y_[i+1] = yi + (h/6.0)*(k1 + 2*k2 + 2*k3 + k4);
            t_[i+1] = ti + h;
            f_[i+1] = EvalF(func, interp, pool, t_[i+1], y_[i+1]);
        }

        // ── Bucle Adams-Bashforth-Moulton ──
        for (int i = 3; i < n; i++)
        {
            // Predictor AB4
            double yPred = y_[i] + (h/24.0)*(55*f_[i] - 59*f_[i-1] + 37*f_[i-2] - 9*f_[i-3]);
            t_[i+1]      = t_[i] + h;
            double fPred = EvalF(func, interp, pool, t_[i+1], yPred);

            // Corrector AM4
            y_[i+1] = y_[i] + (h/24.0)*(9*fPred + 19*f_[i] - 5*f_[i-1] + f_[i-2]);
            f_[i+1] = EvalF(func, interp, pool, t_[i+1], y_[i+1]);
        }

        // Empacar trayectoria
        var tray = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i <= n; i++)
        {
            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(t_[i]));
            fila.Elements.Add(new LatinoNumber(y_[i]));
            tray.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"), tray },
            { new LatinoString("y_final"),     new LatinoNumber(y_[n]) },
            { new LatinoString("t_final"),     new LatinoNumber(t_[n]) },
            { new LatinoString("h"),           new LatinoNumber(h) },
            { new LatinoString("n_pasos"),     new LatinoNumber(n) },
            { new LatinoString("formula"),
              new LatinoString("AB4+AM4: predictor-corrector de orden 4 (multipaso)") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SISTEMAS DE EDOs (vectoriales)
    // ─────────────────────────────────────────────────────────────────────────

    // 6. RK4 para sistemas de EDOs ───────────────────────────────────────────

    [LatinoExtension(
        "edo.sistemaRk4",
        Args = "funcionF, t0, listaY0, tf, n",
        Help  = "Resuelve un sistema de EDOs dY/dt = F(t, Y) con RK4.\n\n" +
                "funcionF: función Latino F(t, Y) donde Y es una lista de estado.\n" +
                "           Debe retornar una lista con las derivadas [dy₁/dt, ..., dyₙ/dt].\n" +
                "t0:        tiempo inicial.\n" +
                "listaY0:  lista con las condiciones iniciales [y₁(t₀), y₂(t₀), ...].\n" +
                "tf:        tiempo final.\n" +
                "n:         número de pasos.\n\n" +
                "Ejemplo EDO de segundo orden y'' + y = 0 como sistema:\n" +
                "  F(t, Y) = [Y[1], -Y[0]]   con Y₀ = [1, 0] → solución: cos(t)\n\n" +
                "Retorna trayectoria: lista de [t, y₁, y₂, ...] en cada paso.")]
    public static LatinoObject SistemaRK4(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 5 || args[2] is not LatinoList lY0)
            throw new RuntimeError(null!, "edo.sistemaRk4 requiere: funcionF, t0, listaY0, tf, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino F(t, Y).");

        double t0 = GetDouble(args[1]);
        double tf = GetDouble(args[3]);
        int    n  = (int)GetDouble(args[4]);

        if (n < 1)    throw new RuntimeError(null!, "n debe ser al menos 1.");
        if (tf <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        int    dim   = lY0.Elements.Count;
        if (dim < 1) throw new RuntimeError(null!, "listaY0 no puede estar vacía.");

        var y0 = new double[dim];
        for (int i = 0; i < dim; i++) y0[i] = GetDouble(lY0.Elements[i]);

        double h       = (tf - t0) / n;
        var    poolY   = new LatinoList(new List<LatinoObject>()); // pool reutilizado para el vector Y
        for (int i = 0; i < dim; i++) poolY.Elements.Add(new LatinoNumber(0.0));
        var poolArgs   = new List<LatinoObject> { null!, poolY };

        var tsList = new List<double>(n + 1);
        var ysList = new List<double[]>(n + 1);

        double   t = t0;
        double[] y = (double[])y0.Clone();
        tsList.Add(t); ysList.Add((double[])y.Clone());

        for (int i = 0; i < n; i++)
        {
            double[] k1 = EvalSistema(func, interp, poolArgs, poolY, t,           y);
            double[] k2 = EvalSistema(func, interp, poolArgs, poolY, t + h/2.0,   SumaEscalar(y, h/2.0, k1));
            double[] k3 = EvalSistema(func, interp, poolArgs, poolY, t + h/2.0,   SumaEscalar(y, h/2.0, k2));
            double[] k4 = EvalSistema(func, interp, poolArgs, poolY, t + h,       SumaEscalar(y, h,     k3));

            var yNew = new double[dim];
            for (int d = 0; d < dim; d++)
                yNew[d] = y[d] + (h/6.0)*(k1[d] + 2*k2[d] + 2*k3[d] + k4[d]);

            t += h;
            y  = yNew;
            tsList.Add(t); ysList.Add((double[])y.Clone());
        }

        var tray = EmpacarTrayectoria(tsList, ysList);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"),  tray },
            { new LatinoString("y_final"),      VecALista(y) },
            { new LatinoString("t_final"),      new LatinoNumber(t) },
            { new LatinoString("h"),            new LatinoNumber(h) },
            { new LatinoString("n_pasos"),      new LatinoNumber(n) },
            { new LatinoString("dimension"),    new LatinoNumber(dim) },
            { new LatinoString("formula"),
              new LatinoString("RK4 vectorial: yₙ₊₁ = yₙ + (h/6)·(k₁ + 2k₂ + 2k₃ + k₄)") }
        };
        return new LatinoDictionary(dict);
    }

    // 7. RKF45 adaptativo para sistemas ──────────────────────────────────────

    [LatinoExtension(
        "edo.sistemaRkf45",
        Args = "funcionF, t0, listaY0, tf, [tolerancia], [hInicial], [hMin], [hMax]",
        Help  = "Resuelve un sistema de EDOs dY/dt = F(t, Y) con RKF45 adaptativo.\n\n" +
                "funcionF: función Latino F(t, Y) que retorna lista de derivadas.\n" +
                "t0, listaY0: condición inicial vectorial.\n" +
                "tf:        tiempo final.\n" +
                "Parámetros opcionales:\n" +
                "  tolerancia: error local máximo (defecto: 1e-6)\n" +
                "  hInicial:  paso inicial (defecto: (tf-t0)/100)\n" +
                "  hMin:      paso mínimo  (defecto: 1e-10)\n" +
                "  hMax:      paso máximo  (defecto: (tf-t0)/10)\n\n" +
                "El error se estima con la norma ‖y₅ - y₄‖∞. Control de paso automático.")]
    public static LatinoObject SistemaRKF45(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 4 || args[2] is not LatinoList lY0)
            throw new RuntimeError(null!,
                "edo.sistemaRkf45 requiere: funcionF, t0, listaY0, tf. " +
                "Opcionalmente: tolerancia, hInicial, hMin, hMax.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser una función Latino F(t, Y).");

        double t0   = GetDouble(args[1]);
        double tf   = GetDouble(args[3]);
        double tol  = args.Count > 4 ? GetDouble(args[4]) : 1e-6;
        double h    = args.Count > 5 ? GetDouble(args[5]) : (tf - t0) / 100.0;
        double hMin = args.Count > 6 ? GetDouble(args[6]) : 1e-10;
        double hMax = args.Count > 7 ? GetDouble(args[7]) : (tf - t0) / 10.0;

        if (tol  <= 0)  throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (tf   <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        int dim = lY0.Elements.Count;
        if (dim < 1) throw new RuntimeError(null!, "listaY0 no puede estar vacía.");

        var y0 = new double[dim];
        for (int i = 0; i < dim; i++) y0[i] = GetDouble(lY0.Elements[i]);

        // Coeficientes de Butcher (Fehlberg) — mismos que en edo.rkf45
        const double c2 = 1.0/4.0, c3 = 3.0/8.0, c4 = 12.0/13.0, c5 = 1.0, c6 = 1.0/2.0;
        const double a21 = 1.0/4.0;
        const double a31 = 3.0/32.0,      a32 = 9.0/32.0;
        const double a41 = 1932.0/2197.0, a42 = -7200.0/2197.0, a43 = 7296.0/2197.0;
        const double a51 = 439.0/216.0,   a52 = -8.0,           a53 = 3680.0/513.0,   a54 = -845.0/4104.0;
        const double a61 = -8.0/27.0,     a62 = 2.0,            a63 = -3544.0/2565.0, a64 = 1859.0/4104.0, a65 = -11.0/40.0;
        const double b1  = 25.0/216.0,    b3  = 1408.0/2565.0,  b4  = 2197.0/4104.0,  b5  = -1.0/5.0;
        const double e1  = 16.0/135.0,    e3  = 6656.0/12825.0, e4  = 28561.0/56430.0, e5  = -9.0/50.0, e6 = 2.0/55.0;

        var poolY    = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < dim; i++) poolY.Elements.Add(new LatinoNumber(0.0));
        var poolArgs = new List<LatinoObject> { null!, poolY };

        var tsList = new List<double>();
        var ysList = new List<double[]>();
        double t   = t0;
        double[] y = (double[])y0.Clone();
        tsList.Add(t); ysList.Add((double[])y.Clone());

        int pasosAceptados  = 0;
        int pasosRechazados = 0;
        int maxPasos        = 100_000;

        while (t < tf && pasosAceptados + pasosRechazados < maxPasos)
        {
            if (t + h > tf) h = tf - t;
            if (h < hMin)   h = hMin;

            double[] k1 = EvalSistema(func, interp, poolArgs, poolY, t,         y);
            double[] k2 = EvalSistema(func, interp, poolArgs, poolY, t + c2*h,  SumaEscalar(y, h*a21, k1));
            double[] k3 = EvalSistema(func, interp, poolArgs, poolY, t + c3*h,  SumaEscalar(SumaEscalar(y, h*a31, k1), h*a32, k2));
            double[] k4 = EvalSistema(func, interp, poolArgs, poolY, t + c4*h,  SumaEscalar(SumaEscalar(SumaEscalar(y, h*a41, k1), h*a42, k2), h*a43, k3));
            double[] k5 = EvalSistema(func, interp, poolArgs, poolY, t + c5*h,  SumaEscalar(SumaEscalar(SumaEscalar(SumaEscalar(y, h*a51, k1), h*a52, k2), h*a53, k3), h*a54, k4));
            double[] k6 = EvalSistema(func, interp, poolArgs, poolY, t + c6*h,  SumaEscalar(SumaEscalar(SumaEscalar(SumaEscalar(SumaEscalar(y, h*a61, k1), h*a62, k2), h*a63, k3), h*a64, k4), h*a65, k5));

            var y4 = new double[dim];
            var y5 = new double[dim];
            for (int d = 0; d < dim; d++)
            {
                y4[d] = y[d] + h*(b1*k1[d] + b3*k3[d] + b4*k4[d] + b5*k5[d]);
                y5[d] = y[d] + h*(e1*k1[d] + e3*k3[d] + e4*k4[d] + e5*k5[d] + e6*k6[d]);
            }

            // Error: norma infinito del vector de diferencias
            double errorLocal = 0;
            for (int d = 0; d < dim; d++)
            {
                double scale = Math.Max(1e-10, Math.Max(Math.Abs(y[d]), Math.Abs(y4[d])));
                double e     = Math.Abs(y5[d] - y4[d]) / scale;
                if (e > errorLocal) errorLocal = e;
            }

            if (errorLocal <= tol || h <= hMin)
            {
                y = y4;
                t += h;
                pasosAceptados++;
                tsList.Add(t); ysList.Add((double[])y.Clone());
            }
            else
            {
                pasosRechazados++;
            }

            if (errorLocal > 0)
            {
                double factor = 0.9 * Math.Pow(tol / errorLocal, 0.2);
                factor = Math.Max(0.1, Math.Min(4.0, factor));
                h = Math.Min(hMax, h * factor);
            }
        }

        bool completo = Math.Abs(t - tf) < 1e-10 * (tf - t0 + 1);
        var  tray     = EmpacarTrayectoria(tsList, ysList);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"),      tray },
            { new LatinoString("y_final"),          VecALista(y) },
            { new LatinoString("t_final"),          new LatinoNumber(t) },
            { new LatinoString("pasos_aceptados"),  new LatinoNumber(pasosAceptados) },
            { new LatinoString("pasos_rechazados"), new LatinoNumber(pasosRechazados) },
            { new LatinoString("dimension"),        new LatinoNumber(dim) },
            { new LatinoString("completo"),         new LatinoBoolean(completo) },
            { new LatinoString("formula"),
              new LatinoString("RKF45 vectorial — error = ‖y₅ - y₄‖∞, paso adaptativo") }
        };
        if (!completo)
            dict[new LatinoString("advertencia")] =
                new LatinoString("No se alcanzó tf. La EDO puede ser rígida — considera edo.sistemaRk4 con más pasos.");
        return new LatinoDictionary(dict);
    }

    // 8. Euler hacia atrás (implícito) — para EDOs rígidas ───────────────────

    [LatinoExtension(
        "edo.eulerImplicito",
        Args = "funcionF, funcionDfDy, t0, y0, tf, n",
        Help  = "Resuelve dy/dt = f(t, y) con el Método de Euler Implícito (hacia atrás).\n" +
                "Incondicionalmente estable — ideal para EDOs rígidas (stiff).\n\n" +
                "funcionF:     f(t, y) → dy/dt.\n" +
                "funcionDfDy: ∂f/∂y(t, y) → derivada parcial respecto a y.\n" +
                "               Requerida para resolver la ecuación implícita con Newton.\n\n" +
                "Ecuación implícita: yₙ₊₁ = yₙ + h·f(tₙ₊₁, yₙ₊₁)\n" +
                "Resuelta con Newton: G(y) = y - yₙ - h·f(tₙ₊₁, y) = 0\n" +
                "Error global: O(h). Estabilidad: A-estable (no hay límite de h por estabilidad).")]
    public static LatinoObject EulerImplicito(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 6)
            throw new RuntimeError(null!,
                "edo.euler_implicito requiere: funcion_f, funcion_df_dy, t0, y0, tf, n.");

        var func   = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser f(t,y).");
        var dfdy   = args[1] as ICallable
            ?? throw new RuntimeError(null!, "El segundo argumento debe ser df/dy(t,y).");

        double t0 = GetDouble(args[2]);
        double y0 = GetDouble(args[3]);
        double tf = GetDouble(args[4]);
        int    n  = (int)GetDouble(args[5]);

        if (n < 1)    throw new RuntimeError(null!, "n debe ser al menos 1.");
        if (tf <= t0) throw new RuntimeError(null!, "tf debe ser mayor que t0.");

        double h     = (tf - t0) / n;
        var    poolF = new List<LatinoObject> { null!, null! };
        var    poolD = new List<LatinoObject> { null!, null! };
        var    tray  = new LatinoList(new List<LatinoObject>());

        double t = t0, y = y0;
        var f0 = new LatinoList(new List<LatinoObject>());
        f0.Elements.Add(new LatinoNumber(t)); f0.Elements.Add(new LatinoNumber(y));
        tray.Elements.Add(f0);

        for (int i = 0; i < n; i++)
        {
            double tNext = t + h;
            // Newton para resolver: G(yNew) = yNew - y - h·f(tNext, yNew) = 0
            double yNew = y; // semilla inicial = Euler explícito
            int    newtonIter = 0;

            while (newtonIter < 50)
            {
                double G    = yNew - y - h * EvalF(func, interp, poolF, tNext, yNew);
                double Gprime = 1.0 - h * EvalF(dfdy, interp, poolD, tNext, yNew);
                if (Math.Abs(Gprime) < 1e-14)
                    throw new RuntimeError(null!,
                        $"G'(y) ≈ 0 en paso {i}, Newton diverge. Verifica df/dy.");
                double delta = G / Gprime;
                yNew -= delta;
                newtonIter++;
                if (Math.Abs(delta) < 1e-12) break;
            }

            y = yNew;
            t = tNext;

            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(t));
            fila.Elements.Add(new LatinoNumber(y));
            tray.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("trayectoria"), tray },
            { new LatinoString("y_final"),     new LatinoNumber(y) },
            { new LatinoString("t_final"),     new LatinoNumber(t) },
            { new LatinoString("h"),           new LatinoNumber(h) },
            { new LatinoString("n_pasos"),     new LatinoNumber(n) },
            { new LatinoString("formula"),
              new LatinoString("yₙ₊₁ = yₙ + h·f(tₙ₊₁, yₙ₊₁) resuelto con Newton (A-estable)") }
        };
        return new LatinoDictionary(dict);
    }

    // 9. Problema de valor de frontera — diferencias finitas ─────────────────

    [LatinoExtension(
        "edo.pvf",
        Args = "funcion_f, a, b, ya, yb, n",
        Help  = "Resuelve un Problema de Valor de Frontera (PVF) de segundo orden:\n" +
                "  y'' = f(x, y, y')  con  y(a) = ya, y(b) = yb\n\n" +
                "Usa el método de Diferencias Finitas centrales.\n" +
                "funcion_f: función Latino f(x, y, yprime) → y''\n" +
                "           (para ecuaciones lineales y'' + p(x)y' + q(x)y = r(x))\n" +
                "a, b:      extremos del intervalo.\n" +
                "ya, yb:    condiciones de frontera.\n" +
                "n:         número de subintervalos interiores (n ≥ 3).\n\n" +
                "NOTA: Solo para ecuaciones lineales. Para no lineales usa edo.disparo.\n" +
                "Fórmula: (yᵢ₋₁ - 2yᵢ + yᵢ₊₁)/h² = f(xᵢ, yᵢ, (yᵢ₊₁-yᵢ₋₁)/(2h))")]
    public static LatinoObject PVF(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 6)
            throw new RuntimeError(null!, "edo.pvf requiere: funcion_f, a, b, ya, yb, n.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser f(x, y, y').");

        double a  = GetDouble(args[1]);
        double b  = GetDouble(args[2]);
        double ya = GetDouble(args[3]);
        double yb = GetDouble(args[4]);
        int    n  = (int)GetDouble(args[5]);

        if (n < 3)  throw new RuntimeError(null!, "n debe ser al menos 3.");
        if (b <= a) throw new RuntimeError(null!, "b debe ser mayor que a.");

        double h   = (b - a) / n;
        int    m   = n - 1; // nodos interiores

        // Nodos interiores x₁ ... xₘ
        var x = new double[m + 2];
        for (int i = 0; i <= n; i++) x[i] = a + i * h;

        // Pool de 3 argumentos para f(x, y, y')
        var pool3 = new List<LatinoObject> { null!, null!, null! };

        // Vector y (incluye fronteras en y[0] y y[n])
        var y = new double[m + 2];
        y[0] = ya; y[n] = yb;

        // Semilla inicial: interpolación lineal
        for (int i = 1; i <= m; i++)
            y[i] = ya + (yb - ya) * (x[i] - a) / (b - a);

        // Iteración de punto fijo (para ecuaciones lineales converge en 1-2 pasos)
        for (int iter = 0; iter < 100; iter++)
        {
            var yNew = (double[])y.Clone();
            double maxCambio = 0;

            for (int i = 1; i <= m; i++)
            {
                double yprime = (y[i + 1] - y[i - 1]) / (2.0 * h);
                pool3[0] = new LatinoNumber(x[i]);
                pool3[1] = new LatinoNumber(y[i]);
                pool3[2] = new LatinoNumber(yprime);
                var res    = func.Call(interp, pool3);
                double fxy = GetDouble(res);

                // y'' ≈ fxy → despejar yᵢ desde esquema de diferencias
                // (yᵢ₋₁ - 2yᵢ + yᵢ₊₁)/h² = fxy
                // yᵢ = (yᵢ₋₁ + yᵢ₊₁ - h²·fxy) / 2
                yNew[i] = (y[i - 1] + y[i + 1] - h * h * fxy) / 2.0;
                double cambio = Math.Abs(yNew[i] - y[i]);
                if (cambio > maxCambio) maxCambio = cambio;
            }

            y = yNew;
            if (maxCambio < 1e-12) break;
        }

        // Empacar solución
        var tray = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i <= n; i++)
        {
            var fila = new LatinoList(new List<LatinoObject>());
            fila.Elements.Add(new LatinoNumber(x[i]));
            fila.Elements.Add(new LatinoNumber(y[i]));
            tray.Elements.Add(fila);
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("solucion"),    tray },
            { new LatinoString("n_nodos"),     new LatinoNumber(n + 1) },
            { new LatinoString("h"),           new LatinoNumber(h) },
            { new LatinoString("formula"),
              new LatinoString("(yᵢ₋₁-2yᵢ+yᵢ₊₁)/h² = f(xᵢ,yᵢ,(yᵢ₊₁-yᵢ₋₁)/2h)  (Diferencias Finitas)") }
        };
        return new LatinoDictionary(dict);
    }

    // 10. Método de Disparo — PVF no lineal ──────────────────────────────────

    [LatinoExtension(
        "edo.disparo",
        Args = "funcion_sistema, a, b, ya, yb, [tolerancia], [max_iter]",
        Help  = "Resuelve un PVF de segundo orden no lineal por el Método de Disparo.\n" +
                "Convierte el PVF en un PVI buscando la pendiente inicial correcta.\n\n" +
                "funcion_sistema: F(t, Y) para el sistema [y, y'] = [Y[1], f(t, Y[0], Y[1])].\n" +
                "                 Misma firma que edo.sistema_rk4.\n" +
                "a, b:   extremos del intervalo.\n" +
                "ya, yb: condiciones de frontera y(a) = ya, y(b) = yb.\n" +
                "Usa la Secante para buscar la pendiente inicial s tal que y(b; s) = yb.\n\n" +
                "Fórmula: buscar s tal que φ(s) = y(b; s) - yb = 0  (método de la secante)")]
    public static LatinoObject Disparo(Interpreter interp, List<LatinoObject> args)
    {
        if (args.Count < 5)
            throw new RuntimeError(null!,
                "edo.disparo requiere: funcion_sistema, a, b, ya, yb. " +
                "Opcionalmente: tolerancia, max_iter.");

        var func = args[0] as ICallable
            ?? throw new RuntimeError(null!, "El primer argumento debe ser F(t, Y).");

        double a       = GetDouble(args[1]);
        double b       = GetDouble(args[2]);
        double ya      = GetDouble(args[3]);
        double yb      = GetDouble(args[4]);
        double tol     = args.Count > 5 ? GetDouble(args[5]) : 1e-8;
        int    maxIter = args.Count > 6 ? (int)GetDouble(args[6]) : 50;

        if (b <= a) throw new RuntimeError(null!, "b debe ser mayor que a.");

        int    nPasos = 200;
        double h      = (b - a) / nPasos;

        var poolY    = new LatinoList(new List<LatinoObject>());
        poolY.Elements.Add(new LatinoNumber(0.0));
        poolY.Elements.Add(new LatinoNumber(0.0));
        var poolArgs = new List<LatinoObject> { null!, poolY };

        // Integra PVI con pendiente inicial s y retorna y(b)
        double Disparar(double s)
        {
            double[] y = { ya, s };
            double   t = a;
            for (int i = 0; i < nPasos; i++)
            {
                double[] k1 = EvalSistema(func, interp, poolArgs, poolY, t,          y);
                double[] k2 = EvalSistema(func, interp, poolArgs, poolY, t + h/2.0,  SumaEscalar(y, h/2.0, k1));
                double[] k3 = EvalSistema(func, interp, poolArgs, poolY, t + h/2.0,  SumaEscalar(y, h/2.0, k2));
                double[] k4 = EvalSistema(func, interp, poolArgs, poolY, t + h,      SumaEscalar(y, h,     k3));
                y = new double[]
                {
                    y[0] + (h/6.0)*(k1[0] + 2*k2[0] + 2*k3[0] + k4[0]),
                    y[1] + (h/6.0)*(k1[1] + 2*k2[1] + 2*k3[1] + k4[1])
                };
                t += h;
            }
            return y[0] - yb; // φ(s) = y(b; s) - yb
        }

        // Semillas iniciales para la Secante
        double s0   = (yb - ya) / (b - a);    // pendiente de la cuerda
        double s1   = s0 + 1.0;
        double phi0 = Disparar(s0);
        double phi1 = Disparar(s1);

        double sOpt = s0;
        int    iter = 0;
        double error = Math.Abs(phi1);

        while (iter < maxIter)
        {
            double dphi = phi1 - phi0;
            if (Math.Abs(dphi) < 1e-14)
                throw new RuntimeError(null!, "Secante diverge en edo.disparo. Ajusta las semillas.");

            double s2   = s1 - phi1 * (s1 - s0) / dphi;
            double phi2 = Disparar(s2);
            error = Math.Abs(phi2);
            iter++;
            sOpt = s2;

            if (error < tol) break;
            s0   = s1; phi0 = phi1;
            s1   = s2; phi1 = phi2;
        }

        // Integración final con s óptimo para obtener la trayectoria
        double[] yF   = { ya, sOpt };
        double   tF   = a;
        var      tray = new LatinoList(new List<LatinoObject>());
        var      row0 = new LatinoList(new List<LatinoObject>());
        row0.Elements.Add(new LatinoNumber(tF));
        row0.Elements.Add(new LatinoNumber(yF[0]));
        tray.Elements.Add(row0);

        for (int i = 0; i < nPasos; i++)
        {
            double[] k1 = EvalSistema(func, interp, poolArgs, poolY, tF,          yF);
            double[] k2 = EvalSistema(func, interp, poolArgs, poolY, tF + h/2.0,  SumaEscalar(yF, h/2.0, k1));
            double[] k3 = EvalSistema(func, interp, poolArgs, poolY, tF + h/2.0,  SumaEscalar(yF, h/2.0, k2));
            double[] k4 = EvalSistema(func, interp, poolArgs, poolY, tF + h,      SumaEscalar(yF, h,     k3));
            yF = new double[]
            {
                yF[0] + (h/6.0)*(k1[0] + 2*k2[0] + 2*k3[0] + k4[0]),
                yF[1] + (h/6.0)*(k1[1] + 2*k2[1] + 2*k3[1] + k4[1])
            };
            tF += h;
            var row = new LatinoList(new List<LatinoObject>());
            row.Elements.Add(new LatinoNumber(tF));
            row.Elements.Add(new LatinoNumber(yF[0]));
            tray.Elements.Add(row);
        }

        bool convergio = error < tol;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("solucion"),         tray },
            { new LatinoString("pendiente_inicial"), new LatinoNumber(sOpt) },
            { new LatinoString("convergio"),         new LatinoBoolean(convergio) },
            { new LatinoString("iteraciones"),       new LatinoNumber(iter) },
            { new LatinoString("error_frontera"),    new LatinoNumber(error) },
            { new LatinoString("formula"),
              new LatinoString("φ(s) = y(b;s) - yb = 0  (Secante + RK4 interno)") }
        };
        if (!convergio)
            dict[new LatinoString("advertencia")] =
                new LatinoString($"No convergió en {maxIter} iteraciones. " +
                                 "Ajusta las semillas o aumenta max_iter.");
        return new LatinoDictionary(dict);
    }
}
