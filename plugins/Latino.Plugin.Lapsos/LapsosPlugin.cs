using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices; // Para OSPlatform
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Lapsos;

public class Main
{
    // --- MÉTODOS PÚBLICOS (Con prefijo 'lapsos.') ---

    [LatinoExtension("lapsos.ahora", Help = "Fecha actual.", Returns = "diccionario")]
    public static LatinoObject Ahora(List<LatinoObject> args) => ToDic(DateTime.Now);

    [LatinoExtension("lapsos.fecha", Help = "Crea fecha.", Args = "a,m,d,[h,m,s]")]
    public static LatinoObject Fecha(List<LatinoObject> args)
    {
        try {
            return ToDic(new DateTime(
                (int)GetNum(args, 0), (int)GetNum(args, 1), (int)GetNum(args, 2),
                args.Count > 3 ? (int)GetNum(args, 3) : 0,
                args.Count > 4 ? (int)GetNum(args, 4) : 0,
                args.Count > 5 ? (int)GetNum(args, 5) : 0
            ));
        }
        catch (RuntimeError) { throw; }
        catch (ArgumentOutOfRangeException)
        {
            throw new RuntimeError(null, "Los valores proporcionados no forman una fecha válida. Verifica días y meses del calendario.");
        }
        catch (Exception)
        {
            throw new RuntimeError(null, "lapsos.fecha() recibió valores inválidos para construir una fecha.");
        }
    }

    [LatinoExtension("lapsos.analizar", Help = "Texto a fecha.", Args = "txt")]
    public static LatinoObject Analizar(List<LatinoObject> args)
    {
        string txt = args[0].ToString();
        if (DateTime.TryParse(txt, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt) ||
            DateTime.TryParse(txt, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return ToDic(dt);
        throw new RuntimeError(null, $"No se pudo interpretar '{txt}' como fecha. Formatos aceptados: 'yyyy-MM-dd', 'dd/MM/yyyy', etc.");
    }

    [LatinoExtension("lapsos.formato", Help = "Fecha a texto.", Args = "fecha, [fmt]")]
    public static LatinoObject Formato(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            string fmt = args.Count > 1 ? args[1].ToString() : "dd/MM/yyyy";
            return new LatinoString(dt.ToString(fmt, CultureInfo.CurrentCulture));
        }
        catch (RuntimeError) { throw; }
        catch (FormatException)
        {
            string fmt = args.Count > 1 ? args[1].ToString() : "dd/MM/yyyy";
            throw new RuntimeError(null, $"El formato '{fmt}' no es válido.");
        }
        catch (Exception)
        {
            throw new RuntimeError(null, "lapsos.formato() recibió una fecha o formato inválido.");
        }
    }

    [LatinoExtension("lapsos.esBisiesto", Help = "Es bisiesto.", Args = "anio_o_fecha")]
    public static LatinoObject EsBisiesto(List<LatinoObject> args)
    {
        try
        {
            int year;
            if (args[0] is LatinoNumber n) year = (int)n.Value;
            else {
                var dt = ToDate(args[0]);
                year = dt.Year;
            }
            return new LatinoBoolean(DateTime.IsLeapYear(year));
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en esBisiesto: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.sumar", Help = "Suma tiempo.", Args = "fecha, cant, unidad")]
    public static LatinoObject Sumar(List<LatinoObject> args) => OperarFecha(args, 1);

    [LatinoExtension("lapsos.restar", Help = "Resta tiempo.", Args = "fecha, cant, unidad")]
    public static LatinoObject Restar(List<LatinoObject> args) => OperarFecha(args, -1);

    [LatinoExtension("lapsos.diferencia", Help = "Diferencia entre fechas.", Args = "f1, f2")]
    public static LatinoObject Diferencia(List<LatinoObject> args)
    {
        try
        {
            var span = ToDate(args[1]) - ToDate(args[0]);
            var r = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            void Add(string k, double v) => r[new LatinoString(k)] = new LatinoNumber(v);
            
            // Fragmentos del tiempo (enteros)
            Add("dias", span.Days);
            Add("horas", span.Hours);
            Add("minutos", span.Minutes);
            Add("segundos", span.Seconds);
            Add("milisegundos", span.Milliseconds);
            
            // Totales acumulados (decimales)
            Add("total_dias", span.TotalDays);
            Add("dias_totales", span.TotalDays); // Alias
            Add("total_horas", span.TotalHours);
            Add("horas_totales", span.TotalHours); // Alias
            Add("total_minutos", span.TotalMinutes);
            Add("total_segundos", span.TotalSeconds);
            
            Add("signo", span.Ticks < 0 ? -1 : 1);
            
            return new LatinoDictionary(r);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en diferencia: {ex.Message}"); }
    }

    // =============================================================================
    // MÉTODOS DE COMPARACIÓN Y CONVERSIÓN (NUEVOS)
    // =============================================================================

    [LatinoExtension("lapsos.ticks", Help = "Obtiene el valor numérico absoluto en ticks (100 nanosegundos desde 0001-01-01).", Args = "fecha")]
    public static LatinoObject Ticks(List<LatinoObject> args)
    {
        try
        {
            return new LatinoNumber(ToDate(args[0]).Ticks);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en ticks: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.desdeTicks", Help = "Crea una fecha desde un valor de ticks.", Args = "ticks")]
    public static LatinoObject DesdeTicks(List<LatinoObject> args)
    {
        try
        {
            long ticks = (long)((LatinoNumber)args[0]).Value;
            
            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                throw new RuntimeError(null, $"Valor de ticks fuera de rango: {ticks}");
            
            return ToDic(new DateTime(ticks));
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en desdeTicks: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.comparar", Help = "Compara dos fechas. Retorna -1 si f1<f2, 0 si son iguales, 1 si f1>f2. Acepta cadenas, objetos fecha o números.", Args = "fecha1, fecha2")]
    public static LatinoObject Comparar(List<LatinoObject> args)
    {
        try
        {
            DateTime t1 = ToDate(args[0]);
            DateTime t2 = ToDate(args[1]);
            return new LatinoNumber(t1.CompareTo(t2));
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex)
        {
            throw new RuntimeError(null, $"Error comparando fechas: {ex.Message}");
        }
    }

    [LatinoExtension("lapsos.unix", Help = "Convierte fecha a timestamp Unix (segundos desde 1970-01-01 UTC).", Args = "fecha")]
    public static LatinoObject Unix(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new LatinoNumber((dt.ToUniversalTime() - epoch).TotalSeconds);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en unix: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.desdeUnix", Help = "Crea una fecha desde timestamp Unix (segundos desde 1970-01-01 UTC).", Args = "timestamp")]
    public static LatinoObject DesdeUnix(List<LatinoObject> args)
    {
        try
        {
            double timestamp = ((LatinoNumber)args[0]).Value;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt = epoch.AddSeconds(timestamp);
            return ToDic(dt.ToLocalTime());
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en desdeUnix: {ex.Message}"); }
    }

    // =============================================================================
    // MÉTODOS DE VALIDACIÓN TEMPORAL (NUEVOS)
    // =============================================================================

    [LatinoExtension("lapsos.esFuturo", Help = "Verifica si la fecha es posterior al momento actual.", Args = "fecha")]
    public static LatinoObject EsFuturo(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            return new LatinoBoolean(dt > DateTime.Now);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en esFuturo: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.esPasado", Help = "Verifica si la fecha es anterior al momento actual.", Args = "fecha")]
    public static LatinoObject EsPasado(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            return new LatinoBoolean(dt < DateTime.Now);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en esPasado: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.esValido", Help = "Valida si una cadena representa una fecha válida según el formato. Retorna verdadero/falso sin lanzar error.", Args = "cadena, [formato]")]
    public static LatinoObject EsValido(List<LatinoObject> args)
    {
        try
        {
            string input = args[0].ToString();
            
            // Si no hay formato, intentar parseo automático
            if (args.Count == 1)
            {
                return new LatinoBoolean(DateTime.TryParse(input, out _));
            }
            
            // Con formato específico
            string format = args[1].ToString();
            return new LatinoBoolean(
                DateTime.TryParseExact(input, format, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, 
                    out _)
            );
        }
        catch { return new LatinoBoolean(false); }
    }

    // =============================================================================
    // MÉTODOS DE UTILIDAD DE CALENDARIO (NUEVOS)
    // =============================================================================

    [LatinoExtension("lapsos.hoy", Help = "Retorna la fecha actual sin componente de hora (00:00:00).", Args = "")]
    public static LatinoObject Hoy(List<LatinoObject> args)
    {
        return ToDic(DateTime.Today);
    }

    [LatinoExtension("lapsos.diasEnMes", Help = "Retorna la cantidad de días en el mes de la fecha.", Args = "fecha")]
    public static LatinoObject DiasEnMes(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            return new LatinoNumber(DateTime.DaysInMonth(dt.Year, dt.Month));
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en diasEnMes: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.inicioMes", Help = "Retorna el primer día del mes de la fecha (a las 00:00:00).", Args = "fecha")]
    public static LatinoObject InicioMes(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            DateTime inicio = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
            return ToDic(inicio);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en inicioMes: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.finMes", Help = "Retorna el último día del mes de la fecha (a las 23:59:59).", Args = "fecha")]
    public static LatinoObject FinMes(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            int ultimoDia = DateTime.DaysInMonth(dt.Year, dt.Month);
            DateTime fin = new DateTime(dt.Year, dt.Month, ultimoDia, 23, 59, 59);
            return ToDic(fin);
        }
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, $"Error en finMes: {ex.Message}"); }
    }

    [LatinoExtension("lapsos.zonaHoraria", Help = "Retorna el nombre de la zona horaria del sistema y el offset UTC.", Args = "")]
    public static LatinoObject ZonaHoraria(List<LatinoObject> args)
    {
        TimeZoneInfo tz = TimeZoneInfo.Local;
        string offset = tz.BaseUtcOffset.ToString(@"hh\:mm");
        string signo = tz.BaseUtcOffset.TotalHours >= 0 ? "+" : "";
        return new LatinoString($"{tz.StandardName} (UTC{signo}{offset})");
    }


    [LatinoExtension("lapsos.convertir", Help = "Convierte cadena, número o fecha a objeto fecha del plugin. Útil cuando trabajas con archivo.fechaModificacion().", Args = "valor")]
    public static LatinoObject Convertir(List<LatinoObject> args)
    {
        try
        {
            DateTime dt = ToDate(args[0]);
            return ToDic(dt);
        }
        catch (RuntimeError)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null, $"Error convirtiendo a fecha: {ex.Message}");
        }
    }

    // --- HELPERS PRIVADOS (Reutilizados) ---

    private static LatinoObject OperarFecha(List<LatinoObject> args, int factor)
    {
        try {
            DateTime dt = ToDate(args[0]);
            double cant = GetNum(args, 1) * factor;
            string unidad = args[2].ToString().ToLower();

            dt = unidad switch {
                "dia" or "dias" => dt.AddDays(cant),
                "mes" or "meses" => dt.AddMonths((int)cant),
                "ano" or "anio" or "año" or "años" => dt.AddYears((int)cant),
                "hora" or "horas" => dt.AddHours(cant),
                "min" or "minutos" => dt.AddMinutes(cant),
                "seg" or "segundos" => dt.AddSeconds(cant),
                _ => throw new RuntimeError(null, $"Unidad desconocida: {unidad}")
            };
            return ToDic(dt);
        } 
        catch (RuntimeError) { throw; }
        catch (Exception ex) { throw new RuntimeError(null, "Error fecha: " + ex.Message); }
    }

    private static LatinoDictionary ToDic(DateTime dt)
    {
        var d = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        void Add(string k, double v) => d[new LatinoString(k)] = new LatinoNumber(v);
        
        Add("anio", dt.Year);
        Add("mes", dt.Month);
        Add("dia", dt.Day);
        Add("hora", dt.Hour);
        Add("minuto", dt.Minute);
        Add("segundo", dt.Second);
        d[new LatinoString("texto")] = new LatinoString(dt.ToString("yyyy-MM-dd HH:mm:ss"));
        return new LatinoDictionary(d);
    }

    /// <summary>
    /// Helper robusto para convertir LatinoObject a DateTime con validación completa.
    /// Acepta: LatinoString (múltiples formatos), objetos internos del plugin, LatinoNumber (ticks/unix).
    /// </summary>
    private static DateTime ToDate(LatinoObject obj)
    {
        // =========================================================================
        // CASO 1: Es un objeto interno del plugin (dict con texto/fecha/anio)
        // =========================================================================
        if (obj is LatinoDictionary dict)
        {
            // Si tiene clave "texto" (resultado de lapsos.ahora(), lapsos.fecha(), etc.)
            if (dict.Pairs.TryGetValue(new LatinoString("texto"), out var textoObj))
            {
                string texto = textoObj.ToString();
                return ParsearFechaFlexible(texto);
            }

            // Fallback manual si tiene componentes
            int Get(string k, int def) => 
                dict.Pairs.TryGetValue(new LatinoString(k), out var v) && v is LatinoNumber n ? (int)n.Value : def;
            
            try { 
                return new DateTime(Get("anio", DateTime.Now.Year), Get("mes", 1), Get("dia", 1), 
                                  Get("hora", 0), Get("minuto", 0), Get("segundo", 0)); 
            }
            catch { throw new RuntimeError(null, "Datos de fecha en diccionario son inválidos."); }
        }
        
        // =========================================================================
        // CASO 2: Es una cadena de texto (LatinoString)
        // =========================================================================
        if (obj is LatinoString latinoStr)
        {
            return ParsearFechaFlexible(latinoStr.Value);
        }
        
        // =========================================================================
        // CASO 3: Es un número (podría ser ticks o timestamp Unix)
        // =========================================================================
        if (obj is LatinoNumber num)
        {
            long valor = (long)num.Value;
            
            // Heurística: valores muy grandes son ticks, valores pequeños son Unix timestamps
            if (valor > 253402300799)  // Máximo timestamp Unix (9999-12-31)
            {
                // Es ticks
                if (valor < DateTime.MinValue.Ticks || valor > DateTime.MaxValue.Ticks)
                    throw new RuntimeError(null, $"Valor de ticks fuera de rango: {valor}");
                
                return new DateTime(valor);
            }
            else
            {
                // Es timestamp Unix (segundos desde 1970)
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(valor).ToLocalTime();
            }
        }
        
        // =========================================================================
        // CASO 4: Tipo no soportado
        // =========================================================================
        throw new RuntimeError(null, 
            $"Tipo de fecha inválido: se esperaba una fecha, cadena de texto o número, " +
            $"pero se recibió {obj.GetType().Name}. " +
            $"Si es una cadena, verifica que tenga formato de fecha válido (ej: 'yyyy-MM-dd HH:mm:ss').");
    }

    /// <summary>
    /// Parsea cadena de texto intentando múltiples formatos comunes.
    /// </summary>
    private static DateTime ParsearFechaFlexible(string texto)
    {
        // Formatos comunes a intentar (en orden de especificidad)
        var formatos = new[]
        {
            "yyyy-MM-dd HH:mm:ss",      // ISO con hora (más común en Latino)
            "yyyy-MM-dd HH:mm:ss tt",   // Con AM/PM
            "yyyy-MM-dd'T'HH:mm:ss",    // ISO 8601
            "yyyy-MM-dd",               // ISO solo fecha
            "dd/MM/yyyy HH:mm:ss",      // Europeo con hora
            "dd/MM/yyyy HH:mm:ss tt",
            "dd/MM/yyyy",               // Europeo
            "MM/dd/yyyy HH:mm:ss",      // Americano con hora
            "MM/dd/yyyy HH:mm:ss tt",
            "MM/dd/yyyy",               // Americano
            "dd-MM-yyyy HH:mm:ss",
            "dd-MM-yyyy",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/dd"
        };
        
        // Intentar parseo con formato exacto
        foreach (var formato in formatos)
        {
            if (DateTime.TryParseExact(texto, formato, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out DateTime resultado))
            {
                return resultado;
            }
        }
        
        // Intentar parseo automático como último recurso
        if (DateTime.TryParse(texto, out DateTime resultadoAuto))
        {
            return resultadoAuto;
        }
        
        // Si todo falla, error descriptivo
        throw new RuntimeError(null, 
            $"No se pudo convertir '{texto}' a fecha. " +
            $"Formatos válidos: 'yyyy-MM-dd HH:mm:ss', 'dd/MM/yyyy', 'yyyy-MM-dd', etc. " +
            $"Si la cadena viene de archivo.fechaModificacion(), asegúrate de que el formato sea estándar.");
    }

    /// <summary>
    /// Verifica si un objeto es una fecha reconocida por el plugin.
    /// Útil para debugging.
    /// </summary>
    private static bool EsObjetoFecha(LatinoObject obj)
    {
        if (obj is LatinoDictionary dict)
        {
            return dict.Pairs.ContainsKey(new LatinoString("texto")) ||
                   dict.Pairs.ContainsKey(new LatinoString("anio"));
        }
        
        if (obj is LatinoString || obj is LatinoNumber)
            return true;
        
        return false;
    }

    private static double GetNum(List<LatinoObject> args, int i) => 
        (i < args.Count && args[i] is LatinoNumber n) ? n.Value : 0;
}
