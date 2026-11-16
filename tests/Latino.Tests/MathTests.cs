using Xunit;
using Latino.Core.Types;

namespace Latino.Tests;

public class MathTests : TestBase
{
    [Fact]
    public void TestPi()
    {
        var result = (LatinoNumber)Eval("mate.pi");
        Assert.Equal(System.Math.PI, result.Value);
    }

    [Fact]
    public void TestRaiz()
    {
        var result = (LatinoNumber)Eval("mate.raiz(16)");
        Assert.Equal(4, result.Value);
    }
}
