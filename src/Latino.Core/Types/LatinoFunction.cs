using System.Collections.Generic;
using Latino.Core.Analysis.Nodes;
using Latino.Core.Runtime;

namespace Latino.Core.Types;

public class LatinoFunction : LatinoObject, ICallable
{
    private readonly FunctionStatement _declaration;
    private readonly LatinoEnvironment _closure;

    public LatinoFunction(FunctionStatement declaration, LatinoEnvironment closure)
    {
        _declaration = declaration;
        _closure = closure;
    }

    public int Arity => _declaration.Params.Count;

    public LatinoObject Call(Interpreter interpreter, List<LatinoObject> arguments)
    {
        // --- CHEQUEO DE PROFUNDIDAD DE RECURSIÓN ---
        if (interpreter.CurrentDepth >= interpreter.MaxDepth)
        {
            throw new RuntimeError(_declaration.Name, $"Límite de recursividad excedido ({interpreter.MaxDepth}).");
        }
        
        // Incrementar profundidad
        interpreter.CurrentDepth++;
        
        try
        {
            var envBase = _closure ?? interpreter.Globals;
            var environment = new LatinoEnvironment(envBase);
            
            var parameters = _declaration.Params;
            
            // --- VALIDACIÓN DE ARIDAD (FIX) ---
            if (arguments.Count > parameters.Count)
            {
                throw new RuntimeError(_declaration.Name, $"La función '{_declaration.Name.Text}' esperaba máximo {parameters.Count} argumentos, pero recibió {arguments.Count}.");
            }
            // ----------------------------------

            for (int i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                LatinoObject value;

                if (i < arguments.Count)
                {
                    value = arguments[i];
                }
                else
                {
                    if (param.DefaultValue != null)
                        value = interpreter.Evaluate(param.DefaultValue);
                    else
                        throw new RuntimeError(param.Name, $"Falta el argumento obligatorio '{param.Name.Text}'.");
                }
                environment.Define(param.Name.Text, value);
            }

            // ─────────────────────────────────────────────────────────────
            // OPTIMIZACIÓN FAST-PATH
            // Si la función tiene exactamente una línea y es un `retornar`,
            // evaluamos la expresión directamente sin ejecutar ExecuteBlock
            // ni lanzar ReturnException. Cubre el 99% de las lambdas usadas
            // en map/filter/reduce.
            //
            // El check de CurrentDepth ya fue validado arriba, así que el
            // fast-path hereda esa protección sin necesidad de repetirla.
            // ─────────────────────────────────────────────────────────────
            if (_declaration.Body.Count == 1 && _declaration.Body[0] is ReturnStatement retStmt)
            {
                if (retStmt.Value != null)
                    return interpreter.EvaluateInEnv(retStmt.Value, environment);

                return LatinoNull.Instance;
            }

            // Camino tradicional para funciones multilínea
            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                return returnValue.Value;
            }

            return LatinoNull.Instance;
        }
        finally
        {
            // Decrementar profundidad al salir
            interpreter.CurrentDepth--;
        }
    }

    public LatinoFunction Bind(LatinoInstance instance)
    {
        var environment = new LatinoEnvironment(_closure);
        // Enlazamos el "Trío de Referencia"
        environment.Define("mi", instance);
        environment.Define("este", instance);
        environment.Define("esta", instance);
        
        return new LatinoFunction(_declaration, environment);
    }
    
    public override object GetValue() => _declaration.Name.Text;
    public override string TypeName => "funcion";
    public override string ToString() => $"<fn {_declaration.Name.Text}>";
}
