using Xunit;
using System.IO;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.StdLib;
using Latino.Core.Types;

namespace Latino.Tests;

public class InputTests
{
    [Fact]
    public void TestLeerConInputSimulado()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);

        // 1. Simulamos que el usuario escribió "Hola Latino" y presionó Enter
        var inputSimulado = new StringReader("Hola Latino");
        interpreter.In = inputSimulado;

        // 2. Ejecutamos el script
        var lexer = new Lexer("x = leer()");
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        // 3. Verificamos que 'x' capturó el texto
        var x = interpreter.Globals.GetAt("x") as LatinoString;
        Assert.NotNull(x);
        Assert.Equal("Hola Latino", x.Value);
    }

    [Fact]
    public void TestLeerConPrompt()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);

        // 1. Capturamos la salida para verificar el prompt
        var output = new StringWriter();
        interpreter.Out = output;
        
        // 2. Simulamos input del usuario
        var inputSimulado = new StringReader("Maria");
        interpreter.In = inputSimulado;

        // 3. Ejecutamos script con prompt
        var lexer = new Lexer("nombre = leer(\"Nombre: \")");
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        // 4. Verificamos que se imprimió el prompt y se capturó el input
        Assert.Contains("Nombre: ", output.ToString());
        
        var nombre = interpreter.Globals.GetAt("nombre") as LatinoString;
        Assert.NotNull(nombre);
        Assert.Equal("Maria", nombre.Value);
    }

    [Fact]
    public void TestPedirAlias()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);

        // Simulamos input
        var inputSimulado = new StringReader("42");
        interpreter.In = inputSimulado;

        // Ejecutamos usando el alias pedir
        var lexer = new Lexer("edad = pedir()");
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        // Verificamos que funciona igual que leer
        var edad = interpreter.Globals.GetAt("edad") as LatinoString;
        Assert.NotNull(edad);
        Assert.Equal("42", edad.Value);
    }
}
