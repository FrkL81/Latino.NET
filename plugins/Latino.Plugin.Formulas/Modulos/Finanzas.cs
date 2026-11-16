using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class FinanzasFormulas
{
    [LatinoExtension("finanzas.interesSimple",
            Args = "capital, tasa_anual, tiempo_anios",
            Help = "Interés simple (sin capitalización).\n\nFórmula: I = C · r · t")]
        public static LatinoObject InteresSimple(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere capital, tasa_anual (decimal, ej: 0.05) y tiempo en años.");
            double C = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            double t = ((LatinoNumber)args[2]).Value;
            if (C < 0) throw new RuntimeError(null!, "El capital no puede ser negativo.");
            if (r < 0) throw new RuntimeError(null!, "La tasa no puede ser negativa.");
            if (t < 0) throw new RuntimeError(null!, "El tiempo no puede ser negativo.");
            double interes = C * r * t;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("monto_final"),      new LatinoNumber(C + interes) },
                { new LatinoString("interes_generado"), new LatinoNumber(interes) },
                { new LatinoString("capital_inicial"),  new LatinoNumber(C) }
            });
            return Respuesta.Crear(interes, "moneda", "I = C · r · t", extras);
        }

    [LatinoExtension("finanzas.interesCompuesto",
            Args = "capital, tasa_anual, tiempo_anios, [periodos_por_anio]",
            Help = "Monto final con capitalización compuesta. periodos_por_anio: 1=anual, 12=mensual, 365=diario.\n\nFórmula: M = C · (1 + r/n)^(n·t)")]
        public static LatinoObject InteresCompuesto(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere capital, tasa_anual, tiempo_anios y opcionalmente periodos_por_anio (defecto: 1).");
            double C = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            double t = ((LatinoNumber)args[2]).Value;
            double n = args.Count > 3 ? ((LatinoNumber)args[3]).Value : 1;
            if (C < 0) throw new RuntimeError(null!, "El capital no puede ser negativo.");
            if (r < 0) throw new RuntimeError(null!, "La tasa no puede ser negativa.");
            if (t < 0) throw new RuntimeError(null!, "El tiempo no puede ser negativo.");
            if (n < 1) throw new RuntimeError(null!, "Los periodos por año deben ser al menos 1.");
            double M = C * Math.Pow(1 + r / n, n * t);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("interes_generado"), new LatinoNumber(M - C) },
                { new LatinoString("capital_inicial"),  new LatinoNumber(C) }
            });
            return Respuesta.Crear(M, "moneda", "M = C · (1 + r/n)^(n·t)", extras);
        }

    [LatinoExtension("finanzas.valorPresente",
            Args = "valor_futuro, tasa_anual, tiempo_anios",
            Help = "Valor Presente: ¿cuánto vale hoy un pago futuro?\n\nFórmula: VP = VF / (1 + r)^t")]
        public static LatinoObject ValorPresente(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere valor_futuro, tasa_anual (decimal) y tiempo_anios.");
            double VF = ((LatinoNumber)args[0]).Value;
            double r  = ((LatinoNumber)args[1]).Value;
            double t  = ((LatinoNumber)args[2]).Value;
            if (r < 0) throw new RuntimeError(null!, "La tasa no puede ser negativa.");
            if (t < 0) throw new RuntimeError(null!, "El tiempo no puede ser negativo.");
            double denominador = Math.Pow(1 + r, t);
            if (Math.Abs(denominador) < 1e-10) throw new RuntimeError(null!, "El denominador es cero: revisa la tasa y el tiempo.");
            return Respuesta.Crear(VF / denominador, "moneda", "VP = VF / (1 + r)^t");
        }

    [LatinoExtension("finanzas.valorFuturo",
            Args = "valor_presente, tasa_anual, tiempo_anios",
            Help = "Valor Futuro de un capital a interés compuesto anual.\n\nFórmula: VF = VP · (1 + r)^t")]
        public static LatinoObject ValorFuturo(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere valor_presente, tasa_anual (decimal) y tiempo_anios.");
            double VP = ((LatinoNumber)args[0]).Value;
            double r  = ((LatinoNumber)args[1]).Value;
            double t  = ((LatinoNumber)args[2]).Value;
            if (r < 0) throw new RuntimeError(null!, "La tasa no puede ser negativa.");
            if (t < 0) throw new RuntimeError(null!, "El tiempo no puede ser negativo.");
            return Respuesta.Crear(VP * Math.Pow(1 + r, t), "moneda", "VF = VP · (1 + r)^t");
        }

    [LatinoExtension("finanzas.amortizacionCuota",
            Args = "capital, tasa_mensual, numero_cuotas",
            Help = "Cuota mensual fija de un préstamo (sistema francés).\n\nFórmula: C = P · [r(1+r)^n] / [(1+r)^n - 1]")]
        public static LatinoObject AmortizacionCuota(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere capital, tasa_mensual (decimal, ej: 0.01) y numero_cuotas.");
            double P = ((LatinoNumber)args[0]).Value;
            double r = ((LatinoNumber)args[1]).Value;
            int    n = (int)((LatinoNumber)args[2]).Value;
            if (P < 0)  throw new RuntimeError(null!, "El capital no puede ser negativo.");
            if (r <= 0) throw new RuntimeError(null!, "La tasa mensual debe ser mayor a cero.");
            if (n <= 0) throw new RuntimeError(null!, "El número de cuotas debe ser mayor a cero.");

            double factorR = Math.Pow(1 + r, n);
            double cuota   = P * (r * factorR) / (factorR - 1);
            double totalPagado = cuota * n;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("total_pagado"),     new LatinoNumber(totalPagado) },
                { new LatinoString("total_intereses"),  new LatinoNumber(totalPagado - P) },
                { new LatinoString("numero_cuotas"),    new LatinoNumber(n) }
            });
            return Respuesta.Crear(cuota, "moneda/mes", "C = P · [r·(1+r)^n] / [(1+r)^n - 1]", extras);
        }

    [LatinoExtension("finanzas.puntoEquilibrio",
            Args = "costos_fijos, precio_venta, costo_variable_unitario",
            Help = "Punto de equilibrio: unidades mínimas para cubrir costos.\n\nFórmula: Q = CF / (PV - CV)")]
        public static LatinoObject PuntoEquilibrio(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere costos_fijos, precio_venta y costo_variable_unitario.");
            double CF = ((LatinoNumber)args[0]).Value;
            double PV = ((LatinoNumber)args[1]).Value;
            double CV = ((LatinoNumber)args[2]).Value;
            if (CF < 0) throw new RuntimeError(null!, "Los costos fijos no pueden ser negativos.");
            double margen = PV - CV;
            if (margen <= 0) throw new RuntimeError(null!, "El precio de venta debe ser mayor al costo variable (margen negativo o cero).");
            double Q = CF / margen;
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("margen_contribucion"), new LatinoNumber(margen) },
                { new LatinoString("ingreso_equilibrio"),  new LatinoNumber(Q * PV) }
            });
            return Respuesta.Crear(Math.Ceiling(Q), "unidades", "Q = CF / (PV - CV)", extras);
        }

}
