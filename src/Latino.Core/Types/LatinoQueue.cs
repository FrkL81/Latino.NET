using System.Collections.Generic;

namespace Latino.Core.Types;

public class LatinoQueue : LatinoObject
{
    public readonly Queue<LatinoObject> Elements = new();

    public override object GetValue() => Elements;
    public override string TypeName => "cola";
    public override string ToString() => $"<cola>[{Elements.Count} elementos]";
}
