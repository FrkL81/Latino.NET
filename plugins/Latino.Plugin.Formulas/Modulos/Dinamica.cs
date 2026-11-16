using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class DinamicaFormulas
{
    [LatinoExtension("dinamica.trabajo", Args = "fuerza, distancia, anguloGrados", Help = "Calcula el trabajo mecánico.\n\nFórmula: W = F · d · cos(θ)")]
        public static LatinoObject Trabajo(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere fuerza, distancia y ángulo.");
            double F = ((LatinoNumber)args[0]).Value;
            double d = ((LatinoNumber)args[1]).Value;
            double theta = ((LatinoNumber)args[2]).Value;
            if (d < 0) throw new RuntimeError(null!, "La distancia no puede ser negativa.");

            double rad = theta * (Constantes.PI / 180.0);
            return Respuesta.Crear(F * d * Math.Cos(rad), "J", "W = F · d · cos(θ)");
        }

    [LatinoExtension("dinamica.newtonFuerza",
            Args = "masa, aceleracion",
            Help = "Segunda Ley de Newton.\n\nFórmula: F = m·a")]
        public static LatinoObject NewtonFuerza(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere masa (kg) y aceleracion (m/s²).");
            double m = ((LatinoNumber)args[0]).Value;
            double a = ((LatinoNumber)args[1]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            return Respuesta.Crear(m * a, "N", "F = m·a");
        }

    [LatinoExtension("dinamica.peso",
            Args = "masa",
            Help = "Peso de un objeto en la superficie terrestre (g = 9.80665 m/s²).\n\nFórmula: W = m·g")]
        public static LatinoObject Peso(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere masa (kg).");
            double m = ((LatinoNumber)args[0]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            return Respuesta.Crear(m * Constantes.G, "N", "W = m·g");
        }

    [LatinoExtension("dinamica.energiaMecanica",
            Args = "masa, velocidad, altura",
            Help = "Energía mecánica total (cinética + potencial).\n\nFórmula: Em = ½·m·v² + m·g·h")]
        public static LatinoObject EnergiaMecanica(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere masa (kg), velocidad (m/s) y altura (m).");
            double m = ((LatinoNumber)args[0]).Value;
            double v = ((LatinoNumber)args[1]).Value;
            double h = ((LatinoNumber)args[2]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            double ec = 0.5 * m * v * v;
            double ep = m * Constantes.G * h;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("energia_cinetica"), new LatinoNumber(ec) },
                { new LatinoString("energia_potencial"), new LatinoNumber(ep) }
            });
            return Respuesta.Crear(ec + ep, "J", "Em = ½·m·v² + m·g·h", extras);
        }

    [LatinoExtension("dinamica.potencia",
            Args = "trabajo, tiempo",
            Help = "Potencia mecánica media.\n\nFórmula: P = W / t")]
        public static LatinoObject Potencia(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere trabajo (J) y tiempo (s).");
            double W = ((LatinoNumber)args[0]).Value;
            double t = ((LatinoNumber)args[1]).Value;
            if (t <= 0) throw new RuntimeError(null!, "El tiempo debe ser mayor a cero.");
            return Respuesta.Crear(W / t, "W", "P = W / t");
        }

    [LatinoExtension("dinamica.penduloSimple",
            Args = "longitud",
            Help = "Período de oscilación de un péndulo simple (g = 9.80665 m/s²).\n\nFórmula: T = 2π·√(L/g)")]
        public static LatinoObject PenduloSimple(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere longitud del péndulo (m).");
            double L = ((LatinoNumber)args[0]).Value;
            if (L <= 0) throw new RuntimeError(null!, "La longitud debe ser mayor a cero.");
            double T = 2 * Constantes.PI * Math.Sqrt(L / Constantes.G);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("frecuencia"), new LatinoNumber(1.0 / T) }
            });
            return Respuesta.Crear(T, "s", "T = 2π·√(L/g)", extras);
        }

    [LatinoExtension("dinamica.segundaLeyRotacional",
            Args = "momento_inercia, aceleracion_angular",
            Help = "Segunda Ley de Newton para rotación.\n\nFórmula: τ = I·α")]
        public static LatinoObject SegundaLeyRotacional(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere momento de inercia (kg·m²) y aceleracion angular (rad/s²).");
            double I     = ((LatinoNumber)args[0]).Value;
            double alpha = ((LatinoNumber)args[1]).Value;
            if (I < 0) throw new RuntimeError(null!, "El momento de inercia no puede ser negativo.");
            return Respuesta.Crear(I * alpha, "N·m", "τ = I·α");
        }

}
