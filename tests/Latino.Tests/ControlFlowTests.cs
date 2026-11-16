using Xunit;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;
using System.Collections.Generic;

namespace Latino.Tests;

public class ControlFlowTests : TestBase
{
    [Fact]
    public void TestIf()
    {
        var script = @"
            global x = 0
            si 10 > 5
                x = 1
            sino
                x = 2
            fin
        ";
        var (_, result) = RunAndGet(script, "x");
        Assert.Equal(1, ((LatinoNumber)result).Value);
    }

    [Fact]
    public void TestWhile()
    {
        var script = @"
            global i = 0
            mientras i < 5
                i = i + 1
            fin
        ";
        var (_, result) = RunAndGet(script, "i");
        Assert.Equal(5, ((LatinoNumber)result).Value);
    }

    // [Fact(Skip = "GoTo no implementado en Runtime aún")]
    // public void TestGoTo() { ... }
}
