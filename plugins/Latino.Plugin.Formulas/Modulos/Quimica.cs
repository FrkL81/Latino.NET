using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class QuimicaFormulas
{
    [LatinoExtension("quimica.phEstado", Args = "concentracionH", Help = "Calcula el pH y el estado de la solución.\n\nFórmula: pH = -log₁₀[H⁺]")]
        public static LatinoObject PHEstado(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere concentración de iones H+.");
            double h = ((LatinoNumber)args[0]).Value;

            if (h <= 0) throw new RuntimeError(null!, "La concentración debe ser mayor a cero.");

            double ph = -Math.Log10(h);
            string tipo = ph < 7 ? "Ácido" : ph > 7 ? "Básico" : "Neutro";

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("estado"), new LatinoString(tipo) }
            });

            return Respuesta.Crear(ph, "adimensional", "pH = -log₁₀[H⁺]", extras);
        }

    [LatinoExtension("quimica.dilucion", Args = "diccionarioVariables", Help = "Ley de dilución. Pasa 3 de: 'M1', 'V1', 'M2', 'V2'.\n\nFórmula: M₁·V₁ = M₂·V₂")]
        public static LatinoObject Dilucion(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoDictionary conocidas)
                throw new RuntimeError(null!, "Se requiere diccionario con 'M1', 'V1', 'M2', 'V2'.");

            bool tieneM1 = Respuesta.TryGetVal(conocidas, "M1", out double M1);
            bool tieneV1 = Respuesta.TryGetVal(conocidas, "V1", out double V1);
            bool tieneM2 = Respuesta.TryGetVal(conocidas, "M2", out double M2);
            bool tieneV2 = Respuesta.TryGetVal(conocidas, "V2", out double V2);

            if ((tieneM1 && M1 < 0) || (tieneM2 && M2 < 0)) throw new RuntimeError(null!, "Concentración no negativa.");
            if ((tieneV1 && V1 <= 0) || (tieneV2 && V2 <= 0)) throw new RuntimeError(null!, "Volumen estrictamente positivo.");

            int count = (tieneM1 ? 1 : 0) + (tieneV1 ? 1 : 0) + (tieneM2 ? 1 : 0) + (tieneV2 ? 1 : 0);
            if (count != 3) throw new RuntimeError(null!, "Proporciona exactamente 3 variables.");

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
            string f = "M₁·V₁ = M₂·V₂";

            if (!tieneM1) {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("M1");
                return Respuesta.Crear((M2 * V2) / V1, "mol/L", f, extras);
            }
            if (!tieneV1) {
                if (M1 == 0) throw new RuntimeError(null!, "M1 no puede ser cero.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("V1");
                return Respuesta.Crear((M2 * V2) / M1, "L", f, extras);
            }
            if (!tieneM2) {
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("M2");
                return Respuesta.Crear((M1 * V1) / V2, "mol/L", f, extras);
            }
            if (!tieneV2) {
                if (M2 == 0) throw new RuntimeError(null!, "M2 no puede ser cero.");
                extras.Pairs[new LatinoString("incognita")] = new LatinoString("V2");
                return Respuesta.Crear((M1 * V1) / M2, "L", f, extras);
            }

            throw new RuntimeError(null!, "Error en solver químico.");
        }

    [LatinoExtension("quimica.rendimientoReaccion",
            Args = "masaObtenida, masaTeorica",
            Help = "Rendimiento porcentual de una reacción química.\n\nFórmula: R% = (m_obtenida / m_teórica) · 100")]
        public static LatinoObject RendimientoReaccion(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere masaObtenida (g) y masaTeorica (g).");
            double mo = ((LatinoNumber)args[0]).Value;
            double mt = ((LatinoNumber)args[1]).Value;
            if (mt <= 0) throw new RuntimeError(null!, "La masa teórica debe ser mayor a cero.");
            if (mo < 0)  throw new RuntimeError(null!, "La masa obtenida no puede ser negativa.");
            if (mo > mt) throw new RuntimeError(null!, "La masa obtenida no puede superar la teórica (viola la conservación de masa).");
            double r = (mo / mt) * 100;
            string calidad = r > 90 ? "Excelente" : r > 70 ? "Bueno" : r > 50 ? "Aceptable" : "Bajo — revisar condiciones";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("calidad"), new LatinoString(calidad) },
                { new LatinoString("masa_perdida"), new LatinoNumber(mt - mo) }
            });
            return Respuesta.Crear(r, "%", "R% = (m_obtenida / m_teórica) · 100", extras);
        }

    [LatinoExtension("quimica.arrhenius",
            Args = "factorPreexponencial, energiaActivacionJ, temperaturaK",
            Help = "Constante de velocidad de reacción (Ecuación de Arrhenius). R = 8.314 J/(mol·K).\n\nFórmula: k = A·e^(-Ea/RT)")]
        public static LatinoObject Arrhenius(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere factorPreexponencial (A), energiaActivacionJ (J/mol) y temperaturaK (K).");
            double A  = ((LatinoNumber)args[0]).Value;
            double Ea = ((LatinoNumber)args[1]).Value;
            double T  = ((LatinoNumber)args[2]).Value;
            if (T <= 0)  throw new RuntimeError(null!, "La temperatura debe estar en Kelvin.");
            if (Ea < 0)  throw new RuntimeError(null!, "La energía de activación no puede ser negativa.");
            double k = A * Math.Exp(-Ea / (Constantes.R * T));
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("nota"), new LatinoString("Constante de velocidad. Unidades dependen del orden de la reaccion.") }
            });
            return Respuesta.Crear(k, "varía", "k = A·e^(-Ea/RT)", extras);
        }

    [LatinoExtension("quimica.celdaElectroquimica",
            Args = "potencialCatodo, potencialAnodo",
            Help = "Voltaje de celda electroquímica estándar.\n\nFórmula: E°_celda = E°_catodo - E°_anodo")]
        public static LatinoObject CeldaElectroquimica(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere potencialCatodo (V) y potencialAnodo (V) en condiciones estándar.");
            double Ec = ((LatinoNumber)args[0]).Value;
            double Ea = ((LatinoNumber)args[1]).Value;
            double E  = Ec - Ea;
            string tipo = E > 0 ? "Celda galvanica (espontanea, genera energia)" :
                          E < 0 ? "Celda electrolitica (no espontanea, requiere energia)" :
                                  "En equilibrio";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("tipo_celda"),    new LatinoString(tipo) },
                { new LatinoString("espontanea"),    new LatinoBoolean(E > 0) }
            });
            return Respuesta.Crear(E, "V", "E°_celda = E°_cátodo - E°_ánodo", extras);
        }

    [LatinoExtension("quimica.concentracionMolar",
            Args = "molesSoluto, volumenLitros",
            Help = "Concentración molar de una solución.\n\nFórmula: M = n / V")]
        public static LatinoObject ConcentracionMolar(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere molesSoluto (mol) y volumenLitros (L).");
            double n = ((LatinoNumber)args[0]).Value;
            double V = ((LatinoNumber)args[1]).Value;
            if (n < 0) throw new RuntimeError(null!, "Los moles no pueden ser negativos.");
            if (V <= 0) throw new RuntimeError(null!, "El volumen debe ser mayor a cero.");
            return Respuesta.Crear(n / V, "mol/L", "M = n / V");
        }

    [LatinoExtension("quimica.hendersonHasselbalch",
            Args = "pka, concentracionBase, concentracionAcido",
            Help = "pH de una solución buffer (Ecuación de Henderson-Hasselbalch).\n\nFórmula: pH = pKa + log₁₀([A⁻]/[HA])")]
        public static LatinoObject HendersonHasselbalch(List<LatinoObject> args)
        {
            if (args.Count < 3) throw new RuntimeError(null!, "Requiere pKa, concentracionBase [A⁻] (mol/L) y concentracionAcido [HA] (mol/L).");
            double pKa  = ((LatinoNumber)args[0]).Value;
            double cBase = ((LatinoNumber)args[1]).Value;
            double cAcid = ((LatinoNumber)args[2]).Value;
            if (cBase <= 0 || cAcid <= 0) throw new RuntimeError(null!, "Las concentraciones deben ser mayores a cero.");
            double pH   = pKa + Math.Log10(cBase / cAcid);
            double razon = cBase / cAcid;
            string estado = Math.Abs(pH - pKa) < 1 ? "Buffer efectivo (ratio entre 0.1 y 10)" : "Buffer debil — ajustar concentraciones";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("ratio_base_acido"),  new LatinoNumber(razon) },
                { new LatinoString("estado_buffer"),     new LatinoString(estado) }
            });
            return Respuesta.Crear(pH, "adimensional", "pH = pKa + log₁₀([A⁻]/[HA])", extras);
        }

    [LatinoExtension("quimica.entalpiaReaccion",
            Args = "sumaProductosKJ, sumaReactivosKJ",
            Help = "Entalpía estándar de reacción (Ley de Hess).\n\nFórmula: ΔH°rxn = ΣΔH°f(productos) - ΣΔH°f(reactivos)")]
        public static LatinoObject EntalpiaReaccion(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere sumaProductosKJ (kJ/mol) y sumaReactivosKJ (kJ/mol).");
            double Hp = ((LatinoNumber)args[0]).Value;
            double Hr = ((LatinoNumber)args[1]).Value;
            double dH = Hp - Hr;
            string tipo = dH < 0 ? "Exotermica (libera calor)" : dH > 0 ? "Endotermica (absorbe calor)" : "Sin cambio entalpico neto";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("tipo_reaccion"), new LatinoString(tipo) }
            });
            return Respuesta.Crear(dH, "kJ/mol", "ΔH°rxn = ΣΔH°f(prod) - ΣΔH°f(react)", extras);
        }

    [LatinoExtension("quimica.michaelisMenten",
        Args = "diccionarioVariables",
        Help = "Cinética enzimática de Michaelis-Menten. Pasa 3 de: 'v', 'Vmax', 'Km', 'S'.\n\nFórmula: v = (Vmax · [S]) / (Km + [S])")]
    public static LatinoObject MichaelisMenten(List<LatinoObject> args)
    {
        if (args.Count == 0 || args[0] is not LatinoDictionary datos)
            throw new RuntimeError(null!, "Se requiere diccionario con 3 de: 'v' (velocidad), 'Vmax' (velocidad máxima), 'Km' (constante Michaelis), 'S' (concentración sustrato).");

        bool tieneV    = Respuesta.TryGetVal(datos, "v",    out double v);
        bool tieneVmax = Respuesta.TryGetVal(datos, "Vmax", out double Vmax);
        bool tieneKm   = Respuesta.TryGetVal(datos, "Km",   out double Km);
        bool tieneS    = Respuesta.TryGetVal(datos, "S",    out double S);

        // Validaciones físicas
        if (tieneVmax && Vmax <= 0) throw new RuntimeError(null!, "Vmax debe ser mayor a cero.");
        if (tieneKm   && Km   <= 0) throw new RuntimeError(null!, "Km debe ser mayor a cero (constante de afinidad enzimática).");
        if (tieneS    && S    <  0) throw new RuntimeError(null!, "La concentración de sustrato no puede ser negativa.");
        if (tieneV    && v    <  0) throw new RuntimeError(null!, "La velocidad de reacción no puede ser negativa.");

        int count = (tieneV ? 1 : 0) + (tieneVmax ? 1 : 0) + (tieneKm ? 1 : 0) + (tieneS ? 1 : 0);
        if (count != 3) throw new RuntimeError(null!, "Proporciona exactamente 3 variables para despejar la cuarta.");

        var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance));
        const string f = "v = (Vmax · [S]) / (Km + [S])";

        if (!tieneV) {
            // Calcular v
            extras.Pairs[new LatinoString("incognita")]  = new LatinoString("v");
            double saturacion = S / (Km + S) * 100;
            extras.Pairs[new LatinoString("saturacion_pct")] = new LatinoNumber(saturacion);
            extras.Pairs[new LatinoString("regimen")] = new LatinoString(
                saturacion > 90 ? "Saturación (enzima al límite — añadir más sustrato no acelera)" :
                saturacion > 50 ? "Régimen mixto" :
                                  "Orden 1 aparente (velocidad proporcional a [S])");
            return Respuesta.Crear((Vmax * S) / (Km + S), "mismas unidades que Vmax", f, extras);
        }
        if (!tieneVmax) {
            // Despejar Vmax: v = Vmax·S/(Km+S) → Vmax = v·(Km+S)/S
            if (S <= 0) throw new RuntimeError(null!, "S debe ser mayor a cero para calcular Vmax.");
            extras.Pairs[new LatinoString("incognita")] = new LatinoString("Vmax");
            return Respuesta.Crear(v * (Km + S) / S, "mismas unidades que v", "Vmax = v·(Km+[S])/[S]", extras);
        }
        if (!tieneKm) {
            // Despejar Km: Km = S·(Vmax - v) / v
            if (v <= 0) throw new RuntimeError(null!, "v debe ser mayor a cero para calcular Km.");
            if (v >= Vmax) throw new RuntimeError(null!, "v no puede ser mayor o igual a Vmax (imposible físicamente).");
            extras.Pairs[new LatinoString("incognita")] = new LatinoString("Km");
            return Respuesta.Crear(S * (Vmax - v) / v, "mismas unidades que S", "Km = [S]·(Vmax - v) / v", extras);
        }
        if (!tieneS) {
            // Despejar S: S = Km·v / (Vmax - v)
            if (v <= 0) throw new RuntimeError(null!, "v debe ser mayor a cero para calcular [S].");
            if (v >= Vmax) throw new RuntimeError(null!, "v no puede ser mayor o igual a Vmax (imposible físicamente).");
            extras.Pairs[new LatinoString("incognita")] = new LatinoString("S");
            return Respuesta.Crear(Km * v / (Vmax - v), "mismas unidades que Km", "[S] = Km·v / (Vmax - v)", extras);
        }

        throw new RuntimeError(null!, "Error en solver Michaelis-Menten.");
    }
}
