using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class TermodinamicaFormulas
{
    [LatinoExtension("termodinamica.gasIdeal", Args = "diccionarioVariables", Help = "Ley de gases ideales. Pasa 3 de: 'P', 'V', 'n', 'T'.\n\nFórmula: P · V = n · R · T")]
        public static LatinoObject GasIdeal(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con 'P', 'V', 'n', 'T'.");

            bool tieneP = Respuesta.TryGetVal(datos, "P", out double P);
            bool tieneV = Respuesta.TryGetVal(datos, "V", out double V);
            bool tieneN = Respuesta.TryGetVal(datos, "n", out double n);
            bool tieneT = Respuesta.TryGetVal(datos, "T", out double T);

            if (tieneT && T < 0) throw new RuntimeError(null!, "La temperatura Kelvin no puede ser negativa.");
            if (tieneV && V <= 0) throw new RuntimeError(null!, "El volumen debe ser mayor a cero.");
            if (tieneN && n <= 0) throw new RuntimeError(null!, "Los moles deben ser mayores a cero.");

            int count = (tieneP ? 1 : 0) + (tieneV ? 1 : 0) + (tieneN ? 1 : 0) + (tieneT ? 1 : 0);
            if (count != 3) throw new RuntimeError(null!, "Proporciona exactamente 3 variables para despejar la cuarta.");

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
            string f = "P · V = n · R · T";

            if (!tieneP) {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("P");
                return Respuesta.Crear((n * Constantes.R * T) / V, "Pa", f, extras);
            }
            if (!tieneV) {
                if (P == 0) throw new RuntimeError(null!, "Presión cero genera volumen infinito.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("V");
                return Respuesta.Crear((n * Constantes.R * T) / P, "m³", f, extras);
            }
            if (!tieneN) {
                if (T == 0) throw new RuntimeError(null!, "Temperatura cero invalida para gas ideal.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("n");
                return Respuesta.Crear((P * V) / (Constantes.R * T), "mol", f, extras);
            }
            if (!tieneT) {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("T");
                return Respuesta.Crear((P * V) / (n * Constantes.R), "K", f, extras);
            }

            throw new RuntimeError(null!, "Error en solver termodinámico.");
        }

    [LatinoExtension("termodinamica.estadoAgua", Args = "temperaturaCelsius", Help = "Determina el estado del agua a 1 atm.\n\nDiagnóstico: Sólido, Líquido, Gaseoso")]
        public static LatinoObject EstadoAgua(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere temperatura en Celsius.");
            double T = ((LatinoNumber)args[0]).Value;

            if (T < -273.15) throw new RuntimeError(null!, "Temperatura inferior al cero absoluto.");

            string estado = T < 0 ? "Sólido (Hielo)" : T < 100 ? "Líquido" : "Gaseoso (Vapor)";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("estado_fisico"), new LatinoString(estado) }
            });
            return Respuesta.Crear(T, "°C", "H₂O State @ 1 atm", extras);
        }

    [LatinoExtension("termodinamica.calorEspecifico",
            Args = "masa, calorEspecifico, deltaTemperatura",
            Help = "Calor absorbido o cedido por una sustancia.\n\nFórmula: Q = m·c·ΔT")]
        public static LatinoObject CalorEspecifico(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere masa (kg), calorEspecifico (J/(kg·K)) y deltaTemperatura (K o °C).");
            double m  = ((LatinoNumber)args[0]).Value;
            double c  = ((LatinoNumber)args[1]).Value;
            double dT = ((LatinoNumber)args[2]).Value;
            if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
            if (c < 0) throw new RuntimeError(null!, "El calor específico no puede ser negativo.");
            string proceso = dT > 0 ? "Absorcion de calor (calentamiento)" : dT < 0 ? "Cesion de calor (enfriamiento)" : "Sin cambio de temperatura";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("proceso"), new LatinoString(proceso) }
            });
            return Respuesta.Crear(m * c * dT, "J", "Q = m·c·ΔT", extras);
        }

    [LatinoExtension("termodinamica.cicloCarnot",
            Args = "temperaturaCalienteK, temperaturaFriaK",
            Help = "Eficiencia máxima teórica del ciclo de Carnot.\n\nFórmula: η = 1 - (T_fría / T_caliente)")]
        public static LatinoObject CicloCarnot(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere temperaturaCaliente (K) y temperaturaFria (K).");
            double Th = ((LatinoNumber)args[0]).Value;
            double Tc = ((LatinoNumber)args[1]).Value;
            if (Th <= 0 || Tc <= 0) throw new RuntimeError(null!, "Las temperaturas deben estar en Kelvin (valores positivos).");
            if (Tc >= Th) throw new RuntimeError(null!, "La temperatura fría debe ser menor que la caliente.");
            double eta = 1.0 - (Tc / Th);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("eficiencia_porcentaje"), new LatinoNumber(eta * 100) },
                { new LatinoString("nota"), new LatinoString("Eficiencia maxima teorica. Ninguna maquina real la alcanza.") }
            });
            return Respuesta.Crear(eta, "adimensional (0-1)", "η = 1 - (Tc / Th)", extras);
        }

    [LatinoExtension("termodinamica.conduccionCalor",
            Args = "conductividad, area, deltaTemperatura, espesor",
            Help = "Flujo de calor por conducción (Ley de Fourier).\n\nFórmula: Q/t = k·A·ΔT / d")]
        public static LatinoObject ConduccionCalor(List<LatinoObject> args)
        {
            if (args.Count < 4) throw new RuntimeError(null!, "Requiere conductividad (W/(m·K)), area (m²), deltaTemperatura (K) y espesor (m).");
            double k  = ((LatinoNumber)args[0]).Value;
            double A  = ((LatinoNumber)args[1]).Value;
            double dT = ((LatinoNumber)args[2]).Value;
            double d  = ((LatinoNumber)args[3]).Value;
            if (k <= 0) throw new RuntimeError(null!, "La conductividad térmica debe ser positiva.");
            if (A <= 0) throw new RuntimeError(null!, "El área debe ser positiva.");
            if (d <= 0) throw new RuntimeError(null!, "El espesor debe ser mayor a cero.");
            return Respuesta.Crear((k * A * dT) / d, "W", "Q/t = k·A·ΔT / d");
        }

    [LatinoExtension("termodinamica.stefanBoltzmann",
            Args = "temperaturaK, [emisividad], [areaM2]",
            Help = "Potencia radiada por un cuerpo (Ley de Stefan-Boltzmann). σ = 5.6704×10⁻⁸ W/(m²·K⁴).\n\nFórmula: P = ε·σ·A·T⁴")]
        public static LatinoObject StefanBoltzmann(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere temperaturaK (K). Opcionalmente emisividad (0-1, defecto: 1.0) y areaM2 (m², defecto: 1.0).");
            double T   = ((LatinoNumber)args[0]).Value;
            double eps = args.Count > 1 ? ((LatinoNumber)args[1]).Value : 1.0;
            double A   = args.Count > 2 ? ((LatinoNumber)args[2]).Value : 1.0;
            if (T <= 0) throw new RuntimeError(null!, "La temperatura debe estar en Kelvin (valor positivo).");
            if (eps < 0 || eps > 1) throw new RuntimeError(null!, "La emisividad debe estar entre 0 y 1.");
            if (A <= 0) throw new RuntimeError(null!, "El área debe ser positiva.");
            const double sigma = 5.670374419e-8;
            return Respuesta.Crear(eps * sigma * A * Math.Pow(T, 4), "W", "P = ε·σ·A·T⁴");
        }

    [LatinoExtension("termodinamica.dilatacionLineal",
            Args = "longitudInicial, coefDilatacion, deltaTemperatura",
            Help = "Dilatación lineal de un sólido por cambio de temperatura.\n\nFórmula: ΔL = L₀·α·ΔT")]
        public static LatinoObject DilatacionLineal(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere longitudInicial (m), coefDilatacion (1/K) y deltaTemperatura (K).");
            double L0    = ((LatinoNumber)args[0]).Value;
            double alpha = ((LatinoNumber)args[1]).Value;
            double dT    = ((LatinoNumber)args[2]).Value;
            if (L0 < 0)    throw new RuntimeError(null!, "La longitud inicial no puede ser negativa.");
            if (alpha < 0) throw new RuntimeError(null!, "El coeficiente de dilatación no puede ser negativo.");
            double dL = L0 * alpha * dT;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("longitud_final"), new LatinoNumber(L0 + dL) }
            });
            return Respuesta.Crear(dL, "m", "ΔL = L₀·α·ΔT", extras);
        }

    [LatinoExtension("termodinamica.entropiaCambio",
            Args = "calorJ, temperaturaK",
            Help = "Cambio de entropía en un proceso reversible isotérmico.\n\nFórmula: ΔS = Q / T")]
        public static LatinoObject EntropiaCambio(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere calorJ (J) y temperaturaK (K).");
            double Q = ((LatinoNumber)args[0]).Value;
            double T = ((LatinoNumber)args[1]).Value;
            if (T <= 0) throw new RuntimeError(null!, "La temperatura debe estar en Kelvin.");
            string proceso = Q > 0 ? "Aumento de entropía (proceso espontáneo probable)" : Q < 0 ? "Disminución de entropía (requiere trabajo externo)" : "Sin cambio de entropía";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("proceso"), new LatinoString(proceso) }
            });
            return Respuesta.Crear(Q / T, "J/K", "ΔS = Q / T", extras);
        }

}
