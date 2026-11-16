namespace Latino.Core.Types;

public class LatinoString : LatinoObject
{
    public string Value { get; }

    public LatinoString(string value)
    {
        Value = value ?? string.Empty;
    }

    public override object GetValue() => Value;
    public override string TypeName => "cadena";

    public override string ToString() => Value;

    public static implicit operator string(LatinoString l) => l.Value;
    public static implicit operator LatinoString(string s) => new LatinoString(s);
}
