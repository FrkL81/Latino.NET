using Xunit;
using Latino.Core.Types;

namespace Latino.Tests;

public class OperatorTests : TestBase
{
    [Fact]
    public void Prefix_Increment_Works()
    {
        var script = @"
            i = 0
            j = ++i
        ";
        var (interp, i) = RunAndGet(script, "i");
        var j = interp.Globals.GetAt("j");

        Assert.Equal(1.0, ((LatinoNumber)i).Value);
        Assert.NotNull(j);
        Assert.Equal(1.0, ((LatinoNumber)j).Value);
    }

    [Fact]
    public void Prefix_Decrement_Works()
    {
        var script = @"
            i = 5
            j = --i
        ";
        var (interp, i) = RunAndGet(script, "i");
        var j = interp.Globals.GetAt("j");

        Assert.Equal(4.0, ((LatinoNumber)i).Value);
        Assert.NotNull(j);
        Assert.Equal(4.0, ((LatinoNumber)j).Value);
    }

    [Fact]
    public void Postfix_Increment_Works()
    {
        var script = @"
            i = 0
            j = i++
        ";
        var (interp, i) = RunAndGet(script, "i");
        var j = interp.Globals.GetAt("j");

        Assert.Equal(1.0, ((LatinoNumber)i).Value);
        Assert.NotNull(j);
        Assert.Equal(0.0, ((LatinoNumber)j).Value);
    }

    [Fact]
    public void Postfix_Decrement_Works()
    {
        var script = @"
            i = 5
            j = i--
        ";
        var (interp, i) = RunAndGet(script, "i");
        var j = interp.Globals.GetAt("j");

        Assert.Equal(4.0, ((LatinoNumber)i).Value);
        Assert.NotNull(j);
        Assert.Equal(5.0, ((LatinoNumber)j).Value);
    }

    [Fact]
    public void Increment_In_While_Loop()
    {
        var script = @"
            i = 0
            suma = 0
            mientras i < 5
                suma += i
                ++i
            fin
        ";
        var (_, suma) = RunAndGet(script, "suma");
        Assert.Equal(10.0, ((LatinoNumber)suma).Value);
    }

    [Fact]
    public void NullCoalescing_And_Assignment_Works()
    {
        var script = @"
            // 1. Fusión nula básica y cortocircuito
            a = nulo ?? 42
            
            // 2. Asignación a variable nueva/inexistente
            b ??= 100
            
            // 3. Asignación a índices de diccionario (Lo que modificó el Agente)
            d = {}
            d['x'] ??= 99
            d['x'] ??= 500 // No debe sobreescribir porque ya es 99
            
            // 4. Asegurarnos que la asignación normal NO se rompió
            d['y'] = 10
            
            res_a = a
            res_b = b
            res_dx = d['x']
            res_dy = d['y']
        ";
        
        var interpreter = CreateInterpreter();
        Run(interpreter, script);

        // Verificaciones
        Assert.Equal(42.0, ((LatinoNumber)interpreter.Globals.GetAt("res_a")!).Value);
        Assert.Equal(100.0, ((LatinoNumber)interpreter.Globals.GetAt("res_b")!).Value);
        Assert.Equal(99.0, ((LatinoNumber)interpreter.Globals.GetAt("res_dx")!).Value);
        Assert.Equal(10.0, ((LatinoNumber)interpreter.Globals.GetAt("res_dy")!).Value);
    }
}
