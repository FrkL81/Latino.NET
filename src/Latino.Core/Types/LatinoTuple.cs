using System;
using System.Collections.Generic;
using System.Linq;
using Latino.Core.Runtime;

namespace Latino.Core.Types;

public class LatinoTuple : LatinoObject
{
    public List<LatinoObject> Elements { get; }

    public LatinoTuple(List<LatinoObject> elements)
    {
        Elements = elements;
    }

    // --- LOGICA DE IGUALDAD (CRÍTICO) ---
    // --- LÓGICA DE IGUALDAD ESTRUCTURAL (PROTEGIDA CONTRA CICLOS) ---
    [ThreadStatic] private static HashSet<object>? _activeEqualsTuple;
    [ThreadStatic] private static HashSet<object>? _activeHashTuple;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not LatinoTuple other) return false;
        if (Elements.Count != other.Elements.Count) return false;

        _activeEqualsTuple ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!_activeEqualsTuple.Add(this)) return true;

        try
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                if (!Elements[i].Equals(other.Elements[i])) return false;
            }
            return true;
        }
        finally
        {
            _activeEqualsTuple.Remove(this);
        }
    }

    public override int GetHashCode()
    {
        _activeHashTuple ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!_activeHashTuple.Add(this)) 
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        try
        {
            var hash = new System.HashCode();
            foreach (var item in Elements) hash.Add(item);
            return hash.ToHashCode();
        }
        finally
        {
            _activeHashTuple.Remove(this);
        }
    }
    // ------------------------------------

    public override object GetValue() => Elements;
    public override string TypeName => "tupla";
    
    public override string ToString()
    {
        return "(" + string.Join(", ", Elements.Select(e => e.ToString())) + ")";
    }

    public LatinoObject Get(int index)
    {
        if (index >= 0 && index < Elements.Count)
        {
            return Elements[index];
        }
        throw new Runtime.RuntimeError(null!, $"Índice de tupla fuera de rango: {index}");
    }
}
