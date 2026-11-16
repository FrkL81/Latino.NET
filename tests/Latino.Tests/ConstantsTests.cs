using Xunit;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;

namespace Latino.Tests;

public class ConstantsTests
{
    [Fact]
    public void ConstanteSeDefineCorrectamente()
    {
        var env = new LatinoEnvironment();
        env.Define("PI", new LatinoNumber(3.1416));
        var pi = env.GetAt("PI");
        Assert.NotNull(pi);
    }

    [Fact]
    public void ReasignarConstanteLanzaError()
    {
        var env = new LatinoEnvironment();
        var token = new Token(TokenType.Identifier, "GRAVEDAD", null, 1, 1);
        env.Define("GRAVEDAD", new LatinoNumber(9.8));

        // Esta llamada a Assign DEBE lanzar la excepción
        var exception = Assert.Throws<RuntimeError>(() => {
            env.Assign(token, new LatinoNumber(10));
        });

        Assert.Equal("No se puede reasignar la constante 'GRAVEDAD'.", exception.Message);
    }
}
