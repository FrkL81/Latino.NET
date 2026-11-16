namespace Latino.Core.Types;

public class LatinoNull : LatinoObject
{
    // Singleton: Solo necesitamos una instancia de 'nulo' en todo el programa
    public static readonly LatinoNull Instance = new LatinoNull();

    private LatinoNull() { }

    public override object? GetValue() => null;
    public override string TypeName => "nulo";
    public override string ToString() => "nulo";
}
