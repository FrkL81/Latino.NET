using Xunit;
using Latino.Core.Analysis;
using Latino.Core.Runtime;
using Latino.Core.Types;
using Latino.Core.StdLib;
using System;

namespace Latino.Tests;

public class SwapAndPipeTests
{
    [Fact]
    public void Swap_IntercambiaDosVariables()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            a = 10
            b = 'Hola'
            a ~ b
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var a = interpreter.GetVariable("a");
        var b = interpreter.GetVariable("b");
        
        Assert.Equal("Hola", a?.ToString());
        Assert.Equal("10", b?.ToString());
    }

    [Fact]
    public void Swap_VariableNoExiste_LanzaError()
    {
        var interpreter = new Interpreter();
        string code = @"
            a = 10
            a ~ noExiste
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        
        Assert.Throws<RuntimeError>(() => interpreter.Interpret(parser.Parse()));
    }

    [Fact]
    public void Pipe_EncadenaDosFunciones()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            funcion duplicar(n) retornar n * 2 fin
            funcion cuadrado(n) retornar n * n fin
            resultado = 5 |> duplicar |> cuadrado
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var resultado = interpreter.GetVariable("resultado");
        // Verificar que sea un número y su valor
        Assert.IsType<LatinoNumber>(resultado);
        Assert.Equal(100.0, ((LatinoNumber)resultado!).Value);
    }

    [Fact]
    public void Pipe_ConFuncionNativa()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            resultado = -42 |> mate.abs |> acadena
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var resultado = interpreter.GetVariable("resultado");
        Assert.Equal("42", resultado?.ToString());
    }

    [Fact]
    public void Pipe_NoEsFuncion_LanzaError()
    {
        var interpreter = new Interpreter();
        string code = @"
            x = 5
            resultado = 10 |> x
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        
        Assert.Throws<RuntimeError>(() => interpreter.Interpret(parser.Parse()));
    }

    [Fact]
    public void Pipe_ConTernario()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            funcion duplicar(n) retornar n * 2 fin
            x = verdadero ? 5 |> duplicar : 0
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var x = interpreter.GetVariable("x");
        Assert.IsType<LatinoNumber>(x);
        Assert.Equal(10.0, ((LatinoNumber)x!).Value);
    }

    [Fact]
    public void RegexMatch_NoConfundeConSwap()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            resultado = 'test@email.com' ~= '^[a-z]+@[a-z]+\.[a-z]+$'
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var resultado = interpreter.GetVariable("resultado");
        Assert.IsType<LatinoBoolean>(resultado);
        Assert.True(((LatinoBoolean)resultado!).Value);
    }

    [Fact]
    public void Lexer_TokenizaSwap()
    {
        var tokens = new Lexer("a ~ b").Tokenize();
        // Verificar tokens específicos
        // tokens[0] -> Identifier 'a'
        // tokens[1] -> Swap '~'
        // tokens[2] -> Identifier 'b'
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(TokenType.Swap, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
    }

    [Fact]
    public void Lexer_TokenizaPipe()
    {
        var tokens = new Lexer("x |> f").Tokenize();
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(TokenType.Pipe, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
    }

    [Fact]
    public void Lexer_DiferenciaPipeDeLogicOr()
    {
        var tokens1 = new Lexer("a || b").Tokenize();
        var tokens2 = new Lexer("a |> b").Tokenize();
        
        Assert.Equal(TokenType.LogicOr, tokens1[1].Type);
        Assert.Equal(TokenType.Pipe, tokens2[1].Type);
    }

    [Fact]
    public void Lexer_ErrorEnPipeSolo()
    {
        // El lexer debe lanzar Exception (System.Exception generica por ahora en Lexer.cs)
        // Ojo: en Lexer.cs lanzamos 'new Exception(...)', asi que capturamos eso.
        var lexer = new Lexer("a | b");
        Assert.Throws<System.Exception>(() => lexer.Tokenize());
    }

    [Fact]
    public void Pipe_ConLlamadaParcialUsuario_Funciona()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            funcion restar(a, b) retornar a - b fin
            resultado = 10 |> restar(3)
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var resultado = interpreter.GetVariable("resultado");
        Assert.IsType<LatinoNumber>(resultado);
        Assert.Equal(7.0, ((LatinoNumber)resultado!).Value);
    }

    [Fact]
    public void Pipe_ConLlamadaAMetodoDeInstancia_Funciona()
    {
        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);
        string code = @"
            miLista = [1, 2, 3]
            4 |> miLista.agregar()
            longitud_final = lista.longitud(miLista)
        ";

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        interpreter.Interpret(parser.Parse());

        var longitud = interpreter.GetVariable("longitud_final");
        Assert.IsType<LatinoNumber>(longitud);
        Assert.Equal(4.0, ((LatinoNumber)longitud!).Value);
    }
}
