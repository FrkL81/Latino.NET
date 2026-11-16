using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class EconomiaFormulas
{
    [LatinoExtension("economia.roi", Args = "gananciaNeta, costoInversion", Help = "Calcula el Retorno de Inversión (ROI).\n\nFórmula: ROI = (Ganancia / Costo) · 100")]
        public static LatinoObject CalcularROI(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere gananciaNeta y costoInversion.");
            double g = ((LatinoNumber)args[0]).Value;
            double c = ((LatinoNumber)args[1]).Value;
            if (c == 0) throw new RuntimeError(null!, "El costo de inversión no puede ser cero.");

            double roi = (g / c) * 100;
            return Respuesta.Crear(roi, "%", "ROI = (Ganancia / Costo) · 100");
        }

}
