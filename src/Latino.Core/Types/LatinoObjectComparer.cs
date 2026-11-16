using System.Collections.Generic;

namespace Latino.Core.Types;

public class LatinoObjectComparer : IEqualityComparer<LatinoObject>
{
    public static readonly LatinoObjectComparer Instance = new LatinoObjectComparer();

    public bool Equals(LatinoObject? x, LatinoObject? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        // FIX CRÍTICO: Usar x.Equals(y) para que LatinoTuple.Equals se ejecute.
        // NO usar x.GetValue().Equals(...) porque eso compara las listas por referencia.
        return x.Equals(y);
    }

    public int GetHashCode(LatinoObject obj)
    {
        // FIX CRÍTICO: Usar obj.GetHashCode() para que LatinoTuple.GetHashCode se ejecute.
        return obj.GetHashCode();
    }
}
