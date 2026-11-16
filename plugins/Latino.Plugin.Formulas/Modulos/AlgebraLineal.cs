using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class AlgebraLinealFormulas
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    private static double GetDouble(LatinoObject obj) =>
        obj is LatinoNumber n ? n.Value
            : throw new RuntimeError(null!, "Se esperaba un número.");

    /// <summary>
    /// LatinoList de LatinoList → double[,]
    /// Valida que todas las filas tengan la misma longitud.
    /// </summary>
    private static double[,] MatrizADouble(LatinoList lista, string nombre = "matriz")
    {
        int filas = lista.Elements.Count;
        if (filas == 0)
            throw new RuntimeError(null!, $"La {nombre} no puede estar vacía.");

        if (lista.Elements[0] is not LatinoList fila0)
            throw new RuntimeError(null!, $"La {nombre} debe ser una lista de listas.");

        int cols = fila0.Elements.Count;
        if (cols == 0)
            throw new RuntimeError(null!, $"Las filas de {nombre} no pueden estar vacías.");

        var M = new double[filas, cols];
        for (int i = 0; i < filas; i++)
        {
            if (lista.Elements[i] is not LatinoList fila)
                throw new RuntimeError(null!, $"Fila {i} de {nombre} no es una lista.");
            if (fila.Elements.Count != cols)
                throw new RuntimeError(null!,
                    $"Todas las filas de {nombre} deben tener la misma longitud. " +
                    $"Fila 0 tiene {cols} columnas, fila {i} tiene {fila.Elements.Count}.");
            for (int j = 0; j < cols; j++)
                M[i, j] = GetDouble(fila.Elements[j]);
        }
        return M;
    }

    /// <summary>LatinoList de números → double[]</summary>
    private static double[] VectorADouble(LatinoList lista, string nombre = "vector")
    {
        int n = lista.Elements.Count;
        if (n == 0) throw new RuntimeError(null!, $"El {nombre} no puede estar vacío.");
        var v = new double[n];
        for (int i = 0; i < n; i++) v[i] = GetDouble(lista.Elements[i]);
        return v;
    }

    /// <summary>double[,] → LatinoList de LatinoList</summary>
    private static LatinoList MatrizALista(double[,] M)
    {
        int filas = M.GetLength(0);
        int cols  = M.GetLength(1);
        var lista = new LatinoList(new List<LatinoObject>());
        for (int i = 0; i < filas; i++)
        {
            var fila = new LatinoList(new List<LatinoObject>());
            for (int j = 0; j < cols; j++)
                fila.Elements.Add(new LatinoNumber(M[i, j]));
            lista.Elements.Add(fila);
        }
        return lista;
    }

    /// <summary>double[] → LatinoList</summary>
    private static LatinoList VectorALista(double[] v)
    {
        var lista = new LatinoList(new List<LatinoObject>());
        foreach (var d in v) lista.Elements.Add(new LatinoNumber(d));
        return lista;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Rutinas internas de álgebra (todo en double puro — nunca LatinoObject)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica una operación entre cada elemento escalar de un tensor
    /// (lista n-dimensional) y un valor escalar. Trabaja recursivamente.
    /// </summary>
    private static LatinoList AplicarEscalar(
        LatinoList origen,
        double escalar,
        Func<double, double, double> operacion)
    {
        var resultado = new LatinoList(
            new List<LatinoObject>(origen.Elements.Count));

        foreach (var item in origen.Elements)
        {
            if (item is LatinoList subLista)
                resultado.Elements.Add(
                    AplicarEscalar(subLista, escalar, operacion));
            else
                resultado.Elements.Add(
                    new LatinoNumber(operacion(GetDouble(item), escalar)));
        }
        return resultado;
    }

    /// <summary>
    /// Aplica una operación término a término entre dos tensores de idéntica
    /// forma (shape). Lanza RuntimeError si las formas no coinciden.
    /// </summary>
    private static LatinoList AplicarTerminoATermino(
        LatinoList a,
        LatinoList b,
        Func<double, double, double> operacion)
    {
        if (a.Elements.Count != b.Elements.Count)
            throw new RuntimeError(null!,
                $"Las colecciones deben tener la misma longitud/dimensión " +
                $"({a.Elements.Count} ≠ {b.Elements.Count}).");

        var resultado = new LatinoList(
            new List<LatinoObject>(a.Elements.Count));

        for (int i = 0; i < a.Elements.Count; i++)
        {
            var itemA = a.Elements[i];
            var itemB = b.Elements[i];

            if (itemA is LatinoList subA && itemB is LatinoList subB)
                resultado.Elements.Add(
                    AplicarTerminoATermino(subA, subB, operacion));
            else if (itemA is LatinoList || itemB is LatinoList)
                throw new RuntimeError(null!,
                    $"Incompatibilidad de forma (shape mismatch) en posición {i}: " +
                    "un elemento es lista y el otro es escalar.");
            else
                resultado.Elements.Add(
                    new LatinoNumber(
                        operacion(GetDouble(itemA), GetDouble(itemB))));
        }
        return resultado;
    }

    /// <summary>
    /// Aplica una función matemática unaria (ufunc) a cada elemento escalar
    /// del tensor, sin importar su profundidad.
    /// </summary>
    private static LatinoList AplicarUniversal(
        LatinoList origen,
        Func<double, double> operacion)
    {
        var resultado = new LatinoList(
            new List<LatinoObject>(origen.Elements.Count));

        foreach (var item in origen.Elements)
        {
            if (item is LatinoList subLista)
                resultado.Elements.Add(AplicarUniversal(subLista, operacion));
            else
                resultado.Elements.Add(
                    new LatinoNumber(operacion(GetDouble(item))));
        }
        return resultado;
    }

    /// <summary>
    /// Eliminación gaussiana con pivoteo parcial.
    /// Resuelve A·x = b. Opera sobre copias — no modifica los originales.
    /// Retorna (x, det_sign) donde det_sign acumula el signo de los intercambios.
    /// </summary>
    private static (double[] x, int detSign) GaussElim(double[,] A, double[] b)
    {
        int n    = b.Length;
        var Ac   = (double[,])A.Clone();
        var bc   = (double[])b.Clone();
        int sign = 1;

        for (int col = 0; col < n; col++)
        {
            // Pivoteo parcial
            int maxRow = col;
            double maxVal = Math.Abs(Ac[col, col]);
            for (int row = col + 1; row < n; row++)
                if (Math.Abs(Ac[row, col]) > maxVal)
                { maxVal = Math.Abs(Ac[row, col]); maxRow = row; }

            if (maxVal < 1e-14)
                throw new RuntimeError(null!,
                    "La matriz es singular (o casi singular). No tiene solución única.");

            if (maxRow != col)
            {
                for (int j = col; j < n; j++) (Ac[col, j], Ac[maxRow, j]) = (Ac[maxRow, j], Ac[col, j]);
                (bc[col], bc[maxRow]) = (bc[maxRow], bc[col]);
                sign = -sign;
            }

            for (int row = col + 1; row < n; row++)
            {
                double factor = Ac[row, col] / Ac[col, col];
                for (int j = col; j < n; j++) Ac[row, j] -= factor * Ac[col, j];
                bc[row] -= factor * bc[col];
            }
        }

        // Sustitución regresiva
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = bc[i];
            for (int j = i + 1; j < n; j++) x[i] -= Ac[i, j] * x[j];
            x[i] /= Ac[i, i];
        }
        return (x, sign);
    }

    /// <summary>
    /// Factorización LU con pivoteo parcial (Doolittle).
    /// Retorna (L, U, perm) donde perm es el vector de permutaciones.
    /// </summary>
    private static (double[,] L, double[,] U, int[] perm, int sign) FactLU(double[,] A)
    {
        int n    = A.GetLength(0);
        var U    = (double[,])A.Clone();
        var L    = new double[n, n];
        var perm = new int[n];
        int sign = 1;

        for (int i = 0; i < n; i++) { L[i, i] = 1.0; perm[i] = i; }

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = Math.Abs(U[col, col]);
            for (int row = col + 1; row < n; row++)
                if (Math.Abs(U[row, col]) > maxVal)
                { maxVal = Math.Abs(U[row, col]); maxRow = row; }

            if (maxVal < 1e-14)
                throw new RuntimeError(null!, "La matriz es singular. LU requiere matriz invertible.");

            if (maxRow != col)
            {
                for (int j = 0; j < n; j++)
                {
                    (U[col, j], U[maxRow, j]) = (U[maxRow, j], U[col, j]);
                    if (j < col) (L[col, j], L[maxRow, j]) = (L[maxRow, j], L[col, j]);
                }
                (perm[col], perm[maxRow]) = (perm[maxRow], perm[col]);
                sign = -sign;
            }

            for (int row = col + 1; row < n; row++)
            {
                if (Math.Abs(U[col, col]) < 1e-14) continue;
                double factor  = U[row, col] / U[col, col];
                L[row, col]    = factor;
                for (int j = col; j < n; j++) U[row, j] -= factor * U[col, j];
            }
        }
        return (L, U, perm, sign);
    }

    /// <summary>Multiplica dos matrices cuadradas o rectangulares A (m×k) × B (k×n).</summary>
    private static double[,] MatMul(double[,] A, double[,] B)
    {
        int m = A.GetLength(0), k = A.GetLength(1), n = B.GetLength(1);
        if (k != B.GetLength(0))
            throw new RuntimeError(null!, "Dimensiones incompatibles para multiplicación de matrices.");
        var C = new double[m, n];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                for (int r = 0; r < k; r++)
                    C[i, j] += A[i, r] * B[r, j];
        return C;
    }

    /// <summary>Transpuesta de A (m×n) → (n×m).</summary>
    private static double[,] Transponer(double[,] A)
    {
        int m = A.GetLength(0), n = A.GetLength(1);
        var T = new double[n, m];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                T[j, i] = A[i, j];
        return T;
    }

    /// <summary>
    /// Factorización de Cholesky A = L·Lᵀ para matrices simétricas definidas positivas.
    /// </summary>
    private static double[,] Cholesky(double[,] A)
    {
        int n = A.GetLength(0);
        var L = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double s = A[i, j];
                for (int k = 0; k < j; k++) s -= L[i, k] * L[j, k];
                if (i == j)
                {
                    if (s <= 0)
                        throw new RuntimeError(null!,
                            "La matriz no es definida positiva. " +
                            "Cholesky requiere que todos los valores propios sean estrictamente positivos.");
                    L[i, j] = Math.Sqrt(s);
                }
                else
                {
                    L[i, j] = s / L[j, j];
                }
            }
        }
        return L;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MÉTODOS REGISTRADOS
    // ─────────────────────────────────────────────────────────────────────────

    // 1. Resolver sistema lineal Ax = b (Gauss) ──────────────────────────────

    [LatinoExtension(
        "algebraLineal.resolver",
        Args = "matriz_A, vector_b",
        Help  = "Resuelve el sistema lineal A·x = b por eliminación gaussiana con pivoteo parcial.\n" +
                "matriz_A: lista de listas (n×n).\n" +
                "vector_b: lista de n valores.\n\n" +
                "Retorna el vector solución x y la norma del residuo ‖Ax - b‖ como verificación.\n" +
                "Fórmula: A·x = b → x = A⁻¹·b  (vía eliminación, no inversión explícita)")]
    public static LatinoObject Resolver(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lA || args[1] is not LatinoList lB)
            throw new RuntimeError(null!, "algebra.resolver requiere: matriz_A (lista de listas), vector_b (lista).");

        var A = MatrizADouble(lA, "matriz_A");
        var b = VectorADouble(lB, "vector_b");
        int n = b.Length;

        if (A.GetLength(0) != n || A.GetLength(1) != n)
            throw new RuntimeError(null!,
                $"matriz_A debe ser cuadrada n×n con n = {n} (longitud de vector_b).");

        var (x, _) = GaussElim(A, b);

        // Norma del residuo ‖Ax - b‖₂
        double normRes = 0;
        for (int i = 0; i < n; i++)
        {
            double Axib = -b[i];
            for (int j = 0; j < n; j++) Axib += A[i, j] * x[j];
            normRes += Axib * Axib;
        }
        normRes = Math.Sqrt(normRes);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("solucion"),      VectorALista(x) },
            { new LatinoString("norma_residuo"), new LatinoNumber(normRes) },
            { new LatinoString("n"),             new LatinoNumber(n) },
            { new LatinoString("formula"),       new LatinoString("A·x = b (Gauss con pivoteo parcial)") }
        };
        if (normRes > 1e-6)
            dict[new LatinoString("advertencia")] =
                new LatinoString($"Norma del residuo = {normRes:G3}. " +
                                 "Puede indicar mal condicionamiento. Verifica la matriz.");
        return new LatinoDictionary(dict);
    }

    // 2. Factorización LU ────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.factorizacionLU",
        Args = "matriz_A",
        Help  = "Descompone A = P·L·U donde:\n" +
                "  P: matriz de permutaciones (pivoteo parcial)\n" +
                "  L: triangular inferior con 1s en la diagonal\n" +
                "  U: triangular superior\n\n" +
                "Útil para resolver múltiples sistemas con la misma A,\n" +
                "calcular determinantes y verificar invertibilidad.\n" +
                "Fórmula: det(A) = (-1)^s · ∏ Uᵢᵢ  donde s = núm. de intercambios.")]
    public static LatinoObject FactorizacionLU(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.factorizacionLU requiere: matriz_A (lista de listas cuadrada).");

        var A = MatrizADouble(lA, "matriz_A");
        int n = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.factorizacionLU requiere una matriz cuadrada.");

        var (L, U, perm, sign) = FactLU(A);

        // Determinante: (-1)^s · ∏ U[i,i]
        double det = sign;
        for (int i = 0; i < n; i++) det *= U[i, i];

        // Matriz P desde el vector perm
        var P = new double[n, n];
        for (int i = 0; i < n; i++) P[i, perm[i]] = 1.0;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("L"),            MatrizALista(L) },
            { new LatinoString("U"),            MatrizALista(U) },
            { new LatinoString("P"),            MatrizALista(P) },
            { new LatinoString("determinante"), new LatinoNumber(det) },
            { new LatinoString("n"),            new LatinoNumber(n) },
            { new LatinoString("formula"),      new LatinoString("A = P·L·U (Doolittle con pivoteo parcial)") }
        };
        return new LatinoDictionary(dict);
    }

    // 3. Factorización de Cholesky ────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.cholesky",
        Args = "matriz_A",
        Help  = "Descompone A = L·Lᵀ para matrices simétricas definidas positivas.\n" +
                "Dos veces más rápido que LU para este tipo de matrices.\n" +
                "Condición: A debe ser simétrica (Aᵢⱼ = Aⱼᵢ) y todos sus valores propios > 0.\n\n" +
                "Aplicaciones: mínimos cuadrados, simulaciones físicas, matrices de covarianza.\n" +
                "Fórmula: Lᵢᵢ = √(Aᵢᵢ - Σ Lᵢₖ²) | Lᵢⱼ = (Aᵢⱼ - Σ LᵢₖLⱼₖ) / Lⱼⱼ")]
    public static LatinoObject FactorizacionCholesky(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.cholesky requiere: matriz_A (simétrica definida positiva).");

        var A = MatrizADouble(lA, "matriz_A");
        int n = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.cholesky requiere una matriz cuadrada.");

        // Verificar simetría (tolerancia razonable)
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                if (Math.Abs(A[i, j] - A[j, i]) > 1e-10 * (1 + Math.Abs(A[i, j])))
                    throw new RuntimeError(null!,
                        $"La matriz no es simétrica: A[{i},{j}] = {A[i,j]:G}, A[{j},{i}] = {A[j,i]:G}.");

        var L  = Cholesky(A);
        var Lt = Transponer(L);

        // Determinante: det(A) = (det L)² = (∏ Lᵢᵢ)²
        double detL = 1.0;
        for (int i = 0; i < n; i++) detL *= L[i, i];
        double det = detL * detL;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("L"),            MatrizALista(L) },
            { new LatinoString("Lt"),           MatrizALista(Lt) },
            { new LatinoString("determinante"), new LatinoNumber(det) },
            { new LatinoString("n"),            new LatinoNumber(n) },
            { new LatinoString("formula"),      new LatinoString("A = L·Lᵀ (Cholesky)") }
        };
        return new LatinoDictionary(dict);
    }

    // 4. Inversión de matriz ──────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.invertir",
        Args = "matriz_A",
        Help  = "Calcula la matriz inversa A⁻¹ usando LU con pivoteo parcial.\n" +
                "Resuelve A·xᵢ = eᵢ para cada vector canónico eᵢ.\n\n" +
                "ADVERTENCIA: Evita multiplicar por A⁻¹ para resolver A·x = b;\n" +
                "usa algebra.resolver directamente — es más preciso y eficiente.\n" +
                "Fórmula: A·A⁻¹ = I  →  ‖A·A⁻¹ - I‖ se reporta como error.")]
    public static LatinoObject Invertir(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.invertir requiere: matriz_A (lista de listas cuadrada).");

        var A = MatrizADouble(lA, "matriz_A");
        int n = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.invertir requiere una matriz cuadrada.");

        var Ainv = new double[n, n];

        // Resolver A·xᵢ = eᵢ para cada i
        for (int i = 0; i < n; i++)
        {
            var ei = new double[n];
            ei[i] = 1.0;
            var (xi, _) = GaussElim(A, ei);
            for (int j = 0; j < n; j++) Ainv[j, i] = xi[j];
        }

        // ‖A·A⁻¹ - I‖_F (error de Frobenius)
        var prod  = MatMul(A, Ainv);
        double errF = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double dif = prod[i, j] - (i == j ? 1.0 : 0.0);
                errF += dif * dif;
            }
        errF = Math.Sqrt(errF);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("inversa"),       MatrizALista(Ainv) },
            { new LatinoString("error_frobenius"), new LatinoNumber(errF) },
            { new LatinoString("n"),             new LatinoNumber(n) },
            { new LatinoString("formula"),       new LatinoString("A·xᵢ = eᵢ para cada columna (vía LU)") }
        };
        if (errF > 1e-8)
            dict[new LatinoString("advertencia")] =
                new LatinoString($"Error de Frobenius = {errF:G3}. La matriz puede estar mal condicionada.");
        return new LatinoDictionary(dict);
    }

    // 5. Determinante ─────────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.determinante",
        Args = "matriz_A",
        Help  = "Calcula el determinante de una matriz cuadrada usando factorización LU.\n" +
                "Fórmula: det(A) = (-1)^s · ∏ Uᵢᵢ  donde s = número de intercambios de filas.\n\n" +
                "Complejidad O(n³). Exacto para matrices hasta ~15×15 con aritmética de punto flotante.")]
    public static LatinoObject Determinante(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.determinante requiere: matriz_A (lista de listas cuadrada).");

        var A = MatrizADouble(lA, "matriz_A");
        int n = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.determinante requiere una matriz cuadrada.");

        var (_, U, _, sign) = FactLU(A);
        double det = sign;
        for (int i = 0; i < n; i++) det *= U[i, i];

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), new LatinoNumber(det) },
            { new LatinoString("n"),         new LatinoNumber(n) },
            { new LatinoString("formula"),   new LatinoString("det(A) = (-1)^s · ∏ Uᵢᵢ  (LU)") }
        };
        return new LatinoDictionary(dict);
    }

    // 6. Valores y vectores propios (método de la potencia) ──────────────────

    [LatinoExtension(
        "algebraLineal.valorPropioDominante",
        Args = "matriz_A, [tolerancia], [max_iter]",
        Help  = "Encuentra el valor propio dominante (mayor en módulo) y su vector propio\n" +
                "usando el Método de la Potencia con normalización.\n" +
                "Parámetros opcionales: tolerancia (defecto 1e-10), max_iter (defecto 10 000).\n\n" +
                "Fórmula: x_{k+1} = A·xₖ / ‖A·xₖ‖,  λ = xᵀ·A·x / xᵀ·x (cociente de Rayleigh)\n" +
                "ADVERTENCIA: Solo converge al valor propio de mayor módulo. " +
                "Para todos los valores propios usa algebra.valores_propios.")]
    public static LatinoObject ValorPropioDominante(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.valorPropioDominante requiere: matriz_A. Opcionalmente: tolerancia, max_iter.");

        var A  = MatrizADouble(lA, "matriz_A");
        int n  = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.valorPropioDominante requiere una matriz cuadrada.");

        double tol     = args.Count > 1 ? GetDouble(args[1]) : 1e-10;
        int    maxIter = args.Count > 2 ? (int)GetDouble(args[2]) : 10_000;
        if (tol <= 0)    throw new RuntimeError(null!, "La tolerancia debe ser mayor a cero.");
        if (maxIter < 1) throw new RuntimeError(null!, "max_iter debe ser al menos 1.");

        // Vector inicial: todos unos normalizado
        var x     = new double[n];
        for (int i = 0; i < n; i++) x[i] = 1.0 / Math.Sqrt(n);

        double lambda = 0, lambdaPrev = double.MaxValue, error = double.MaxValue;
        int iter = 0;

        while (iter < maxIter)
        {
            // y = A·x
            var y = new double[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    y[i] += A[i, j] * x[j];

            // Cociente de Rayleigh: λ = xᵀ·y / xᵀ·x  (xᵀ·x = 1 ya que x normalizado)
            lambda = 0;
            for (int i = 0; i < n; i++) lambda += x[i] * y[i];

            // Normalizar y → nueva x
            double norma = 0;
            for (int i = 0; i < n; i++) norma += y[i] * y[i];
            norma = Math.Sqrt(norma);
            if (norma < 1e-15)
                throw new RuntimeError(null!, "El vector iterable colapsó a cero. La matriz puede ser singular.");
            for (int i = 0; i < n; i++) x[i] = y[i] / norma;

            error = Math.Abs(lambda - lambdaPrev);
            iter++;

            if (error < tol) break;
            lambdaPrev = lambda;
        }

        bool convergio = error < tol;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("valor_propio"),   new LatinoNumber(lambda) },
            { new LatinoString("vector_propio"),  VectorALista(x) },
            { new LatinoString("convergio"),      new LatinoBoolean(convergio) },
            { new LatinoString("iteraciones"),    new LatinoNumber(iter) },
            { new LatinoString("error_estimado"), new LatinoNumber(error) },
            { new LatinoString("formula"),
              new LatinoString("λ = xᵀ·A·x / xᵀ·x (cociente de Rayleigh, método de la potencia)") }
        };
        if (!convergio)
            dict[new LatinoString("advertencia")] =
                new LatinoString($"No convergió en {maxIter} iteraciones. " +
                                 "Puede haber dos valores propios de igual módulo.");
        return new LatinoDictionary(dict);
    }

    // 7. Valores propios por iteración QR (matrices pequeñas) ────────────────

    [LatinoExtension(
        "algebraLineal.valoresPropios",
        Args = "matriz_A, [max_iter], [tolerancia]",
        Help  = "Calcula todos los valores propios de una matriz cuadrada real\n" +
                "usando el Algoritmo QR iterativo (desplazamiento de Wilkinson).\n" +
                "Recomendado para matrices de hasta 20×20.\n" +
                "max_iter: iteraciones por valor propio (defecto: 1 000).\n" +
                "tolerancia: criterio de deflación (defecto: 1e-10).\n\n" +
                "Fórmula: A = Q·R → A' = R·Q (cada iteración acerca A a forma triangular superior).\n" +
                "Los valores propios son los elementos de la diagonal al converger.")]
    public static LatinoObject ValoresPropios(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.valoresPropios requiere: matriz_A. Opcionalmente: max_iter, tolerancia.");

        var A  = MatrizADouble(lA, "matriz_A");
        int n  = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.valoresPropios requiere una matriz cuadrada.");
        if (n > 20)
            throw new RuntimeError(null!, "algebra.valores_propios soporta matrices de hasta 20×20. " +
                                          "Para matrices mayores usa algebra.valor_propio_dominante.");

        int    maxIter = args.Count > 1 ? (int)GetDouble(args[1]) : 1_000;
        double tol     = args.Count > 2 ? GetDouble(args[2]) : 1e-10;

        // Trabajar sobre una copia
        var H = (double[,])A.Clone();

        // Reducción a forma de Hessenberg superior (acelera convergencia QR)
        HessenbergInPlace(H, n);

        // Iteración QR con desplazamiento de Wilkinson
        var lambdas = new double[n];
        int size     = n;

        while (size > 1)
        {
            int iter = 0;
            while (iter < maxIter)
            {
                // Desplazamiento de Wilkinson
                double mu = DesplazamientoWilkinson(H, size);

                // Desplazar: H = H - μI
                for (int i = 0; i < size; i++) H[i, i] -= mu;

                // Descomposición QR vía rotaciones de Givens
                var (Q, R) = QRGivens(H, size);

                // Actualizar H = R·Q + μI
                var RQ = MatMulSubmatriz(R, Q, size);
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        H[i, j] = RQ[i, j];
                for (int i = 0; i < size; i++) H[i, i] += mu;

                iter++;

                // Criterio de deflación: subdiagonal casi cero
                if (Math.Abs(H[size - 1, size - 2]) < tol * (Math.Abs(H[size - 2, size - 2]) + Math.Abs(H[size - 1, size - 1])))
                    break;
            }

            lambdas[size - 1] = H[size - 1, size - 1];
            size--;
        }
        lambdas[0] = H[0, 0];

        // Ordenar de mayor a menor
        Array.Sort(lambdas, (x, y) => Math.Abs(y).CompareTo(Math.Abs(x)));

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("valores_propios"), VectorALista(lambdas) },
            { new LatinoString("n"),               new LatinoNumber(n) },
            { new LatinoString("formula"),
              new LatinoString("Algoritmo QR iterativo con desplazamiento de Wilkinson") },
            { new LatinoString("nota"),
              new LatinoString("Valores propios ordenados por módulo decreciente. " +
                               "Solo valores propios reales (matrices no simétricas pueden tener complejos).") }
        };
        return new LatinoDictionary(dict);
    }

    // 8. Norma de vector y matriz ────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.norma",
        Args = "lista, [tipo]",
        Help  = "Calcula la norma de un vector o matriz.\n" +
                "lista: lista de números (vector) o lista de listas (matriz).\n" +
                "tipo para vector: '1' (suma abs), '2' (euclidiana, defecto), 'inf' (máx abs).\n" +
                "tipo para matriz: '1' (col máx), 'frobenius' (defecto), 'inf' (fila máx).\n\n" +
                "Fórmulas vector: ‖x‖₁ = Σ|xᵢ| | ‖x‖₂ = √(Σxᵢ²) | ‖x‖∞ = max|xᵢ|\n" +
                "Fórmulas matriz: ‖A‖_F = √(ΣΣ aᵢⱼ²) | ‖A‖₁ = max_j Σᵢ|aᵢⱼ| | ‖A‖∞ = max_i Σⱼ|aᵢⱼ|")]
    public static LatinoObject Norma(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!, "algebra.norma requiere: lista (vector o matriz). Opcionalmente: tipo.");

        string tipo = args.Count > 1 ? args[1].ToString().ToLower().Trim() : "2";

        bool esMatriz = lista.Elements.Count > 0 && lista.Elements[0] is LatinoList;

        double norma;
        string formula;

        if (esMatriz)
        {
            var M    = MatrizADouble(lista, "matriz");
            int filas = M.GetLength(0), cols = M.GetLength(1);

            switch (tipo)
            {
                case "frobenius": case "f": case "2":
                    double s = 0;
                    for (int i = 0; i < filas; i++)
                        for (int j = 0; j < cols; j++) s += M[i, j] * M[i, j];
                    norma   = Math.Sqrt(s);
                    formula = "‖A‖_F = √(ΣΣ aᵢⱼ²)";
                    tipo    = "frobenius";
                    break;
                case "1":
                    double maxCol = 0;
                    for (int j = 0; j < cols; j++)
                    {
                        double col = 0;
                        for (int i = 0; i < filas; i++) col += Math.Abs(M[i, j]);
                        if (col > maxCol) maxCol = col;
                    }
                    norma   = maxCol;
                    formula = "‖A‖₁ = max_j Σᵢ|aᵢⱼ|";
                    break;
                case "inf": case "∞":
                    double maxFila = 0;
                    for (int i = 0; i < filas; i++)
                    {
                        double fila = 0;
                        for (int j = 0; j < cols; j++) fila += Math.Abs(M[i, j]);
                        if (fila > maxFila) maxFila = fila;
                    }
                    norma   = maxFila;
                    formula = "‖A‖∞ = max_i Σⱼ|aᵢⱼ|";
                    break;
                default:
                    throw new RuntimeError(null!, "tipo de norma matricial válido: '1', 'frobenius', 'inf'.");
            }
        }
        else
        {
            var v = VectorADouble(lista, "vector");

            switch (tipo)
            {
                case "1":
                    double s1 = 0; foreach (var vi in v) s1 += Math.Abs(vi);
                    norma   = s1;
                    formula = "‖x‖₁ = Σ|xᵢ|";
                    break;
                case "2": case "euclidiana":
                    double s2 = 0; foreach (var vi in v) s2 += vi * vi;
                    norma   = Math.Sqrt(s2);
                    formula = "‖x‖₂ = √(Σxᵢ²)";
                    break;
                case "inf": case "∞":
                    double mx = 0; foreach (var vi in v) if (Math.Abs(vi) > mx) mx = Math.Abs(vi);
                    norma   = mx;
                    formula = "‖x‖∞ = max|xᵢ|";
                    break;
                default:
                    throw new RuntimeError(null!, "tipo de norma vectorial válido: '1', '2', 'inf'.");
            }
        }

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), new LatinoNumber(norma) },
            { new LatinoString("tipo"),      new LatinoString(tipo) },
            { new LatinoString("formula"),   new LatinoString(formula) }
        };
        return new LatinoDictionary(dict);
    }

    // 9. Multiplicación de matrices ──────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.multiplicar",
        Args = "matriz_A, matriz_B",
        Help  = "Multiplica dos matrices A (m×k) y B (k×n).\n" +
                "El número de columnas de A debe coincidir con el número de filas de B.\n\n" +
                "Fórmula: C[i,j] = Σₖ A[i,k]·B[k,j]   O(m·k·n)")]
    public static LatinoObject Multiplicar(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lA || args[1] is not LatinoList lB)
            throw new RuntimeError(null!, "algebra.multiplicar requiere: matriz_A, matriz_B.");

        var A = MatrizADouble(lA, "matriz_A");
        var B = MatrizADouble(lB, "matriz_B");

        int mA = A.GetLength(0), kA = A.GetLength(1);
        int mB = B.GetLength(0), nB = B.GetLength(1);

        if (kA != mB)
            throw new RuntimeError(null!,
                $"Dimensiones incompatibles: A es {mA}×{kA} y B es {mB}×{nB}. " +
                $"Se necesita cols(A) = filas(B) = {kA} = {mB}.");

        var C = MatMul(A, B);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), MatrizALista(C) },
            { new LatinoString("filas"),     new LatinoNumber(mA) },
            { new LatinoString("columnas"),  new LatinoNumber(nB) },
            { new LatinoString("formula"),   new LatinoString("C[i,j] = Σₖ A[i,k]·B[k,j]") }
        };
        return new LatinoDictionary(dict);
    }

    // 10. Transpuesta ────────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.transponer",
        Args = "matriz_A",
        Help  = "Calcula la transpuesta Aᵀ de una matriz.\n" +
                "Fórmula: Aᵀ[i,j] = A[j,i]")]
    public static LatinoObject TransponerMatriz(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.transponer requiere: matriz_A.");

        var A  = MatrizADouble(lA, "matriz_A");
        var At = Transponer(A);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), MatrizALista(At) },
            { new LatinoString("filas"),     new LatinoNumber(At.GetLength(0)) },
            { new LatinoString("columnas"),  new LatinoNumber(At.GetLength(1)) },
            { new LatinoString("formula"),   new LatinoString("Aᵀ[i,j] = A[j,i]") }
        };
        return new LatinoDictionary(dict);
    }

    // 11. Traza ───────────────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.traza",
        Args = "matriz_A",
        Help  = "Calcula la traza (suma de la diagonal) de una matriz cuadrada.\n" +
                "Fórmula: tr(A) = Σ Aᵢᵢ\n\n" +
                "Propiedad útil: tr(A) = Σ λᵢ (suma de valores propios).")]
    public static LatinoObject Traza(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lA)
            throw new RuntimeError(null!, "algebra.traza requiere: matriz_A (lista de listas cuadrada).");

        var A = MatrizADouble(lA, "matriz_A");
        int n = A.GetLength(0);
        if (A.GetLength(1) != n)
            throw new RuntimeError(null!, "algebra.traza requiere una matriz cuadrada.");

        double traza = 0;
        for (int i = 0; i < n; i++) traza += A[i, i];

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), new LatinoNumber(traza) },
            { new LatinoString("n"),         new LatinoNumber(n) },
            { new LatinoString("formula"),   new LatinoString("tr(A) = Σ Aᵢᵢ") }
        };
        return new LatinoDictionary(dict);
    }

    // 12. Producto punto y cruz ───────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.punto",
        Args = "vector_a, vector_b",
        Help  = "Calcula el producto punto (escalar) de dos vectores de igual longitud.\n" +
                "Fórmula: a·b = Σ aᵢ·bᵢ\n\n" +
                "Si los vectores son unitarios: a·b = cos(θ) donde θ es el ángulo entre ellos.")]
    public static LatinoObject ProductoPunto(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList la || args[1] is not LatinoList lb)
            throw new RuntimeError(null!, "algebra.punto requiere: vector_a, vector_b.");

        var a = VectorADouble(la, "vector_a");
        var b = VectorADouble(lb, "vector_b");
        if (a.Length != b.Length)
            throw new RuntimeError(null!, $"Los vectores deben tener la misma longitud ({a.Length} ≠ {b.Length}).");

        double prod = 0;
        for (int i = 0; i < a.Length; i++) prod += a[i] * b[i];

        // Ángulo entre vectores
        double normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++) { normA += a[i] * a[i]; normB += b[i] * b[i]; }
        normA = Math.Sqrt(normA); normB = Math.Sqrt(normB);
        double angulo = (normA > 0 && normB > 0)
            ? Math.Acos(Math.Max(-1.0, Math.Min(1.0, prod / (normA * normB))))
            : 0.0;

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"),    new LatinoNumber(prod) },
            { new LatinoString("angulo_rad"),   new LatinoNumber(angulo) },
            { new LatinoString("angulo_grados"),new LatinoNumber(angulo * 180.0 / Math.PI) },
            { new LatinoString("formula"),      new LatinoString("a·b = Σ aᵢ·bᵢ") }
        };
        return new LatinoDictionary(dict);
    }

    [LatinoExtension(
        "algebraLineal.cruz",
        Args = "vector_a, vector_b",
        Help  = "Calcula el producto cruz (vectorial) de dos vectores en ℝ³.\n" +
                "Ambos vectores deben tener exactamente 3 componentes.\n\n" +
                "Fórmula: a×b = (a₂b₃-a₃b₂, a₃b₁-a₁b₃, a₁b₂-a₂b₁)\n" +
                "El resultado es perpendicular a ambos vectores. ‖a×b‖ = ‖a‖·‖b‖·sin(θ).")]
    public static LatinoObject ProductoCruz(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList la || args[1] is not LatinoList lb)
            throw new RuntimeError(null!, "algebra.cruz requiere: vector_a, vector_b (ambos de longitud 3).");

        var a = VectorADouble(la, "vector_a");
        var b = VectorADouble(lb, "vector_b");
        if (a.Length != 3 || b.Length != 3)
            throw new RuntimeError(null!, "algebra.cruz solo opera en vectores de ℝ³ (longitud 3).");

        var c = new double[]
        {
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0]
        };

        double norma = Math.Sqrt(c[0]*c[0] + c[1]*c[1] + c[2]*c[2]);

        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), VectorALista(c) },
            { new LatinoString("norma"),     new LatinoNumber(norma) },
            { new LatinoString("formula"),   new LatinoString("a×b = (a₂b₃-a₃b₂, a₃b₁-a₁b₃, a₁b₂-a₂b₁)") }
        };
        return new LatinoDictionary(dict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ARITMÉTICA VECTORIZADA — Operaciones con Escalar
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.sumarEscalar",
        Args = "coleccion, n",
        Help  = "Suma un escalar n a cada elemento de un vector o matriz " +
                "(soporta tensores n-dimensionales).\n" +
                "Fórmula: resultado[i] = coleccion[i] + n")]
    public static LatinoObject SumarEscalar(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.sumarEscalar requiere: coleccion (lista), n (número).");
        return AplicarEscalar(lista, GetDouble(args[1]), (x, c) => x + c);
    }

    [LatinoExtension(
        "algebraLineal.restarEscalar",
        Args = "coleccion, n",
        Help  = "Resta un escalar n a cada elemento de un vector o matriz.\n" +
                "Fórmula: resultado[i] = coleccion[i] - n")]
    public static LatinoObject RestarEscalar(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.restarEscalar requiere: coleccion (lista), n (número).");
        return AplicarEscalar(lista, GetDouble(args[1]), (x, c) => x - c);
    }

    [LatinoExtension(
        "algebraLineal.multiplicarEscalar",
        Args = "coleccion, n",
        Help  = "Multiplica cada elemento de un vector o matriz por un escalar n.\n" +
                "Equivalente al producto escalar-vector de álgebra lineal.\n" +
                "Fórmula: resultado[i] = coleccion[i] × n")]
    public static LatinoObject MultiplicarEscalar(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.multiplicarEscalar requiere: coleccion (lista), n (número).");
        return AplicarEscalar(lista, GetDouble(args[1]), (x, c) => x * c);
    }

    [LatinoExtension(
        "algebraLineal.dividirEscalar",
        Args = "coleccion, n",
        Help  = "Divide cada elemento de un vector o matriz por un escalar n.\n" +
                "Lanza error si n es cero.\n" +
                "Fórmula: resultado[i] = coleccion[i] / n")]
    public static LatinoObject DividirEscalar(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.dividirEscalar requiere: coleccion (lista), n (número).");
        double divisor = GetDouble(args[1]);
        if (Math.Abs(divisor) < double.Epsilon)
            throw new RuntimeError(null!, "División por cero: el escalar n no puede ser 0.");
        return AplicarEscalar(lista, divisor, (x, c) => x / c);
    }

    [LatinoExtension(
        "algebraLineal.potenciaEscalar",
        Args = "coleccion, exp",
        Help  = "Eleva cada elemento del vector o matriz al exponente dado.\n" +
                "Fórmula: resultado[i] = coleccion[i] ^ exp")]
    public static LatinoObject PotenciaEscalar(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.potenciaEscalar requiere: coleccion (lista), exp (número).");
        return AplicarEscalar(lista, GetDouble(args[1]), Math.Pow);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ARITMÉTICA VECTORIZADA — Operaciones Término a Término
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.sumar",
        Args = "colA, colB",
        Help  = "Suma dos vectores o matrices elemento a elemento.\n" +
                "Ambas colecciones deben tener la misma forma (shape).\n" +
                "Fórmula: C[i] = A[i] + B[i]")]
    public static LatinoObject SumarTerminos(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList a || args[1] is not LatinoList b)
            throw new RuntimeError(null!,
                "algebraLineal.sumar requiere: colA (lista), colB (lista).");
        return AplicarTerminoATermino(a, b, (x, y) => x + y);
    }

    [LatinoExtension(
        "algebraLineal.restar",
        Args = "colA, colB",
        Help  = "Resta dos vectores o matrices elemento a elemento.\n" +
                "Ambas colecciones deben tener la misma forma (shape).\n" +
                "Fórmula: C[i] = A[i] - B[i]")]
    public static LatinoObject RestarTerminos(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList a || args[1] is not LatinoList b)
            throw new RuntimeError(null!,
                "algebraLineal.restar requiere: colA (lista), colB (lista).");
        return AplicarTerminoATermino(a, b, (x, y) => x - y);
    }

    [LatinoExtension(
        "algebraLineal.multiplicarTerminos",
        Args = "colA, colB",
        Help  = "Producto de Hadamard: multiplica dos vectores o matrices elemento " +
                "a elemento (NO es multiplicación matricial).\n" +
                "Usa algebraLineal.multiplicar para producto matricial estándar.\n" +
                "Fórmula: C[i] = A[i] × B[i]")]
    public static LatinoObject MultiplicarTerminos(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList a || args[1] is not LatinoList b)
            throw new RuntimeError(null!,
                "algebraLineal.multiplicarTerminos requiere: colA (lista), colB (lista).");
        return AplicarTerminoATermino(a, b, (x, y) => x * y);
    }

    [LatinoExtension(
        "algebraLineal.dividirTerminos",
        Args = "colA, colB",
        Help  = "Divide dos vectores o matrices elemento a elemento.\n" +
                "Lanza error si algún elemento de colB es cero.\n" +
                "Fórmula: C[i] = A[i] / B[i]")]
    public static LatinoObject DividirTerminos(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList a || args[1] is not LatinoList b)
            throw new RuntimeError(null!,
                "algebraLineal.dividirTerminos requiere: colA (lista), colB (lista).");
        return AplicarTerminoATermino(a, b, (x, y) =>
        {
            if (Math.Abs(y) < double.Epsilon)
                throw new RuntimeError(null!,
                    "División por cero: uno de los elementos de colB es 0.");
            return x / y;
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FUNCIONES MATEMÁTICAS UNIVERSALES (ufuncs)
    // ─────────────────────────────────────────────────────────────────────────

    [LatinoExtension(
        "algebraLineal.abs",
        Args = "coleccion",
        Help  = "Aplica valor absoluto a cada elemento del vector o matriz.\n" +
                "Fórmula: resultado[i] = |coleccion[i]|")]
    public static LatinoObject AbsVectorial(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.abs requiere: coleccion (lista).");
        return AplicarUniversal(lista, Math.Abs);
    }

    [LatinoExtension(
        "algebraLineal.raiz",
        Args = "coleccion",
        Help  = "Aplica raíz cuadrada a cada elemento del vector o matriz.\n" +
                "Lanza error si algún elemento es negativo.\n" +
                "Fórmula: resultado[i] = √coleccion[i]")]
    public static LatinoObject RaizVectorial(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.raiz requiere: coleccion (lista).");
        return AplicarUniversal(lista, x =>
        {
            if (x < 0)
                throw new RuntimeError(null!,
                    $"Raíz cuadrada de número negativo ({x}). " +
                    "Todos los elementos deben ser ≥ 0.");
            return Math.Sqrt(x);
        });
    }

    [LatinoExtension(
        "algebraLineal.redondear",
        Args = "coleccion, [decimales]",
        Help  = "Redondea cada elemento del vector o matriz al número de decimales " +
                "indicado (defecto: 0 decimales).\n" +
                "Fórmula: resultado[i] = redondear(coleccion[i], decimales)")]
    public static LatinoObject RedondearVectorial(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.redondear requiere: coleccion (lista). " +
                "Opcionalmente: decimales (entero ≥ 0).");
        int dec = args.Count > 1 ? (int)GetDouble(args[1]) : 0;
        if (dec < 0)
            throw new RuntimeError(null!, "decimales debe ser un entero ≥ 0.");
        return AplicarUniversal(lista,
            x => Math.Round(x, dec, MidpointRounding.AwayFromZero));
    }

    [LatinoExtension(
        "algebraLineal.negativo",
        Args = "coleccion",
        Help  = "Invierte el signo de cada elemento del vector o matriz.\n" +
                "Equivalente a multiplicarEscalar(col, -1) pero más directo.\n" +
                "Fórmula: resultado[i] = -coleccion[i]")]
    public static LatinoObject NegativoVectorial(List<LatinoObject> args)
    {
        if (args.Count < 1 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.negativo requiere: coleccion (lista)..");
        return AplicarUniversal(lista, x => -x);
    }

    [LatinoExtension(
        "algebraLineal.clip",
        Args = "coleccion, minimo, maximo",
        Help  = "Limita (clamp) cada elemento del vector o matriz al rango " +
                "[minimo, maximo].\n" +
                "Útil para normalización y preprocesamiento de datos.\n" +
                "Fórmula: resultado[i] = max(minimo, min(maximo, coleccion[i]))")]
    public static LatinoObject ClipVectorial(List<LatinoObject> args)
    {
        if (args.Count < 3 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!,
                "algebraLineal.clip requiere: coleccion (lista), minimo (número), " +
                "maximo (número).");
        double minVal = GetDouble(args[1]);
        double maxVal = GetDouble(args[2]);
        if (minVal > maxVal)
            throw new RuntimeError(null!,
                $"minimo ({minVal}) no puede ser mayor que maximo ({maxVal}).");
        return AplicarUniversal(lista,
            x => Math.Max(minVal, Math.Min(maxVal, x)));
    }

    [LatinoExtension(
        "algebraLineal.iguales",
        Args = "colA, colB, [tolerancia]",
        Help  = "Compara dos tensores elemento a elemento dentro de una tolerancia.\n" +
                "Retorna verdadero solo si todos los elementos son iguales (o " +
                "aproximadamente iguales dentro de la tolerancia dada).\n" +
                "tolerancia defecto: 1e-9. Útil para verificar resultados numéricos.\n" +
                "Fórmula: ∀i: |A[i] - B[i]| ≤ tolerancia")]
    public static LatinoObject IgualesVectorial(List<LatinoObject> args)
    {
        if (args.Count < 2 || args[0] is not LatinoList a || args[1] is not LatinoList b)
            throw new RuntimeError(null!,
                "algebraLineal.iguales requiere: colA (lista), colB (lista). " +
                "Opcionalmente: tolerancia (número > 0).");
        double tol = args.Count > 2 ? GetDouble(args[2]) : 1e-9;
        if (tol < 0)
            throw new RuntimeError(null!, "La tolerancia no puede ser negativa.");
        return new LatinoBoolean(SonIgualesRecursivo(a, b, tol));
    }

    /// <summary>Compara recursivamente dos tensores con tolerancia numérica.</summary>
    private static bool SonIgualesRecursivo(LatinoList a, LatinoList b, double tol)
    {
        if (a.Elements.Count != b.Elements.Count) return false;
        for (int i = 0; i < a.Elements.Count; i++)
        {
            var itemA = a.Elements[i];
            var itemB = b.Elements[i];
            if (itemA is LatinoList subA && itemB is LatinoList subB)
            {
                if (!SonIgualesRecursivo(subA, subB, tol)) return false;
            }
            else if (itemA is LatinoList || itemB is LatinoList)
            {
                return false; // Formas incompatibles
            }
            else
            {
                if (Math.Abs(GetDouble(itemA) - GetDouble(itemB)) > tol) return false;
            }
        }
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers privados para QR / Hessenberg
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Reducción in-place a forma de Hessenberg superior.</summary>
    private static void HessenbergInPlace(double[,] A, int n)
    {
        for (int k = 0; k < n - 2; k++)
        {
            double sigma = 0;
            for (int i = k + 1; i < n; i++) sigma += A[i, k] * A[i, k];
            sigma = Math.Sqrt(sigma);
            if (sigma < 1e-14) continue;

            double beta = A[k + 1, k] > 0 ? -(A[k + 1, k] + sigma) : (sigma - A[k + 1, k]);
            var v = new double[n];
            v[k + 1] = 1.0;
            for (int i = k + 2; i < n; i++) v[i] = A[i, k] / beta;
            double vv = 0;
            for (int i = k + 1; i < n; i++) vv += v[i] * v[i];
            if (vv < 1e-14) continue;

            // A = (I - 2vvᵀ/‖v‖²)·A·(I - 2vvᵀ/‖v‖²)
            // Aplicar desde la izquierda
            for (int j = k; j < n; j++)
            {
                double dot = 0;
                for (int i = k + 1; i < n; i++) dot += v[i] * A[i, j];
                dot *= 2.0 / vv;
                for (int i = k + 1; i < n; i++) A[i, j] -= dot * v[i];
            }
            // Aplicar desde la derecha
            for (int i = 0; i < n; i++)
            {
                double dot = 0;
                for (int j = k + 1; j < n; j++) dot += A[i, j] * v[j];
                dot *= 2.0 / vv;
                for (int j = k + 1; j < n; j++) A[i, j] -= dot * v[j];
            }
        }
    }

    /// <summary>Descomposición QR de una submatriz size×size via rotaciones de Givens.</summary>
    private static (double[,] Q, double[,] R) QRGivens(double[,] H, int size)
    {
        var R = new double[size, size];
        var Q = new double[size, size];
        for (int i = 0; i < size; i++) { Q[i, i] = 1.0; for (int j = 0; j < size; j++) R[i, j] = H[i, j]; }

        for (int j = 0; j < size - 1; j++)
        {
            for (int i = j + 1; i < size; i++)
            {
                if (Math.Abs(R[i, j]) < 1e-15) continue;
                double r = Math.Sqrt(R[j, j] * R[j, j] + R[i, j] * R[i, j]);
                double c = R[j, j] / r;
                double s = -R[i, j] / r;

                // Aplicar rotación de Givens a R
                for (int k = j; k < size; k++)
                {
                    double tmp  = c * R[j, k] - s * R[i, k];
                    R[i, k]     = s * R[j, k] + c * R[i, k];
                    R[j, k]     = tmp;
                }
                // Acumular en Q
                for (int k = 0; k < size; k++)
                {
                    double tmp  = c * Q[j, k] - s * Q[i, k];
                    Q[i, k]     = s * Q[j, k] + c * Q[i, k];
                    Q[j, k]     = tmp;
                }
            }
        }
        return (Transponer2(Q, size), R);
    }

    /// <summary>Transpuesta de una submatriz size×size dentro de un array mayor.</summary>
    private static double[,] Transponer2(double[,] A, int size)
    {
        var T = new double[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                T[i, j] = A[j, i];
        return T;
    }

    /// <summary>Producto de dos submatrices size×size.</summary>
    private static double[,] MatMulSubmatriz(double[,] A, double[,] B, int size)
    {
        var C = new double[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                for (int k = 0; k < size; k++)
                    C[i, j] += A[i, k] * B[k, j];
        return C;
    }

    /// <summary>Desplazamiento de Wilkinson para iteración QR.</summary>
    private static double DesplazamientoWilkinson(double[,] H, int size)
    {
        if (size < 2) return H[0, 0];
        double a = H[size - 2, size - 2];
        double b = H[size - 1, size - 1];
        double c = H[size - 1, size - 2];
        double delta = (a - b) / 2.0;
        double sign  = delta >= 0 ? 1.0 : -1.0;
        return b - sign * c * c / (Math.Abs(delta) + Math.Sqrt(delta * delta + c * c));
    }
}
