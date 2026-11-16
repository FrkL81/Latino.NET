using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

/*
================================================================================
REGLAS ESTRICTAS PARA EL PLUGIN FORMULAS:
================================================================================
1. CONSTANTES UNIVERSALES:
   Se deben usar SOLAMENTE las constantes de la clase interna `Constantes`.
2. FORMATO DE RESPUESTA:
   Obligatorio usar los métodos de la clase `Respuesta`.
3. TIPO DE RETORNO:
   TODA función debe retornar un LatinoDictionary con las siguientes claves base:
   - "resultado" -> El valor numérico principal.
   - "unidad"    -> La unidad de medida o clasificación física como string.
   - "formula"   -> La fórmula en representación simbólica unicode.
   Y todas las claves extra que sean necesarias como diagnóstico, parámetros...
4. SEGURIDAD DE TIPOS Y CASTEO LATINO:
   Se debe usar strict-casting hacia LatinoNumber `((LatinoNumber)args[X]).Value`.
================================================================================
*/

public static class Constantes
{
    public const double G = 9.80665;
    public const double C = 299792458;
    public const double G_UNIV = 6.67430e-11;
    public const double H = 6.62607015e-34;
    public const double K_B = 1.380649e-23;
    public const double R = 8.314462618;
    public const double NA = 6.02214076e23;
    public const double N_A = 6.02214076e23;
    public const double E = 1.602176634e-19;
    public const double E_CHARGE = 1.602176634e-19;
    public const double ME = 9.1093837015e-31;
    public const double M_E = 9.1093837015e-31;
    public const double M_P = 1.67262192369e-27;
    public const double PI = Math.PI;
    public const double K_COULOMB = 8.9875517873681764e9;
    public const double EPSILON_0 = 8.8541878128e-12;
    public const double MU_0 = 1.2566370614359172953850573533118e-6; // 4 * pi * 1e-7
}

public static class Respuesta
{
    public static LatinoDictionary Crear(double resultado, string unidad, string formula, LatinoDictionary? extras = null)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("resultado"), new LatinoNumber(resultado) },
            { new LatinoString("unidad"),    new LatinoString(unidad) },
            { new LatinoString("formula"),   new LatinoString(formula) }
        };
        if (extras != null)
        {
            foreach (var kvp in extras.Pairs)
            {
                dict[kvp.Key] = kvp.Value;
            }
        }
        return new LatinoDictionary(dict);
    }

    public static LatinoDictionary Estructurada(Dictionary<string, double> valores, string formula)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("formula"), new LatinoString(formula) }
        };
        foreach (var kvp in valores)
        {
            dict[new LatinoString(kvp.Key)] = new LatinoNumber(kvp.Value);
        }
        return new LatinoDictionary(dict);
    }

    public static bool TryGetVal(LatinoDictionary d, string key, out double val)
    {
        val = 0;
        if (d.Pairs.TryGetValue(new LatinoString(key), out LatinoObject? obj))
        {
            if (obj is LatinoNumber num)
            {
                val = num.Value;
                return true;
            }
        }
        return false;
    }
}

public static class FormulasPlugin
{
    [LatinoExtension("formulas.constantes", Help = "Devuelve un diccionario con constantes físicas universales.")]
    public static LatinoObject GetConstantes(List<LatinoObject> args)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("g"), new LatinoNumber(Constantes.G) },
            { new LatinoString("G_univ"), new LatinoNumber(Constantes.G_UNIV) },
            { new LatinoString("c"), new LatinoNumber(Constantes.C) },
            { new LatinoString("R"), new LatinoNumber(Constantes.R) },
            { new LatinoString("NA"), new LatinoNumber(Constantes.NA) },
            { new LatinoString("h"), new LatinoNumber(Constantes.H) },
            { new LatinoString("k_B"), new LatinoNumber(Constantes.K_B) },
            { new LatinoString("e_charge"), new LatinoNumber(Constantes.E_CHARGE) },
            { new LatinoString("k_coulomb"), new LatinoNumber(Constantes.K_COULOMB) },
            { new LatinoString("ε₀"), new LatinoNumber(Constantes.EPSILON_0) },
            { new LatinoString("μ₀"), new LatinoNumber(Constantes.MU_0) },
            { new LatinoString("π"), new LatinoNumber(Constantes.PI) },
            { new LatinoString("e"), new LatinoNumber(Constantes.E) },
            { new LatinoString("m_e"), new LatinoNumber(Constantes.ME) }
        };
        return new LatinoDictionary(dict, isReadOnly: true);
    }
}
