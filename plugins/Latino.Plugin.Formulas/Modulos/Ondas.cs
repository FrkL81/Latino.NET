using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class OndasFormulas
{
    [LatinoExtension("ondas.velocidadOnda",
            Args = "frecuencia, longitud_onda",
            Help = "Velocidad de propagación de una onda.\n\nFórmula: v = f·λ")]
        public static LatinoObject VelocidadOnda(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere frecuencia (Hz) y longitud de onda (m).");
            double f      = ((LatinoNumber)args[0]).Value;
            double lambda = ((LatinoNumber)args[1]).Value;
            if (f <= 0 || lambda <= 0) throw new RuntimeError(null!, "Frecuencia y longitud de onda deben ser positivas.");
            return Respuesta.Crear(f * lambda, "m/s", "v = f·λ");
        }

    [LatinoExtension("ondas.frecuenciaPeriodo",
            Args = "diccionario_variables",
            Help = "Relación inversa entre frecuencia y período. Pasa 'f' o 'T'.\n\nFórmula: f = 1 / T")]
        public static LatinoObject FrecuenciaPeriodo(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con 'f' (Hz) o 'T' (s).");

            bool tieneF = Respuesta.TryGetVal(datos, "f", out double f);
            bool tieneT = Respuesta.TryGetVal(datos, "T", out double T);

            if (tieneF && f > 0)
            {
                var ex1 = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                    { new LatinoString("incognita"), new LatinoString("T") }
                });
                return Respuesta.Crear(1.0 / f, "s", "T = 1 / f", ex1);
            }
            if (tieneT && T > 0)
            {
                var ex2 = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                    { new LatinoString("incognita"), new LatinoString("f") }
                });
                return Respuesta.Crear(1.0 / T, "Hz", "f = 1 / T", ex2);
            }
            throw new RuntimeError(null!, "Proporciona 'f' o 'T' con valor positivo.");
        }

    [LatinoExtension("ondas.efectoDoppler",
            Args = "frecuencia_fuente, velocidad_sonido, velocidad_observador, velocidad_fuente",
            Help = "Frecuencia percibida por efecto Doppler (+ si se acercan, - si se alejan).\n\nFórmula: f' = f·(v + v_obs) / (v + v_src)")]
        public static LatinoObject EfectoDoppler(List<LatinoObject> args)
        {
            if (args.Count < 4) throw new RuntimeError(null!, "Requiere frecuencia_fuente (Hz), velocidad_sonido (m/s), velocidad_observador (m/s) y velocidad_fuente (m/s). Usa + si se acercan, - si se alejan.");
            double f0   = ((LatinoNumber)args[0]).Value;
            double vs   = ((LatinoNumber)args[1]).Value;
            double vObs = ((LatinoNumber)args[2]).Value;
            double vSrc = ((LatinoNumber)args[3]).Value;
            if (vs <= 0) throw new RuntimeError(null!, "La velocidad del sonido debe ser positiva.");
            if (f0 <= 0) throw new RuntimeError(null!, "La frecuencia de la fuente debe ser positiva.");
            double denominador = vs + vSrc;
            if (Math.Abs(denominador) < 1e-10) throw new RuntimeError(null!, "La velocidad de la fuente iguala la del sonido: singularidad (onda de choque).");
            double fPrima = f0 * (vs + vObs) / denominador;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("frecuencia_fuente"), new LatinoNumber(f0) },
                { new LatinoString("delta_f"), new LatinoNumber(fPrima - f0) }
            });
            return Respuesta.Crear(fPrima, "Hz", "f' = f·(v + v_obs) / (v + v_src)", extras);
        }

    [LatinoExtension("ondas.longitudOndaLuz",
            Args = "frecuencia",
            Help = "Longitud de onda de la luz en el vacío dado su frecuencia (c = 299 792 458 m/s).\n\nFórmula: λ = c / f")]
        public static LatinoObject LongitudOndaLuz(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere frecuencia (Hz).");
            double f = ((LatinoNumber)args[0]).Value;
            if (f <= 0) throw new RuntimeError(null!, "La frecuencia debe ser positiva.");
            double lambda = Constantes.C / f;

            string espectro;
            if      (f < 3e8)   espectro = "Radio";
            else if (f < 3e11)  espectro = "Microondas";
            else if (f < 4.3e14) espectro = "Infrarrojo";
            else if (f < 7.5e14) espectro = "Visible";
            else if (f < 3e17)  espectro = "Ultravioleta";
            else if (f < 3e19)  espectro = "Rayos X";
            else                espectro = "Rayos Gamma";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("espectro"), new LatinoString(espectro) }
            });
            return Respuesta.Crear(lambda, "m", "λ = c / f", extras);
        }

}
