using Xunit;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;

namespace Latino.Tests;

public class FunctionTests : TestBase
{
    [Fact]
    public void FunctionCall()
    {
        var script = @"
            funcion suma(a, b)
                retornar a + b
            fin
            global res = suma(5, 7)
        ";
        var (_, result) = RunAndGet(script, "res");
        Assert.Equal(12, ((LatinoNumber)result).Value);
    }

    [Fact]
    public void Recursion()
    {
        var script = @"
            funcion fib(n)
                si n <= 1 retornar n fin
                retornar fib(n-1) + fib(n-2)
            fin
            global res = fib(10)
        ";
        // Fib(10) = 55
        var (_, result) = RunAndGet(script, "res");
        Assert.Equal(55, ((LatinoNumber)result).Value);
    }
}
