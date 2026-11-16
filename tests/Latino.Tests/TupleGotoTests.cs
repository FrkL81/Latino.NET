using Xunit;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;
using Latino.Core.StdLib;

namespace Latino.Tests;

public class TupleGotoTests : TestBase
{
    [Fact]
    public void TestCreacionTupla()
    {
        var result = (LatinoTuple)Eval("(1, \"dos\", verdadero)");
        Assert.Equal(3, result.Elements.Count);
        Assert.Equal(1.0, ((LatinoNumber)result.Elements[0]).Value);
        Assert.Equal("dos", ((LatinoString)result.Elements[1]).Value);
        Assert.True(((LatinoBoolean)result.Elements[2]).Value);
    }

    [Fact]
    public void TestTuplaUnElementoConComa()
    {
        var result = (LatinoTuple)Eval("(42,)");
        Assert.Single(result.Elements);
        Assert.Equal(42.0, ((LatinoNumber)result.Elements[0]).Value);
    }

    [Fact]
    public void TestAgrupacionNoEsTupla()
    {
        var result = (LatinoNumber)Eval("(1 + 2)");
        Assert.Equal(3.0, result.Value);
    }

    [Fact]
    public void TestLibreriaTupla()
    {
        var (_, result) = RunAndGet(@"
            global mi_tupla = (10, 20)
            global longitud = tupla.longitud(mi_tupla)
            global elemento = tupla.obtener(mi_tupla, 1)
        ", "elemento");

        Assert.Equal(20.0, ((LatinoNumber)result).Value);
    }

    [Fact]
    public void TestGotoSaltoAdelante()
    {
        var (_, result) = RunAndGet(@"
            global a = 1
            ir a final
            a = 2
            final:
            a = 3
        ", "a");

        Assert.Equal(3.0, ((LatinoNumber)result).Value);
    }
}
