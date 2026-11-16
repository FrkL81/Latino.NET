using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class GeometriaFormulas
{
    [LatinoExtension("geometria.areaCirculo", Args = "radio", Help = "Calcula el área de un círculo.\n\nFórmula: A = π · r²")]
        public static LatinoObject AreaCirculo(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere el radio.");
            double r = ((LatinoNumber)args[0]).Value;
            if (r < 0) throw new RuntimeError(null!, "El radio no puede ser negativo.");
            return Respuesta.Crear(Constantes.PI * r * r, "m²", "A = π · r²");
        }

    [LatinoExtension("geometria.cilindroCompleto", Args = "radio, altura", Help = "Área y volumen de un cilindro.\n\nFórmulas: A_lat = 2πrh | A_tot = 2πr(h+r) | V = πr²h")]
        public static LatinoObject CilindroCompleto(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere radio y altura.");
            double r = ((LatinoNumber)args[0]).Value;
            double h = ((LatinoNumber)args[1]).Value;
            if (r < 0 || h < 0) throw new RuntimeError(null!, "Radio y altura deben ser no negativos.");

            double areaLateral = 2 * Constantes.PI * r * h;
            double areaBase = Constantes.PI * r * r;
            double areaTotal = areaLateral + 2 * areaBase;
            double volumen = Constantes.PI * r * r * h;

            var valores = new Dictionary<string, double>
            {
                { "area_lateral", areaLateral },
                { "area_total", areaTotal },
                { "volumen", volumen }
            };
            return Respuesta.Estructurada(valores, "A_lat = 2πrh | A_tot = 2πr(h+r) | V = πr²h");
        }

    [LatinoExtension("geometria.esfera",
            Args = "radio",
            Help = "Área superficial y volumen de una esfera.\n\nFórmulas: A = 4πr² | V = (4/3)πr³")]
        public static LatinoObject Esfera(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere el radio (m).");
            double r = ((LatinoNumber)args[0]).Value;
            if (r < 0) throw new RuntimeError(null!, "El radio no puede ser negativo.");
            var valores = new Dictionary<string, double>
            {
                { "area_superficial", 4 * Constantes.PI * r * r },
                { "volumen",          (4.0 / 3.0) * Constantes.PI * r * r * r }
            };
            return Respuesta.Estructurada(valores, "A = 4πr² | V = (4/3)πr³");
        }

    [LatinoExtension("geometria.cono",
            Args = "radio, altura",
            Help = "Área total y volumen de un cono recto.\n\nFórmulas: g = √(r²+h²) | A_tot = πr(r+g) | V = (1/3)πr²h")]
        public static LatinoObject Cono(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere radio (m) y altura (m).");
            double r = ((LatinoNumber)args[0]).Value;
            double h = ((LatinoNumber)args[1]).Value;
            if (r < 0 || h < 0) throw new RuntimeError(null!, "Radio y altura deben ser no negativos.");
            double g        = Math.Sqrt(r * r + h * h);          // generatriz
            double areaBase = Constantes.PI * r * r;
            double areaLat  = Constantes.PI * r * g;
            var valores = new Dictionary<string, double>
            {
                { "generatriz",    g },
                { "area_base",     areaBase },
                { "area_lateral",  areaLat },
                { "area_total",    areaBase + areaLat },
                { "volumen",       (1.0 / 3.0) * Constantes.PI * r * r * h }
            };
            return Respuesta.Estructurada(valores, "g = √(r²+h²) | A_tot = πr(r+g) | V = (1/3)πr²h");
        }

    [LatinoExtension("geometria.piramideRectangular",
            Args = "base_largo, base_ancho, altura",
            Help = "Área total y volumen de una pirámide de base rectangular.\n\nFórmula: V = (1/3)·A_base·h")]
        public static LatinoObject PiramideRectangular(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere base_largo (m), base_ancho (m) y altura (m).");
            double l = ((LatinoNumber)args[0]).Value;
            double a = ((LatinoNumber)args[1]).Value;
            double h = ((LatinoNumber)args[2]).Value;
            if (l < 0 || a < 0 || h < 0) throw new RuntimeError(null!, "Las dimensiones no pueden ser negativas.");
            double areaBase = l * a;
            // Apotemas de cada cara triangular
            double apotL = Math.Sqrt(h * h + (a / 2.0) * (a / 2.0));
            double apotA = Math.Sqrt(h * h + (l / 2.0) * (l / 2.0));
            double areaLat = l * apotL + a * apotA;
            var valores = new Dictionary<string, double>
            {
                { "area_base",    areaBase },
                { "area_lateral", areaLat },
                { "area_total",   areaBase + areaLat },
                { "volumen",      (1.0 / 3.0) * areaBase * h }
            };
            return Respuesta.Estructurada(valores, "V = (1/3)·l·a·h");
        }

    [LatinoExtension("geometria.elipse",
            Args = "semieje_mayor, semieje_menor",
            Help = "Área y perímetro aproximado de una elipse (aproximación de Ramanujan).\n\nFórmulas: A = π·a·b | P ≈ π·[3(a+b) - √((3a+b)(a+3b))]")]
        public static LatinoObject Elipse(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere semieje_mayor (a) y semieje_menor (b).");
            double a = ((LatinoNumber)args[0]).Value;
            double b = ((LatinoNumber)args[1]).Value;
            if (a < 0 || b < 0) throw new RuntimeError(null!, "Los semiejes no pueden ser negativos.");
            if (b > a) throw new RuntimeError(null!, "El semieje mayor (a) debe ser >= semieje menor (b).");
            // Aproximación de Ramanujan (error < 0.0001% para la mayoría de elipses)
            double P = Constantes.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
            double excentricidad = Math.Sqrt(1 - (b * b) / (a * a));
            var valores = new Dictionary<string, double>
            {
                { "area",         Constantes.PI * a * b },
                { "perimetro",    P },
                { "excentricidad", excentricidad }
            };
            return Respuesta.Estructurada(valores, "A = π·a·b | P ≈ π·[3(a+b) - √((3a+b)(a+3b))]");
        }

    [LatinoExtension("geometria.trapecio",
            Args = "base_mayor, base_menor, altura",
            Help = "Área de un trapecio.\n\nFórmula: A = ((B + b) / 2) · h")]
        public static LatinoObject Trapecio(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere base_mayor (m), base_menor (m) y altura (m).");
            double B = ((LatinoNumber)args[0]).Value;
            double b = ((LatinoNumber)args[1]).Value;
            double h = ((LatinoNumber)args[2]).Value;
            if (B < 0 || b < 0 || h < 0) throw new RuntimeError(null!, "Las dimensiones no pueden ser negativas.");
            return Respuesta.Crear((B + b) / 2.0 * h, "m²", "A = ((B + b) / 2) · h");
        }

    [LatinoExtension("geometria.poligonoRegular",
            Args = "num_lados, longitud_lado",
            Help = "Área y perímetro de un polígono regular.\n\nFórmula: A = (n·l²) / (4·tan(π/n))")]
        public static LatinoObject PoligonoRegular(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere num_lados (entero >= 3) y longitud_lado (m).");
            int    n = (int)((LatinoNumber)args[0]).Value;
            double l = ((LatinoNumber)args[1]).Value;
            if (n < 3) throw new RuntimeError(null!, "Un polígono requiere al menos 3 lados.");
            if (l < 0) throw new RuntimeError(null!, "La longitud del lado no puede ser negativa.");
            double area = (n * l * l) / (4.0 * Math.Tan(Constantes.PI / n));
            double apotema = l / (2.0 * Math.Tan(Constantes.PI / n));
            var valores = new Dictionary<string, double>
            {
                { "area",           area },
                { "perimetro",      n * l },
                { "apotema",        apotema },
                { "angulo_interior", (double)(n - 2) * 180.0 / n }
            };
            return Respuesta.Estructurada(valores, "A = (n·l²) / (4·tan(π/n))");
        }

    [LatinoExtension("geometria.distanciaPuntos",
            Args = "diccionario_puntos",
            Help = "Distancia euclidiana entre dos puntos en 2D o 3D.\nPasa: {'x1','y1','x2','y2'} o {'x1','y1','z1','x2','y2','z2'}")]
        public static LatinoObject DistanciaPuntos(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con coordenadas.");

            bool as_x1 = Respuesta.TryGetVal(datos, "x1", out double x1);
            bool as_y1 = Respuesta.TryGetVal(datos, "y1", out double y1);
            bool as_z1 = Respuesta.TryGetVal(datos, "z1", out double z1);
            bool as_x2 = Respuesta.TryGetVal(datos, "x2", out double x2);
            bool as_y2 = Respuesta.TryGetVal(datos, "y2", out double y2);
            bool as_z2 = Respuesta.TryGetVal(datos, "z2", out double z2);

            if (!as_x1 || !as_y1 || !as_x2 || !as_y2) throw new RuntimeError(null!, "Proporciona al menos x1, y1, x2, y2.");

            double dist = 0;
            string formula = "d = √((x₂-x₁)² + (y₂-y₁)²)";
            if (as_z1 && as_z2) {
                dist = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));
                formula = "d = √((x₂-x₁)² + (y₂-y₁)² + (z₂-z₁)²)";
            } else {
                dist = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            }
            return Respuesta.Crear(dist, "m", formula);
        }
}
