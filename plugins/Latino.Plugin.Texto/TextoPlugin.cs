using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Plugin.Texto;

public static class TextoPlugin
{
    // =========================================================================
    // Wrapper thread-safe para StringBuilder
    // =========================================================================
    // ConcurrentDictionary protege el ACCESO al diccionario (agregar/remover claves),
    // pero NO las operaciones sobre el StringBuilder en sí. Si dos hilos del plugin
    // Hilos llaman texto.anexar con el mismo ID simultáneamente, el StringBuilder
    // interno se corrompería sin error visible.
    // BufferEntry resuelve esto llevando su propio lock por instancia.

    private sealed class BufferEntry
    {
        private readonly object        _lock = new();
        private readonly StringBuilder _sb;

        public BufferEntry(int capacidadInicial = 256)
            => _sb = new StringBuilder(capacidadInicial);

        public void   Append(string valor)        { lock (_lock) { _sb.Append(valor); } }
        public void   AppendLine(string valor)    { lock (_lock) { _sb.Append(valor); _sb.AppendLine(); } }
        public void   AppendLine()                { lock (_lock) { _sb.AppendLine(); } }
        public void   Replace(string viejo, string nuevo) { lock (_lock) { _sb.Replace(viejo, nuevo); } }
        public void   Clear()                     { lock (_lock) { _sb.Clear(); } }
        public string GetString()                 { lock (_lock) { return _sb.ToString(); } }
        public int    Length                      { get { lock (_lock) { return _sb.Length; } } }
    }

    private static readonly ConcurrentDictionary<string, BufferEntry> _buffers = new();

    // =========================================================================
    // Helpers
    // =========================================================================

    private static BufferEntry ObtenerBuffer(string id)
    {
        if (!_buffers.TryGetValue(id, out var entry))
            throw new RuntimeError(null!,
                $"texto: Constructor no encontrado o ya eliminado: '{id}'. " +
                "Verifica que llamaste texto.crear() y no usaste texto.eliminar() antes.");
        return entry;
    }

    // =========================================================================
    // texto.crear([capacidad_inicial])
    // =========================================================================

    [LatinoExtension("texto.crear",
        Args = "[capacidad_inicial]",
        Help = "Crea un nuevo constructor de texto mutable y retorna su ID único. " +
               "El parámetro opcional 'capacidad_inicial' pre-reserva memoria en bytes " +
               "para evitar realocaciones cuando se conoce el tamaño aproximado del resultado. " +
               "Ejemplo: texto.crear(1048576) reserva 1 MB desde el inicio.")]
    public static LatinoObject Crear(List<LatinoObject> args)
    {
        int capacidad = 256; // Default razonable para uso general
        if (args.Count > 0 && args[0] is LatinoNumber n)
        {
            if (n.Value < 1)
                throw new RuntimeError(null!, "texto.crear: La capacidad inicial debe ser al menos 1.");
            capacidad = (int)n.Value;
        }

        string id = Guid.NewGuid().ToString();
        _buffers[id] = new BufferEntry(capacidad);
        return new LatinoString(id);
    }

    // =========================================================================
    // texto.anexar(id, valores...)
    // =========================================================================

    [LatinoExtension("texto.anexar",
        Args = "id, valores...",
        Help = "Añade uno o más valores al final del texto actual sin salto de línea. " +
               "Acepta múltiples valores: texto.anexar(t, a, b, c) es más eficiente " +
               "que texto.anexar(t, a .. b .. c) porque evita strings intermedios. " +
               "Retorna el ID para permitir encadenamiento fluido con el operador Pipeline (|>). " +
               "Thread-safe: múltiples hilos pueden escribir al mismo buffer con seguridad.")]
    public static LatinoObject Anexar(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!,
                "texto.anexar requiere el ID y al menos un valor. " +
                "Ejemplo: texto.anexar(t, \"hola\")");

        var buffer = ObtenerBuffer(args[0].ToString());
        for (int i = 1; i < args.Count; i++)
            buffer.Append(args[i].ToString());

        return args[0]; // Retorna el ID para encadenamiento fluido (|>)
    }

    // =========================================================================
    // texto.anexarLinea(id, [valores...])
    // =========================================================================

    [LatinoExtension("texto.anexarLinea",
        Args = "id, [valores...]",
        Help = "Añade valores opcionales seguidos de un salto de línea. " +
               "texto.anexarLinea(t) sin valores extra añade solo el salto de línea. " +
               "Retorna el ID para permitir encadenamiento fluido con el operador Pipeline (|>). " +
               "Usa el salto de línea del sistema operativo actual (\\r\\n en Windows, \\n en Linux/Mac).")]
    public static LatinoObject AnexarLinea(List<LatinoObject> args)
    {
        if (args.Count < 1)
            throw new RuntimeError(null!, "texto.anexarLinea requiere al menos el ID.");

        var buffer = ObtenerBuffer(args[0].ToString());

        if (args.Count > 1)
        {
            for (int i = 1; i < args.Count; i++)
                buffer.Append(args[i].ToString());
        }

        buffer.AppendLine();
        return args[0]; // Retorna el ID para encadenamiento fluido (|>)
    }

    // =========================================================================
    // texto.reemplazar(id, viejo, nuevo)
    // =========================================================================

    [LatinoExtension("texto.reemplazar",
        Args = "id, viejo, nuevo",
        Help = "Reemplaza todas las ocurrencias de 'viejo' por 'nuevo' DENTRO del buffer, " +
               "sin materializar el string completo. Ideal para rellenar plantillas con " +
               "placeholders: texto.reemplazar(t, \"{{NOMBRE}}\", nombre_usuario). " +
               "Retorna el ID para permitir encadenamiento fluido con el operador Pipeline (|>). " +
               "Más eficiente que obtener() → cadena.reemplazar() → limpiar() → anexar().")]
    public static LatinoObject Reemplazar(List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!,
                "texto.reemplazar requiere 3 argumentos: id, viejo, nuevo. " +
                "Ejemplo: texto.reemplazar(t, \"{{X}}\", valor)");

        var buffer = ObtenerBuffer(args[0].ToString());
        string viejo = args[1].ToString();
        string nuevo  = args[2].ToString();

        if (string.IsNullOrEmpty(viejo))
            throw new RuntimeError(null!, "texto.reemplazar: El valor 'viejo' no puede estar vacío.");

        buffer.Replace(viejo, nuevo);
        return args[0]; // Retorna el ID para encadenamiento fluido (|>)
    }

    // =========================================================================
    // texto.longitud(id)
    // =========================================================================

    [LatinoExtension("texto.longitud",
        Args = "id",
        Help = "Retorna la longitud actual del texto en el buffer en O(1), " +
               "sin materializar el string completo. Útil para verificar límites " +
               "de tamaño durante la construcción: si texto.longitud(t) > 1000000 entonces...")]
    public static LatinoObject Longitud(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "texto.longitud requiere el ID.");

        var buffer = ObtenerBuffer(args[0].ToString());
        return new LatinoNumber(buffer.Length);
    }

    // =========================================================================
    // texto.obtener(id)
    // =========================================================================

    [LatinoExtension("texto.obtener",
        Args = "id",
        Help = "Materializa y retorna como cadena inmutable todo el texto construido. " +
               "Esta es la única operación que crea un nuevo string en memoria. " +
               "Llámala solo cuando necesites el resultado final.")]
    public static LatinoObject Obtener(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "texto.obtener requiere el ID.");

        return new LatinoString(ObtenerBuffer(args[0].ToString()).GetString());
    }

    // =========================================================================
    // texto.limpiar(id)
    // =========================================================================

    [LatinoExtension("texto.limpiar",
        Args = "id",
        Help = "Vacía el contenido del buffer sin destruir el objeto ni liberar la memoria " +
               "reservada. Útil para reutilizar el buffer en múltiples pasadas " +
               "sin el overhead de crear() y eliminar() repetidamente. " +
               "Retorna el ID para permitir encadenamiento fluido con el operador Pipeline (|>).")]
    public static LatinoObject Limpiar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "texto.limpiar requiere el ID.");

        ObtenerBuffer(args[0].ToString()).Clear();
        return args[0]; // Retorna el ID para encadenamiento fluido (|>)
    }

    // =========================================================================
    // texto.eliminar(id)
    // =========================================================================

    [LatinoExtension("texto.eliminar",
        Args = "id",
        Help = "Destruye el buffer y lo elimina del registro, liberando su memoria. " +
               "Retorna verdadero si el buffer existía, falso si ya había sido eliminado. " +
               "Llámalo siempre al terminar de usar el buffer para evitar fugas de memoria.")]
    public static LatinoObject Eliminar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null!, "texto.eliminar requiere el ID.");

        string id = args[0].ToString();
        if (_buffers.TryRemove(id, out _))
            return new LatinoBoolean(true);

        return new LatinoBoolean(false); // Idempotente: falso si ya estaba eliminado
    }
}
