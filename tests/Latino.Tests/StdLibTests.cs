using Xunit;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;
using Latino.Core.StdLib;
using Latino.Core.Analysis.Nodes;
using System.IO;

namespace Latino.Tests;

public class StdLibTests : TestBase
{

    [Fact]
    public void ListaAgregarLongitud()
    {
        var (_, result) = RunAndGet("global mi_lista = lista.crear(0); lista.agregar(mi_lista, 10);", "mi_lista");
        var len = ((LatinoList)result).Elements.Count;
        Assert.Equal(1, len);
    }

    [Fact]
    public void DicContieneLlave()
    {
        var result = (LatinoBoolean)Eval("dic.contiene({ 'a': 1 }, 'a')");
        Assert.True(result.Value);
    }

    [Fact]
    public void ConjuntoUnion()
    {
        var result = (LatinoSet)Eval("conjunto.union(conjunto.crear(1, 2), conjunto.crear(2, 3))");
        Assert.Equal(3, result.Elements.Count);
    }

    [Fact]
    public void PilaApilarDesapilar()
    {
        var (_, result) = RunAndGet("global p = pila.crear(); pila.apilar(p, 5); pila.desapilar(p);", "p");
        var len = ((LatinoStack)result).Elements.Count;
        Assert.Equal(0, len);
    }

    [Fact]
    public void ColaEncolarDesencolar()
    {
        var (_, result) = RunAndGet("global c = cola.crear(); cola.encolar(c, 10); global valor = cola.desencolar(c);", "valor");
        Assert.Equal(10, ((LatinoNumber)result).Value);
    }
}
