using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class OpticaFormulas
{
    [LatinoExtension("optica.leySnell",
            Args = "n1, angulo1_grados, n2",
            Help = "Ley de Snell-Descartes (refracción). Calcula el ángulo de refracción.\n\nFórmula: n₁·sin(θ₁) = n₂·sin(θ₂)")]
        public static LatinoObject LeySnell(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere n1 (índice medio 1), angulo1_grados y n2 (índice medio 2).");
            double n1 = ((LatinoNumber)args[0]).Value;
            double a1 = ((LatinoNumber)args[1]).Value;
            double n2 = ((LatinoNumber)args[2]).Value;
            if (n1 <= 0 || n2 <= 0) throw new RuntimeError(null!, "Los índices de refracción deben ser positivos.");
            if (a1 < 0 || a1 > 90) throw new RuntimeError(null!, "El ángulo de incidencia debe estar entre 0° y 90°.");

            double rad1   = a1 * Constantes.PI / 180.0;
            double sinA2  = (n1 / n2) * Math.Sin(rad1);

            if (Math.Abs(sinA2) > 1.0)
            {
                var extRefl = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                    { new LatinoString("fenomeno"), new LatinoString("Reflexion Total Interna") }
                });
                return Respuesta.Crear(double.NaN, "grados", "n₁·sin(θ₁) = n₂·sin(θ₂)", extRefl);
            }

            double a2Rad  = Math.Asin(sinA2);
            double a2Grad = a2Rad * 180.0 / Constantes.PI;

            string fenomeno = n2 > n1 ? "Refraccion hacia la normal" : "Refraccion alejandose de la normal";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("fenomeno"), new LatinoString(fenomeno) }
            });
            return Respuesta.Crear(a2Grad, "grados", "n₁·sin(θ₁) = n₂·sin(θ₂)", extras);
        }

    [LatinoExtension("optica.lenteDelgada",
            Args = "diccionario_variables",
            Help = "Ecuación de lente delgada. Pasa 2 de: 'f', 'do', 'di' (distancias en metros).\n\nFórmula: 1/f = 1/do + 1/di")]
        public static LatinoObject LenteDelgada(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con 'f' (focal), 'do' (objeto), 'di' (imagen).");

            bool tieneF  = Respuesta.TryGetVal(datos, "f",  out double f);
            bool tieneDo = Respuesta.TryGetVal(datos, "do", out double dobj);
            bool tieneDi = Respuesta.TryGetVal(datos, "di", out double dimg);

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));

            if (tieneDo && tieneDi)
            {
                if (dobj == 0 || dimg == 0) throw new RuntimeError(null!, "Las distancias no pueden ser cero.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("f");
                double fVal = 1.0 / (1.0 / dobj + 1.0 / dimg);
                extras.Pairs[new LatinoString("aumento")]   = new LatinoNumber(-dimg / dobj);
                return Respuesta.Crear(fVal, "m", "1/f = 1/do + 1/di", extras);
            }
            if (tieneF && tieneDo)
            {
                if (f == 0 || dobj == 0) throw new RuntimeError(null!, "f y do no pueden ser cero.");
                double diVal = 1.0 / (1.0 / f - 1.0 / dobj);
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("di");
                extras.Pairs[new LatinoString("aumento")]   = new LatinoNumber(-diVal / dobj);
                string tipo = diVal > 0 ? "Real e invertida" : "Virtual y derecha";
                extras.Pairs[new LatinoString("tipo_imagen")] = new LatinoString(tipo);
                return Respuesta.Crear(diVal, "m", "di = f·do / (do - f)", extras);
            }
            if (tieneF && tieneDi)
            {
                if (f == 0 || dimg == 0) throw new RuntimeError(null!, "f y di no pueden ser cero.");
                double doVal = 1.0 / (1.0 / f - 1.0 / dimg);
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("do");
                extras.Pairs[new LatinoString("aumento")]   = new LatinoNumber(-dimg / doVal);
                return Respuesta.Crear(doVal, "m", "do = f·di / (di - f)", extras);
            }

            throw new RuntimeError(null!, "Proporciona exactamente 2 de: f, do, di.");
        }

    [LatinoExtension("optica.espejoEsferico",
            Args = "radio_curvatura, distancia_objeto",
            Help = "Posición de la imagen en un espejo esférico. Radio positivo = cóncavo, negativo = convexo.\n\nFórmulas: f = R/2 | 1/f = 1/do + 1/di")]
        public static LatinoObject EspejoEsferico(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere radio de curvatura (m) y distancia al objeto (m).");
            double R  = ((LatinoNumber)args[0]).Value;
            double dObj = ((LatinoNumber)args[1]).Value;
            if (dObj <= 0) throw new RuntimeError(null!, "La distancia al objeto debe ser positiva.");
            if (R == 0) throw new RuntimeError(null!, "El radio de curvatura no puede ser cero.");

            double f    = R / 2.0;
            double dImg = 1.0 / (1.0 / f - 1.0 / dObj);
            double m    = -dImg / dObj;
            string tipo = R > 0 ? "Concavo" : "Convexo";
            string imgTipo = dImg > 0 ? "Real e invertida" : "Virtual y derecha";

            var valores = new Dictionary<string, double>
            {
                { "focal", f },
                { "distancia_imagen", dImg },
                { "aumento", m }
            };
            var result = Respuesta.Estructurada(valores, "f = R/2 | 1/f = 1/do + 1/di");
            result.Pairs[new LatinoString("resultado")] = new LatinoNumber(dImg);
            result.Pairs[new LatinoString("unidad")]    = new LatinoString("m");
            result.Pairs[new LatinoString("tipo_espejo")] = new LatinoString(tipo);
            result.Pairs[new LatinoString("tipo_imagen")]  = new LatinoString(imgTipo);
            return result;
        }

}
