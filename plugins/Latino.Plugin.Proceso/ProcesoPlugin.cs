using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Plugin.Proceso;

public static class ProcesoPlugin
{
    // =========================================================================
    // Estado global para procesos asíncronos
    // =========================================================================

    private static readonly object _lockAsync = new();
    private static int _nextId = 1;

    private record AsyncEntry(
        Process       Proc,
        StringBuilder StdoutBuffer,
        StringBuilder StderrBuffer,
        Task          WaitTask
    )
    {
        public bool   Terminado { get; set; } = false;
        public int    Codigo    { get; set; } = -1;
    }

    private static readonly Dictionary<int, AsyncEntry> _asyncProcs = new();

    // =========================================================================
    // proceso.ejecutar(cmd, [args], [timeout_ms])
    // =========================================================================

    [LatinoExtension("proceso.ejecutar",
        Args = "cmd, [args], [timeout_ms]",
        Help = "Ejecuta un comando del sistema y espera su resultado. " +
               "Retorna un diccionario {salida, error, codigo, timeout}. " +
               "'args' puede ser una lista (correcto para argumentos con espacios) " +
               "o una cadena (se divide por espacios — no usar si los args contienen espacios). " +
               "'timeout_ms' es el tiempo máximo de espera en milisegundos (defecto: 30000). " +
               "Si el timeout expira, el proceso es terminado y 'timeout' es verdadero.")]
    public static LatinoObject Ejecutar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.ejecutar requiere al menos el nombre del comando.");

        string cmd        = args[0].ToString();
        string argsStr    = "";
        int    timeoutMs  = 30_000;

        if (args.Count > 1 && args[1] is not LatinoNull)
        {
            if (args[1] is LatinoList lista)
                argsStr = ConstruirArgs(lista);
            else
                argsStr = args[1].ToString();
        }

        if (args.Count > 2 && args[2] is LatinoNumber t)
            timeoutMs = (int)t.Value;

        return EjecutarInterno(cmd, argsStr, timeoutMs);
    }

    // =========================================================================
    // proceso.shell(cmd_string, [timeout_ms])
    // =========================================================================

    [LatinoExtension("proceso.shell",
        Args = "cmd_string, [timeout_ms]",
        Help = "Ejecuta un comando a través del shell del sistema " +
               "(/bin/bash -c en Linux/Mac, cmd.exe /C en Windows). " +
               "Soporta pipes (|), redirección (>), variables de entorno ($VAR). " +
               "⚠️ SEGURIDAD: No pasar input externo sin sanitizar — riesgo de inyección de shell. " +
               "Retorna {salida, error, codigo, timeout}.")]
    public static LatinoObject Shell(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.shell requiere el comando a ejecutar.");

        string cmd       = args[0].ToString();
        int    timeoutMs = 30_000;

        if (args.Count > 1 && args[1] is LatinoNumber t)
            timeoutMs = (int)t.Value;

        bool esWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        string shell   = esWindows ? "cmd.exe"     : "/bin/bash";
        string flag    = esWindows ? $"/C \"{cmd}\"" : $"-c \"{cmd.Replace("\"", "\\\"")}\"";

        return EjecutarInterno(shell, flag, timeoutMs);
    }

    // =========================================================================
    // proceso.ejecutarAsync(cmd, [args])
    // =========================================================================

    [LatinoExtension("proceso.ejecutarAsync",
        Args = "cmd, [args]",
        Help = "Inicia un proceso en segundo plano y retorna un ID interno. " +
               "Usa proceso.esperar(id), proceso.terminado(id) y proceso.resultado(id) " +
               "para controlar el proceso. El ID interno NO es el PID del sistema operativo. " +
               "El resultado se limpia automáticamente al recuperarlo con esperar() o resultado().")]
    public static LatinoObject EjecutarAsync(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.ejecutarAsync requiere el nombre del comando.");

        string cmd     = args[0].ToString();
        string argsStr = "";

        if (args.Count > 1 && args[1] is not LatinoNull)
        {
            if (args[1] is LatinoList lista)
                argsStr = ConstruirArgs(lista);
            else
                argsStr = args[1].ToString();
        }

        try
        {
            var stdoutBuf = new StringBuilder();
            var stderrBuf = new StringBuilder();

            var psi = CrearStartInfo(cmd, argsStr);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError  = true;

            var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

            proc.OutputDataReceived += (_, e) => {
                if (e.Data != null)
                    lock (stdoutBuf) { stdoutBuf.AppendLine(e.Data); }
            };
            proc.ErrorDataReceived += (_, e) => {
                if (e.Data != null)
                    lock (stderrBuf) { stderrBuf.AppendLine(e.Data); }
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // Tarea que espera la terminación del proceso y actualiza el estado
            int idInterno;
            lock (_lockAsync)
            {
                idInterno = _nextId++;
                var entry = new AsyncEntry(proc, stdoutBuf, stderrBuf,
                    Task.Run(() => proc.WaitForExit()));
                _asyncProcs[idInterno] = entry;
            }

            // Encadenar la actualización del estado cuando el proceso termine
            int capturedId = idInterno;
            Task.Run(() => {
                proc.WaitForExit();
                lock (_lockAsync)
                {
                    if (_asyncProcs.TryGetValue(capturedId, out var e))
                    {
                        e.Terminado = true;
                        e.Codigo    = proc.ExitCode;
                    }
                }
            });

            return new LatinoNumber((double)idInterno);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"proceso.ejecutarAsync: {ex.Message}");
        }
    }

    // =========================================================================
    // proceso.esperar(id, [timeout_ms])
    // =========================================================================

    [LatinoExtension("proceso.esperar",
        Args = "id, [timeout_ms]",
        Help = "Bloquea hasta que el proceso asíncrono con el ID interno dado termine. " +
               "Retorna {salida, error, codigo, timeout}. " +
               "El ID se elimina del registro tras recuperar el resultado — " +
               "una segunda llamada con el mismo ID retorna nulo. " +
               "'timeout_ms' defecto: 60000 ms.")]
    public static LatinoObject Esperar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.esperar requiere el ID del proceso asíncrono.");

        int id        = (int)((LatinoNumber)args[0]).Value;
        int timeoutMs = 60_000;
        if (args.Count > 1 && args[1] is LatinoNumber t)
            timeoutMs = (int)t.Value;

        AsyncEntry? entry;
        lock (_lockAsync)
        {
            if (!_asyncProcs.TryGetValue(id, out entry))
                return LatinoNull.Instance; // Ya fue recuperado o ID inválido
        }

        bool terminoATiempo = entry.WaitTask.Wait(timeoutMs);

        if (!terminoATiempo)
        {
            try { entry.Proc.Kill(); } catch { }
        }

        // Esperar a que los buffers de eventos drenen completamente
        try { entry.Proc.WaitForExit(500); } catch { }

        string salida  = entry.StdoutBuffer.ToString().TrimEnd();
        string error   = entry.StderrBuffer.ToString().TrimEnd();
        int    codigo  = terminoATiempo ? entry.Proc.ExitCode : -1;

        lock (_lockAsync) { _asyncProcs.Remove(id); }

        return ConstruirResultado(salida, error, codigo, !terminoATiempo);
    }

    // =========================================================================
    // proceso.terminado(id)
    // =========================================================================

    [LatinoExtension("proceso.terminado",
        Args = "id",
        Help = "Verifica si el proceso asíncrono con el ID interno dado ha terminado. " +
               "Retorna verdadero o falso. No bloqueante. " +
               "Retorna verdadero también si el ID no existe (ya fue recuperado).")]
    public static LatinoObject Terminado(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.terminado requiere el ID del proceso asíncrono.");

        int id = (int)((LatinoNumber)args[0]).Value;

        lock (_lockAsync)
        {
            if (!_asyncProcs.TryGetValue(id, out var entry))
                return new LatinoBoolean(true); // No existe = ya fue recuperado o nunca existió
            return new LatinoBoolean(entry.Terminado);
        }
    }

    // =========================================================================
    // proceso.resultado(id)
    // =========================================================================

    [LatinoExtension("proceso.resultado",
        Args = "id",
        Help = "Recupera el resultado de un proceso asíncrono terminado. " +
               "Retorna {salida, error, codigo} si el proceso terminó, nulo si aún está corriendo. " +
               "El ID se elimina del registro tras recuperar el resultado.")]
    public static LatinoObject Resultado(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.resultado requiere el ID del proceso asíncrono.");

        int id = (int)((LatinoNumber)args[0]).Value;

        AsyncEntry? entry;
        lock (_lockAsync)
        {
            if (!_asyncProcs.TryGetValue(id, out entry))
                return LatinoNull.Instance;

            if (!entry.Terminado)
                return LatinoNull.Instance; // Aún corriendo
        }

        // Esperar a que los buffers drenen
        try { entry.Proc.WaitForExit(500); } catch { }

        string salida = entry.StdoutBuffer.ToString().TrimEnd();
        string error  = entry.StderrBuffer.ToString().TrimEnd();
        int    codigo = entry.Codigo;

        lock (_lockAsync) { _asyncProcs.Remove(id); }

        return ConstruirResultado(salida, error, codigo, false);
    }

    // =========================================================================
    // proceso.listar()
    // =========================================================================

    [LatinoExtension("proceso.listar",
        Help = "Lista los procesos activos del sistema operativo. " +
               "Retorna una lista de diccionarios {pid, nombre, memoria_mb}. " +
               "En Windows puede requerir permisos elevados para ver procesos de otros usuarios.")]
    public static LatinoObject Listar(List<LatinoObject> args)
    {
        try
        {
            var lista   = new List<LatinoObject>();
            var procesos = Process.GetProcesses();

            foreach (var p in procesos)
            {
                try
                {
                    long memoriaMb = 0;
                    try { memoriaMb = p.WorkingSet64 / (1024 * 1024); } catch { }

                    var pares = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
                    {
                        [new LatinoString("pid")]        = new LatinoNumber((double)p.Id),
                        [new LatinoString("nombre")]     = new LatinoString(p.ProcessName),
                        [new LatinoString("memoria_mb")] = new LatinoNumber((double)memoriaMb)
                    };
                    lista.Add(new LatinoDictionary(pares));
                }
                catch { /* Proceso terminó entre la enumeración y la lectura — ignorar */ }
                finally
                {
                    p.Dispose();
                }
            }

            return new LatinoList(lista);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"proceso.listar: {ex.Message}");
        }
    }

    // =========================================================================
    // proceso.matar(pid)
    // =========================================================================

    [LatinoExtension("proceso.matar",
        Args = "pid",
        Help = "Termina un proceso del sistema operativo por su PID real. " +
               "⚠️ Usa SIGKILL en Linux/Mac (sin cleanup graceful) y TerminateProcess en Windows. " +
               "El PID es el identificador del SO, NO el ID interno de ejecutarAsync. " +
               "Lanza error si el PID no existe o si no tienes permisos suficientes.")]
    public static LatinoObject Matar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "proceso.matar requiere el PID del proceso.");

        int pid = (int)((LatinoNumber)args[0]).Value;

        try
        {
            using var proc = Process.GetProcessById(pid);
            proc.Kill();
            return new LatinoBoolean(true);
        }
        catch (ArgumentException)
        {
            throw new RuntimeError(null!, $"proceso.matar: No existe ningún proceso con PID {pid}.");
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeError(null!, $"proceso.matar: El proceso {pid} ya terminó antes de poder matarlo.");
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"proceso.matar: Sin permisos para terminar el proceso {pid}. {ex.Message}");
        }
    }

    // =========================================================================
    // Helpers privados
    // =========================================================================

    private static ProcessStartInfo CrearStartInfo(string cmd, string argsStr)
    {
        return new ProcessStartInfo
        {
            FileName               = cmd,
            Arguments              = argsStr,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
            // Respetar el encoding del terminal del usuario para no corromper
            // caracteres especiales en salidas de comandos del SO
            StandardOutputEncoding = Console.OutputEncoding,
            StandardErrorEncoding  = Console.OutputEncoding
        };
    }

    private static LatinoObject EjecutarInterno(string cmd, string argsStr, int timeoutMs)
    {
        try
        {
            var psi = CrearStartInfo(cmd, argsStr);
            using var proc = new Process { StartInfo = psi };

            proc.Start();

            // Leer stdout y stderr en paralelo para evitar deadlock cuando
            // ambos buffers se llenan y bloquean al proceso hijo.
            // Este es el deadlock clásico de subprocess: si solo leemos stdout
            // de forma síncrona, el proceso hijo puede bloquearse intentando
            // escribir en el buffer de stderr (que nadie está leyendo).
            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var stderrTask = proc.StandardError.ReadToEndAsync();

            bool terminoATiempo = proc.WaitForExit(timeoutMs);

            if (!terminoATiempo)
            {
                try { proc.Kill(); } catch { }
                // Dar 1 segundo para que los buffers de lectura drenen tras el kill
                proc.WaitForExit(1000);
            }

            string salida = stdoutTask.GetAwaiter().GetResult().TrimEnd();
            string error  = stderrTask.GetAwaiter().GetResult().TrimEnd();
            int    codigo = terminoATiempo ? proc.ExitCode : -1;

            return ConstruirResultado(salida, error, codigo, !terminoATiempo);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            // Comando no encontrado en el PATH del sistema
            throw new RuntimeError(null!,
                $"Comando no encontrado: '{cmd}'. " +
                $"Verifica que esté instalado y en el PATH del sistema. ({ex.Message})");
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"proceso.ejecutar: {ex.Message}");
        }
    }

    private static LatinoObject ConstruirResultado(string salida, string error, int codigo, bool timeout)
    {
        var pares = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            [new LatinoString("salida")]  = new LatinoString(salida),
            [new LatinoString("error")]   = new LatinoString(error),
            [new LatinoString("codigo")]  = new LatinoNumber(codigo),
            [new LatinoString("timeout")] = new LatinoBoolean(timeout)
        };
        return new LatinoDictionary(pares);
    }

    private static string ConstruirArgs(LatinoList lista)
    {
        // Cada argumento que contenga espacios se envuelve en comillas dobles.
        // Los argumentos simples se pasan directamente.
        var partes = new List<string>();
        foreach (var item in lista.Elements)
        {
            string s = item.ToString();
            partes.Add(s.Contains(' ') ? $"\"{s}\"" : s);
        }
        return string.Join(" ", partes);
    }
}
