using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class AstronomiaFormulas
{
    [LatinoExtension("astronomia.velocidadEscape", Args = "masaCuerpo, radio", Help = "Calcula la velocidad de escape.\n\nFórmula: v_e = √(2·G·M / r)")]
        public static LatinoObject VelocidadEscape(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere masaCuerpo (kg) y radio (m).");
            double M = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            if (M <= 0 || r <= 0) throw new RuntimeError(null!, "Masa y radio deben ser mayores a cero.");

            double ve = Math.Sqrt(2.0 * Constantes.G_UNIV * M / r);
            return Respuesta.Crear(ve, "m/s", "v_e = √(2·G·M / r)");
        }

    [LatinoExtension("astronomia.fuerzaGravitacional", Args = "masa1, masa2, distancia", Help = "Fuerza de atracción gravitacional entre dos cuerpos.\n\nFórmula: F = G·m₁·m₂ / r²")]
    public static LatinoObject FuerzaGravitacional(List<LatinoObject> args)
    {
        if (args.Count < 3) throw new RuntimeError(null!, "Requiere masa1 (kg), masa2 (kg) y distancia (m).");
        double m1 = ((LatinoNumber)args[0]).Value;
        double m2 = ((LatinoNumber)args[1]).Value;
        double r  = ((LatinoNumber)args[2]).Value;
        if (m1 < 0 || m2 < 0) throw new RuntimeError(null!, "Las masas no pueden ser negativas.");
        if (r <= 0) throw new RuntimeError(null!, "La distancia debe ser mayor a cero.");
        
        double f = Constantes.G_UNIV * m1 * m2 / (r * r);
        return Respuesta.Crear(f, "N", "F = G·m₁·m₂ / r²");
    }

    [LatinoExtension("astronomia.distanciaParalaje", Args = "paralajeArcsec", Help = "Distancia a una estrella basada en su ángulo de paralaje.\n\nFórmula: d = 1 / p")]
    public static LatinoObject DistanciaParalaje(List<LatinoObject> args)
    {
        if (args.Count < 1) throw new RuntimeError(null!, "Requiere ángulo de paralaje en segundos de arco (arcsec).");
        double p = ((LatinoNumber)args[0]).Value;
        if (p <= 0) throw new RuntimeError(null!, "El ángulo de paralaje debe ser positivo.");
        
        double d = 1.0 / p;
        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("distancia_anios_luz"), new LatinoNumber(d * 3.26156) }
        });
        return Respuesta.Crear(d, "parsecs", "d = 1 / p", extras);
    }

    [LatinoExtension("astronomia.keplerTercera", Args = "diccionarioVariables", Help = "Tercera Ley de Kepler. Pasa 2 de: 'T' (periodo s), 'a' (semieje m), 'M' (masa central kg).\n\nFórmula: T² = (4π² / GM) · a³")]
    public static LatinoObject KeplerTercera(List<LatinoObject> args)
    {
        if (args.Count == 0 || args[0] is not LatinoDictionary datos)
            throw new RuntimeError(null!, "Se requiere diccionario con 2 de: 'T', 'a', 'M'.");

        bool tieneT = Respuesta.TryGetVal(datos, "T", out double T);
        bool tienea = Respuesta.TryGetVal(datos, "a", out double a);
        bool tieneM = Respuesta.TryGetVal(datos, "M", out double M);

        int count = (tieneT ? 1 : 0) + (tienea ? 1 : 0) + (tieneM ? 1 : 0);
        if (count != 2) throw new RuntimeError(null!, "Proporciona exactamente 2 variables para despejar la tercera.");

        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
        string f = "T² = (4π² / GM) · a³";
        double k = 4 * Math.Pow(Constantes.PI, 2) / Constantes.G_UNIV;

        if (!tieneT) {
            if (a <= 0 || M <= 0) throw new RuntimeError(null!, "'a' y 'M' deben ser estrictamente positivos.");
            extras.Pairs[new LatinoString("incognita")] = new LatinoString("T");
            double res = Math.Sqrt((k / M) * Math.Pow(a, 3));
            return Respuesta.Crear(res, "s", f, extras);
        }
        if (!tienea) {
            if (T <= 0 || M <= 0) throw new RuntimeError(null!, "'T' y 'M' deben ser estrictamente positivos.");
            extras.Pairs[new LatinoString("incognita")] = new LatinoString("a");
            double res = Math.Cbrt((Math.Pow(T, 2) * M) / k);
            return Respuesta.Crear(res, "m", f, extras);
        }
        if (!tieneM) {
            if (T <= 0 || a <= 0) throw new RuntimeError(null!, "'T' y 'a' deben ser estrictamente positivos.");
            extras.Pairs[new LatinoString("incognita")] = new LatinoString("M");
            double res = (k * Math.Pow(a, 3)) / Math.Pow(T, 2);
            return Respuesta.Crear(res, "kg", f, extras);
        }

        throw new RuntimeError(null!, "Error en solver de Kepler.");
    }

}
