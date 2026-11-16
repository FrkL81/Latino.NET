using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class ElectromagnetismoFormulas
{
    [LatinoExtension("electricidad.leyOhm", Args = "diccionarioVariables", Help = "Ley de Ohm. Pasa 2 valores: 'V', 'I' o 'R'.\n\nFórmula: V = I · R")]
        public static LatinoObject LeyOhm(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary conocidas)
                throw new RuntimeError(null!, "Se requiere diccionario con 'V', 'I', 'R'.");

            bool tieneV = Respuesta.TryGetVal(conocidas, "V", out double V);
            bool tieneI = Respuesta.TryGetVal(conocidas, "I", out double I);
            bool tieneR = Respuesta.TryGetVal(conocidas, "R", out double R);

            if (tieneR && R < 0) throw new RuntimeError(null!, "La resistencia no puede ser negativa.");

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));

            if (tieneI && tieneR) {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("V");
                return Respuesta.Crear(I * R, "V", "V = I · R", extras);
            }
            if (tieneV && tieneR) {
                if (R == 0) throw new RuntimeError(null!, "Cortocircuito: Resistencia 0.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("I");
                return Respuesta.Crear(V / R, "A", "I = V / R", extras);
            }
            if (tieneV && tieneI) {
                if (I == 0) throw new RuntimeError(null!, "Corriente 0 impide calcular R.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("R");
                return Respuesta.Crear(V / I, "Ω", "R = V / I", extras);
            }

            throw new RuntimeError(null!, "Proporciona exactamente 2 variables: V, I, o R.");
        }

    [LatinoExtension("electricidad.leyCoulomb", Args = "carga1, carga2, distancia", Help = "Fuerza electrostática entre dos cargas.\n\nFórmula: F = k·q₁·q₂ / r²")]
        public static LatinoObject LeyCoulomb(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere carga1, carga2 (C) y distancia (m).");
            double q1 = ((LatinoNumber)args[0]).Value;
            double q2 = ((LatinoNumber)args[1]).Value;
            double r = ((LatinoNumber)args[2]).Value;
            if (r <= 0) throw new RuntimeError(null!, "La distancia debe ser estrictamente positiva.");
            double F = Constantes.K_COULOMB * Math.Abs(q1 * q2) / (r * r);
            return Respuesta.Crear(F, "N", "F = k·|q₁·q₂| / r²");
        }

    [LatinoExtension("electromagnetismo.campoElectrico",
            Args = "cargaC, distanciaM",
            Help = "Intensidad del campo eléctrico generado por una carga puntual.\n\nFórmula: E = k·|q| / r²")]
        public static LatinoObject CampoElectrico(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere cargaC (C) y distanciaM (m).");
            double q = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            if (r <= 0) throw new RuntimeError(null!, "La distancia debe ser mayor a cero.");
            return Respuesta.Crear(Constantes.K_COULOMB * Math.Abs(q) / (r * r), "N/C", "E = k·|q| / r²");
        }

    [LatinoExtension("electromagnetismo.potencialElectrico",
            Args = "cargaC, distanciaM",
            Help = "Potencial eléctrico en un punto a distancia r de una carga puntual.\n\nFórmula: V = k·q / r")]
        public static LatinoObject PotencialElectrico(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere cargaC (C) y distanciaM (m).");
            double q = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            if (r <= 0) throw new RuntimeError(null!, "La distancia debe ser mayor a cero.");
            return Respuesta.Crear(Constantes.K_COULOMB * q / r, "V", "V = k·q / r");
        }

    [LatinoExtension("electromagnetismo.capacitorEnergia",
            Args = "capacitanciaF, voltajeV",
            Help = "Energía almacenada en un capacitor cargado.\n\nFórmula: U = ½·C·V²")]
        public static LatinoObject CapacitorEnergia(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere capacitanciaF (F) y voltajeV (V).");
            double C = ((LatinoNumber)args[0]).Value;
            double V = ((LatinoNumber)args[1]).Value;
            if (C < 0) throw new RuntimeError(null!, "La capacitancia no puede ser negativa.");
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("carga_almacenada_C"), new LatinoNumber(C * V) }
            });
            return Respuesta.Crear(0.5 * C * V * V, "J", "U = ½·C·V²", extras);
        }

    [LatinoExtension("electromagnetismo.inductorEnergia",
            Args = "inductanciaH, corrienteA",
            Help = "Energía almacenada en el campo magnético de un inductor.\n\nFórmula: U = ½·L·I²")]
        public static LatinoObject InductorEnergia(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere inductanciaH (H) y corrienteA (A).");
            double L = ((LatinoNumber)args[0]).Value;
            double I = ((LatinoNumber)args[1]).Value;
            if (L < 0) throw new RuntimeError(null!, "La inductancia no puede ser negativa.");
            return Respuesta.Crear(0.5 * L * I * I, "J", "U = ½·L·I²");
        }

    [LatinoExtension("electromagnetismo.potenciaElectrica",
            Args = "diccionarioVariables",
            Help = "Potencia eléctrica. Pasa 2 de: 'V', 'I', 'R'.\n\nFórmulas: P = V·I | P = I²·R | P = V²/R")]
        public static LatinoObject PotenciaElectrica(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con combinación de 'V', 'I', 'R'.");
            bool tieneV = Respuesta.TryGetVal(datos, "V", out double V);
            bool tieneI = Respuesta.TryGetVal(datos, "I", out double I);
            bool tieneR = Respuesta.TryGetVal(datos, "R", out double R);
            var extras  = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));

            if (tieneV && tieneI) {
                extras.Pairs[new LatinoString("formula_usada")] = new LatinoString("P = V·I");
                return Respuesta.Crear(V * I, "W", "P = V·I", extras);
            }
            if (tieneI && tieneR) {
                if (R < 0) throw new RuntimeError(null!, "La resistencia no puede ser negativa.");
                extras.Pairs[new LatinoString("formula_usada")] = new LatinoString("P = I²·R");
                return Respuesta.Crear(I * I * R, "W", "P = I²·R", extras);
            }
            if (tieneV && tieneR) {
                if (R <= 0) throw new RuntimeError(null!, "La resistencia debe ser mayor a cero.");
                extras.Pairs[new LatinoString("formula_usada")] = new LatinoString("P = V²/R");
                return Respuesta.Crear(V * V / R, "W", "P = V²/R", extras);
            }
            throw new RuntimeError(null!, "Proporciona 2 de: V (voltios), I (amperes), R (ohmios).");
        }

    [LatinoExtension("electromagnetismo.resonanciaLC",
            Args = "inductanciaH, capacitanciaF",
            Help = "Frecuencia de resonancia de un circuito LC.\n\nFórmula: f₀ = 1 / (2π·√(L·C))")]
        public static LatinoObject ResonanciaLC(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere inductanciaH (H) y capacitanciaF (F).");
            double L = ((LatinoNumber)args[0]).Value;
            double C = ((LatinoNumber)args[1]).Value;
            if (L <= 0 || C <= 0) throw new RuntimeError(null!, "Inductancia y capacitancia deben ser positivas.");
            double f0 = 1.0 / (2 * Constantes.PI * Math.Sqrt(L * C));
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("periodo_s"),         new LatinoNumber(1.0 / f0) },
                { new LatinoString("omega_rad_s"),       new LatinoNumber(2 * Constantes.PI * f0) }
            });
            return Respuesta.Crear(f0, "Hz", "f₀ = 1 / (2π·√(L·C))", extras);
        }

    [LatinoExtension("electromagnetismo.leyFaraday",
            Args = "numEspiras, deltaFlujoWb, deltaTiempoS",
            Help = "FEM inducida por variación del flujo magnético (Ley de Faraday).\n\nFórmula: ε = -N·ΔΦ/Δt")]
        public static LatinoObject LeyFaraday(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere numEspiras (entero), deltaFlujoWb (Wb) y deltaTiempoS (s).");
            double N  = ((LatinoNumber)args[0]).Value;
            double dF = ((LatinoNumber)args[1]).Value;
            double dt = ((LatinoNumber)args[2]).Value;
            if (N < 1)  throw new RuntimeError(null!, "El número de espiras debe ser al menos 1.");
            if (dt <= 0) throw new RuntimeError(null!, "El intervalo de tiempo debe ser mayor a cero.");
            double fem = -N * dF / dt;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("nota"), new LatinoString("Signo negativo: Ley de Lenz (la FEM se opone al cambio de flujo)") }
            });
            return Respuesta.Crear(fem, "V", "ε = -N·ΔΦ/Δt", extras);
        }

    [LatinoExtension("electromagnetismo.transformador",
            Args = "voltajePrimario, espirasPrimario, espirasSecundario",
            Help = "Voltaje en el secundario de un transformador ideal.\n\nFórmula: V₂ = V₁·(N₂/N₁)")]
        public static LatinoObject Transformador(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere voltajePrimario (V), espirasPrimario y espirasSecundario.");
            double V1 = ((LatinoNumber)args[0]).Value;
            double N1 = ((LatinoNumber)args[1]).Value;
            double N2 = ((LatinoNumber)args[2]).Value;
            if (N1 <= 0 || N2 <= 0) throw new RuntimeError(null!, "El número de espiras debe ser mayor a cero.");
            double V2    = V1 * (N2 / N1);
            double ratio = N2 / N1;
            string tipo  = ratio > 1 ? "Elevador (step-up)" : ratio < 1 ? "Reductor (step-down)" : "Aislante (1:1)";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("relacion_transformacion"), new LatinoNumber(ratio) },
                { new LatinoString("tipo"),                   new LatinoString(tipo) }
            });
            return Respuesta.Crear(V2, "V", "V₂ = V₁·(N₂/N₁)", extras);
        }

    [LatinoExtension("electricidad.filtroRC",
        Args = "resistenciaOhm, capacitanciaF",
        Help = "Frecuencia de corte (-3dB) de un filtro pasa-bajos RC.\n\nFórmula: f_c = 1 / (2π·R·C)")]
    public static LatinoObject FiltroRC(List<LatinoObject> args)
    {
        if (args.Count < 2) throw new RuntimeError(null!, "Requiere resistenciaOhm (Ω) y capacitanciaF (F).");
        double R = ((LatinoNumber)args[0]).Value;
        double C = ((LatinoNumber)args[1]).Value;

        if (R <= 0) throw new RuntimeError(null!, "La resistencia debe ser mayor a cero.");
        if (C <= 0) throw new RuntimeError(null!, "La capacitancia debe ser mayor a cero.");

        double fc = 1.0 / (2 * Constantes.PI * R * C);
        double tau = R * C; // Constante de tiempo

        // Clasificar el rango de frecuencia de corte
        string rango;
        if      (fc < 20)       rango = "Sub-audible (< 20 Hz) — aplicaciones de control lento";
        else if (fc < 20_000)   rango = "Audible (20 Hz – 20 kHz) — audio y música";
        else if (fc < 1e6)      rango = "Radiofrecuencia baja (kHz) — AM, señales industriales";
        else if (fc < 1e9)      rango = "Radiofrecuencia (MHz) — FM, WiFi, comunicaciones";
        else                    rango = "Microondas / RF alta (GHz) — componentes de alta frecuencia";

        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("constante_tiempo_s"), new LatinoNumber(tau) },
            { new LatinoString("rango"),              new LatinoString(rango) },
            { new LatinoString("nota"),
              new LatinoString("Para pasa-altos RC: misma f_c, la señal se invierte. " +
                               "Para pasa-bajos activo: añadir amplificador operacional.") }
        });
        return Respuesta.Crear(fc, "Hz", "f_c = 1 / (2π·R·C)", extras);
    }
}
