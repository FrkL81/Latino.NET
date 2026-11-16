using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class AerodinamicaFormulas
{
    [LatinoExtension("aerodinamica.numeroMach", Args = "velocidadObjeto, velocidadSonido", Help = "Calcula el Número de Mach y el régimen de vuelo.\n\nFórmula: M = v / c")]
        public static LatinoObject NumeroMach(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere la velocidadObjeto y la velocidadSonido local.");
            double v = ((LatinoNumber)args[0]).Value;
            double c = ((LatinoNumber)args[1]).Value;

            if (c <= 0) throw new RuntimeError(null!, "La velocidad del sonido debe ser mayor a cero.");
            if (v < 0) throw new RuntimeError(null!, "La velocidad del objeto no puede ser negativa en este contexto escalar.");

            double mach = v / c;

            string regimen;
            if (mach < 0.8) regimen = "Subsónico";
            else if (mach < 1.2) regimen = "Transónico";
            else if (mach < 5.0) regimen = "Supersónico";
            else regimen = "Hipersónico";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("regimen_vuelo"), new LatinoString(regimen) }
            });

            return Respuesta.Crear(mach, "Mach", "M = v / c", extras);
        }

}
