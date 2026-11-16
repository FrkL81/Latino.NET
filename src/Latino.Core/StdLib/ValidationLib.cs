using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class ValidationLib
{
    // Casos dinámicos que usan SysLib.RegexTimeout para seguridad
    private static bool Match(string input, string pattern, RegexOptions options = RegexOptions.None)
    {
        try
        {
            return Regex.IsMatch(input, pattern, options, SysLib.RegexTimeout);
        }
        catch (RegexMatchTimeoutException)
        {
            return false; // En validación, el timeout se trata como no-match
        }
    }

    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        
        void Def(string name, Func<List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc, pams);
            methods[name] = func;
        }

        // =====================================================================
        // VALIDACIONES DE FORMATO
        // =====================================================================
        
        Def("email", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            return new LatinoBoolean(Match(texto.Trim(), @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase));
        }, 1, "Valida formato de correo electrónico (usuario@dominio.ext).", "texto");
        
        Def("url", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            return new LatinoBoolean(Match(texto.Trim(), @"^https?://(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&/=]*)$", RegexOptions.IgnoreCase));
        }, 1, "Valida formato de URL (http:// o https://).", "texto");
        
        Def("ipv4", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            return new LatinoBoolean(Match(texto.Trim(), @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"));
        }, 1, "Valida dirección IPv4 (ej: 192.168.1.1).", "texto");
        
        Def("telefono", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            // Remover espacios excesivos para validación más flexible
            string limpio = System.Text.RegularExpressions.Regex.Replace(
                texto.Trim(), @"\s+", " "
            );
            
            return new LatinoBoolean(Match(limpio, @"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,4}[-\s\.]?[0-9]{1,9}$"));
        }, 1, "Valida formato telefónico flexible (soporta +, -, espacios, paréntesis).", "texto");
        
        // =====================================================================
        // VALIDACIONES DE TIPO
        // =====================================================================
        
        Def("numero", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            // Intentar parsear como número (entero o decimal)
            return new LatinoBoolean(
                double.TryParse(
                    texto.Trim(), 
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out _
                )
            );
        }, 1, "Verifica si el texto representa un número válido (entero o decimal).", "texto");
        
        Def("entero", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            // Solo enteros (sin decimales)
            return new LatinoBoolean(
                long.TryParse(texto.Trim(), out _)
            );
        }, 1, "Verifica si el texto representa un número entero.", "texto");
        
        Def("alfanumerico", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            return new LatinoBoolean(Match(texto, @"^[a-zA-Z0-9]+$"));
        }, 1, "Verifica si el texto contiene solo letras y números (sin símbolos).", "texto");
        
        // =====================================================================
        // VALIDACIONES ESPECIALES
        // =====================================================================
        
        Def("esPalindromo", args => {
            string? valor = GetString(args[0]);
            if (valor == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrEmpty(valor))
                return new LatinoBoolean(true); // Vacío es palíndromo vacuamente
            
            // Normalizar: remover espacios y convertir a minúsculas
            string normalizado = valor.Replace(" ", "").ToLowerInvariant();
            
            // Comparar con versión invertida
            char[] chars = normalizado.ToCharArray();
            Array.Reverse(chars);
            string invertido = new string(chars);
            
            return new LatinoBoolean(normalizado == invertido);
        }, 1, "Verifica si el valor se lee igual al derecho y al revés (ignora espacios y mayúsculas).", "valor");
        
        Def("longitud", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);

            // Blindar argumentos numéricos contra nulos y tipos incorrectos
            if (args[1] is not LatinoNumber minNum || args[2] is not LatinoNumber maxNum)
                throw new RuntimeError(null!, 
                    "validar.longitud() requiere que min y max sean números.");

            int min = (int)minNum.Value;
            int max = (int)maxNum.Value;

            if (min < 0 || max < 0)
                throw new RuntimeError(null!, 
                    "validar.longitud() requiere valores mínimo y máximo no negativos.");

            if (min > max)
                throw new RuntimeError(null!, 
                    $"validar.longitud(): El mínimo ({min}) no puede ser mayor que el máximo ({max}).");

            int len = texto.Length;
            return new LatinoBoolean(len >= min && len <= max);
        }, 3, "Verifica si la longitud del texto está en el rango [min, max].", "texto, min, max");
        
        // =====================================================================
        // VALIDACIONES DE DOCUMENTO (BONUS - Específicas de región)
        // =====================================================================
        
        Def("cedula", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            return new LatinoBoolean(EsFormatoCedulaValido(texto.Trim()));
        }, 1, "Valida formato estructural de cédula panameña (personal, extranjero, naturalizado). Acepta con o sin guiones.", "texto");
        
        Def("analizarCedula", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return BuildInvalidResult("Valor nulo");

            string entrada = texto.Trim();
            return AnalizarCedula(entrada);
        }, 1, "Analiza y clasifica una cédula panameña. Retorna diccionario con: valida, tipo, provincia, tomo, asiento, normalizada.", "texto");
        
        Def("ruc", args => {
            string? raw = GetString(args[0]);
            if (raw == null) return new LatinoBoolean(false);

            string texto = raw.Replace("-", "").Trim();
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            // Verificar que solo contenga dígitos y letras (algunos RUC tienen sufijos)
            if (!texto.All(c => char.IsLetterOrDigit(c)))
                return new LatinoBoolean(false);
            
            // Longitud típica de RUC panameño (sin guiones): 9-15 caracteres
            int len = texto.Length;
            return new LatinoBoolean(len >= 9 && len <= 15);
        }, 1, "Valida formato básico de RUC panameño (9-15 caracteres alfanuméricos).", "texto");
        
        // =====================================================================
        // VALIDACIONES DE CONTRASEÑA (BONUS)
        // =====================================================================
        
        Def("contrasenaSegura", args => {
            string? texto = GetString(args[0]);
            if (texto == null) return new LatinoBoolean(false);
            
            if (string.IsNullOrWhiteSpace(texto))
                return new LatinoBoolean(false);
            
            // Criterios de seguridad:
            // - Mínimo 8 caracteres
            // - Al menos 1 mayúscula
            // - Al menos 1 minúscula
            // - Al menos 1 dígito
            // - Al menos 1 símbolo
            
            bool longitudOk = texto.Length >= 8;
            bool tieneMayuscula = texto.Any(char.IsUpper);
            bool tieneMinuscula = texto.Any(char.IsLower);
            bool tieneDigito = texto.Any(char.IsDigit);
            bool tieneSimbolo = texto.Any(c => !char.IsLetterOrDigit(c));
            
            bool esSegura = longitudOk && tieneMayuscula && tieneMinuscula && 
                           tieneDigito && tieneSimbolo;
            
            return new LatinoBoolean(esSegura);
        }, 1, "Verifica si la contraseña es segura (≥8 chars, mayúscula, minúscula, dígito, símbolo).", "texto");
        
        // Registrar módulo
        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("validar", modulo);
    }
    
    /// <summary>
    /// Extrae el texto del objeto. Retorna null si el objeto es LatinoNull.
    /// Centraliza la validación de nulos para toda la librería.
    /// </summary>
    /// <summary>
    /// Tipos de cédula panameña reconocidos.
    /// </summary>
    private static readonly Dictionary<string, string> PrefijosTipo = new(StringComparer.OrdinalIgnoreCase)
    {
        { "PE", "Panameño nacido en el extranjero" },
        { "E",  "Extranjero naturalizado"          },
        { "N",  "Naturalizado"                     },
    };

    /// <summary>
    /// Valida si el texto tiene una estructura de cédula panameña reconocida.
    /// Acepta con o sin guiones. Ej: "4-123-0044", "PE-12-3456", "E-8-12345"
    /// </summary>
    private static bool EsFormatoCedulaValido(string entrada)
    {
        // Normalizar: separar por guiones para análisis estructural
        var partes = entrada.Split('-');

        // Cédula personal: PROVINCIA-TOMO-ASIENTO  (3 partes numéricas)
        // Provincia: 1-2 dígitos (1-13 provincias + comarcas hasta 99)
        // Tomo: 1-4 dígitos
        // Asiento: 1-6 dígitos
        if (partes.Length == 3)
        {
            // ¿Primera parte es un prefijo de letra conocido?
            if (PrefijosTipo.ContainsKey(partes[0]))
            {
                // Tipo especial: PE-TOMO-ASIENTO o E-TOMO-ASIENTO
                return IsDigitsOnly(partes[1]) && partes[1].Length >= 1 && partes[1].Length <= 4
                    && IsDigitsOnly(partes[2]) && partes[2].Length >= 1 && partes[2].Length <= 6;
            }

            // Tipo personal: PROVINCIA-TOMO-ASIENTO
            return IsDigitsOnly(partes[0]) && partes[0].Length >= 1 && partes[0].Length <= 2
                && IsDigitsOnly(partes[1]) && partes[1].Length >= 1 && partes[1].Length <= 4
                && IsDigitsOnly(partes[2]) && partes[2].Length >= 1 && partes[2].Length <= 6;
        }

        // Sin guiones: solo aceptamos secuencias numéricas de 7 a 12 dígitos
        // (longitud razonable de una cédula personal normalizada sin guiones)
        if (partes.Length == 1 && IsDigitsOnly(entrada))
            return entrada.Length >= 7 && entrada.Length <= 12;

        return false;
    }

    /// <summary>
    /// Analiza la estructura de una cédula y retorna un diccionario con sus componentes.
    /// </summary>
    private static LatinoDictionary AnalizarCedula(string entrada)
    {
        var partes = entrada.Split('-');
        var dict   = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);

        void Set(string k, LatinoObject v) => dict[new LatinoString(k)] = v;

        if (partes.Length == 3)
        {
            string tipoClave = partes[0].ToUpper();

            // Prefijo de letra (PE, E, N, etc.)
            if (PrefijosTipo.ContainsKey(tipoClave))
            {
                bool tomoOk   = IsDigitsOnly(partes[1]) && partes[1].Length >= 1 && partes[1].Length <= 4;
                bool asientoOk = IsDigitsOnly(partes[2]) && partes[2].Length >= 1 && partes[2].Length <= 6;
                bool valida   = tomoOk && asientoOk;

                Set("valida",      new LatinoBoolean(valida));
                Set("tipo",        new LatinoString(PrefijosTipo[tipoClave]));
                Set("provincia",   new LatinoString(tipoClave));
                Set("tomo",        valida ? (LatinoObject)new LatinoString(partes[1]) : LatinoNull.Instance);
                Set("asiento",     valida ? (LatinoObject)new LatinoString(partes[2]) : LatinoNull.Instance);
                Set("normalizada", valida ? new LatinoString($"{tipoClave}-{partes[1]}-{partes[2]}") : LatinoNull.Instance);
                return new LatinoDictionary(dict);
            }

            // Cédula personal: PROVINCIA-TOMO-ASIENTO
            bool provOk    = IsDigitsOnly(partes[0]) && partes[0].Length >= 1 && partes[0].Length <= 2;
            bool tomoOk2   = IsDigitsOnly(partes[1]) && partes[1].Length >= 1 && partes[1].Length <= 4;
            bool asientoOk2 = IsDigitsOnly(partes[2]) && partes[2].Length >= 1 && partes[2].Length <= 6;
            bool validaP   = provOk && tomoOk2 && asientoOk2;

            Set("valida",      new LatinoBoolean(validaP));
            Set("tipo",        new LatinoString("Panameño"));
            Set("provincia",   validaP ? (LatinoObject)new LatinoString(partes[0]) : LatinoNull.Instance);
            Set("tomo",        validaP ? (LatinoObject)new LatinoString(partes[1]) : LatinoNull.Instance);
            Set("asiento",     validaP ? (LatinoObject)new LatinoString(partes[2]) : LatinoNull.Instance);
            Set("normalizada", validaP ? new LatinoString($"{partes[0]}-{partes[1]}-{partes[2]}") : LatinoNull.Instance);
            return new LatinoDictionary(dict);
        }

        // Sin guiones: solo podemos confirmar si el rango es plausible
        if (partes.Length == 1 && IsDigitsOnly(entrada) && entrada.Length >= 7 && entrada.Length <= 12)
        {
            Set("valida",      new LatinoBoolean(true));
            Set("tipo",        new LatinoString("Panameño (sin guiones, estructura no verificable)"));
            Set("provincia",   LatinoNull.Instance);
            Set("tomo",        LatinoNull.Instance);
            Set("asiento",     LatinoNull.Instance);
            Set("normalizada", new LatinoString(entrada));
            return new LatinoDictionary(dict);
        }

        return BuildInvalidResult("Formato no reconocido");
    }

    private static LatinoDictionary BuildInvalidResult(string motivo)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            [new LatinoString("valida")]      = new LatinoBoolean(false),
            [new LatinoString("tipo")]        = new LatinoString($"Inválido: {motivo}"),
            [new LatinoString("provincia")]   = LatinoNull.Instance,
            [new LatinoString("tomo")]        = LatinoNull.Instance,
            [new LatinoString("asiento")]     = LatinoNull.Instance,
            [new LatinoString("normalizada")] = LatinoNull.Instance,
        };
        return new LatinoDictionary(dict);
    }

    private static bool IsDigitsOnly(string s) =>
        !string.IsNullOrEmpty(s) && s.All(char.IsDigit);

    private static string? GetString(LatinoObject obj)
    {
        if (obj is LatinoNull) return null;
        return obj.ToString();
    }

    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) 
    {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) 
            output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
