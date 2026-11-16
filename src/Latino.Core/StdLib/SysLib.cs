using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics; // Necesario para Process
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class SysLib
{
    // =========================================================================
    // CONFIGURACIÓN GLOBAL: REGEX TIMEOUT (SEGURIDAD ReDoS)
    // =========================================================================
    public static double RegexTimeoutSegundos = 2.0;
    public static TimeSpan RegexTimeout => TimeSpan.FromSeconds(RegexTimeoutSegundos);

    // =========================================================================
    // HELPER: NORMALIZACIÓN DE EMOJIS (UI/UX)
    // =========================================================================
    internal static class EmojiHelper
    {
        public static int EspaciadoEmojis = 0;
        private static Dictionary<int, int>? _anchoEmojiCache = null;

        // Emojis con Emoji=Yes + Emoji_Presentation=No en Unicode 15.1.
        // Necesitan U+FE0F para forzar presentación emoji. xterm.js los renderiza
        // como ancho 2, pero .NET los reporta como ancho 1 → bug de alineación.
        // HashSet garantiza búsqueda O(1). Generado por audit_emoji_spacing.csx.
        // FUENTE NORMATIVA: https://unicode.org/Public/15.1.0/ucd/emoji/emoji-data.txt
        private static readonly HashSet<int> _emojisConFE0F = new HashSet<int>
        {
            // ── Flechas ──────────────────────────────────────────────────
            0x2194,  // ↔️  Left right arrow
            0x2195,  // ↕️  Up down arrow
            0x2196,  // ↖️  Upper left arrow
            0x2197,  // ↗️  Upper right arrow
            0x2198,  // ↘️  Lower right arrow
            0x2199,  // ↙️  Lower left arrow
            0x21A9,  // ↩️  Right arrow curving left
            0x21AA,  // ↪️  Left arrow curving right
            0x27A1,  // ➡️  Right arrow
            0x2B05,  // ⬅️  Left arrow
            0x2B06,  // ⬆️  Up arrow
            0x2B07,  // ⬇️  Down arrow

            // ── UI / Aviso ────────────────────────────────────────────────
            0x2139,  // ℹ️  Information
            0x26A0,  // ⚠️  Warning
            0x2620,  // ☠️  Skull and crossbones
            0x2622,  // ☢️  Radioactive
            0x2623,  // ☣️  Biohazard
            0x26D1,  // ⛑️  Rescue worker's helmet

            // ── Herramientas ─────────────────────────────────────────────
            0x2692,  // ⚒️  Hammer and pick
            0x2694,  // ⚔️  Crossed swords
            0x2696,  // ⚖️  Balance scale
            0x2697,  // ⚗️  Alembic
            0x2699,  // ⚙️  Gear
            0x26CF,  // ⛏️  Pick
            0x1F6E0, // 🛠️  Hammer and wrench
            0x1F6E1, // 🛡️  Shield
            0x1F6E2, // 🛢️  Oil drum

            // ── Calendario ───────────────────────────────────────────────
            0x1F5D1, // 🗑️  Wastebasket
            0x1F5D2, // 🗒️  Spiral notepad
            0x1F5D3, // 🗓️  Spiral calendar

            // ── Tiempo ───────────────────────────────────────────────────
            0x23F1,  // ⏱️  Stopwatch
            0x23F2,  // ⏲️  Timer clock
            0x1F570, // 🕰️  Mantelpiece clock

            // ── Controles de reproducción ────────────────────────────────
            0x23CF,  // ⏏️  Eject button
            0x23F8,  // ⏸️  Pause button
            0x23F9,  // ⏹️  Stop button
            0x23FA,  // ⏺️  Record button
            0x25B6,  // ▶️  Play button
            0x25C0,  // ◀️  Reverse button

            // ── Clima ────────────────────────────────────────────────────
            0x2600,  // ☀️  Sun
            0x2601,  // ☁️  Cloud
            0x2602,  // ☂️  Umbrella
            0x2603,  // ☃️  Snowman
            0x2604,  // ☄️  Comet

            // ── Deportes ─────────────────────────────────────────────────
            0x26F7,  // ⛷️  Skier
            0x26F8,  // ⛸️  Ice skate
            0x26F9,  // ⛹️  Person bouncing ball

            // ── Lugares / Transporte ─────────────────────────────────────
            0x26F0,  // ⛰️  Mountain
            0x26F1,  // ⛱️  Umbrella on ground
            0x26F4,  // ⛴️  Ferry

            // ── Cultura / Religión ───────────────────────────────────────
            0x2626,  // ☦️  Orthodox cross
            0x262A,  // ☪️  Star and crescent
            0x262E,  // ☮️  Peace symbol
            0x262F,  // ☯️  Yin yang
            0x2638,  // ☸️  Wheel of dharma
            0x271D,  // ✝️  Latin cross
            0x2721,  // ✡️  Star of David

            // ── Naipes ───────────────────────────────────────────────────
            0x2660,  // ♠️  Spade suit
            0x2663,  // ♣️  Club suit
            0x2665,  // ♥️  Heart suit
            0x2666,  // ♦️  Diamond suit
            0x2764,  // ❤️  Red heart

            // ── Texto / Puntuación ───────────────────────────────────────
            0x203C,  // ‼️  Double exclamation mark
            0x2049,  // ⁉️  Exclamation question mark
            0x2716,  // ✖️  Heavy multiplication x
        };
        
        // Detecta el tipo de terminal y su comportamiento con emojis
        private static string DetectarTipoTerminal()
        {
            string? termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
            string? psModulePath = Environment.GetEnvironmentVariable("PSModulePath");
            string? vscodePid = Environment.GetEnvironmentVariable("VSCODE_PID");
            string? vscodeIpc = Environment.GetEnvironmentVariable("VSCODE_IPC_HOOK");
            
            // VSCode establece múltiples variables, verificamos varias
            bool esVSCode = termProgram == "vscode" || 
                           !string.IsNullOrEmpty(vscodePid) ||
                           !string.IsNullOrEmpty(vscodeIpc);
            
            // PowerShell tiene PSModulePath definido
            bool esPowerShell = !string.IsNullOrEmpty(psModulePath);
            
            // PowerShell en VSCode (el caso problemático)
            if (esVSCode && esPowerShell)
                return "vscode-pwsh";
            
            // Bash/Zsh en VSCode
            if (esVSCode)
                return "vscode-bash";
            
            // PowerShell nativo de Windows
            if (esPowerShell && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "pwsh-windows";
            
            // Terminal nativo de Windows (cmd)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "cmd-windows";
            
            return "unknown";
        }
        
        // Calcula el ancho visual de un emoji según el terminal
        private static int ObtenerAnchoEmoji(int code, string tipoTerminal)
        {
            // Inicializar cache si no existe
            if (_anchoEmojiCache == null)
            {
                _anchoEmojiCache = new Dictionary<int, int>();
                
                // PowerShell en VSCode tiene un bug: reporta ancho 1 pero renderiza ancho 2
                if (tipoTerminal == "vscode-pwsh")
                {
                    // GRUPO 1: poblar caché desde el HashSet estático.
                    // La fuente de verdad es _emojisConFE0F — no duplicar aquí.
                    foreach (var emoji in _emojisConFE0F)
                        _anchoEmojiCache[emoji] = 2;

                    // GRUPO 2: Emojis con Emoji_Presentation=Yes — ya son ancho 2
                    // en xterm.js de forma consistente, no necesitan compensación.
                    var emojisSinCompensacion = new int[]
                    {
                        0x2705,  // ✅ Check mark
                        0x274C,  // ❌ Cross mark
                        0x1F525, // 🔥 Fire
                        0x1F4A1, // 💡 Light bulb
                        0x1F4CC, // 📌 Pushpin
                        0x1F4D6, // 📖 Open book
                        0x1F4CA, // 📊 Bar chart
                        0x1F4CB, // 📋 Clipboard
                        0x1F4DD, // 📝 Memo
                        0x1F9EA, // 🧪 Test tube
                        0x1F50D, // 🔍 Magnifying glass
                        0x1F527, // 🔧 Wrench
                        0x1F44D, // 👍 Thumbs up
                        0x1F44E, // 👎 Thumbs down
                        0x1F44B, // 👋 Waving hand
                        0x2B50,  // ⭐ Star
                        0x2728,  // ✨ Sparkles
                        0x1F31F, // 🌟 Glowing star
                        0x23F0,  // ⏰ Alarm clock
                        0x231B,  // ⌛ Hourglass
                        0x23F3,  // ⏳ Hourglass flowing
                        0x1F4E6, // 📦 Package
                        0x1F4C1, // 📁 Folder
                        0x1F4C2, // 📂 Open folder
                        0x1F5C2, // 🗂️ Card index dividers
                        0x1F4E2, // 📢 Loudspeaker
                        0x1F4E3, // 📣 Megaphone
                        0x1F6AB, // 🚫 Prohibited
                        0x26D4,  // ⛔ No entry
                    };
                    foreach (var emoji in emojisSinCompensacion)
                        _anchoEmojiCache[emoji] = 1;
                }
            }
            
            // Consultar cache
            if (_anchoEmojiCache.TryGetValue(code, out int ancho))
                return ancho;
            
            // Por defecto, asumir ancho 1 (la mayoría de terminales modernos)
            return 1;
        }
        
        // Obtiene el ajuste de espaciado necesario para compensar el ancho del emoji
        private static int ObtenerAjusteEspaciado(int emojiCode)
        {
            // Si hay configuración manual, usarla directamente
            if (EspaciadoEmojis > 0) return EspaciadoEmojis;
            
            // Detección automática basada en el ancho del emoji
            string tipoTerminal = DetectarTipoTerminal();
            int anchoVisual = ObtenerAnchoEmoji(emojiCode, tipoTerminal);
            
            // Si el emoji ocupa 2 columnas visualmente pero la terminal reporta 1,
            // necesitamos compensar con -1 espacio (menos espacios de lo esperado)
            if (tipoTerminal == "vscode-pwsh" && anchoVisual == 2)
            {
                return -1; // Compensar el espacio extra que PowerShell ya agregó
            }
            
            return 0;
        }

        private static bool EsEmoji(int code)
        {
            // Rangos Unicode completos para detección precisa
            return (code >= 0x1F300 && code <= 0x1F9FF) ||  // Emojis básicos y extra
                   (code >= 0x2600 && code <= 0x27BF) ||    // Símbolos y flechas
                   (code >= 0x1F1E6 && code <= 0x1F1FF) ||  // Banderas (Regional Indicator)
                   (code >= 0x1F000 && code <= 0x1F0FF) ||  // Mahjong, Cartas, etc
                   (code >= 0x1FA70 && code <= 0x1FAFF) ||  // Símbolos extendidos
                   (code >= 0x2100 && code <= 0x214F) ||    // Letras tipo símbolo (ℹ️, ™️)
                   (code >= 0x2300 && code <= 0x23FF) ||    // Misceláneos técnicos (Relojes, UI)
                   (code == 0x2B50) || (code == 0x00A9) || (code == 0x00AE) ||
                   (code >= 0x2190 && code <= 0x21FF) ||    // Flechas
                   (code >= 0x25AA && code <= 0x25FE) ||    // Geometría
                   (code >= 0x2934 && code <= 0x2935) ||    // Flechas curvas
                   (code >= 0x3200 && code <= 0x32FF) ||    // Ideogramas circulados (㊗, ㊙)
                   (code == 0x203C) || (code == 0x2049) || (code == 0x24C2);
        }

        private static bool EsModificadorEmoji(int code)
        {
            return (code >= 0x1F3FB && code <= 0x1F3FF) ||  // Tono de piel
                   (code == 0x200D) ||                      // ZWJ (Zero Width Joiner)
                   (code >= 0xFE00 && code <= 0xFE0F) ||    // Variación
                   (code == 0x20E3);                        // Keycap (Enclosing)
        }

        public static string Normalizar(string texto)
        {
            if (EspaciadoEmojis <= 0 || string.IsNullOrEmpty(texto)) return texto;

            var sb = new System.Text.StringBuilder(texto.Length + 16);
            string tipoTerminal = DetectarTipoTerminal();
            
            // DEBUG: Descomentar para ver qué terminal se detectó
            // Console.WriteLine($"[DEBUG] Terminal detectado: {tipoTerminal}");
            
            // Usar enumerador de elementos de texto para manejar correctamente pares subrogados (emojis)
            var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(texto);
            List<string> elements = new List<string>();
            while (enumerator.MoveNext())
            {
                elements.Add(enumerator.GetTextElement());
            }

            for (int i = 0; i < elements.Count; i++)
            {
                string element = elements[i];
                sb.Append(element);

                // Conversión segura a codepoint: verificar longitud antes de acceder a [1]
                // para evitar IndexOutOfRangeException con strings corruptos (surrogates huérfanos)
                int code;
                if (char.IsHighSurrogate(element[0]) && element.Length > 1)
                    code = char.ConvertToUtf32(element[0], element[1]);
                else
                    code = element[0];

                if (EsEmoji(code) && i < elements.Count - 1)
                {
                    string nextElement = elements[i + 1];
                    int nextCode;

                    if (char.IsHighSurrogate(nextElement[0]) && nextElement.Length > 1)
                        nextCode = char.ConvertToUtf32(nextElement[0], nextElement[1]);
                    else
                        nextCode = nextElement[0];
                    
                    if (!EsEmoji(nextCode) && !EsModificadorEmoji(nextCode))
                    {
                        int espaciosConfig = EspaciadoEmojis;
                        
                        // CORRECCIÓN PARA POWERSHELL EN VSCODE
                        // PowerShell cuenta emojis como 2 columnas, lo que "consume" 1 espacio visual
                        // Para lograr N espacios visuales, necesitamos agregar N+1 espacios en código
                        if (tipoTerminal == "vscode-pwsh")
                        {
                            int anchoReportado = ObtenerAnchoEmoji(code, tipoTerminal);
                            if (anchoReportado == 2)
                            {
                                // AGREGAR 1 espacio extra para compensar el que PowerShell consume
                                espaciosConfig = espaciosConfig + 1;
                            }
                        }
                        
                        // Ajustar por espacios existentes en el texto
                        int espacioExistente = (nextCode == ' ' || nextCode == '\t') ? 1 : 0;
                        int espaciosDeseados = espaciosConfig - espacioExistente;
                        
                        if (espaciosDeseados > 0) 
                            sb.Append(' ', espaciosDeseados);
                    }
                }
            }
            return sb.ToString();
        }

        public static string NormalizarArgumentos(List<LatinoObject> args)
        {
            if (EspaciadoEmojis <= 0) return string.Join(" ", args);
            
            var partes = new List<string>();
            foreach (var arg in args) partes.Add(Normalizar(arg.ToString()));
            return string.Join(" ", partes);
        }
    }

    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        
        void Def(string name, Func<Interpreter, List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc, pams);
            methods[name] = func;
        }

        // --- COMANDOS DEL SISTEMA (NUEVO) ---
        Def("ejecutar", (i, args) => {
            string cmd = args[0].ToString();
            
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "bash",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c {cmd}" : $"-c \"{cmd}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                var process = Process.Start(processInfo);
                if (process == null)
                    throw new RuntimeError(null!, "No se pudo iniciar el proceso.");
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                int exitCode = process.ExitCode;
                
                // Si hay error, lanzar excepción con detalles
                if (exitCode != 0)
                {
                    string errorMsg = !string.IsNullOrEmpty(error) 
                        ? error.Trim() 
                        : $"El comando falló con código {exitCode}";
                    throw new RuntimeError(null!, $"Error ejecutando comando: {errorMsg}");
                }
                
                return new LatinoString(output.Trim());
            }
            catch (Exception ex) when (ex is not RuntimeError)
            {
                throw new RuntimeError(null!, $"Error al ejecutar comando del sistema: {ex.Message}");
            }
        }, 1, "Ejecuta un comando del sistema operativo y retorna la salida. Lanza error si el comando falla.", "comando");

        // --- TIEMPO ---
        Def("dormir", (i, args) => {
            double val = ((LatinoNumber)args[0]).Value;
            int ms = (int)(val * 1000);
            if (ms < 0) throw new RuntimeError(null!, "Tiempo negativo.");
            Thread.Sleep(ms);
            return LatinoNull.Instance;
        }, 1, "Pausa la ejecución (segundos).", "segundos");

        Def("esperar", (i, args) => {
            int ms = (int)((LatinoNumber)args[0]).Value;
            if (ms < 0) throw new RuntimeError(null!, "Tiempo negativo.");
            Thread.Sleep(ms);
            return LatinoNull.Instance;
        }, 1, "Pausa la ejecución (milisegundos).", "ms");

        // Tiempo con formato (Mejora)
        Def("tiempo", (i, args) => {
            if (args.Count == 0) return new LatinoString(DateTime.Now.ToString());
            
            string fmt = args[0].ToString();
            
            // Mapeo de especificadores C-style a .NET
            // Documentación: %Y=año(4d), %m=mes(2d), %d=día(2d), %H=hora(24h), %M=min, %S=seg
            fmt = fmt.Replace("%Y", "yyyy")
                     .Replace("%y", "yy")
                     .Replace("%m", "MM")
                     .Replace("%d", "dd")
                     .Replace("%H", "HH")
                     .Replace("%M", "mm")
                     .Replace("%S", "ss")
                     .Replace("%I", "hh")      // Hora 12h
                     .Replace("%p", "tt");     // AM/PM
            
            try
            {
                return new LatinoString(DateTime.Now.ToString(fmt));
            }
            catch (FormatException)
            {
                throw new RuntimeError(null!, 
                    $"Formato de fecha inválido: '{args[0]}'. " +
                    $"Especificadores válidos: %Y(año), %m(mes), %d(día), %H(hora), %M(min), %S(seg)");
            }
        }, -1, "Retorna la fecha/hora actual. Formato opcional: %Y(año), %m(mes), %d(día), %H(hora), %M(min), %S(seg)", "[formato]");
        
        Def("fecha", (i, args) => {
            string opt = args[0].ToString().ToLower().Trim();
            var now = DateTime.Now;
            
            double res = opt switch
            {
                "seg" or "segundo" => now.Second,
                "min" or "minuto" => now.Minute,
                "hora" => now.Hour,
                "dia" or "d_mes" => now.Day,
                "mes" => now.Month,
                "anio" or "año" or "year" => now.Year,
                "d_semana" or "dia_semana" => (int)now.DayOfWeek,
                "d_año" or "d_anio" or "dia_año" => now.DayOfYear,
                _ => throw new RuntimeError(null!, 
                    $"Componente de fecha desconocido: '{args[0]}'. " +
                    $"Valores válidos: seg, min, hora, dia, mes, anio, d_semana, d_año")
            };
            
            return new LatinoNumber(res);
        }, 1, "Retorna componentes de fecha. Valores: seg, min, hora, dia, mes, anio, d_semana, d_año", "componente");

        Def("milisegundos", (i, a) => new LatinoNumber(Environment.TickCount64), 0, "Obtiene ms del sistema.", "");
        
        // --- SISTEMA ---
        Def("salir", (i, args) => {
            int codigo = 0;
            if (args.Count > 0 && args[0] is LatinoNumber num)
            {
                codigo = (int)num.Value;
            }
            Environment.Exit(codigo);
            return LatinoNull.Instance;
        }, -1, "Termina el programa con un código de salida opcional (0 por defecto).", "[codigo]");
        Def("cwd", (i, a) => new LatinoString(Environment.CurrentDirectory), 0, "Directorio actual.", "");
        Def("directorioActual", (i, a) => new LatinoString(Environment.CurrentDirectory), 0, 
            "Retorna el directorio de trabajo actual.", "");
        Def("dirActual", (i, a) => new LatinoString(Environment.CurrentDirectory), 0, 
            "Alias de directorioActual. Retorna el directorio de trabajo actual.", "");
        Def("usuario", (i, a) => new LatinoString(Environment.UserName), 0, "Usuario actual.", "");
        
        Def("entorno", (i, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "sis.entorno() requiere el nombre de la variable de entorno.");
            
            string varName = args[0].ToString();
            string? value = Environment.GetEnvironmentVariable(varName);
            
            if (value == null)
            {
                // Si hay segundo argumento, usarlo como valor por defecto
                if (args.Count > 1)
                    return args[1];
                
                return LatinoNull.Instance;
            }
            
            return new LatinoString(value);
        }, -1, "Obtiene una variable de entorno. Retorna nulo si no existe, o valor por defecto (arg2).", "nombre_var, [valor_defecto]");
        
        Def("operativo", (i, args) => {
            string query = args[0].ToString().ToUpper().Trim();
            
            bool isWin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isLin = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            
            // Validar nombres aceptados
            var validNames = new[] { 
                "WINDOWS", "WIN32", "WIN",
                "LINUX", "UNIX",
                "MAC", "MACOS", "OSX", "APPLE"
            };
            
            if (!validNames.Contains(query))
            {
                throw new RuntimeError(null!, 
                    $"Sistema operativo desconocido: '{args[0]}'. " +
                    $"Valores válidos: WINDOWS/WIN, LINUX/UNIX, MAC/MACOS/OSX");
            }
            
            // Verificar coincidencias
            if ((query == "WINDOWS" || query == "WIN32" || query == "WIN") && isWin) 
                return new LatinoBoolean(true);
            if ((query == "LINUX" || query == "UNIX") && isLin) 
                return new LatinoBoolean(true);
            if ((query == "MAC" || query == "MACOS" || query == "OSX" || query == "APPLE") && isMac) 
                return new LatinoBoolean(true);
            
            return new LatinoBoolean(false);
        }, 1, 
        "Verifica si el SO actual coincide con el nombre dado (retorna verdadero/falso). " +
        "Para información completa del sistema usa sis.plataforma().",
        "nombre_so");
        
        // ═════════════════════════════════════════════════════════════════
        // INFORMACIÓN COMPLETA DE LA PLATAFORMA
        // ═════════════════════════════════════════════════════════════════
        // 
        // DIFERENCIAS CON FUNCIONES EXISTENTES:
        // • sis.usuario()     → Solo retorna el nombre de usuario (string)
        // • sis.operativo()   → Solo verifica SO (retorna booleano)
        // • sis.cwd()         → Solo retorna directorio actual (string)
        // • sis.plataforma()  → Retorna TODO en un diccionario completo
        //
        // CASOS DE USO TÍPICOS:
        // 1. Generación de rutas portables (usando separador_rutas)
        // 2. Validación de requisitos (64 bits, núcleos, etc.)
        // 3. Reportes de diagnóstico
        // 4. Lógica condicional por plataforma (Windows vs Unix)
        // 5. Depuración y logging de entorno
        //
        // NOTA: La clave "memoria_proceso" muestra la RAM usada por el proceso
        // actual, NO la RAM total del sistema (requeriría P/Invoke específico).
        // ═════════════════════════════════════════════════════════════════

        Def("plataforma", (i, args) => {
            var info = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            // ─────────────────────────────────────────────────────────────
            // SISTEMA OPERATIVO
            // ─────────────────────────────────────────────────────────────
            
            string nombreSO;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                nombreSO = "Windows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                nombreSO = "Linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                nombreSO = "macOS";
            else
                nombreSO = "Desconocido";
            
            info[new LatinoString("so")] = new LatinoString(nombreSO);
            
            // Versión completa del SO (ej: "Microsoft Windows NT 10.0.19045.0")
            info[new LatinoString("version")] = new LatinoString(
                RuntimeInformation.OSDescription.Trim()
            );
            
            // ─────────────────────────────────────────────────────────────
            // ARQUITECTURA DEL PROCESADOR
            // ─────────────────────────────────────────────────────────────
            
            string arquitectura = RuntimeInformation.OSArchitecture switch
            {
                Architecture.X86 => "X86",
                Architecture.X64 => "X64",
                Architecture.Arm => "Arm",
                Architecture.Arm64 => "Arm64",
                Architecture.Wasm => "WebAssembly",
                Architecture.S390x => "S390x",
                _ => "Desconocida"
            };
            
            info[new LatinoString("arquitectura")] = new LatinoString(arquitectura);
            
            // ─────────────────────────────────────────────────────────────
            // RUNTIME .NET
            // ─────────────────────────────────────────────────────────────
            
            info[new LatinoString("dotnet")] = new LatinoString(
                RuntimeInformation.FrameworkDescription.Trim()
            );
            
            // ─────────────────────────────────────────────────────────────
            // INFORMACIÓN DE LA MÁQUINA
            // ─────────────────────────────────────────────────────────────
            
            info[new LatinoString("nombre_maquina")] = new LatinoString(
                Environment.MachineName
            );
            
            // Bits del proceso actual (32 o 64)
            int bits = Environment.Is64BitProcess ? 64 : 32;
            info[new LatinoString("bits")] = new LatinoNumber(bits);
            info[new LatinoString("es_64bits")] = new LatinoBoolean(Environment.Is64BitProcess);
            
            // ─────────────────────────────────────────────────────────────
            // HARDWARE
            // ─────────────────────────────────────────────────────────────
            
            // Número de procesadores lógicos (núcleos + hyperthreading)
            info[new LatinoString("procesadores")] = new LatinoNumber(
                Environment.ProcessorCount
            );
            
            // Memoria física total (aproximada, en MB)
            // NOTA: .NET no expone directamente la RAM total del sistema.
            // Usamos WorkingSet (memoria del proceso) como referencia.
            // Para valores exactos se requeriría P/Invoke por plataforma.
            long memoriaProcesoBytes = Environment.WorkingSet;
            long memoriaMB = memoriaProcesoBytes / (1024 * 1024);
            
            info[new LatinoString("memoria_proceso")] = new LatinoNumber(memoriaMB);
            
            // ─────────────────────────────────────────────────────────────
            // USUARIO Y DIRECTORIOS
            // ─────────────────────────────────────────────────────────────
            
            info[new LatinoString("usuario")] = new LatinoString(
                Environment.UserName
            );
            
            info[new LatinoString("directorio_actual")] = new LatinoString(
                Environment.CurrentDirectory
            );
            
            // Directorio home del usuario
            string directorioHome = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile
            );
            info[new LatinoString("directorio_home")] = new LatinoString(directorioHome);
            
            // ─────────────────────────────────────────────────────────────
            // CONVENCIONES DE PLATAFORMA
            // ─────────────────────────────────────────────────────────────
            
            // Separador de rutas (\ en Windows, / en Unix)
            info[new LatinoString("separador_rutas")] = new LatinoString(
                Path.DirectorySeparatorChar.ToString()
            );
            
            // Salto de línea por defecto (\r\n en Windows, \n en Unix)
            info[new LatinoString("salto_linea")] = new LatinoString(
                Environment.NewLine
            );
            
            // Separador de variables PATH (; en Windows, : en Unix)
            info[new LatinoString("separador_path")] = new LatinoString(
                Path.PathSeparator.ToString()
            );
            
            // ─────────────────────────────────────────────────────────────
            // TIEMPO DE EJECUCIÓN
            // ─────────────────────────────────────────────────────────────
            
            // Milisegundos desde que se inició el sistema
            info[new LatinoString("tiempo_sistema")] = new LatinoNumber(
                Environment.TickCount64
            );
            
            return new LatinoDictionary(info);
            
        }, 0, 
        "Retorna un diccionario con información completa de la plataforma: SO, arquitectura, hardware, usuario, directorios y convenciones del sistema.",
        "");
        
        Def("limpiar", (i, a) => { try { Console.Clear(); } catch {} return LatinoNull.Instance; }, 0, "Limpia consola.", "");
        Def("iraxy", (i, args) => {
            try { Console.SetCursorPosition((int)((LatinoNumber)args[0]).Value, (int)((LatinoNumber)args[1]).Value); }
            catch { }
            return LatinoNull.Instance;
        }, 2, "Mueve cursor.", "x, y");

        Def("manual", (interp, a) => {
            try
            {
                // 1. Buscar el PDF en la carpeta del ejecutable
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Patrones de búsqueda (ordenados por preferencia)
                var patterns = new[]
                {
                    "Manual_De_Latino.NET_*.pdf",
                    "Manual De Latino.NET *.pdf",
                    "Manual_Latino.NET_*.pdf",
                    "Manual Latino.NET *.pdf",
                    "Manual_De_Latino_*.pdf",
                    "Manual De Latino *.pdf",
                    "Manual_Latino_*.pdf",
                    "Manual Latino *.pdf",
                    "Manual*.pdf"
                };

                string? manualPath = null;
                foreach (var pattern in patterns)
                {
                    var files = Directory.GetFiles(baseDir, pattern);
                    if (files.Length > 0)
                    {
                        // Tomar el más reciente si hay varios
                        manualPath = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                        break;
                    }
                }

                if (manualPath == null)
                {
                    interp.Out.WriteLine("⚠️ No se encontró el archivo del manual en: " + baseDir);
                    interp.Out.WriteLine("   Patrones buscados: Manual*.pdf");
                    return new LatinoBoolean(false);
                }

                interp.Out.WriteLine($"📖 Abriendo manual: {Path.GetFileName(manualPath)}...");

                // 2. Abrir según el Sistema Operativo
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(manualPath) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", manualPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", manualPath);
                }
                else
                {
                    interp.Out.WriteLine("⚠️ Sistema operativo no soportado para abrir archivos.");
                    return new LatinoBoolean(false);
                }

                return new LatinoBoolean(true);
            }
            catch (Exception ex)
            {
                interp.Out.WriteLine($"❌ Error al intentar abrir el manual: {ex.Message}");
                return new LatinoBoolean(false);
            }
        }, 0, "Busca y abre el manual PDF oficial incluido en la instalación.", "");

        // --- APOYAR PROYECTO (NUEVO) ---
        Def("apoyar", (interp, a) => {
            interp.Out.WriteLine("☕ Proyecto Latino.NET — Libre, Abierto y Gratuito\n");
            interp.Out.WriteLine("Latino.NET es un esfuerzo independiente para democratizar la programación industrial en español.");
            interp.Out.WriteLine("Tu apoyo nos permite dedicar más horas de ingeniería, infraestructura y documentación al proyecto.\n");
            interp.Out.WriteLine("Abriendo en tu navegador: https://frkl81.gumroad.com/l/LatinoNET\n");
            interp.Out.WriteLine("¡Gracias por hacer posible que esta herramienta siga creciendo!");

            bool exito = AbrirUrl("https://frkl81.gumroad.com/l/LatinoNET");
            return new LatinoBoolean(exito);
        }, 0, "Muestra información para apoyar el proyecto y abre el enlace de patrocinio en el navegador.", "");

        Def("config", (interp, args) => {
            if (args.Count < 1) 
                throw new RuntimeError(null!, "sis.config() requiere una clave.");

            string key = args[0].ToString().ToLower().Trim();

            // --- CASO 1: MAX_STACK ---
            if (key == "max_stack")
            {
                if (args.Count > 1) {
                    int val = (int)((LatinoNumber)args[1]).Value;
                    if (val < 10) throw new RuntimeError(null!, "max_stack debe ser >= 10.");
                    interp.MaxDepth = val;
                    return LatinoNull.Instance;
                }
                return new LatinoNumber(interp.MaxDepth);
            }

            // --- CASO 2: EMOJI_SPACING ---
            else if (key == "emoji_spacing")
            {
                if (args.Count > 1) {
                    int val = (int)((LatinoNumber)args[1]).Value;
                    if (val < 0 || val > 10) 
                        throw new RuntimeError(null!, "emoji_spacing debe estar entre 0 y 10.");
                    EmojiHelper.EspaciadoEmojis = val;
                    return LatinoNull.Instance;
                }
                return new LatinoNumber(EmojiHelper.EspaciadoEmojis);
            }

            // --- CASO 3: REGEX_TIMEOUT ---
            else if (key == "regex_timeout")
            {
                if (args.Count > 1) {
                    double val = ((LatinoNumber)args[1]).Value;
                    if (val < 0.1 || val > 60) 
                        throw new RuntimeError(null!, "regex_timeout debe estar entre 0.1 y 60 segundos.");
                    RegexTimeoutSegundos = val;
                    return LatinoNull.Instance;
                }
                return new LatinoNumber(RegexTimeoutSegundos);
            }

            else
            {
                throw new RuntimeError(null!, $"Clave desconocida: '{key}'. Válidas: 'max_stack', 'emoji_spacing', 'regex_timeout'");
            }
        }, -1, "Configuración del motor. Válidas: max_stack, emoji_spacing, regex_timeout", "clave, [valor]");

        Def("argumentos", (i, args) => {
            var cmdArgs = Environment.GetCommandLineArgs();
            
            // Determinar offset: si hay script, saltar ejecutable + script
            // Si no hay script (REPL), retornar vacío
            int offset = cmdArgs.Length > 1 && cmdArgs[1].EndsWith(".lat") ? 2 : cmdArgs.Length;
            
            var lista = new List<LatinoObject>();
            for (int k = offset; k < cmdArgs.Length; k++)
            {
                lista.Add(new LatinoString(cmdArgs[k]));
            }
            
            return new LatinoList(lista);
        }, 0, "Retorna lista de argumentos pasados al script desde línea de comandos.", "");

        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("sis", modulo);
    }
    
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }

    /// <summary>
    /// Abre una URL en el navegador web predeterminado del sistema operativo de forma segura.
    /// </summary>
    public static bool AbrirUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
