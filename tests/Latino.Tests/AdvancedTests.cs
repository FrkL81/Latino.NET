using Xunit;
using Latino.Core.Types;

namespace Latino.Tests;

public class AdvancedTests : TestBase
{
    [Fact]
    public void RegexMatch_Operator_Works()
    {
        // Caso positivo
        var result1 = Eval("\"correo@test.com\" ~= \".*@.*\"");
        Assert.True(((LatinoBoolean)result1).Value);

        // Caso negativo
        var result2 = Eval("\"hola\" ~= \"\\\\d+\"");
        Assert.False(((LatinoBoolean)result2).Value);
    }

    [Fact]
    public void TryCatch_Catches_DivisionByZero()
    {
        var script = @"
            errores = 0
            msj = ''
            intentar
                x = 10 / 0
            atrapar (e)
                errores = 1
                msj = e
            fin
        ";
        var (interp, res) = RunAndGet(script, "errores");
        var msj = interp.Globals.GetAt("msj");

        Assert.Equal(1.0, ((LatinoNumber)res).Value);
        Assert.Equal("[Línea 5] División por cero.", msj?.ToString());
    }

    [Fact]
    public void TryCatch_Catches_PropertyNotFound()
    {
        var script = @"
            msj = ''
            intentar
                d = {}
                x = d.propiedad_inexistente
            atrapar (e)
                msj = e
            fin
        ";
        var (interp, _) = RunAndGet(script, "msj");
        var msj = interp.Globals.GetAt("msj");

        Assert.Contains("no tiene la clave", msj?.ToString());
    }

    [Fact]
    public void Concat_Operator_Works()
    {
        // El operador .. debe funcionar para concatenar strings
        var result = Eval("\"Hola \" .. \"Mundo\"");
        Assert.Equal("Hola Mundo", result.ToString());
    }
}
