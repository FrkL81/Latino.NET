using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using Latino.Core.Runtime;
using Latino.Core.Analysis;
using Latino.Core.Types;
using Latino.Core.StdLib;

namespace Latino.Tests;

public abstract class TestBase
{
    protected Interpreter CreateInterpreter()
    {
        var interpreter = new Interpreter();
        
        // 1. Redirigir salida a un StringWriter por defecto para capturarla
        interpreter.Out = new StringWriter(); 
        
        // 2. Cargar Librerías Estándar (CRÍTICO)
        StdLibLoader.LoadAll(interpreter);
        
        // 3. Definir 'escribir' nativa para tests
        var escribirFn = new NativeFunction((interp, args) => {
            interp.Out.WriteLine(string.Join(" ", args));
            return LatinoNull.Instance;
        });
        interpreter.Globals.Define("escribir", escribirFn);
        interpreter.Globals.Define("imprimir", escribirFn);
        interpreter.Globals.Define("poner", escribirFn);

        return interpreter;
    }

    // Sobrecarga para compatibilidad con tests existentes que esperan (Interpreter, LatinoObject)
    protected (Interpreter, LatinoObject) RunAndGet(string script, string varName)
    {
        var interpreter = CreateInterpreter();
        Run(interpreter, script);
        return (interpreter, interpreter.Globals.GetAt(varName) ?? LatinoNull.Instance);
    }

    // Sobrecarga para compatibilidad con tests existentes que esperan la salida en string
    protected string Run(string script)
    {
        var interpreter = CreateInterpreter();
        Run(interpreter, script);
        return interpreter.Out.ToString()?.Trim() ?? "";
    }

    protected void Run(Interpreter interpreter, string script)
    {
        var lexer = new Lexer(script);
        var parser = new Parser(lexer.Tokenize());
        var statements = parser.Parse();
        interpreter.Interpret(statements);
    }
    
    // Helper para evaluar expresiones sueltas (ej: "1 + 1")
    protected LatinoObject Eval(string expression)
    {
        var (_, result) = RunAndGet($"__temp__ = {expression}", "__temp__");
        return result;
    }
}
