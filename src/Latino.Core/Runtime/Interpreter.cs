using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // Vital para Regex
using Latino.Core.Analysis;
using Latino.Core.Analysis.Nodes;
using Latino.Core.Types;
using Latino.Core.StdLib; // Acceso a configuración global

namespace Latino.Core.Runtime;

public class Interpreter
{
    public readonly LatinoEnvironment Globals = new();
    private LatinoEnvironment _environment;
    public TextWriter Out { get; set; } = Console.Out;
    public TextReader In { get; set; } = Console.In; // NUEVO: Abstracción de entrada
    public int MaxDepth { get; set; } = 100; // Nuevo default solicitado
    public int CurrentDepth = 0; // Profundidad actual de recursión

    public Interpreter()
    {
        _environment = Globals;
    }

    /// <summary>
    /// Fast-Path: Evalúa una expresión en un entorno específico sin la sobrecarga
    /// de ExecuteBlock ni el costo de ReturnException.
    /// Utilizado exclusivamente por LatinoFunction para optimizar lambdas de una sola línea.
    ///
    /// NOTA DE SEGURIDAD: Este método swapea _environment temporalmente.
    /// Es seguro porque cada hilo de Latino.NET tiene su propia instancia de Interpreter.
    /// No usar en contextos donde múltiples hilos compartan la misma instancia.
    /// </summary>
    public LatinoObject EvaluateInEnv(Expression expr, LatinoEnvironment env)
    {
        LatinoEnvironment previous = _environment;
        try
        {
            _environment = env;
            return Evaluate(expr);
        }
        finally
        {
            _environment = previous;
        }
    }

    public void Interpret(List<Statement> statements)
    {
        if (statements == null) return;
        try
        {
            ExecuteBlock(statements, _environment);
        }
        // catch (RuntimeError error)
        // {
        //     string lineInfo = error.Token != null ? $" [Línea {error.Token.Line}]" : "";
        //     Out.WriteLine($"Error Runtime{lineInfo}: {error.Message}");
        //     Out.Flush();
        // }
        catch (BreakException)
        {
            Out.WriteLine("Error [Global]: 'romper' usado fuera de un bucle.");
            Out.Flush();
        }
        catch (ContinueException)
        {
            Out.WriteLine("Error [Global]: 'continuar' usado fuera de un bucle.");
            Out.Flush();
        }
        catch (GotoException ex)
        {
            Out.WriteLine($"Error Runtime: Etiqueta '{ex.TargetLabel}' no encontrada.");
            Out.Flush();
        }
        catch (ReturnException)
        {
            Out.WriteLine("Error [Global]: 'retornar' usado fuera de una función.");
            Out.Flush();
        }
        catch (RuntimeError)
        {
            throw;
        }
        catch (Exception ex) // CATCH-ALL de seguridad
        {
            Out.WriteLine($"FATAL ERROR INTERNO: {ex.GetType().Name} - {ex.Message}");
            Out.WriteLine(ex.StackTrace);
            Out.Flush();
        }
    }

    /// <summary>
    /// Ejecuta código Latino y retorna el valor de la última expresión evaluada.
    /// Diferencia clave con Interpret(): Devuelve valor en lugar de solo ejecutar.
    /// </summary>
    /// <param name="source">Código fuente en Latino</param>
    /// <returns>Resultado de la última expresión, o LatinoNull si no hay expresiones</returns>
    public LatinoObject RunEval(string source)
    {
        // 1. Compilar código
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        var statements = parser.Parse();

        LatinoObject lastResult = LatinoNull.Instance;

        if (statements == null || statements.Count == 0)
            return lastResult;

        try
        {
            // 2. Ejecutar sentencias y capturar última expresión
            foreach (var stmt in statements)
            {
                if (stmt is ExpressionStatement exprStmt)
                {
                    // Expresiones: evaluar y guardar resultado
                    lastResult = Evaluate(exprStmt.Expression);
                }
                else
                {
                    // Sentencias de control (if, while, etc.): ejecutar sin retorno
                    Execute(stmt);
                    lastResult = LatinoNull.Instance;
                }
            }
        }
        catch (BreakException)
        {
            throw new RuntimeError(null!, "'romper' usado fuera de un bucle en evaluar().");
        }
        catch (ContinueException)
        {
            throw new RuntimeError(null!, "'continuar' usado fuera de un bucle en evaluar().");
        }
        catch (ReturnException ex)
        {
            // Si evaluar() ejecuta código con 'retornar', capturamos su valor
            return ex.Value;
        }
        catch (RuntimeError)
        {
            // Re-lanzar errores de runtime sin envolver
            throw;
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null!, $"Error al evaluar código: {ex.Message}");
        }

        return lastResult;
    }

    public void ExecuteBlock(List<Statement> statements, LatinoEnvironment environment)
    {
        LatinoEnvironment previous = _environment;
        try
        {
            _environment = environment;
            // Pre-escaneo de etiquetas para GoTo
            var labels = new Dictionary<string, int>();
            for (int k = 0; k < statements.Count; k++)
            {
                if (statements[k] is LabelStatement label) labels[label.Name.Text] = k;
            }

            for (int i = 0; i < statements.Count; i++)
            {
                try
                {
                    Execute(statements[i]);
                }
                catch (GotoException ex)
                {
                    if (labels.ContainsKey(ex.TargetLabel))
                    {
                        i = labels[ex.TargetLabel] - 1;
                        continue;
                    }
                    throw;
                }
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    private void Execute(Statement stmt)
    {
        switch (stmt)
        {
            case ExpressionStatement e: Evaluate(e.Expression); break;
            case BlockStatement b: ExecuteBlock(b.Statements, new LatinoEnvironment(_environment)); break;
            case VarDeclaration v:
                object? val = null;
                if (v.Initializer != null) val = Evaluate(v.Initializer);
                
                // Si es global, definimos en el entorno global
                if (v.IsLocal)
                {
                    _environment.Define(v.Name.Text, (LatinoObject?)val ?? LatinoNull.Instance);
                }
                else
                {
                    // Es global o variable normal - definimos en el entorno global
                    Globals.Define(v.Name.Text, (LatinoObject?)val ?? LatinoNull.Instance);
                }
                break;
            case IfStatement i:
                if (IsTruthy(Evaluate(i.Condition))) Execute(i.ThenBranch);
                else if (i.ElseBranch != null) Execute(i.ElseBranch);
                break;
            case WhileStatement w:
                while (IsTruthy(Evaluate(w.Condition)))
                {
                    try { Execute(w.Body); }
                    catch (BreakException) { break; }
                    catch (ContinueException) { }
                }
                break;
            case DoWhileStatement d:
                do {
                    try { Execute(d.Body); }
                    catch (BreakException) { break; }
                    catch (ContinueException) { }
                } while (!IsTruthy(Evaluate(d.Condition)));
                break;
            case ForStatement f:
                LatinoEnvironment prevFor = _environment;
                _environment = new LatinoEnvironment(_environment);
                try {
                    if (f.Initializer != null) Execute(f.Initializer);
                    while (true) {
                        if (f.Condition != null && !IsTruthy(Evaluate(f.Condition))) break;
                        try { Execute(f.Body); }
                        catch (BreakException) { break; }
                        catch (ContinueException) { }
                        if (f.Increment != null) Evaluate(f.Increment);
                    }
                } finally { _environment = prevFor; }
                break;
            case ForeachStatement fe:
                var iterable = Evaluate(fe.Iterable);
                if (iterable is LatinoList list) {
                    try
                    {
                        foreach (var item in list.Elements) {
                            var loopEnv = new LatinoEnvironment(_environment);
                            loopEnv.Define(fe.Variable.Text, item);
                            try { ExecuteBlock(((BlockStatement)fe.Body).Statements, loopEnv); }
                            catch (BreakException) { break; }
                            catch (ContinueException) { }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Capturamos el error de modificación de colección de .NET
                        throw new RuntimeError(fe.Variable, "La lista fue modificada durante la iteración.");
                    }
                }
                break;
            case SwitchStatement s:
                var value = Evaluate(s.Value);
                bool matched = false;
                foreach (var kase in s.Cases) {
                    if (IsEqual(value, Evaluate(kase.Value))) {
                        Execute(kase.Body);
                        matched = true;
                        break;
                    }
                }
                if (!matched && s.DefaultBranch != null) Execute(s.DefaultBranch);
                break;
            
            // --- TRY-CATCH CRÍTICO ---
            case TryCatchStatement t:
                try
                {
                    Execute(t.TryBranch);
                }
                catch (RuntimeError ex)
                {
                    if (t.CatchBranch != null)
                    {
                        if (t.ErrorVariable != null)
                        {
                            LatinoEnvironment catchEnv = new LatinoEnvironment(_environment);
                            
                            // MAGIA: Construir el mensaje enriquecido para el usuario
                            string prefix = ex.Token != null ? $"[Línea {ex.Token.Line}] " : "";
                            catchEnv.Define(t.ErrorVariable.Text, new LatinoString($"{prefix}{ex.Message}"));
                            
                            ExecuteBlock(((BlockStatement)t.CatchBranch).Statements, catchEnv);
                        }
                        else
                        {
                            Execute(t.CatchBranch);
                        }
                    }
                }
                break;

            case FunctionStatement fn:
                var func = new LatinoFunction(fn, _environment);
                _environment.Define(fn.Name.Text, func);
                break;
            case ReturnStatement r:
                LatinoObject? ret = null;
                if (r.Value != null) ret = Evaluate(r.Value);
                throw new ReturnException(ret ?? LatinoNull.Instance);
            case GotoStatement g: throw new GotoException(g.LabelName.Text);
            case LabelStatement: break;
            
            // NUEVO: Swap Genérico Avanzado
            case SwapStatement swap:
                // 1. Evaluar y obtener los valores actuales de ambos lados
                var valLeft = Evaluate(swap.Left);
                var valRight = Evaluate(swap.Right);
                
                // 2. Inyectar los valores cruzados usando el helper de asignación
                PerformAssignment(swap.Left, valRight);
                PerformAssignment(swap.Right, valLeft);
                break;
            
            case BreakStatement: throw new BreakException();
            case ContinueStatement: throw new ContinueException();
            case ClassStatement c:
                LatinoClass? superclass = null;
                if (c.Superclass != null)
                {
                    var superObj = Evaluate(c.Superclass);
                    if (superObj is not LatinoClass sc) throw new RuntimeError(c.Superclass.Name, "La superclase debe ser una clase.");
                    superclass = sc;
                }
                _environment.Define(c.Name.Text, null!);
                var methods = new Dictionary<string, LatinoFunction>();
                var klass = new LatinoClass(c.Name.Text, superclass, methods);
                _environment.Assign(c.Name, klass);
                foreach (var method in c.Methods)
                {
                    var methodFunc = new LatinoFunction(method, _environment);
                    methods[method.Name.Text] = methodFunc;
                }
                break;
            default: break;
        }
    }

    // Helper para realizar asignaciones dinámicas en el operador Swap
    private void PerformAssignment(Expression target, LatinoObject value)
    {
        if (target is VariableExpression v)
        {
            _environment.Assign(v.Name, value);
        }
        else if (target is GetExpression get)
        {
            // Convertimos la lectura (Get) en escritura (Set) dinámicamente
            Evaluate(new SetExpression(get.Object, get.Name, new LiteralExpression(value)));
        }
        else if (target is IndexExpression idx)
        {
            // Convertimos el acceso a índice (Index) en escritura (IndexSet) dinámicamente
            Evaluate(new IndexSetExpression(idx.Object, idx.Index, new LiteralExpression(value)));
        }
        else
        {
            throw new RuntimeError(null!, "El destino del operador de intercambio (~) es inválido. Solo se admiten variables, propiedades o índices.");
        }
    }

    public LatinoObject? GetVariable(string name)
    {
        try 
        {
            // Creamos un token temporal para reutilizar la lógica de búsqueda de entorno
            // Usamos coordenadas 0,0 porque es una búsqueda interna
            var token = new Token(TokenType.Identifier, name, null, 0, 0);
            return _environment.Get(token);
        } 
        catch (RuntimeError) 
        { 
            return null; // Si no existe, devolvemos null para no romper la ejecución
        }
    }

    public LatinoObject Evaluate(Expression expr)
    {
        switch (expr)
        {
            case LiteralExpression l: return (LatinoObject)l.Value!;
            case VariableExpression v: return _environment.Get(v.Name);
            case AssignExpression a:
                var val = Evaluate(a.Value);
                _environment.Assign(a.Name, val);
                return val;
            case BinaryExpression b: return EvaluateBinary(b);
            case UnaryExpression u:
                if (u.Operator.Type == TokenType.Increment || u.Operator.Type == TokenType.Decrement)
                {
                    if (u.Right is VariableExpression vPrefix)
                    {
                        var currentPrefix = Evaluate(vPrefix);
                        if (currentPrefix is LatinoNumber nPrefix)
                        {
                            double change = (u.Operator.Type == TokenType.Increment) ? 1 : -1;
                            var newVal = new LatinoNumber(nPrefix.Value + change);
                            _environment.Assign(vPrefix.Name, newVal);
                            return newVal;
                        }
                        throw new RuntimeError(u.Operator, "Solo se pueden incrementar/decrementar números.");
                    }
                    throw new RuntimeError(u.Operator, "Solo se pueden incrementar/decrementar variables.");
                }
                var right = Evaluate(u.Right);
                if (u.Operator.Type == TokenType.Minus) return new LatinoNumber(-((LatinoNumber)right).Value);
                if (u.Operator.Type == TokenType.LogicNot) return new LatinoBoolean(!IsTruthy(right));
                return LatinoNull.Instance;
            case PostfixExpression p:
                if (p.Operator.Type == TokenType.Increment || p.Operator.Type == TokenType.Decrement)
                {
                    if (p.Left is VariableExpression vPostfix)
                    {
                        var currentPostfix = Evaluate(vPostfix);
                        if (currentPostfix is LatinoNumber nPostfix)
                        {
                            double change = (p.Operator.Type == TokenType.Increment) ? 1 : -1;
                            var newVal = new LatinoNumber(nPostfix.Value + change);
                            _environment.Assign(vPostfix.Name, newVal);
                            return currentPostfix; // Devolver el valor original antes del cambio
                        }
                        throw new RuntimeError(p.Operator, "Solo se pueden incrementar/decrementar números.");
                    }
                    throw new RuntimeError(p.Operator, "Solo se pueden incrementar/decrementar variables.");
                }
                return LatinoNull.Instance;
            case GroupingExpression g: return Evaluate(g.Expression);
            case CallExpression c:
                var callee = Evaluate(c.Callee);
                
                if (callee is ICallable function) 
                {
                    // OPTIMIZACIÓN 1: Sin LINQ. Evita crear delegados y enumeradores en cada llamada
                    // recursiva — reduce la presión sobre el GC en recursión profunda.
                    var args = new List<LatinoObject>(c.Arguments.Count);
                    foreach (var argExpr in c.Arguments)
                    {
                        args.Add(Evaluate(argExpr));
                    }

                    // OPTIMIZACIÓN 2: El try-catch "engorda" el Stack Frame de C# porque el JIT
                    // debe generar metadatos de protección adicionales por cada frame activo.
                    // Al limitar el bloque try-catch exclusivamente a NativeFunction (plugins y StdLib),
                    // las llamadas recursivas de funciones Latino quedan en un Fast-Path ultraligero
                    // que maximiza la profundidad de recursión alcanzable.
                    if (function is NativeFunction)
                    {
                        // RUTA PROTEGIDA: Solo las funciones nativas necesitan enriquecimiento,
                        // porque no tienen acceso al árbol de sintaxis y sus RuntimeError carecen de Token.
                        try
                        {
                            return function.Call(this, args);
                        }
                        catch (RuntimeError ex)
                        {
                            if (ex.Token == null) throw new RuntimeError(c.Paren, ex.Message);
                            throw;
                        }
                    }
                    else
                    {
                        // FAST-PATH: LatinoFunction (funciones definidas por el usuario).
                        // Sin try-catch, el frame de C# es mínimo — permite recursión profunda.
                        // Las LatinoFunction ya generan RuntimeError con Token propio al fallar.
                        return function.Call(this, args);
                    }
                }
                throw new RuntimeError(c.Paren, "No es una función.");
            case GetExpression ge:
            {
                var obj = Evaluate(ge.Object);
                string memberName = ge.Name.Text;

                // ═════════════════════════════════════════════════════════════════
                // PRIORIDAD 1: POO - Instancias de Clase
                // ═════════════════════════════════════════════════════════════════
                if (obj is LatinoInstance instance)
                    return instance.Get(ge.Name);

                // ═════════════════════════════════════════════════════════════════
                // PRIORIDAD 2: DICCIONARIOS
                // Regla: Las CLAVES tienen prioridad sobre los métodos
                // ═════════════════════════════════════════════════════════════════
                if (obj is LatinoDictionary dict)
                {
                    var key = new LatinoString(memberName);
                    
                    // A. Si existe la clave, devolver su valor (comportamiento esperado)
                    if (dict.Pairs.TryGetValue(key, out var dictValue))
                        return dictValue;
                    
                    // B. Si NO existe la clave, buscar método en librería 'dic'
                    var dictMethod = BindPrimitiveMethod(obj, "dic", memberName);
                    if (dictMethod != null)
                        return dictMethod;
                    
                    // C. No existe ni como clave ni como método
                    throw new RuntimeError(ge.Name, 
                        $"El diccionario no tiene la clave '{memberName}' ni existe el método dic.{memberName}().");
                }

                // ═════════════════════════════════════════════════════════════════
                // PRIORIDAD 3: LISTAS
                // ═════════════════════════════════════════════════════════════════
                if (obj is LatinoList)
                {
                    var listMethod = BindPrimitiveMethod(obj, "lista", memberName);
                    if (listMethod != null)
                        return listMethod;
                    
                    throw new RuntimeError(ge.Name, 
                        $"No existe el método lista.{memberName}(). Métodos disponibles: agregar, eliminar, longitud, etc.");
                }

                // ═════════════════════════════════════════════════════════════════
                // PRIORIDAD 4: CADENAS
                // ═════════════════════════════════════════════════════════════════
                if (obj is LatinoString)
                {
                    var stringMethod = BindPrimitiveMethod(obj, "cadena", memberName);
                    if (stringMethod != null)
                        return stringMethod;
                    
                    throw new RuntimeError(ge.Name, 
                        $"No existe el método cadena.{memberName}(). Métodos disponibles: mayusculas, minusculas, longitud, etc.");
                }

                // ═════════════════════════════════════════════════════════════════
                // PRIORIDAD 5: NÚMEROS (Futuro - por ahora no hay librería 'numero')
                // ═════════════════════════════════════════════════════════════════
                if (obj is LatinoNumber)
                {
                    // Por ahora lanzar error descriptivo
                    throw new RuntimeError(ge.Name, 
                        $"Los números no tienen métodos. Usa funciones de 'mate' (ej: mate.abs({obj})).");
                }

                // ═════════════════════════════════════════════════════════════════
                // FALLBACK: Tipo desconocido
                // ═════════════════════════════════════════════════════════════════
                throw new RuntimeError(ge.Name, 
                    $"El objeto de tipo '{obj.TypeName}' no tiene propiedades ni métodos.");
            }
            case SetExpression se:
                var target = Evaluate(se.Object);
                var value = Evaluate(se.Value);
                if (target is LatinoInstance inst) return inst.Set(se.Name, value);
                throw new RuntimeError(se.Name, "Solo las instancias tienen campos.");
            case IndexExpression ie:
                {
                    var idxObj = Evaluate(ie.Object);
                    var index = Evaluate(ie.Index);
                    if (idxObj is LatinoList list && index is LatinoNumber n) {
                        int i = (int)n.Value;
                        
                        // Soporte de índices negativos (Python-style)
                        if (i < 0) i += list.Elements.Count;
                        
                        if (i < 0 || i >= list.Elements.Count) 
                            throw new RuntimeError(ie.Bracket, $"Índice de lista fuera de rango: {n.Value}. La lista tiene {list.Elements.Count} elementos.");
                        
                        return list.Elements[i];
                    }
                    if (idxObj is LatinoDictionary d) {
                        if (d.Pairs.TryGetValue(index, out var valIdx)) return valIdx;
                        throw new RuntimeError(ie.Bracket, $"Clave no encontrada en diccionario: {index}");
                    }
                    // NUEVO: Soporte Avanzado para Cadenas (Unicode + Índices Negativos)
                    if (idxObj is LatinoString s && index is LatinoNumber sn)
                    {
                        // 1. Validar que sea entero
                        if (Math.Abs(sn.Value % 1) > double.Epsilon)
                            throw new RuntimeError(ie.Bracket, $"El índice debe ser un número entero, no {sn.Value}");
                        
                        int idx = (int)sn.Value;
                        
                        // 2. Usar StringInfo para manejar Emojis y acentos como un solo elemento
                        var si = new System.Globalization.StringInfo(s.Value);
                        int length = si.LengthInTextElements;
                        
                        // 3. Soporte de índices negativos (Python style)
                        if (idx < 0) idx += length;
                        
                        // 4. Validación de rango segura
                        if (idx < 0 || idx >= length)
                            throw new RuntimeError(ie.Bracket, $"Índice {sn.Value} fuera de rango. La cadena tiene {length} elementos visuales.");
                        
                        // 5. Retornar el grafema completo
                        return new LatinoString(si.SubstringByTextElements(idx, 1));
                    }
                    if (idxObj is LatinoTuple tuple && index is LatinoNumber tn) {
                        int i = (int)tn.Value;
                        if (i < 0 || i >= tuple.Elements.Count) throw new RuntimeError(ie.Bracket, $"Índice fuera de rango: {i}");
                        return tuple.Elements[i];
                    }
                    throw new RuntimeError(ie.Bracket, "No se puede indexar el objeto.");
                }
            case ListLiteralExpression lle:
                return new LatinoList(lle.Elements.Select(Evaluate).ToList());
            case TupleExpression te:
                return new LatinoTuple(te.Elements.Select(Evaluate).ToList());
            case DictLiteralExpression dle:
                {
                    var pairs = new Dictionary<LatinoObject, LatinoObject>(new LatinoObjectComparer());
                    for (int i = 0; i < dle.Keys.Count; i++) pairs[Evaluate(dle.Keys[i])] = Evaluate(dle.Values[i]);
                    return new LatinoDictionary(pairs);
                }
            case IndexSetExpression ise:
                {
                    var targetObj = Evaluate(ise.Object);
                    var indexVal = Evaluate(ise.Index);
                    var newVal = Evaluate(ise.Value);

                    if (targetObj is LatinoDictionary dictSetter)
                    {
                        if (dictSetter.IsReadOnly)
                        {
                            throw new RuntimeError(null!, "No se puede modificar un diccionario protegido (Librería Nativa).");
                        }
                        dictSetter.Pairs[indexVal] = newVal;
                        return newVal;
                    }
                    if (targetObj is LatinoList listSetter)
                    {
                        if (indexVal is LatinoNumber nSetter)
                        {
                            int i = (int)nSetter.Value;
                            
                            // Soporte de índices negativos (Python-style)
                            if (i < 0) i += listSetter.Elements.Count;

                            if (i >= 0 && i < listSetter.Elements.Count)
                            {
                                listSetter.Elements[i] = newVal;
                                return newVal;
                            }
                            throw new RuntimeError(null!, $"Índice de lista fuera de rango: {nSetter.Value}. La lista tiene {listSetter.Elements.Count} elementos.");
                        }
                        throw new RuntimeError(null!, "El índice de lista debe ser numérico.");
                    }
                    throw new RuntimeError(null!, "Solo se pueden modificar índices de listas o diccionarios.");
                }
            case AnonymousFunctionExpression afe:
                return new LatinoFunction(new FunctionStatement(new Token(TokenType.Identifier, "anonima", null!, 0, 0), afe.Params, afe.Body), _environment);
            case TernaryExpression ternary:
                return IsTruthy(Evaluate(ternary.Condition)) ? Evaluate(ternary.ThenBranch) : Evaluate(ternary.ElseBranch);
            default: return LatinoNull.Instance;
        }
    }

    private LatinoObject EvaluateBinary(BinaryExpression b)
    {
        // Pipe Operator (evaluación especial estilo Elixir)
        if (b.Operator.Type == TokenType.Pipe)
        {
            // 1. Evaluar el dato base (lado izquierdo)
            var argument = Evaluate(b.Left);

            // 2. MAGIA: Si el lado derecho es una llamada a función con parámetros parciales (ej. f(x))
            if (b.Right is CallExpression callExpr)
            {
                var calleeObj = Evaluate(callExpr.Callee);
                if (calleeObj is not ICallable funcCall)
                {
                    throw new RuntimeError(b.Operator, 
                        $"El lado derecho del operador pipe (|>) debe apuntar a una función.");
                }

                // Construir lista de argumentos de forma explícita (preserva orden y facilita debugging)
                var args = new List<LatinoObject> { argument };
                foreach (var argExpr in callExpr.Arguments)
                {
                    args.Add(Evaluate(argExpr));
                }

                return funcCall.Call(this, args);
            }

            // 3. Comportamiento clásico (el lado derecho es una referencia limpia a función, ej: mate.abs)
            var functionObj = Evaluate(b.Right);
            if (functionObj is not ICallable func)
            {
                throw new RuntimeError(b.Operator, 
                    $"El lado derecho del operador pipe (|>) debe ser una función, no '{functionObj.GetType().Name}'.");
            }
            
            return func.Call(this, new List<LatinoObject> { argument });
        }

        // NUEVO: Operador de Fusión Nula (??) - Cortocircuito seguro
        if (b.Operator.Type == TokenType.NullCoalescing)
        {
            LatinoObject leftNC;
            try 
            {
                leftNC = Evaluate(b.Left);
            } 
            catch (RuntimeError ex) when (
                ex.Message.StartsWith("Variable no definida") ||
                ex.Message.StartsWith("Clave no encontrada") ||
                ex.Message.Contains("no tiene la clave")
            )
            {
                // Solo tratamos como 'nulo' si la variable o clave no existe
                leftNC = LatinoNull.Instance;
            }

            if (leftNC is not LatinoNull) return leftNC;
            return Evaluate(b.Right);
        }

        var left = Evaluate(b.Left);

        // --- CORTOCIRCUITO LÓGICO ---
        if (b.Operator.Type == TokenType.LogicAnd)
        {
            if (!IsTruthy(left)) return new LatinoBoolean(false);
            return new LatinoBoolean(IsTruthy(Evaluate(b.Right)));
        }
        if (b.Operator.Type == TokenType.LogicOr)
        {
            if (IsTruthy(left)) return new LatinoBoolean(true);
            return new LatinoBoolean(IsTruthy(Evaluate(b.Right)));
        }

        var right = Evaluate(b.Right);

        // --- REGEX BLINDADO (Operador ~=) ---
        if (b.Operator.Type == TokenType.RegexMatch)
        {
            try
            {
                string input = left.ToString();
                string pattern = right.ToString();
                
                // USAR CONFIGURACIÓN GLOBAL
                return new LatinoBoolean(System.Text.RegularExpressions.Regex.IsMatch(
                    input, pattern, 
                    System.Text.RegularExpressions.RegexOptions.None, 
                    SysLib.RegexTimeout));
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                throw new RuntimeError(b.Operator, 
                    $"La expresión regular excedió el tiempo límite de ejecución ({SysLib.RegexTimeoutSegundos}s). " +
                    "Puedes aumentarlo con sis.config('regex_timeout', segundos)");
            }
            catch (Exception ex)
            {
                throw new RuntimeError(b.Operator, $"Error en Regex: {ex.Message}");
            }
        }

        // --- ARITMÉTICA LAXA (Alquimia) ---
        if (b.Operator.Type == TokenType.Plus)
        {
            // NUEVA REGLA: + es exclusivamente aritmético
            // Aplica Smart Casting: "10" + 5 → 15
            // Rechaza texto no numérico: "Hola" + 5 → ERROR
            return new LatinoNumber(ToDouble(left, b.Operator) + ToDouble(right, b.Operator));
        }
        
        if (b.Operator.Type == TokenType.Minus) return new LatinoNumber(ToDouble(left, b.Operator) - ToDouble(right, b.Operator));
        if (b.Operator.Type == TokenType.Star) return new LatinoNumber(ToDouble(left, b.Operator) * ToDouble(right, b.Operator));
        if (b.Operator.Type == TokenType.Slash) 
        {
            double den = ToDouble(right, b.Operator);
            if (den == 0) throw new RuntimeError(b.Operator, "División por cero.");
            return new LatinoNumber(ToDouble(left, b.Operator) / den);
        }
        if (b.Operator.Type == TokenType.Modulo) return new LatinoNumber(ToDouble(left, b.Operator) % ToDouble(right, b.Operator));
        if (b.Operator.Type == TokenType.Power) return new LatinoNumber(Math.Pow(ToDouble(left, b.Operator), ToDouble(right, b.Operator)));

        if (b.Operator.Type == TokenType.Concat) return new LatinoString(left.ToString() + right.ToString());

        // --- COMPARACIONES ---
        if (b.Operator.Type == TokenType.EqualEqual) return new LatinoBoolean(IsEqual(left, right));
        if (b.Operator.Type == TokenType.NotEqual) return new LatinoBoolean(!IsEqual(left, right));
        
        if (b.Operator.Type == TokenType.Greater || b.Operator.Type == TokenType.GreaterEqual ||
            b.Operator.Type == TokenType.Less || b.Operator.Type == TokenType.LessEqual)
        {
            // Caso A: Ambos son cadenas -> Comparación lexicográfica
            if (left is LatinoString lS && right is LatinoString rS)
            {
                int comp = string.Compare(lS.Value, rS.Value);
                if (b.Operator.Type == TokenType.Greater) return new LatinoBoolean(comp > 0);
                if (b.Operator.Type == TokenType.GreaterEqual) return new LatinoBoolean(comp >= 0);
                if (b.Operator.Type == TokenType.Less) return new LatinoBoolean(comp < 0);
                if (b.Operator.Type == TokenType.LessEqual) return new LatinoBoolean(comp <= 0);
            }
            
            // Caso B: Tratar como números (admite conversiones automáticas de bools, nulos, y cadenas numéricas)
            double lVal = ToDouble(left, b.Operator);
            double rVal = ToDouble(right, b.Operator);
            
            if (b.Operator.Type == TokenType.Greater) return new LatinoBoolean(lVal > rVal);
            if (b.Operator.Type == TokenType.GreaterEqual) return new LatinoBoolean(lVal >= rVal);
            if (b.Operator.Type == TokenType.Less) return new LatinoBoolean(lVal < rVal);
            if (b.Operator.Type == TokenType.LessEqual) return new LatinoBoolean(lVal <= rVal);
        }


        throw new RuntimeError(b.Operator, $"Operador binario desconocido: {b.Operator.Text}");
    }

    private double ToDouble(LatinoObject obj, Token oper)
    {
        if (obj is LatinoNumber n) return n.Value;
        if (obj is LatinoBoolean b) return b.Value ? 1.0 : 0.0;
        
        if (obj is LatinoString s)
        {
            if (double.TryParse(s.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
        }
        
        if (obj is LatinoNull) return 0.0;
        
        throw new RuntimeError(oper, $"No se puede convertir '{obj.TypeName}' a número.");
    }

    public bool IsTruthy(LatinoObject obj) => !(obj is LatinoNull || (obj is LatinoBoolean b && !b.Value));

    // ═════════════════════════════════════════════════════════════════════
    // MÉTODOS DE EXTENSIÓN PARA PRIMITIVOS: GUÍA DE COMPORTAMIENTO
    // ═════════════════════════════════════════════════════════════════════
    //
    // SINTAXIS:
    //   objeto.metodo(args) → libreria.metodo(objeto, args)
    //
    // EJEMPLOS:
    //   "hola".mayusculas()     → cadena.mayusculas("hola")
    //   [1,2].agregar(3)        → lista.agregar([1,2], 3)
    //   {"a":1}.contiene("a")   → dic.contiene({"a":1}, "a")
    //
    // REGLAS DE PRIORIDAD:
    // 1. POO (Instancias de Clase) - Máxima prioridad
    //    usuario.nombre → Busca en campos/métodos de la clase
    //
    // 2. DICCIONARIOS - Prioridad de CLAVES sobre métodos
    //    d = {"longitud": 100}
    //    d.longitud     → Retorna 100 (valor de la clave)
    //    dic.longitud(d) → Retorna 1 (número de claves)
    //
    // 3. PRIMITIVOS (Listas, Cadenas) - Métodos de librería
    //    [1,2].agregar(3) → lista.agregar([1,2], 3)
    //
    // MANEJO DE CONFLICTOS (Sombreado de Miembros):
    //   Problema: ¿Qué pasa si un diccionario tiene una clave llamada igual
    //             que un método? Ej: {"contiene": true}
    //   Solución: Las claves SIEMPRE tienen prioridad.
    //             - d.contiene → Valor de la clave
    //             - dic.contiene(d, "x") → Sintaxis explícita para el método
    //
    // RETROCOMPATIBILIDAD: 100% garantizada
    //   Sintaxis antigua (cadena.mayusculas(txt)) sigue funcionando igual.
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Busca un método en una librería nativa y crea un wrapper que inyecta
    /// el objeto 'self' como primer argumento automáticamente.
    /// </summary>
    private LatinoObject? BindPrimitiveMethod(LatinoObject self, string libraryName, string methodName)
    {
        // 1. Buscar librería en el scope global
        try
        {
            var libraryObj = Globals.GetAt(libraryName);
            if (libraryObj is not LatinoDictionary lib)
                return null;
            
            // 2. Buscar función en la librería
            var methodKey = new LatinoString(methodName);
            if (!lib.Pairs.TryGetValue(methodKey, out var funcObj))
                return null;
            
            if (funcObj is not NativeFunction nativeFunc)
                return null;
            
            // 3. Crear wrapper que pre-carga 'self' como primer argumento
            var wrapper = new NativeFunction((interp, args) => 
            {
                var fullArgs = new List<LatinoObject> { self };
                fullArgs.AddRange(args);
                return nativeFunc.Call(interp, fullArgs);
            }, -1);
            
            wrapper.Description = nativeFunc.Description;
            wrapper.ParameterNames = nativeFunc.ParameterNames;
            
            return wrapper;
        }
        catch
        {
            return null;
        }
    }
    private bool IsEqual(LatinoObject a, LatinoObject b)
    {
        // FIX CRÍTICO: Usar a.Equals(b) para que LatinoTuple.Equals se ejecute.
        // NO usar GetValue().Equals() porque eso compara las listas por referencia.
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
}
