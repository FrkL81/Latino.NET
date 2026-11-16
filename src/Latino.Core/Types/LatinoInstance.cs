using System.Collections.Generic;
using Latino.Core.Analysis;
using Latino.Core.Runtime;

namespace Latino.Core.Types;

public class LatinoInstance : LatinoObject
{
    private readonly LatinoClass _klass;
    private readonly Dictionary<string, LatinoObject> _fields = new();

    public LatinoInstance(LatinoClass klass)
    {
        _klass = klass;
    }

    public LatinoObject Get(Token name)
    {
        // 1. Buscar en los campos de la instancia (estado)
        if (_fields.TryGetValue(name.Text, out var value))
        {
            return value;
        }

        // 2. Buscar método en la clase (comportamiento)
        var method = _klass.FindMethod(name.Text);
        if (method != null)
        {
            return method.Bind(this); // Enlazamos 'mi' a esta instancia
        }

        // 3. Si no existe nada, devolvemos nulo (comportamiento dinámico)
        return LatinoNull.Instance;
    }

    public LatinoObject Set(Token name, LatinoObject value)
    {
        _fields[name.Text] = value;
        return value;
    }

    // FIX: Retornar el nombre de la clase generadora (ej: "Perro")
    public override string TypeName => _klass.Name;

    public override object GetValue() => "instancia de " + _klass.Name;
    public override string ToString() => $"<instancia {_klass.Name}>";

    // --- LÓGICA DE IGUALDAD POR REFERENCIA (POO) ---
    // Las instancias de clase se comparan por su dirección en memoria, no por sus cadenas de texto.
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }
}
