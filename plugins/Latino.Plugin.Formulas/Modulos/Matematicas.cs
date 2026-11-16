using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class MatematicasFormulas
{



    [LatinoExtension("matematicas.serieGeometrica",
            Args = "primerTermino, razon, numTerminos",
            Help = "Suma de una serie geométrica finita.\n\nFórmula: S = a·(1 - rⁿ) / (1 - r)")]
        public static LatinoObject SerieGeometrica(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere primer_termino (a), razon (r) y num_terminos (n).");
            double a = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            int    n = (int)((LatinoNumber)args[2]).Value;
            if (n < 1) throw new RuntimeError(null!, "El número de términos debe ser al menos 1.");

            double S;
            string formula;
            if (Math.Abs(r - 1.0) < 1e-12) {
                S = a * n;
                formula = "S = a·n (caso especial r = 1)";
            } else {
                S = a * (1 - Math.Pow(r, n)) / (1 - r);
                formula = "S = a·(1 - rⁿ) / (1 - r)";
            }

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
            if (Math.Abs(r) < 1)
                extras.Pairs[new LatinoString("suma_infinita")] = new LatinoNumber(a / (1 - r));

            return Respuesta.Crear(S, "unidades", formula, extras);
        }

    [LatinoExtension("matematicas.progresionAritmetica",
            Args = "primerTermino, diferenciaComun, numTerminos",
            Help = "Suma y término n-ésimo de una progresión aritmética.\n\nFórmulas: aₙ = a₁ + (n-1)·d | S = n·(a₁ + aₙ)/2")]
        public static LatinoObject ProgresionAritmetica(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere primer_termino (a1), diferencia_comun (d) y num_terminos (n).");
            double a1 = ((LatinoNumber)args[0]).Value;
            double d  = ((LatinoNumber)args[1]).Value;
            int    n  = (int)((LatinoNumber)args[2]).Value;
            if (n < 1) throw new RuntimeError(null!, "El número de términos debe ser al menos 1.");
            double an = a1 + (n - 1) * d;
            double S  = n * (a1 + an) / 2.0;
            var valores = new Dictionary<string, double>
            {
                { "suma",           S },
                { "termino_n",      an },
                { "media",          (a1 + an) / 2.0 }
            };
            return Respuesta.Estructurada(valores, "aₙ = a₁ + (n-1)·d | S = n·(a₁+aₙ)/2");
        }

    [LatinoExtension("matematicas.logaritmoBase",
            Args = "valor, base",
            Help = "Logaritmo de un número en cualquier base (cambio de base).\n\nFórmula: log_b(x) = ln(x) / ln(b)")]
        public static LatinoObject LogaritmoBase(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere valor (x > 0) y base (b > 0, b ≠ 1).");
            double x = ((LatinoNumber)args[0]).Value;
            double b = ((LatinoNumber)args[1]).Value;
            if (x <= 0) throw new RuntimeError(null!, "El valor debe ser mayor a cero.");
            if (b <= 0 || Math.Abs(b - 1) < 1e-12) throw new RuntimeError(null!, "La base debe ser positiva y distinta de 1.");
            return Respuesta.Crear(Math.Log(x) / Math.Log(b), "adimensional", "log_b(x) = ln(x) / ln(b)");
        }
}
