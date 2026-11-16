using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class HidraulicaFormulas
{

    [LatinoExtension("hidraulica.bernoulli",
            Args = "diccionario_punto1, diccionario_punto2, densidad_kg_m3",
            Help = "Verifica la ecuación de Bernoulli entre dos puntos.\nCada punto: {'P','v','z'}\n\nFórmula: H = P/ρg + v²/(2g) + z")]
        public static LatinoObject Bernoulli(List<LatinoObject> args)
        {
            if (args.Count < 3 || args[0] is not LatinoDictionary p1 || args[1] is not LatinoDictionary p2) 
                throw new RuntimeError(null!, "Requiere diccionario para punto 1, diccionario para punto 2, y densidad del fluido.");
            
            double rho = ((LatinoNumber)args[2]).Value;
            if (rho <= 0) throw new RuntimeError(null!, "La densidad debe ser positiva.");

            Respuesta.TryGetVal(p1, "P", out double P1); Respuesta.TryGetVal(p1, "v", out double v1); Respuesta.TryGetVal(p1, "z", out double z1);
            Respuesta.TryGetVal(p2, "P", out double P2); Respuesta.TryGetVal(p2, "v", out double v2); Respuesta.TryGetVal(p2, "z", out double z2);

            double H1 = P1 / (rho * Constantes.G) + (v1 * v1) / (2 * Constantes.G) + z1;
            double H2 = P2 / (rho * Constantes.G) + (v2 * v2) / (2 * Constantes.G) + z2;

            double diff = Math.Abs(H1 - H2);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("carga_punto_1_m"), new LatinoNumber(H1) },
                { new LatinoString("carga_punto_2_m"), new LatinoNumber(H2) },
                { new LatinoString("diferencia_carga_m"), new LatinoNumber(diff) },
                { new LatinoString("es_ideal"), new LatinoBoolean(diff < 1e-6) }
            });
            return Respuesta.Crear(H1, "m", "H = P/ρg + v²/(2g) + z", extras);
        }

    [LatinoExtension("hidraulica.continuidad",
            Args = "area1_m2, velocidad1_m_s, area2_m2",
            Help = "Velocidad en la sección 2 por la ecuación de continuidad (flujo incompresible).\n\nFórmula: A₁·v₁ = A₂·v₂")]
        public static LatinoObject Continuidad(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere area1 (m²), velocidad1 (m/s) y area2 (m²).");
            double A1 = ((LatinoNumber)args[0]).Value;
            double v1 = ((LatinoNumber)args[1]).Value;
            double A2 = ((LatinoNumber)args[2]).Value;
            if (A1 <= 0 || A2 <= 0) throw new RuntimeError(null!, "Las áreas deben ser mayores a cero.");
            if (v1 < 0) throw new RuntimeError(null!, "La velocidad no puede ser negativa.");
            double v2 = (A1 * v1) / A2;
            double Q  = A1 * v1;
            string efecto = A2 < A1 ? "Sección menor → velocidad aumenta (efecto Venturi)" :
                            A2 > A1 ? "Sección mayor → velocidad disminuye" : "Misma sección";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("caudal_m3_s"), new LatinoNumber(Q) },
                { new LatinoString("efecto"),      new LatinoString(efecto) }
            });
            return Respuesta.Crear(v2, "m/s", "v₂ = (A₁·v₁) / A₂", extras);
        }

    [LatinoExtension("hidraulica.numeroFroude",
            Args = "velocidad_m_s, profundidad_hidraulica_m",
            Help = "Número de Froude para clasificar el régimen de flujo en canal abierto.\n\nFórmula: Fr = v / √(g·D)")]
        public static LatinoObject NumeroFroude(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere velocidad (m/s) y profundidad hidráulica (m).");
            double v = ((LatinoNumber)args[0]).Value;
            double D = ((LatinoNumber)args[1]).Value;
            if (D <= 0) throw new RuntimeError(null!, "La profundidad hidráulica debe ser mayor a cero.");
            if (v < 0) throw new RuntimeError(null!, "La velocidad no puede ser negativa.");
            double Fr = v / Math.Sqrt(Constantes.G * D);
            string regimen = Fr < 1.0 ? "Subcrítico (tranquilo) — ondas se propagan aguas arriba" :
                             Fr > 1.0 ? "Supercrítico (rápido) — ondas NO se propagan aguas arriba" :
                                        "Crítico — condición de diseño especial";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("regimen"), new LatinoString(regimen) }
            });
            return Respuesta.Crear(Fr, "adimensional", "Fr = v / √(g·D)", extras);
        }

    [LatinoExtension("hidraulica.caudalManning",
            Args = "coef_manning, area_hidraulica_m2, radio_hidraulico_m, pendiente",
            Help = "Caudal en canal abierto por la fórmula de Manning.\n\nFórmula: Q = (1/n)·A·R^(2/3)·S^(1/2)")]
        public static LatinoObject CaudalManning(List<LatinoObject> args)
        {
            if (args.Count < 4) throw new RuntimeError(null!, "Requiere coef_manning (n), area (m²), radio_hidraulico (m) y pendiente (m/m).");
            double n = ((LatinoNumber)args[0]).Value;
            double A = ((LatinoNumber)args[1]).Value;
            double R = ((LatinoNumber)args[2]).Value;
            double S = ((LatinoNumber)args[3]).Value;
            if (n <= 0) throw new RuntimeError(null!, "El coeficiente de Manning debe ser mayor a cero (típico: 0.010-0.050).");
            if (A <= 0 || R <= 0) throw new RuntimeError(null!, "Área y radio hidráulico deben ser positivos.");
            if (S < 0) throw new RuntimeError(null!, "La pendiente no puede ser negativa.");
            double Q = (1.0 / n) * A * Math.Pow(R, 2.0 / 3.0) * Math.Sqrt(S);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("velocidad_media_m_s"), new LatinoNumber(Q / A) }
            });
            return Respuesta.Crear(Q, "m³/s", "Q = (1/n)·A·R^(2/3)·S^(1/2)", extras);
        }

    [LatinoExtension("hidraulica.darcyWeisbach",
        Args = "factor_friccion, longitud_m, diametro_m, velocidad_m_s",
        Help = "Pérdida de carga por fricción en tuberías (Darcy-Weisbach).\n\nFórmula: h_f = f · (L/D) · v²/(2g)")]
    public static LatinoObject DarcyWeisbach(List<LatinoObject> args)
    {
        if (args.Count < 4) throw new RuntimeError(null!, "Requiere factor_friccion (f), longitud (m), diametro (m) y velocidad (m/s).");
        double f = ((LatinoNumber)args[0]).Value;
        double L = ((LatinoNumber)args[1]).Value;
        double D = ((LatinoNumber)args[2]).Value;
        double v = ((LatinoNumber)args[3]).Value;

        if (f < 0 || f > 1) throw new RuntimeError(null!, "El factor de fricción debe estar entre 0 y 1 (típico: 0.01-0.05 para flujo turbulento).");
        if (L <= 0) throw new RuntimeError(null!, "La longitud debe ser mayor a cero.");
        if (D <= 0) throw new RuntimeError(null!, "El diámetro debe ser mayor a cero.");
        if (v < 0)  throw new RuntimeError(null!, "La velocidad no puede ser negativa.");

        double hf = f * (L / D) * (v * v) / (2 * Constantes.G);

        // Contexto útil: presión perdida (Pa) además de metros de columna de agua
        double deltaP_agua = hf * 1000 * Constantes.G; // ρ_agua = 1000 kg/m³
        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("presion_perdida_Pa"),    new LatinoNumber(deltaP_agua) },
            { new LatinoString("nota"),                  new LatinoString("Usa reynolds() para calcular f si no lo conoces (Moody).") }
        });
        return Respuesta.Crear(hf, "m", "h_f = f · (L/D) · v²/(2g)", extras);
    }

    [LatinoExtension("hidraulica.hazenWilliams",
        Args = "coef_c, radio_hidraulico_m, pendiente",
        Help = "Velocidad en tuberías por Hazen-Williams (agua a temperatura normal).\nCoeficiente C: PVC=150, Acero nuevo=140, Hierro dúctil=130, Hierro fundido=100, Concreto=120.\n\nFórmula: v = 0.849 · C · R^0.63 · S^0.54")]
    public static LatinoObject HazenWilliams(List<LatinoObject> args)
    {
        if (args.Count < 3) throw new RuntimeError(null!, "Requiere coef_c (coeficiente de rugosidad), radio_hidraulico (m) y pendiente (m/m).");
        double C = ((LatinoNumber)args[0]).Value;
        double R = ((LatinoNumber)args[1]).Value;
        double S = ((LatinoNumber)args[2]).Value;

        if (C <= 0 || C > 160) throw new RuntimeError(null!, "Coeficiente C fuera de rango (típico: 60-150). PVC=150, Hierro fundido=100.");
        if (R <= 0) throw new RuntimeError(null!, "El radio hidráulico debe ser mayor a cero.");
        if (S < 0)  throw new RuntimeError(null!, "La pendiente no puede ser negativa.");
        if (S == 0) throw new RuntimeError(null!, "Pendiente cero implica velocidad cero (flujo sin gradiente).");

        double v = 0.849 * C * Math.Pow(R, 0.63) * Math.Pow(S, 0.54);

        // Clasificar material por coeficiente C
        string material;
        if      (C >= 145) material = "Plástico / PVC (excelente)";
        else if (C >= 130) material = "Acero nuevo o hierro dúctil (muy bueno)";
        else if (C >= 115) material = "Concreto liso / hierro galvanizado (bueno)";
        else if (C >= 90)  material = "Hierro fundido envejecido (aceptable)";
        else               material = "Tubería deteriorada / rugosa (revisar)";

        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("material_referencia"), new LatinoString(material) },
            { new LatinoString("nota"),                new LatinoString("Válida solo para agua a 15-25°C. Usa Darcy-Weisbach para otros fluidos.") }
        });
        return Respuesta.Crear(v, "m/s", "v = 0.849 · C · R^0.63 · S^0.54", extras);
    }
}
