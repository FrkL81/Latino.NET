using System.Collections.Generic;
using System.Linq;

namespace Latino.Core.Types;

public class LatinoSet : LatinoObject
{
    // IMPORTANTE: Usar el Comparer explícito
    public HashSet<LatinoObject> Items { get; } = new HashSet<LatinoObject>(LatinoObjectComparer.Instance);
    
    // Mantener compatibilidad con código existente
    public HashSet<LatinoObject> Elements => Items;

    public LatinoSet() { }

    public LatinoSet(IEnumerable<LatinoObject> items) 
    {
        foreach(var item in items) Items.Add(item);
    }

    public override object GetValue() => Items;
    public override string TypeName => "conjunto";

    public override string ToString() => "{" + string.Join(", ", Items) + "}";
}
