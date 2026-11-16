using System;
using System.Collections.Generic;
using Latino.Core.Runtime;

namespace Latino.Core.Types;

public class NativeFunction : LatinoObject, ICallable
{
    // El delegado ahora recibe (Interpreter, Args) -> Resultado
    private readonly Func<Interpreter, List<LatinoObject>, LatinoObject> _nativeCall;
    public int Arity { get; }
    
    // NUEVO: Metadatos para el LSP
    public string Description { get; set; } = "";
    public string ParameterNames { get; set; } = ""; // Ej: "a, b"

    public NativeFunction(Func<Interpreter, List<LatinoObject>, LatinoObject> nativeCall, int arity = -1)
    {
        _nativeCall = nativeCall;
        Arity = arity;
    }

    // Sobrecarga para cuando no nos importa el interpreter (facilita MathLib)
    public NativeFunction(Func<List<LatinoObject>, LatinoObject> simpleCall, int arity = -1)
    {
        _nativeCall = (_, args) => simpleCall(args);
        Arity = arity;
    }

    // Método fluido para añadir documentación fácilmente
    public NativeFunction WithDocs(string desc, string paramsList = "")
    {
        Description = desc;
        ParameterNames = paramsList;
        return this;
    }

    public LatinoObject Call(Interpreter interpreter, List<LatinoObject> arguments)
    {
        // ═══════════════════════════════════════════════════════════════════
        // VALIDACIÓN AUTOMÁTICA DE ARIDAD
        // ═══════════════════════════════════════════════════════════════════
        if (Arity >= 0) // Aridad fija (0, 1, 2, 3...)
        {
            if (arguments.Count < Arity)
            {
                // Construir nombre de función para el mensaje de error
                string nombreFuncion = "La función";
                
                // Intentar extraer nombre desde ParameterNames si está disponible
                // Formato típico: "numero" o "base, exponente"
                if (!string.IsNullOrEmpty(ParameterNames))
                {
                    // No podemos inferir el nombre real desde aquí, usar genérico
                    nombreFuncion = "Esta función";
                }
                
                throw new RuntimeError(null!, 
                    $"{nombreFuncion} requiere al menos {Arity} argumento(s), pero se recibieron {arguments.Count}.");
            }
        }
        // Para Arity == -1 (variadic): No validar aquí.
        // Las funciones variadic que necesiten mínimos deben validar internamente.
        
        return _nativeCall(interpreter, arguments);
    }
    
    public override object GetValue() => "<funcion nativa>";
    public override string TypeName => "funcion";
}
