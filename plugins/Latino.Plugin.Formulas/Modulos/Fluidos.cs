using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class FluidosFormulas
{
    [LatinoExtension("fluidos.reynolds", Args = "densidad, velocidad, diametro, viscosidad", Help = "Calcula el número de Reynolds y su régimen.\n\nFórmula: Re = (ρ·v·D) / μ")]
        public static LatinoObject Reynolds(List<LatinoObject> args)
        {
            if (args.Count < 4) throw new RuntimeError(null!, "Requiere densidad, velocidad, diámetro y viscosidad.");
            double rho = ((LatinoNumber)args[0]).Value;
            double v = ((LatinoNumber)args[1]).Value;
            double d = ((LatinoNumber)args[2]).Value;
            double mu = ((LatinoNumber)args[3]).Value;

            if (mu <= 0) throw new RuntimeError(null!, "La viscosidad debe ser mayor a cero.");
            if (rho <= 0 || d <= 0) throw new RuntimeError(null!, "Densidad y diámetro deben ser positivos.");

            double re = (rho * v * d) / mu;
            string regimen = re < 2000 ? "Laminar" : re > 4000 ? "Turbulento" : "Transicional";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("regimen"), new LatinoString(regimen) }
            });
            return Respuesta.Crear(re, "adimensional", "Re = (ρ·v·D) / μ", extras);
        }

}
