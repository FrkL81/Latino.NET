using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq; // Necesario para el chequeo LINQ
using Latino.Core.Analysis;
using Latino.Core.Types;

namespace Latino.Core.Runtime;

public class LatinoEnvironment
{
    private readonly LatinoEnvironment? _enclosing;
    private readonly ConcurrentDictionary<string, LatinoObject> _values = new();

    public LatinoEnvironment()
    {
        _enclosing = null;
    }

    public LatinoEnvironment(LatinoEnvironment enclosing)
    {
        _enclosing = enclosing;
    }

    public void Define(string name, LatinoObject value)
    {
        _values[name] = value;
    }

    public LatinoObject Get(Token name)
    {
        if (_values.TryGetValue(name.Text, out var value))
        {
            return value;
        }

        if (_enclosing != null) return _enclosing.Get(name);

        throw new RuntimeError(name, $"Variable no definida: '{name.Text}'.");
    }

    // Método auxiliar para tests (acceso por string)
    public LatinoObject? GetAt(string name)
    {
        if (_values.TryGetValue(name, out var val)) return val;
        return _enclosing?.GetAt(name);
    }

    public void Assign(Token name, LatinoObject value)
    {
        // 1. Protección de Constantes
        if (IsConstantName(name.Text) && Exists(name.Text))
        {
            throw new RuntimeError(name, $"No se puede reasignar la constante '{name.Text}'.");
        }

        // 2. Si ya existe en este entorno, la actualizamos de forma segura (Thread-Safe)
        // Usamos ContainsKey porque en Latino las variables no se borran (no hay riesgo de que desaparezca)
        // y el indexador [] de ConcurrentDictionary es una operación atómica AddOrUpdate internamente.
        if (_values.ContainsKey(name.Text))
        {
            _values[name.Text] = value;
            return;
        }

        // 3. Si no está aquí, pero existe en entornos padres, delegamos hacia arriba
        if (_enclosing != null && _enclosing.Exists(name.Text))
        {
            _enclosing.Assign(name, value);
            return;
        }

        // 4. Si es totalmente nueva, nace aquí. El indexador [] maneja concurrencia atómicamente.
        _values[name.Text] = value;
    }

    private bool Exists(string name)
    {
        if (_values.ContainsKey(name)) return true;
        if (_enclosing != null) return _enclosing.Exists(name);
        return false;
    }

    private bool IsConstantName(string text)
    {
        // Debe tener al menos una letra para ser considerada constante (evitar "_")
        bool hasLetters = text.Any(char.IsLetter);

        // Todas las letras que contenga deben ser mayúsculas
        bool allUpper = text.All(c => !char.IsLetter(c) || char.IsUpper(c));

        return hasLetters && allUpper;
    }

    public IEnumerable<string> GetKeys() => _values.Keys;
}
