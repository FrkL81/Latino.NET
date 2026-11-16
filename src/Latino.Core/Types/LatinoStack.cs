using System.Collections.Generic;

namespace Latino.Core.Types;

public class LatinoStack : LatinoObject
{
    public readonly Stack<LatinoObject> Elements = new();

    public override object GetValue() => Elements;
    public override string TypeName => "pila";
    public override string ToString() => $"<pila>[{Elements.Count} elementos]";
}
