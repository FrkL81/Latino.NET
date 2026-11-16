using System.Globalization;

namespace Latino.Core.Types;

public class LatinoNumber : LatinoObject
{
    public double Value { get; }

    public LatinoNumber(double value)
    {
        Value = value;
    }

    public override object GetValue() => Value;
    public override string TypeName => "decimal";

    public override string ToString()
    {
        // Aseguramos que se use el punto decimal independientemente del idioma del PC
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    public static implicit operator double(LatinoNumber l) => l.Value;
    public static implicit operator LatinoNumber(double d) => new LatinoNumber(d);
}
