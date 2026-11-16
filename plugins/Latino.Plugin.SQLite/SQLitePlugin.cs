using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.SQLite;

public static class SQLitePlugin
{
    private static readonly ConcurrentDictionary<string, SqliteConnection> _conexiones = new();

    // Rastreador de transacciones activas por conexión.
    // Permite que Ejecutar() y Consultar() participen en la transacción
    // correcta sin requerir que el usuario la pase como argumento.
    private static readonly ConcurrentDictionary<string, SqliteTransaction> _transacciones = new();

    // Índice inverso: ruta normalizada → ID de conexión activa.
    // Previene que el mismo archivo sea abierto dos veces simultáneamente.
    private static readonly ConcurrentDictionary<string, string> _rutasActivas = new(StringComparer.OrdinalIgnoreCase);

    private static void AsignarTransaccionSiActiva(SqliteCommand comando, string id)
    {
        if (_transacciones.TryGetValue(id, out var tx))
            comando.Transaction = tx;
    }

    [LatinoExtension("sqlite.conectar", Args = "ruta", Help = "Abre o crea una base de datos SQLite y retorna su ID de conexión.")]
    public static LatinoObject Conectar(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "sqlite.conectar requiere la ruta del archivo .db.");

        SQLitePCL.Batteries_V2.Init();

        string ruta = args[0].ToString().Trim();
        string connectionString;
        bool esMemoria = string.Equals(ruta, ":memory:", StringComparison.OrdinalIgnoreCase);

        if (esMemoria)
        {
            connectionString = "Data Source=:memory:;";
        }
        else
        {
            // Normalizar la ruta para comparar independientemente de ./ o mayúsculas
            ruta = System.IO.Path.GetFullPath(ruta);

            // Verificar si ya hay una conexión abierta para este archivo
            if (_rutasActivas.TryGetValue(ruta, out var idExistente))
                throw new RuntimeError(null!,
                    $"El archivo '{System.IO.Path.GetFileName(ruta)}' ya tiene una conexión abierta (ID: {idExistente}). " +
                    "Cierra la conexión existente antes de abrir una nueva.");

            connectionString = $"Data Source={ruta};";
        }

        var conexion = new SqliteConnection(connectionString);
        conexion.Open();

        string id = Guid.NewGuid().ToString();
        _conexiones[id] = conexion;
        if (!esMemoria)
        {
            _rutasActivas[ruta] = id; // Registrar ruta como ocupada
        }

        return new LatinoString(id);
    }

    [LatinoExtension("sqlite.cerrar", Args = "id", Help = "Cierra la conexión y libera el archivo inmediatamente.")]
    public static LatinoObject Cerrar(List<LatinoObject> args)
    {
        string id = args[0].ToString();
        if (_conexiones.TryRemove(id, out var conexion))
        {
            conexion.Close();
            conexion.Dispose();
            
            // Liberar la ruta en el índice inverso
            var rutaOcupada = _rutasActivas.FirstOrDefault(kvp => kvp.Value == id).Key;
            if (rutaOcupada != null) _rutasActivas.TryRemove(rutaOcupada, out _);
            
            // Forzar la liberación del pool para este string de conexión.
            // Sin esto, .NET mantiene el archivo bloqueado aunque la conexión
            // esté "cerrada", impidiendo borrarlo o moverlo desde el SO.
            SqliteConnection.ClearAllPools();
            
            return new LatinoBoolean(true);
        }
        return new LatinoBoolean(false);
    }

    [LatinoExtension("sqlite.ejecutar", Args = "id, query, [parametros]", Help = "Ejecuta una consulta (INSERT, UPDATE, CREATE) y devuelve las filas afectadas.")]
    public static LatinoObject Ejecutar(List<LatinoObject> args)
    {
        if (args.Count < 2) throw new RuntimeError(null!, "Faltan argumentos.");
        string id = args[0].ToString();
        string sql = args[1].ToString();
        LatinoDictionary? parametros = args.Count > 2 ? args[2] as LatinoDictionary : null;

        if (!_conexiones.TryGetValue(id, out var conexion)) throw new RuntimeError(null!, "Conexión inválida.");

        try {
            using var comando = conexion.CreateCommand();
            comando.CommandText = sql;
            AsignarTransaccionSiActiva(comando, id);
            PrepararParametros(comando, parametros);
            return new LatinoNumber(comando.ExecuteNonQuery());
        } catch (Exception ex) { throw new RuntimeError(null!, $"Error SQL: {ex.Message}"); }
    }

    [LatinoExtension("sqlite.consultar", Args = "id, query, [parametros]", Help = "Ejecuta un SELECT y retorna una lista de diccionarios.")]
    public static LatinoObject Consultar(List<LatinoObject> args)
    {
        if (args.Count < 2) throw new RuntimeError(null!, "Faltan argumentos.");
        string id = args[0].ToString();
        string sql = args[1].ToString();
        LatinoDictionary? parametros = args.Count > 2 ? args[2] as LatinoDictionary : null;

        if (!_conexiones.TryGetValue(id, out var conexion)) throw new RuntimeError(null!, "Conexión inválida.");

        try {
            using var comando = conexion.CreateCommand();
            comando.CommandText = sql;
            AsignarTransaccionSiActiva(comando, id);
            PrepararParametros(comando, parametros);
            using var lector = comando.ExecuteReader();
            var resultados = new List<LatinoObject>();

            while (lector.Read()) {
                var fila = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
                for (int i = 0; i < lector.FieldCount; i++) {
                    fila[new LatinoString(lector.GetName(i))] = ConvertirALatino(lector.GetValue(i));
                }
                resultados.Add(new LatinoDictionary(fila));
            }
            return new LatinoList(resultados);
        } catch (Exception ex) { throw new RuntimeError(null!, $"Error SQL: {ex.Message}"); }
    }

    [LatinoExtension("sqlite.ultimo_id", Args = "id", Help = "Devuelve el ID de la última fila insertada en esta conexión.")]
    public static LatinoObject UltimoId(List<LatinoObject> args)
    {
        string id = args[0].ToString();
        if (!_conexiones.TryGetValue(id, out var conexion)) throw new RuntimeError(null!, "Conexión inválida.");
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT last_insert_rowid()";
        return new LatinoNumber(Convert.ToInt64(cmd.ExecuteScalar()));
    }

    [LatinoExtension("sqlite.transaccion", Args = "id, funcion_cuerpo", Help = "Ejecuta una función dentro de una transacción atómica con rollback automático.")]
    public static LatinoObject Transaccion(Interpreter interp, List<LatinoObject> args)
    {
        string id = args[0].ToString();
        if (args[1] is not ICallable funcion) throw new RuntimeError(null!, "Se requiere una función callback.");
        if (!_conexiones.TryGetValue(id, out var conexion)) throw new RuntimeError(null!, "Conexión inválida.");

        using var tx = conexion.BeginTransaction();
        _transacciones[id] = tx; // Registrar transacción activa para este ID

        try
        {
            var resultado = funcion.Call(interp, new List<LatinoObject> { new LatinoString(id) });
            tx.Commit();
            return resultado;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
        finally
        {
            // Siempre limpiar la transacción activa, tanto en éxito como en error
            _transacciones.TryRemove(id, out _);
        }
    }

    // --- HELPERS ---
    private static void PrepararParametros(SqliteCommand comando, LatinoDictionary? parametros)
    {
        if (parametros == null) return;
        foreach (var kvp in parametros.Pairs) {
            string nombre = kvp.Key.ToString();
            if (!nombre.StartsWith("@") && !nombre.StartsWith("$") && !nombre.StartsWith(":")) nombre = "@" + nombre;
            
            object valor = kvp.Value switch {
                LatinoNumber n => n.Value,
                LatinoString s => s.Value,
                LatinoBoolean b => b.Value ? 1 : 0, // SQLite usa 1/0 para booleanos
                LatinoNull => DBNull.Value,
                _ => kvp.Value.ToString()
            };
            comando.Parameters.AddWithValue(nombre, valor);
        }
    }

    private static LatinoObject ConvertirALatino(object valor)
    {
        if (valor == DBNull.Value || valor == null) return LatinoNull.Instance;
        
        // Cubrir todos los tipos numéricos que SQLite y .NET pueden retornar,
        // incluyendo los que vienen de operaciones de agregación (AVG, SUM, etc.)
        if (valor is long l)    return new LatinoNumber(l);
        if (valor is double d)  return new LatinoNumber(d);
        if (valor is int i)     return new LatinoNumber(i);
        if (valor is float f)   return new LatinoNumber(f);
        if (valor is decimal dec) return new LatinoNumber((double)dec);
        if (valor is short s)   return new LatinoNumber(s);
        if (valor is byte b)    return new LatinoNumber(b);
        
        return new LatinoString(valor.ToString() ?? "");
    }
}
