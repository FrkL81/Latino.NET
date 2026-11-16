using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class CinematicaFormulas
{
    [LatinoExtension("cinematica.mru", Args = "diccionarioVariables", Help = "Movimiento Rectilíneo Uniforme. Pasa 2 de: 'v', 'd', 't'.\n\nFórmula: v = d / t")]
        public static LatinoObject MRU(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con 'v', 'd', 't'.");

            bool tieneV = Respuesta.TryGetVal(datos, "v", out double v);
            bool tieneD = Respuesta.TryGetVal(datos, "d", out double d);
            bool tieneT = Respuesta.TryGetVal(datos, "t", out double t);

            if (tieneT && t <= 0) throw new RuntimeError(null!, "El tiempo debe ser mayor a cero.");
            if (tieneD && d < 0) throw new RuntimeError(null!, "La distancia no puede ser negativa.");

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));

            if (tieneD && tieneT) 
            {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("v");
                return Respuesta.Crear(d / t, "m/s", "v = d / t", extras);
            }
            if (tieneV && tieneT)
            {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("d");
                return Respuesta.Crear(v * t, "m", "d = v · t", extras);
            }
            if (tieneV && tieneD)
            {
                if (v == 0) throw new RuntimeError(null!, "Velocidad cero implica tiempo infinito para recorrer una distancia.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("t");
                return Respuesta.Crear(d / v, "s", "t = d / v", extras);
            }

            throw new RuntimeError(null!, "Proporciona exactamente 2 variables válidas: v, d, o t.");
        }

    [LatinoExtension("cinematica.mruaVelocidad",
            Args = "velocidadInicial, aceleracion, tiempo",
            Help = "Velocidad final en Movimiento Rectilíneo Uniformemente Acelerado.\n\nFórmula: v = v₀ + a·t")]
        public static LatinoObject MRUAVelocidad(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere velocidadInicial (m/s), aceleracion (m/s²) y tiempo (s).");
            double v0 = ((LatinoNumber)args[0]).Value;
            double a  = ((LatinoNumber)args[1]).Value;
            double t  = ((LatinoNumber)args[2]).Value;
            if (t < 0) throw new RuntimeError(null!, "El tiempo no puede ser negativo.");
            return Respuesta.Crear(v0 + a * t, "m/s", "v = v₀ + a·t");
        }

    [LatinoExtension("cinematica.mruaPosicion",
            Args = "posicionInicial, velocidadInicial, aceleracion, tiempo",
            Help = "Posición en MRUA (segunda ecuación cinemática).\n\nFórmula: x = x₀ + v₀·t + ½·a·t²")]
        public static LatinoObject MRUAPosicion(List<LatinoObject> args)
        {
            if (args.Count < 4) throw new RuntimeError(null!, "Requiere posicionInicial (m), velocidadInicial (m/s), aceleracion (m/s²) y tiempo (s).");
            double x0 = ((LatinoNumber)args[0]).Value;
            double v0 = ((LatinoNumber)args[1]).Value;
            double a  = ((LatinoNumber)args[2]).Value;
            double t  = ((LatinoNumber)args[3]).Value;
            if (t < 0) throw new RuntimeError(null!, "El tiempo no puede ser negativo.");
            return Respuesta.Crear(x0 + v0 * t + 0.5 * a * t * t, "m", "x = x₀ + v₀·t + ½·a·t²");
        }

    [LatinoExtension("cinematica.energiaCinetica",
            Args = "masa, velocidad",
            Help = "Energía cinética de un objeto en movimiento.\n\nFórmula: Ec = ½·m·v²")]
        public static LatinoObject EnergiaCinetica(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere masa (kg) y velocidad (m/s).");
            double m = ((LatinoNumber)args[0]).Value;
            double v = ((LatinoNumber)args[1]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            return Respuesta.Crear(0.5 * m * v * v, "J", "Ec = ½·m·v²");
        }

    [LatinoExtension("cinematica.energiaPotencial",
            Args = "masa, altura",
            Help = "Energía potencial gravitatoria (g = 9.80665 m/s²).\n\nFórmula: Ep = m·g·h")]
        public static LatinoObject EnergiaPotencial(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere masa (kg) y altura (m).");
            double m = ((LatinoNumber)args[0]).Value;
            double h = ((LatinoNumber)args[1]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            return Respuesta.Crear(m * Constantes.G * h, "J", "Ep = m·g·h");
        }

    [LatinoExtension("cinematica.momentoLineal",
            Args = "masa, velocidad",
            Help = "Cantidad de movimiento (momento lineal).\n\nFórmula: p = m·v")]
        public static LatinoObject MomentoLineal(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere masa (kg) y velocidad (m/s).");
            double m = ((LatinoNumber)args[0]).Value;
            double v = ((LatinoNumber)args[1]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            return Respuesta.Crear(m * v, "kg·m/s", "p = m·v");
        }

    [LatinoExtension("cinematica.impulso",
            Args = "fuerza, tiempo",
            Help = "Impulso de una fuerza (variación de momento lineal).\n\nFórmula: J = F·Δt")]
        public static LatinoObject Impulso(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere fuerza (N) y tiempo (s).");
            double F  = ((LatinoNumber)args[0]).Value;
            double dt = ((LatinoNumber)args[1]).Value;
            if (dt < 0) throw new RuntimeError(null!, "El intervalo de tiempo no puede ser negativo.");
            return Respuesta.Crear(F * dt, "N·s", "J = F·Δt");
        }

    [LatinoExtension("cinematica.tiroParabolico", Args = "velocidadInicial, anguloGrados", Help = "Componentes de tiro parabólico.\n\nFórmulas: h_max = (v₀²·sin²(θ))/2g | d_max = (v₀²·sin(2θ))/g")]
        public static LatinoObject TiroParabolico(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere velocidadInicial y anguloGrados.");
            double v0 = ((LatinoNumber)args[0]).Value;
            double angulo = ((LatinoNumber)args[1]).Value;

            if (v0 < 0) throw new RuntimeError(null!, "La velocidad inicial no puede ser negativa.");
            if (angulo < 0 || angulo > 90) throw new RuntimeError(null!, "El ángulo debe estar entre 0 y 90 grados.");

            double rad = angulo * (Constantes.PI / 180.0);
            double hMax = (v0 * v0 * Math.Pow(Math.Sin(rad), 2)) / (2 * Constantes.G);
            double dMax = (v0 * v0 * Math.Sin(2 * rad)) / Constantes.G;
            double tVuelo = (2 * v0 * Math.Sin(rad)) / Constantes.G;

            var valores = new Dictionary<string, double>
            {
                { "altura_maxima", hMax },
                { "alcance_maximo", dMax },
                { "tiempo_vuelo", tVuelo }
            };
            return Respuesta.Estructurada(valores, "h_max = (v₀²·sin²(θ))/2g | d_max = (v₀²·sin(2θ))/g | t = (2v₀·sin(θ))/g");
        }

}
