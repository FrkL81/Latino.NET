using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class EstadisticaFormulas
{
    [LatinoExtension("estadistica.media",
            Args = "lista_valores",
            Help = "Media aritmética de una lista de valores.\n\nFórmula: x̄ = (Σxᵢ) / n")]
        public static LatinoObject Media(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoList lista)
                throw new RuntimeError(null!, "Requiere una lista de números.");
            if (lista.Elements.Count == 0)
                throw new RuntimeError(null!, "La lista no puede estar vacía.");

            double suma = 0;
            foreach (var e in lista.Elements)
            {
                if (e is not LatinoNumber n) throw new RuntimeError(null!, "Todos los elementos deben ser números.");
                suma += n.Value;
            }
            return Respuesta.Crear(suma / lista.Elements.Count, "unidades", "x̄ = (Σxᵢ) / n");
        }

    [LatinoExtension("estadistica.varianzaDesviacion",
            Args = "lista_valores, [es_muestra]",
            Help = "Varianza y desviación estándar. es_muestra=verdadero usa N-1 (Bessel).\n\nFórmula: σ² = Σ(xᵢ - x̄)² / N")]
        public static LatinoObject VarianzaDesviacion(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoList lista)
                throw new RuntimeError(null!, "Requiere una lista de números.");
            if (lista.Elements.Count < 2)
                throw new RuntimeError(null!, "Se necesitan al menos 2 elementos para calcular la varianza.");

            bool esMuestra = args.Count > 1 && args[1] is LatinoBoolean b && b.Value;

            double suma = 0;
            var vals = new List<double>();
            foreach (var e in lista.Elements)
            {
                if (e is not LatinoNumber n) throw new RuntimeError(null!, "Todos los elementos deben ser números.");
                suma += n.Value;
                vals.Add(n.Value);
            }

            double media    = suma / vals.Count;
            double sumaCuad = 0;
            foreach (var v in vals) sumaCuad += (v - media) * (v - media);

            int divisor   = esMuestra ? vals.Count - 1 : vals.Count;
            double varian = sumaCuad / divisor;
            double desv   = Math.Sqrt(varian);

            var valores = new Dictionary<string, double>
            {
                { "media",     media },
                { "varianza",  varian },
                { "desviacion_estandar", desv },
                { "n", vals.Count }
            };
            string formula = esMuestra ? "s² = Σ(xᵢ - x̄)² / (N-1)" : "σ² = Σ(xᵢ - x̄)² / N";
            return Respuesta.Estructurada(valores, formula);
        }

    [LatinoExtension("estadistica.combinaciones",
            Args = "n, r",
            Help = "Combinaciones sin repetición (coeficiente binomial).\n\nFórmula: C(n,r) = n! / (r!·(n-r)!)")]
        public static LatinoObject Combinaciones(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere n (total) y r (elegidos).");
            int n = (int)((LatinoNumber)args[0]).Value;
            int r = (int)((LatinoNumber)args[1]).Value;
            if (n < 0 || r < 0) throw new RuntimeError(null!, "n y r deben ser no negativos.");
            if (r > n) throw new RuntimeError(null!, "r no puede ser mayor que n.");
            return Respuesta.Crear(Combinatoria(n, r), "combinaciones", "C(n,r) = n! / (r!·(n-r)!)");
        }

    [LatinoExtension("estadistica.permutaciones",
            Args = "n, r",
            Help = "Permutaciones sin repetición.\n\nFórmula: P(n,r) = n! / (n-r)!")]
        public static LatinoObject Permutaciones(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere n (total) y r (elegidos).");
            int n = (int)((LatinoNumber)args[0]).Value;
            int r = (int)((LatinoNumber)args[1]).Value;
            if (n < 0 || r < 0) throw new RuntimeError(null!, "n y r deben ser no negativos.");
            if (r > n) throw new RuntimeError(null!, "r no puede ser mayor que n.");
            return Respuesta.Crear(Factorial(n) / Factorial(n - r), "permutaciones", "P(n,r) = n! / (n-r)!");
        }

    [LatinoExtension("estadistica.puntajeZ",
            Args = "valor, media, desviacion_estandar",
            Help = "Puntaje Z (estandarización): cuántas desviaciones estándar se aleja un valor de la media.\n\nFórmula: z = (x - μ) / σ")]
        public static LatinoObject PuntajeZ(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere valor, media y desviacion_estandar.");
            double x    = ((LatinoNumber)args[0]).Value;
            double mu   = ((LatinoNumber)args[1]).Value;
            double sigma = ((LatinoNumber)args[2]).Value;
            if (sigma <= 0) throw new RuntimeError(null!, "La desviación estándar debe ser mayor a cero.");
            double z = (x - mu) / sigma;

            string interpretacion;
            double absZ = Math.Abs(z);
            if      (absZ < 1.0) interpretacion = "Dentro de 1σ (~68% de probabilidad)";
            else if (absZ < 2.0) interpretacion = "Entre 1σ y 2σ (~95% acumulado)";
            else if (absZ < 3.0) interpretacion = "Entre 2σ y 3σ (~99.7% acumulado)";
            else                 interpretacion = "Mas alla de 3σ (valor atipico)";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("interpretacion"), new LatinoString(interpretacion) }
            });
            return Respuesta.Crear(z, "σ", "z = (x - μ) / σ", extras);
        }

    [LatinoExtension("estadistica.coeficienteVariacion",
            Args = "media, desviacion_estandar",
            Help = "Coeficiente de Variación: dispersión relativa como porcentaje de la media.\n\nFórmula: CV = (σ / μ) · 100")]
        public static LatinoObject CoeficienteVariacion(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere media y desviacion_estandar.");
            double mu    = ((LatinoNumber)args[0]).Value;
            double sigma = ((LatinoNumber)args[1]).Value;
            if (mu == 0) throw new RuntimeError(null!, "La media no puede ser cero para el CV.");
            if (sigma < 0) throw new RuntimeError(null!, "La desviación estándar no puede ser negativa.");
            double cv = (sigma / Math.Abs(mu)) * 100;

            string nivel = cv < 15 ? "Baja dispersion" : cv < 30 ? "Dispersion moderada" : "Alta dispersion";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("nivel"), new LatinoString(nivel) }
            });
            return Respuesta.Crear(cv, "%", "CV = (σ / μ) · 100", extras);
        }

    private static double Factorial(int n)
        {
            if (n < 0) throw new ArgumentException("El factorial no está definido para negativos.");
            double result = 1;
            for (int i = 2; i <= n; i++) result *= i;
            return result;
        }

    private static double Combinatoria(int n, int r)
        {
            // Usar algoritmo multiplicativo para evitar overflow con n grandes
            if (r == 0 || r == n) return 1;
            if (r > n - r) r = n - r; // Optimización: C(n,r) = C(n, n-r)
            double result = 1;
            for (int i = 0; i < r; i++)
            {
                result = result * (n - i) / (i + 1);
            }
            return Math.Round(result);
        }

}
