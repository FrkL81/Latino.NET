using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Latino.Core.Types;
using Latino.Core.Runtime;
using Latino.Core.StdLib;

namespace Latino.Plugin.Hilos;

/// <summary>
/// Plugin de concurrencia para Latino.NET.
/// Implementa hilos, mutex y canales.
/// </summary>
public static class HilosPlugin
{
    private static readonly ConcurrentDictionary<string, Task<LatinoObject>> _tasks = new();
    private static readonly ConcurrentDictionary<string, object> _mutexes = new();
    private static readonly ConcurrentDictionary<string, BlockingCollection<LatinoObject>> _channels = new();

    // =========================================================================
    // SECCIÓN 1: GESTIÓN DE HILOS
    // =========================================================================

    [LatinoExtension("hilos.lanzar", Args = "funcion, [args...]", Help = "Ejecuta una función en un hilo separado.")]
    public static LatinoObject Lanzar(Interpreter i, List<LatinoObject> args)
    {
        if (args.Count == 0 || args[0] is not ICallable func)
            throw new RuntimeError(null!, "hilos.lanzar requiere una función como primer argumento.");

        var funcArgs = args.Count > 1 ? args.GetRange(1, args.Count - 1) : new List<LatinoObject>();
        string id = Guid.NewGuid().ToString();

        // Capturar estado del padre
        var parentOut = i.Out;
        var parentIn = i.In;
        int parentMaxDepth = i.MaxDepth;
        string dllPath = typeof(HilosPlugin).Assembly.Location;

        var task = Task.Run(() =>
        {
            // Nuevo intérprete aislado para el hilo
            var childInterp = new Interpreter
            {
                Out = parentOut,
                In = parentIn,
                MaxDepth = parentMaxDepth
            };

            // Cargar librerías estándar en el nuevo hilo
            StdLibLoader.LoadAll(childInterp);
            
            // Re-cargar este mismo plugin en el hilo hijo para que tenga hilos.*, mutex.* y canal.*
            try {
                PluginLoader.Load(childInterp, dllPath);
            } catch (Exception ex) {
                parentOut.WriteLine($"\n[Error cargando hilos en Hilo {id.Substring(0, 8)}]: {ex.Message}");
            }

            try
            {
                return func.Call(childInterp, funcArgs);
            }
            catch (RuntimeError re)
            {
                string lineInfo = re.Token != null ? $" Lín {re.Token.Line}" : "";
                parentOut.WriteLine($"\n[Error en Hilo {id.Substring(0, 8)}{lineInfo}]: {re.Message}");
                return LatinoNull.Instance;
            }
            catch (Exception ex)
            {
                parentOut.WriteLine($"\n[Error Crítico en Hilo {id.Substring(0, 8)}]: {ex.Message}");
                return LatinoNull.Instance;
            }
        });

        _tasks[id] = task;
        return new LatinoString(id);
    }

    [LatinoExtension("hilos.esperar", Args = "id, [timeoutMs]", Help = "Espera a que un hilo termine. Con timeout, retorna nulo si no termina a tiempo (el hilo sigue vivo).")]
    public static LatinoObject Esperar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "hilos.esperar requiere el ID del hilo.");

        string id = args[0].ToString();
        int timeoutMs = (args.Count > 1 && args[1] is LatinoNumber ms) ? (int)ms.Value : Timeout.Infinite;

        if (!_tasks.TryGetValue(id, out var task))
            throw new RuntimeError(null!, $"Hilo no encontrado o ya finalizado: '{id}'");

        bool completed;
        try
        {
            completed = task.Wait(timeoutMs);
        }
        catch (AggregateException ex)
        {
            // El hilo falló — limpiar siempre para no filtrar memoria
            _tasks.TryRemove(id, out _);
            throw new RuntimeError(null!, $"Error en el hilo: {ex.InnerException?.Message ?? ex.Message}");
        }

        // Timeout: el hilo sigue vivo, conservar el ID para futura espera
        if (!completed) return LatinoNull.Instance;

        // Completado: liberar el registro
        _tasks.TryRemove(id, out _);

        if (task.IsFaulted && task.Exception != null)
            throw new RuntimeError(null!, $"Error en el hilo: {task.Exception.InnerException?.Message ?? task.Exception.Message}");

        return task.Result;
    }

    [LatinoExtension("hilos.esperarTodos", Args = "lista_ids", Help = "Espera a que todos los hilos en la lista terminen. Retorna lista con resultados (nulo para hilos fallidos).")]
    public static LatinoObject EsperarTodos(List<LatinoObject> args)
    {
        if (args.Count == 0 || args[0] is not LatinoList list)
            throw new RuntimeError(null!, "hilos.esperarTodos requiere una lista de IDs.");

        var results = new List<LatinoObject>();
        foreach (var idObj in list.Elements)
        {
            string id = idObj.ToString();
            if (!_tasks.TryGetValue(id, out var task))
            {
                results.Add(LatinoNull.Instance);
                continue;
            }

            try
            {
                task.Wait();
            }
            catch
            {
                // No relanzar: procesar todos los hilos y retornar nulo para los fallidos
            }
            finally
            {
                // SIEMPRE liberar el registro, haya fallado o no
                _tasks.TryRemove(id, out _);
            }

            results.Add(task.IsFaulted ? LatinoNull.Instance : task.Result);
        }
        return new LatinoList(results);
    }

    [LatinoExtension("hilos.estaVivo", Args = "id", Help = "Verifica si un hilo sigue en ejecución.")]
    public static LatinoObject EstaVivo(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "hilos.estaVivo requiere el ID del hilo.");
        string id = args[0].ToString();

        if (_tasks.TryGetValue(id, out var task))
            return new LatinoBoolean(!task.IsCompleted);

        return new LatinoBoolean(false);
    }

    [LatinoExtension("hilos.limpiar", Help = "Limpia la tabla de hilos finalizados.")]
    public static LatinoObject Limpiar(List<LatinoObject> args)
    {
        var completedIds = _tasks.Keys.Where(id => _tasks[id].IsCompleted).ToList();
        foreach (var id in completedIds) _tasks.TryRemove(id, out _);
        return new LatinoNumber(completedIds.Count);
    }
    [LatinoExtension("hilos.dormir", Args = "ms", Help = "Pausa la ejecución del hilo actual por N milisegundos.")]
    public static LatinoObject Dormir(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "hilos.dormir requiere milisegundos.");
        int ms = (int)((LatinoNumber)args[0]).Value;
        if (ms > 0) Thread.Sleep(ms);
        return LatinoNull.Instance;
    }

    // =========================================================================
    // SECCIÓN 2: MUTEX
    // =========================================================================

    [LatinoExtension("mutex.crear", Help = "Crea un nuevo objeto mutex.")]
    public static LatinoObject MutexCrear(List<LatinoObject> args)
    {
        string id = Guid.NewGuid().ToString();
        _mutexes[id] = new object();
        return new LatinoString(id);
    }

    [LatinoExtension("mutex.bloquear", Args = "id", Help = "Adquiere el bloqueo de un mutex.")]
    public static LatinoObject MutexBloquear(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "mutex.bloquear requiere el ID del mutex.");
        string id = args[0].ToString();

        if (!_mutexes.TryGetValue(id, out var mtx))
            throw new RuntimeError(null!, $"Mutex no encontrado: '{id}'");

        Monitor.Enter(mtx);
        return LatinoNull.Instance;
    }

    [LatinoExtension("mutex.liberar", Args = "id", Help = "Libera el bloqueo de un mutex.")]
    public static LatinoObject MutexLiberar(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "mutex.liberar requiere el ID del mutex.");
        string id = args[0].ToString();

        if (!_mutexes.TryGetValue(id, out var mtx))
            throw new RuntimeError(null!, $"Mutex no encontrado: '{id}'");

        try
        {
            Monitor.Exit(mtx);
        }
        catch (SynchronizationLockException)
        {
            throw new RuntimeError(null!, "mutex.liberar: Este hilo no es el propietario del candado.");
        }
        return LatinoNull.Instance;
    }

    [LatinoExtension("mutex.eliminar", Args = "id", Help = "Elimina un mutex de la memoria. Lanza error si está bloqueado actualmente.")]
    public static LatinoObject MutexEliminar(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "mutex.eliminar requiere el ID del mutex.");
        string id = args[0].ToString();

        if (!_mutexes.TryGetValue(id, out var mtx))
            return new LatinoBoolean(false); // Ya no existe, operación idempotente

        // Intentar adquirir el bloqueo sin esperar para verificar si está libre
        bool estaLibre = Monitor.TryEnter(mtx, 0);
        if (!estaLibre)
            throw new RuntimeError(null!,
                "mutex.eliminar: No se puede eliminar un mutex que está bloqueado. " +
                "Libéralo antes de eliminarlo.");

        // Liberar inmediatamente (solo lo tomamos para verificar)
        Monitor.Exit(mtx);
        _mutexes.TryRemove(id, out _);
        return new LatinoBoolean(true);
    }

    // =========================================================================
    // SECCIÓN 3: CANALES
    // =========================================================================

    [LatinoExtension("canal.crear", Help = "Crea un nuevo canal de comunicación.")]
    public static LatinoObject CanalCrear(List<LatinoObject> args)
    {
        string id = Guid.NewGuid().ToString();
        _channels[id] = new BlockingCollection<LatinoObject>();
        return new LatinoString(id);
    }

    [LatinoExtension("canal.enviar", Args = "id, mensaje", Help = "Envía un mensaje a un canal.")]
    public static LatinoObject CanalEnviar(List<LatinoObject> args)
    {
        if (args.Count < 2) throw new RuntimeError(null!, "canal.enviar requiere ID y mensaje.");
        string id = args[0].ToString();

        if (!_channels.TryGetValue(id, out var ch))
            throw new RuntimeError(null!, $"Canal no encontrado: '{id}'");

        try
        {
            ch.Add(args[1]);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeError(null!, $"canal.enviar: El canal '{id}' está cerrado.");
        }
        return LatinoNull.Instance;
    }

    [LatinoExtension("canal.recibir", Args = "id, [timeoutMs]", Help = "Recibe un mensaje de un canal.")]
    public static LatinoObject CanalRecibir(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "canal.recibir requiere ID.");
        string id = args[0].ToString();
        int timeoutMs = (args.Count > 1 && args[1] is LatinoNumber ms) ? (int)ms.Value : Timeout.Infinite;

        if (!_channels.TryGetValue(id, out var ch))
            throw new RuntimeError(null!, $"Canal no encontrado: '{id}'");

        try
        {
            if (ch.TryTake(out var item, timeoutMs)) return item;
            return LatinoNull.Instance;
        }
        catch (OperationCanceledException)
        {
            return LatinoNull.Instance;
        }
    }

    [LatinoExtension("canal.cerrar", Args = "id", Help = "Cierra un canal.")]
    public static LatinoObject CanalCerrar(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "canal.cerrar requiere ID.");
        string id = args[0].ToString();

        if (!_channels.TryGetValue(id, out var ch))
            throw new RuntimeError(null!, $"Canal no encontrado: '{id}'");

        try
        {
            ch.CompleteAdding();
        }
        catch (ObjectDisposedException) { }
        return LatinoNull.Instance;
    }

    [LatinoExtension("canal.eliminar", Args = "id", Help = "Cierra y devuelve un canal de la memoria liberando sus recursos.")]
    public static LatinoObject CanalEliminar(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "canal.eliminar requiere ID.");
        string id = args[0].ToString();

        if (!_channels.TryRemove(id, out var ch))
            return new LatinoBoolean(false); // Ya no existe, operación idempotente

        try
        {
            // Señalar que no habrá más mensajes — desbloquea a los consumidores bloqueados
            ch.CompleteAdding();
        }
        catch (ObjectDisposedException) { /* Ya estaba cerrado */ }

        // Dar tiempo mínimo para que consumidores bloqueados reaccionen al CompleteAdding
        // antes de disponer el objeto subyacente
        Thread.Sleep(10);

        try
        {
            ch.Dispose();
        }
        catch (ObjectDisposedException) { /* Ya fue dispuesto */ }

        return new LatinoBoolean(true);
    }
}
