using System;
using Xunit;

namespace Latino.Tests;

public class IntegrationTests : TestBase
{
    [Fact]
    public void TestScriptCompleto()
    {
        var source = @"
            clase Contador
                constructor(inicio)
                    mi.valor = inicio;
                fin

                funcion incrementar()
                    mi.valor = mi.valor + 1;
                    retornar mi.valor;
                fin
            fin

            global miContador = nueva Contador(10);
            miContador.incrementar();
            miContador.incrementar();

            global resultado = miContador.valor; // 12

            global lista = [1, 2, 3];
            global suma = 0;
            para item en lista
                suma = suma + item;
            fin

            escribir('Contador: ' .. resultado);
            escribir('Suma de lista: ' .. suma);
        ";

        var expectedOutput = $"Contador: 12{Environment.NewLine}Suma de lista: 6";
        var actualOutput = Run(source);

        Assert.Equal(expectedOutput, actualOutput);
    }
}
