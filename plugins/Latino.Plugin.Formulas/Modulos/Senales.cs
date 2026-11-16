using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class SenalesFormulas
{
    [LatinoExtension("senales.decibelesPotencia",
            Args = "potencia_salida, potencia_entrada",
            Help = "Nivel en decibeles para relación de potencias.\n\nFórmula: dB = 10·log₁₀(P_out / P_in)")]
        public static LatinoObject DecibelesP(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere potencia_salida (W) y potencia_entrada (W).");
            double Pout = ((LatinoNumber)args[0]).Value;
            double Pin  = ((LatinoNumber)args[1]).Value;
            if (Pout <= 0 || Pin <= 0) throw new RuntimeError(null!, "Las potencias deben ser mayores a cero.");
            double db = 10.0 * Math.Log10(Pout / Pin);
            string descripcion = db > 0 ? "Ganancia" : db < 0 ? "Atenuacion" : "Sin cambio";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("tipo"), new LatinoString(descripcion) }
            });
            return Respuesta.Crear(db, "dB", "dB = 10·log₁₀(P_out / P_in)", extras);
        }

    [LatinoExtension("senales.decibelesVoltaje",
            Args = "voltaje_salida, voltaje_entrada",
            Help = "Nivel en decibeles para relación de voltajes (o amplitudes).\n\nFórmula: dBV = 20·log₁₀(V_out / V_in)")]
        public static LatinoObject DecibelesV(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere voltaje_salida (V) y voltaje_entrada (V).");
            double Vout = ((LatinoNumber)args[0]).Value;
            double Vin  = ((LatinoNumber)args[1]).Value;
            if (Vout <= 0 || Vin <= 0) throw new RuntimeError(null!, "Los voltajes deben ser mayores a cero.");
            return Respuesta.Crear(20.0 * Math.Log10(Vout / Vin), "dBV", "dBV = 20·log₁₀(V_out / V_in)");
        }

    [LatinoExtension("senales.frecuenciaNyquist",
            Args = "frecuencia_maxima_senal",
            Help = "Frecuencia mínima de muestreo para evitar aliasing (Teorema de Nyquist-Shannon).\n\nFórmula: fs_min = 2·f_max")]
        public static LatinoObject FrecuenciaNyquist(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere la frecuencia máxima de la señal (Hz).");
            double fMax = ((LatinoNumber)args[0]).Value;
            if (fMax <= 0) throw new RuntimeError(null!, "La frecuencia máxima debe ser positiva.");
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("frecuencia_nyquist_Hz"), new LatinoNumber(fMax) },
                { new LatinoString("nota"), new LatinoString("Muestrear POR ENCIMA de este valor (no igual) garantiza reconstruccion perfecta.") }
            });
            return Respuesta.Crear(2.0 * fMax, "Hz", "fs_min = 2·f_max", extras);
        }

    [LatinoExtension("senales.snr",
            Args = "potencia_senal, potencia_ruido",
            Help = "Relación Señal-Ruido (Signal-to-Noise Ratio).\n\nFórmula: SNR = 10·log₁₀(P_señal / P_ruido)")]
        public static LatinoObject SNR(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere potencia_senal (W) y potencia_ruido (W).");
            double Ps = ((LatinoNumber)args[0]).Value;
            double Pn = ((LatinoNumber)args[1]).Value;
            if (Ps <= 0 || Pn <= 0) throw new RuntimeError(null!, "Las potencias deben ser positivas.");
            double snr = 10.0 * Math.Log10(Ps / Pn);

            string calidad;
            if      (snr > 40) calidad = "Excelente";
            else if (snr > 25) calidad = "Buena";
            else if (snr > 10) calidad = "Aceptable";
            else               calidad = "Mala — alto ruido";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("calidad_senal"), new LatinoString(calidad) }
            });
            return Respuesta.Crear(snr, "dB", "SNR = 10·log₁₀(P_s / P_n)", extras);
        }

    [LatinoExtension("senales.capacidadCanalShannon",
            Args = "ancho_banda_Hz, snr_lineal",
            Help = "Capacidad máxima teórica de un canal con ruido (Teorema de Shannon-Hartley).\n\nFórmula: C = B·log₂(1 + SNR)")]
        public static LatinoObject CapacidadCanalShannon(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere ancho_banda (Hz) y snr_lineal (relacion, NO en dB).");
            double B   = ((LatinoNumber)args[0]).Value;
            double snr = ((LatinoNumber)args[1]).Value;
            if (B <= 0)   throw new RuntimeError(null!, "El ancho de banda debe ser positivo.");
            if (snr < 0)  throw new RuntimeError(null!, "El SNR lineal no puede ser negativo.");
            double C = B * Math.Log(1 + snr, 2);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("capacidad_Mbps"), new LatinoNumber(C / 1e6) },
                { new LatinoString("nota"), new LatinoString("Limite teorico maximo. Los protocolos reales usan ~70% de esto.") }
            });
            return Respuesta.Crear(C, "bps", "C = B·log₂(1 + SNR)", extras);
        }

    [LatinoExtension("senales.anchoBandaModulacionAM",
            Args = "frecuencia_portadora_Hz, frecuencia_maxima_audio_Hz",
            Help = "Ancho de banda de una señal AM (doble banda lateral).\n\nFórmula: BW = 2·f_m")]
        public static LatinoObject AnchoBandaAM(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere frecuencia_portadora (Hz) y frecuencia_maxima_audio (Hz).");
            double fc = ((LatinoNumber)args[0]).Value;
            double fm = ((LatinoNumber)args[1]).Value;
            if (fc <= 0 || fm <= 0) throw new RuntimeError(null!, "Las frecuencias deben ser positivas.");
            if (fm >= fc) throw new RuntimeError(null!, "La frecuencia del audio debe ser menor que la portadora.");
            double bw = 2 * fm;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("frecuencia_superior_Hz"), new LatinoNumber(fc + fm) },
                { new LatinoString("frecuencia_inferior_Hz"), new LatinoNumber(fc - fm) }
            });
            return Respuesta.Crear(bw, "Hz", "BW_AM = 2·f_m", extras);
        }

}
