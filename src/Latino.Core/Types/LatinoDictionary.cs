using System.Collections.Generic;
using System.Linq;

namespace Latino.Core.Types;

public class LatinoDictionary : LatinoObject
{
    public Dictionary<LatinoObject, LatinoObject> Pairs { get; }
        = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);

    // Flag de seguridad para librerías nativas
    public bool IsReadOnly { get; private set; }

    public LatinoDictionary(Dictionary<LatinoObject, LatinoObject> pairs, bool isReadOnly = false)
    {
        Pairs = new Dictionary<LatinoObject, LatinoObject>(pairs, LatinoObjectComparer.Instance);
        IsReadOnly = isReadOnly;
    }

    // Método para sellar el objeto
    public void MakeReadOnly()
    {
        IsReadOnly = true;
    }

    public override object GetValue() => Pairs;
    public override string TypeName => "diccionario";
    public override string ToString()
    {
        return ToStringSafe(new HashSet<LatinoObject>(ReferenceEqualityComparer.Instance));
    }

    public string ToStringSafe(HashSet<LatinoObject> visited)
    {
        if (visited.Contains(this)) return "{...}";
        
        visited.Add(this);

        try
        {
            var items = Pairs.Select(kv => 
            {
                string kStr = kv.Key is LatinoList lk ? lk.ToStringSafe(visited) :
                              kv.Key is LatinoDictionary dk ? dk.ToStringSafe(visited) : 
                              kv.Key.ToString();

                string vStr = kv.Value is LatinoList lv ? lv.ToStringSafe(visited) :
                              kv.Value is LatinoDictionary dv ? dv.ToStringSafe(visited) : 
                              kv.Value.ToString();

                return $"{kStr}: {vStr}";
            });

            return "{" + string.Join(", ", items) + "}";
        }
        finally
        {
            visited.Remove(this);
        }
    }

    // --- LÓGICA DE IGUALDAD ESTRUCTURAL (PROTEGIDA CONTRA CICLOS) ---
    [ThreadStatic] private static HashSet<object>? _activeEqualsDict;
    [ThreadStatic] private static HashSet<object>? _activeHashDict;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not LatinoDictionary other) return false;
        if (Pairs.Count != other.Pairs.Count) return false;

        _activeEqualsDict ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!_activeEqualsDict.Add(this)) return true;

        try
        {
            foreach (var kvp in Pairs)
            {
                if (!other.Pairs.TryGetValue(kvp.Key, out var otherValue)) return false;
                if (!kvp.Value.Equals(otherValue)) return false;
            }
            return true;
        }
        finally
        {
            _activeEqualsDict.Remove(this);
        }
    }

    public override int GetHashCode()
    {
        _activeHashDict ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!_activeHashDict.Add(this)) 
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        try
        {
            var hash = new System.HashCode();
            int dictHash = 0;
            foreach (var kvp in Pairs) dictHash ^= System.HashCode.Combine(kvp.Key, kvp.Value);
            hash.Add(dictHash);
            return hash.ToHashCode();
        }
        finally
        {
            _activeHashDict.Remove(this);
        }
    }
}
