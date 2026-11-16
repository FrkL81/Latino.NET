using System.Collections.Generic;
using System.Linq;

namespace Latino.Core.Types;

public class LatinoList : LatinoObject
{
    public readonly List<LatinoObject> Elements;

    public LatinoList(List<LatinoObject> elements)
    {
        Elements = elements;
    }

    public override object GetValue() => Elements;
    public override string TypeName => "lista";
    public override string ToString()
    {
        return ToStringSafe(new HashSet<LatinoObject>(ReferenceEqualityComparer.Instance));
    }

    public string ToStringSafe(HashSet<LatinoObject> visited)
    {
        // 1. Detección de Ciclo
        if (visited.Contains(this)) return "[...]";
        
        // 2. Registrar visita
        visited.Add(this);

        try
        {
            var items = Elements.Select(e => 
            {
                // Si el elemento es una colección, usamos la versión segura pasando el historial
                if (e is LatinoList l) return l.ToStringSafe(visited);
                if (e is LatinoDictionary d) return d.ToStringSafe(visited);
                // Si es primitivo, ToString normal
                return e.ToString();
            });
            
            return "[" + string.Join(", ", items) + "]";
        }
        finally
        {
            // 3. Limpiar visita al salir (para permitir estructuras tipo árbol no cíclicas)
            visited.Remove(this);
        }
    }

    // --- LÓGICA DE IGUALDAD ESTRUCTURAL (PROTEGIDA CONTRA CICLOS) ---
    [ThreadStatic] private static HashSet<object>? _activeEqualsList;
    [ThreadStatic] private static HashSet<object>? _activeHashList;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not LatinoList other) return false;
        if (Elements.Count != other.Elements.Count) return false;

        _activeEqualsList ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!_activeEqualsList.Add(this)) return true; // Asumir igualdad al detectar ciclo

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
            _activeEqualsList.Remove(this);
        }
    }

    public override int GetHashCode()
    {
        _activeHashList ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!_activeHashList.Add(this)) 
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this); // Retornar hash de referencia en ciclo

        try
        {
            var hash = new System.HashCode();
            foreach (var item in Elements) hash.Add(item);
            return hash.ToHashCode();
        }
        finally
        {
            _activeHashList.Remove(this);
        }
    }
}
