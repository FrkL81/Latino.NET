namespace Latino.Core.Types;

public class LatinoBoolean : LatinoObject
{
    public bool Value { get; }

    public LatinoBoolean(bool value)
    {
        Value = value;
    }

    public override object GetValue() => Value;
    public override string TypeName => "logico";

    // En Latino, los booleanos se imprimen como "cierto" o "falso"
    public override string ToString() => Value ? "cierto" : "falso";

    // Operadores de ayuda implícitos para facilitar el código C#
    public static implicit operator bool(LatinoBoolean l) => l.Value;
    public static implicit operator LatinoBoolean(bool b) => new LatinoBoolean(b);

    // Método estático para parsear palabras clave (incluyendo alias como 'verdadero')
    public static LatinoBoolean? FromString(string text)
    {
        return text.ToLower() switch
        {
            "cierto" => new LatinoBoolean(true),
            "verdadero" => new LatinoBoolean(true),
            "falso" => new LatinoBoolean(false),
            "espurio" => new LatinoBoolean(false), // NUEVO: Alias culto
            _ => null
        };
    }
}
