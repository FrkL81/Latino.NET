using System.Collections.Generic;
using Latino.Core.Runtime;

namespace Latino.Core.Types;

public class LatinoClass : LatinoObject, ICallable
{
    public string Name { get; }
    public readonly LatinoClass? Superclass;
    private readonly Dictionary<string, LatinoFunction> _methods;

    public LatinoClass(string name, LatinoClass? superclass, Dictionary<string, LatinoFunction> methods)
    {
        Name = name;
        Superclass = superclass;
        _methods = methods;
    }

    public LatinoFunction? FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out var method)) return method;
        if (Superclass != null) return Superclass.FindMethod(name);
        return null;
    }

    // --- IMPLEMENTACIÓN DE ICALLABLE (LA CLAVE DEL FIX) ---

    // La aridad depende del constructor. Si no hay constructor, es 0.
    public int Arity
    {
        get
        {
            var ctor = FindMethod("constructor");
            return ctor != null ? ctor.Arity : 0;
        }
    }

    public LatinoObject Call(Interpreter interpreter, List<LatinoObject> arguments)
    {
        // 1. Crear la instancia vacía
        var instance = new LatinoInstance(this);

        // 2. Recolectar la jerarquía de herencia usando un Stack (Bottom-Up)
        var hierarchy = new Stack<LatinoClass>();
        var currentClass = this;
        
        while (currentClass != null)
        {
            hierarchy.Push(currentClass);
            currentClass = currentClass.Superclass;
        }

        // 3. Ejecutar constructores (Top-Down: Ancestros primero, finalizando en esta clase)
        while (hierarchy.Count > 0)
        {
            var klass = hierarchy.Pop();
            
            // Usamos directamente _methods para evitar que FindMethod nos traiga el constructor 
            // de un padre si esta clase específica no lo definió.
            if (klass._methods.TryGetValue("constructor", out var ctor))
            {
                // Regla de Resolución de Argumentos:
                // Los ancestros se inicializan sin argumentos (estado por defecto).
                // Solo la clase instanciada (la "hoja") recibe los argumentos del usuario.
                var argsToPass = (klass == this) ? arguments : new List<LatinoObject>();
                
                ctor.Bind(instance).Call(interpreter, argsToPass);
            }
        }

        // 4. Retornar la nueva instancia con su estado completamente ensamblado
        return instance;
    }

    public override object GetValue() => Name;
    public override string ToString() => $"<clase {Name}>";
}
