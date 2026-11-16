using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class FisicaCuanticaFormulas
{
    [LatinoExtension("fisicaCuantica.modeloBohr", Args = "numeroCuanticoN, numeroAtomicoZ", Help = "Radio, Energía y Velocidad del electrón en un átomo hidrogenoide (Modelo de Bohr).\n\nFórmulas: r = (n²·a₀)/Z | E = -13.6·(Z²/n²)")]
        public static LatinoObject ModeloBohr(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere número cuántico principal (numeroCuanticoN) y número atómico (numeroAtomicoZ).");
            double n = ((LatinoNumber)args[0]).Value;
            double Z = ((LatinoNumber)args[1]).Value;

            if (n < 1 || n % 1 != 0) throw new RuntimeError(null!, "El número cuántico 'n' debe ser un entero positivo mayor a 0.");
            if (Z < 1 || Z % 1 != 0) throw new RuntimeError(null!, "El número atómico 'Z' debe ser un entero positivo mayor a 0.");

            double a0 = 5.29177210903e-11; // Radio de Bohr en metros
            double e_ev = -13.6; // Energía fundamental del Hidrógeno en eV

            double radio = (n * n * a0) / Z;
            double energia = e_ev * (Z * Z) / (n * n);
            double velocidad = (Z * Math.Pow(Constantes.E_CHARGE, 2)) / (2 * Constantes.EPSILON_0 * n * Constantes.H);

            var valores = new Dictionary<string, double>
            {
                { "radio_orbital_m", radio },
                { "energia_eV", energia },
                { "velocidad_tangencial_ms", velocidad }
            };
            return Respuesta.Estructurada(valores, "r = (n²·a₀)/Z | E = -13.6·(Z²/n²) eV | v = (Z·e²)/(2·ε₀·n·h)");
        }

    [LatinoExtension("fisicaCuantica.energiaMasa", Args = "masaKg", Help = "Equivalencia entre masa y energía (Relatividad Especial de Einstein).\n\nFórmula: E = m·c²")]
    public static LatinoObject EnergiaMasa(List<LatinoObject> args)
    {
        if (args.Count < 1) throw new RuntimeError(null!, "Requiere masaKg (kg).");
        double m = ((LatinoNumber)args[0]).Value;
        if (m < 0) throw new RuntimeError(null!, "La masa no puede ser negativa.");
        
        double e = m * Math.Pow(Constantes.C, 2);
        return Respuesta.Crear(e, "J", "E = m·c²");
    }

}
