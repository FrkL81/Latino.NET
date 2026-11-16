using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class EstructurasFormulas
{
    [LatinoExtension("estructuras.tensionNormal",
            Args = "fuerza_N, area_m2",
            Help = "Tensión (esfuerzo) normal en una sección transversal.\n\nFórmula: σ = F / A")]
        public static LatinoObject TensionNormal(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere fuerza (N) y area de la sección (m²).");
            double F = ((LatinoNumber)args[0]).Value;
            double A = ((LatinoNumber)args[1]).Value;
            if (A <= 0) throw new RuntimeError(null!, "El área de la sección debe ser mayor a cero.");
            return Respuesta.Crear(F / A, "Pa", "σ = F / A");
        }

    [LatinoExtension("estructuras.moduloYoung",
            Args = "diccionario_variables",
            Help = "Módulo de Young (elasticidad). Omni-Solver: pasa 2 de 'E', 'sigma', 'epsilon'.\n\nFórmula: E = σ / ε")]
        public static LatinoObject ModuloYoung(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary datos)
                throw new RuntimeError(null!, "Se requiere diccionario con 2 de: 'E' (Pa), 'sigma' (Pa), 'epsilon' (adimensional).");

            bool tieneE   = Respuesta.TryGetVal(datos, "E",       out double E);
            bool tieneSig = Respuesta.TryGetVal(datos, "sigma",   out double sigma);
            bool tieneEps = Respuesta.TryGetVal(datos, "epsilon", out double epsilon);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));

            if (tieneSig && tieneEps) {
                if (Math.Abs(epsilon) < 1e-15) throw new RuntimeError(null!, "La deformación (epsilon) no puede ser cero.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("E");
                return Respuesta.Crear(sigma / epsilon, "Pa", "E = σ / ε", extras);
            }
            if (tieneE && tieneEps) {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("sigma");
                return Respuesta.Crear(E * epsilon, "Pa", "σ = E · ε", extras);
            }
            if (tieneE && tieneSig) {
                if (E <= 0) throw new RuntimeError(null!, "El módulo de Young debe ser mayor a cero.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("epsilon");
                return Respuesta.Crear(sigma / E, "adimensional", "ε = σ / E", extras);
            }
            throw new RuntimeError(null!, "Proporciona exactamente 2 de: E, sigma, epsilon.");
        }

    [LatinoExtension("estructuras.factorSeguridad",
            Args = "resistencia_material, tension_aplicada",
            Help = "Factor de seguridad estructural. Mínimo recomendado: 1.5 (estático), 3.0 (dinámico).\n\nFórmula: FS = σ_resistencia / σ_aplicada")]
        public static LatinoObject FactorSeguridad(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere resistencia del material (Pa) y tensión aplicada (Pa).");
            double sigR = ((LatinoNumber)args[0]).Value;
            double sigA = ((LatinoNumber)args[1]).Value;
            if (sigR <= 0) throw new RuntimeError(null!, "La resistencia del material debe ser positiva.");
            if (sigA <= 0) throw new RuntimeError(null!, "La tensión aplicada debe ser positiva.");
            double fs = sigR / sigA;

            string diagnostico;
            if      (fs < 1.0) diagnostico = "FALLO INMINENTE — la pieza ya está cediendo";
            else if (fs < 1.5) diagnostico = "PELIGROSO — insuficiente para carga estática";
            else if (fs < 2.0) diagnostico = "Aceptable para cargas estáticas conocidas";
            else if (fs < 3.0) diagnostico = "Bueno — adecuado para la mayoria de aplicaciones";
            else               diagnostico = "Conservador — adecuado para cargas dinamicas o inciertas";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("diagnostico"), new LatinoString(diagnostico) }
            });
            return Respuesta.Crear(fs, "adimensional", "FS = σ_resist / σ_aplicada", extras);
        }

    [LatinoExtension("estructuras.vigaFlechaMax",
            Args = "carga_distribuida_N_m, longitud_m, modulo_young_Pa, inercia_m4",
            Help = "Flecha máxima de una viga simplemente apoyada con carga distribuida uniforme.\n\nFórmula: δ_max = (5·w·L⁴) / (384·E·I)")]
        public static LatinoObject VigaFlechaMax(List<LatinoObject> args)
        {
            if (args.Count < 4) throw new RuntimeError(null!, "Requiere carga_distribuida (N/m), longitud (m), modulo_young (Pa) y momento_inercia (m⁴).");
            double w = ((LatinoNumber)args[0]).Value;
            double L = ((LatinoNumber)args[1]).Value;
            double E = ((LatinoNumber)args[2]).Value;
            double I = ((LatinoNumber)args[3]).Value;
            if (L <= 0) throw new RuntimeError(null!, "La longitud debe ser mayor a cero.");
            if (E <= 0) throw new RuntimeError(null!, "El módulo de Young debe ser mayor a cero.");
            if (I <= 0) throw new RuntimeError(null!, "El momento de inercia debe ser mayor a cero.");
            double delta = (5 * w * Math.Pow(L, 4)) / (384 * E * I);
            double limiteL300 = L / 300.0; // Límite típico en construcción (L/300)
            string estado = delta <= limiteL300 ? "Dentro del limite L/300 (aceptable)" : "Supera L/300 — revisar sección transversal";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("limite_L_300_m"),  new LatinoNumber(limiteL300) },
                { new LatinoString("estado"),          new LatinoString(estado) }
            });
            return Respuesta.Crear(delta, "m", "δ_max = (5·w·L⁴) / (384·E·I)", extras);
        }

    [LatinoExtension("estructuras.momentoInerciaRectangulo",
            Args = "base, altura",
            Help = "Momento de inercia de una sección rectangular respecto al eje centroidal.\n\nFórmula: I = (b·h³) / 12")]
        public static LatinoObject MomentoInerciaRectangulo(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere base (m) y altura (m) de la sección transversal.");
            double b = ((LatinoNumber)args[0]).Value;
            double h = ((LatinoNumber)args[1]).Value;
            if (b <= 0 || h <= 0) throw new RuntimeError(null!, "Base y altura deben ser mayores a cero.");
            var valores = new Dictionary<string, double>
            {
                { "inercia_xx",   (b * Math.Pow(h, 3)) / 12.0 },   // Eje horizontal (el relevante para flexión)
                { "inercia_yy",   (h * Math.Pow(b, 3)) / 12.0 },   // Eje vertical
                { "modulo_resistente_xx", (b * Math.Pow(h, 3)) / 12.0 / (h / 2.0) }
            };
            return Respuesta.Estructurada(valores, "I_xx = (b·h³) / 12 | I_yy = (h·b³) / 12");
        }

    [LatinoExtension("estructuras.pandeoEuler",
        Args = "modulo_young_Pa, inercia_m4, longitud_m, condicion_apoyo",
        Help = "Carga crítica de pandeo de Euler para columnas.\ncondicion_apoyo: 'pp' (ambos articulados, Le=L), 'ef' (empotrado-libre, Le=2L), 'ep' (empotrado-articulado, Le=0.7L), 'ee' (ambos empotrados, Le=0.5L).\n\nFórmula: P_cr = π²·E·I / Le²")]
    public static LatinoObject PandeoEuler(List<LatinoObject> args)
    {
        if (args.Count < 4) throw new RuntimeError(null!, "Requiere modulo_young (Pa), inercia (m⁴), longitud (m) y condicion_apoyo ('pp','ef','ep','ee').");
        double E  = ((LatinoNumber)args[0]).Value;
        double I  = ((LatinoNumber)args[1]).Value;
        double L  = ((LatinoNumber)args[2]).Value;
        string cond = args[3].ToString().ToLower().Trim();

        if (E <= 0) throw new RuntimeError(null!, "El módulo de Young debe ser mayor a cero.");
        if (I <= 0) throw new RuntimeError(null!, "El momento de inercia debe ser mayor a cero.");
        if (L <= 0) throw new RuntimeError(null!, "La longitud debe ser mayor a cero.");

        double k; // Factor de longitud efectiva
        string descripcion;
        switch (cond)
        {
            case "pp":
                k = 1.0;
                descripcion = "Articulado-Articulado (ambos extremos rotan libremente). Le = L.";
                break;
            case "ef":
                k = 2.0;
                descripcion = "Empotrado-Libre (voladizo). Le = 2L. Caso más desfavorable.";
                break;
            case "ep":
                k = 0.7;
                descripcion = "Empotrado-Articulado. Le = 0.7L.";
                break;
            case "ee":
                k = 0.5;
                descripcion = "Empotrado-Empotrado (ambos extremos fijos). Le = 0.5L. Caso más favorable.";
                break;
            default:
                throw new RuntimeError(null!, "Condición de apoyo inválida. Usa: 'pp' (art-art), 'ef' (emp-libre), 'ep' (emp-art), 'ee' (emp-emp).");
        }

        double Le  = k * L;
        double Pcr = (Constantes.PI * Constantes.PI * E * I) / (Le * Le);

        string advertencia = k >= 1.5
            ? "ADVERTENCIA: Condición de apoyo desfavorable. Considera refuerzo lateral o reducir longitud libre."
            : "Condición de apoyo favorable. Verifica con factor de seguridad >= 3 para cargas dinámicas.";

        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("longitud_efectiva_m"),   new LatinoNumber(Le) },
            { new LatinoString("condicion"),             new LatinoString(descripcion) },
            { new LatinoString("advertencia"),           new LatinoString(advertencia) }
        });
        return Respuesta.Crear(Pcr, "N", "P_cr = π²·E·I / Le²", extras);
    }

    [LatinoExtension("estructuras.vonMises",
        Args = "sigma_x_Pa, sigma_y_Pa, tau_xy_Pa",
        Help = "Criterio de falla de Von Mises para materiales dúctiles (estado plano de tensiones).\n\nFórmula: σ_v = √(σx² - σx·σy + σy² + 3·τxy²)")]
    public static LatinoObject VonMises(List<LatinoObject> args)
    {
        if (args.Count < 3) throw new RuntimeError(null!, "Requiere sigma_x (Pa), sigma_y (Pa) y tau_xy (Pa). Para tensión uniaxial: sigma_y=0, tau_xy=0.");
        double sx  = ((LatinoNumber)args[0]).Value;
        double sy  = ((LatinoNumber)args[1]).Value;
        double txy = ((LatinoNumber)args[2]).Value;

        double sv = Math.Sqrt(sx * sx - sx * sy + sy * sy + 3 * txy * txy);

        // El criterio solo cobra sentido cuando se compara con el límite elástico (Sy)
        // Incluimos la fórmula y dejamos que el usuario compare con estructuras.factor_seguridad
        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
            { new LatinoString("uso"),
              new LatinoString("Compara este resultado con el límite elástico del material (Sy). " +
                               "Usa estructuras.factor_seguridad(Sy, σ_von_mises) para el FS.") },
            { new LatinoString("nota"),
              new LatinoString("Válido para materiales dúctiles (acero, aluminio). Para frágiles, usar Rankine.") }
        });
        return Respuesta.Crear(sv, "Pa", "σ_v = √(σx² - σx·σy + σy² + 3·τxy²)", extras);
    }
}
