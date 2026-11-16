namespace Latino.Core.Types;

/// <summary>
/// Representa cualquier valor dentro del lenguaje Latino.
/// Todos los tipos de datos (números, cadenas, listas) heredarán de aquí.
/// </summary>
public abstract class LatinoObject
{
    /// <summary>
    /// Devuelve el valor nativo de C# (object) que envuelve este objeto.
    /// </summary>
    public abstract object? GetValue();

    /// <summary>
    /// Nombre del tipo de dato en Latino (ej: "número", "cadena").
    /// </summary>
    public virtual string TypeName => "desconocido";

    /// <summary>
    /// Representación en texto para mostrar en consola (ej: al usar 'imprimir').
    /// </summary>
    public override string ToString()
    {
        return GetValue()?.ToString() ?? "nulo";
    }

    public override bool Equals(object? obj)
    {
        if (obj is LatinoObject other)
        {
            var valA = GetValue();
            var valB = other.GetValue();
            if (valA == null && valB == null) return true;
            if (valA == null || valB == null) return false;
            return valA.Equals(valB);
        }
        return false;
    }

    public override int GetHashCode() => GetValue()?.GetHashCode() ?? 0;
}
