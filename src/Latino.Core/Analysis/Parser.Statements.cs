using System.Collections.Generic;
using Latino.Core.Analysis.Nodes;

namespace Latino.Core.Analysis;

public partial class Parser
{
    public List<Statement> Parse()
    {
        List<Statement> statements = new List<Statement>();
        while (!IsAtEnd())
        {
            var stmt = Declaration();
            if (stmt != null) statements.Add(stmt);
        }
        return statements;
    }

    private Statement? Declaration()
    {
        try
        {
            // Etiquetas (Label): Identificador + Dos Puntos
            if (Check(TokenType.Identifier) && PeekNext().Type == TokenType.Colon)
            {
                Token name = Advance();
                Consume(TokenType.Colon, "Se esperaba ':' tras etiqueta.");
                return new LabelStatement(name);
            }

            if (MatchKeywords("clase")) return ClassDeclaration();
            if (MatchKeywords("local")) return LocalDeclaration(); // NUEVO: Reconocer 'local'
            if (MatchKeywords("global")) return GlobalDeclaration();

            return Statement();
        }
        catch (ParseError ex)
        {
            Errors.Add(ex);
            Synchronize();
            return null;
        }
        catch (System.Exception ex)
        {
            Errors.Add(new ParseError(Peek(), ex.Message));
            Synchronize();
            return null;
        }
    }

    private Statement GlobalDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Se esperaba nombre de variable.");
        ValidateIdentifier(name); // Nuevo nombre para evitar conflictos
        Expression? initializer = null;
        if (Match(TokenType.Equal)) initializer = Expression();
        Match(TokenType.Semicolon);
        return new VarDeclaration(name, initializer, false); // No es local
    }

    // --- Nueva declaración local ---
    private Statement LocalDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Se esperaba nombre de variable.");
        ValidateIdentifier(name); 
        Expression? initializer = null;
        if (Match(TokenType.Equal)) initializer = Expression();
        Match(TokenType.Semicolon);
        return new VarDeclaration(name, initializer, true); // ¡El flag es true!
    }

    private Statement ClassDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Nombre de clase.");
        ValidateIdentifier(name);

        VariableExpression? superclass = null;
        if (MatchKeywords("es"))
        {
            superclass = new VariableExpression(Consume(TokenType.Identifier, "Nombre superclase."));
        }

        List<FunctionStatement> methods = new();
        while (!CheckKeyword("fin") && !IsAtEnd())
        {
            if (MatchKeywords("funcion", "fun"))
            {
                methods.Add((FunctionStatement)ParseFunction("método"));
            }
            else if (MatchKeywords("constructor"))
            {
                Token kw = Previous();
                Token ctorName = new Token(TokenType.Identifier, "constructor", null, kw.Line, kw.Column);
                var parts = ParseFunctionBody();
                ConsumeKeyword("fin", "Se esperaba 'fin' tras constructor.");
                methods.Add(new FunctionStatement(ctorName, parts.Params, parts.Body));
            }
            else
            {
                throw Error(Peek(), "Se esperaba funcion o constructor.");
            }
        }
        ConsumeKeyword("fin", "Se esperaba 'fin' tras clase.");
        return new ClassStatement(name, superclass, methods);
    }

    private Statement Statement()
    {

        if (MatchKeywords("si")) return IfStatement();
        if (MatchKeywords("intentar")) return TryCatchStatement();
        if (MatchKeywords("mientras")) return WhileStatement();
        if (MatchKeywords("repetir")) return DoWhileStatement();
        if (MatchKeywords("desde")) return ForStatement();
        if (MatchKeywords("para")) return ForeachStatement();
        if (MatchKeywords("elegir")) return SwitchStatement();
        if (MatchKeywords("funcion", "fun")) return ParseFunction("funcion");
        if (MatchKeywords("retornar", "retorno", "ret")) return ReturnStatement();
        if (MatchKeywords("romper")) return BreakStatement();
        if (MatchKeywords("continuar")) return ContinueStatement();
        if (MatchKeywords("ir")) return GotoStatement();
        if (Match(TokenType.LBrace)) return new BlockStatement(Block());
        return ExpressionStatement();
    }

    // --- MÉTODOS AUXILIARES RENOMBRADOS ---

    // Valida que un identificador no sea una palabra reservada
    private void ValidateIdentifier(Token token)
    {
        if (Lexer.Keywords.ContainsKey(token.Text))
            throw Error(token, $"'{token.Text}' es una palabra reservada.");
    }

    // Verifica si el token actual es una palabra clave específica
    private bool MatchKeywords(params string[] keywords)
    {
        if (Check(TokenType.Keyword))
        {
            foreach (var k in keywords)
            {
                if (Peek().Text == k)
                {
                    Advance();
                    return true;
                }
            }
        }
        return false;
    }

    private void ConsumeKeyword(string keyword, string msg)
    {
        if (!MatchKeywords(keyword)) throw Error(Peek(), msg);
    }

    private bool CheckKeyword(string keyword) => Check(TokenType.Keyword) && Peek().Text == keyword;

    // --- PARSEO DE FUNCIONES ---

    private Statement ParseFunction(string kind)
    {
        Token name = Consume(TokenType.Identifier, $"Nombre de {kind}.");
        ValidateIdentifier(name);
        var parts = ParseFunctionBody();
        ConsumeKeyword("fin", $"Fin de {kind}.");
        return new FunctionStatement(name, parts.Params, parts.Body);
    }

    private (List<Parameter> Params, List<Statement> Body) ParseFunctionBody()
    {
        Consume(TokenType.LParen, "Se esperaba '(' después del nombre.");
        List<Parameter> parameters = new();
        
        if (!Check(TokenType.RParen))
        {
            bool optionalStarted = false;
            do
            {
                if (parameters.Count >= 255) Error(Peek(), "Máximo 255 parámetros.");

                Token paramName = Consume(TokenType.Identifier, "Se esperaba nombre de parámetro.");
                Expression? paramDefault = null;

                if (Match(TokenType.Equal))
                {
                    optionalStarted = true;
                    paramDefault = Expression(); // Lee el valor por defecto
                }
                else if (optionalStarted)
                {
                    throw Error(paramName, "Los parámetros obligatorios no pueden ir después de los opcionales.");
                }

                parameters.Add(new Parameter(paramName, paramDefault));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RParen, "Se esperaba ')' después de parámetros.");
        
        List<Statement> body = Block();
        return (parameters, body);
    }

    // --- OTRAS SENTENCIAS ---

    private Statement GotoStatement()
    {
        // Soporte tanto para 'ir etiqueta' como 'ir a etiqueta'
        // 'a' suele ser un identificador en este punto si no es keyword
        if (Check(TokenType.Identifier) && Peek().Text == "a" && PeekNext().Type == TokenType.Identifier)
        {
            Advance(); // Consumir 'a'
        }
        
        Token label = Consume(TokenType.Identifier, "Se esperaba el nombre de una etiqueta.");
        Match(TokenType.Semicolon);
        return new GotoStatement(label);
    }

    private List<Statement> Block()
    {
        List<Statement> stmts = new();
        while (!IsBlockEnd() && !IsAtEnd())
        {
            var s = Declaration();
            if (s != null) stmts.Add(s);
        }
        return stmts;
    }

    private bool IsBlockEnd()
    {
        return CheckKeyword("fin") || 
               CheckKeyword("sino") || 
               CheckKeyword("osi") || 
               CheckKeyword("hasta") || 
               CheckKeyword("caso") || 
               CheckKeyword("otro") || 
               CheckKeyword("defecto") ||
               CheckKeyword("atrapar");
    }

    private Statement IfStatement() {
        Expression cond = Expression();
        Statement thenB = new BlockStatement(Block());
        Statement? elseB = null;
        if (MatchKeywords("osi")) elseB = IfStatement();
        else if (MatchKeywords("sino")) {
            elseB = new BlockStatement(Block());
            ConsumeKeyword("fin", "fin del sino");
        } else ConsumeKeyword("fin", "fin del si");
        return new IfStatement(cond, thenB, elseB);
    }

    private Statement WhileStatement()
    {
        Expression condition = Expression();
        Statement body = new BlockStatement(Block());
        ConsumeKeyword("fin", "Se esperaba 'fin' al final del 'mientras'.");
        return new WhileStatement(condition, body);
    }

    private Statement DoWhileStatement()
    {
        Statement body = new BlockStatement(Block());
        ConsumeKeyword("hasta", "Se esperaba 'hasta' después del bloque 'repetir'.");
        Expression condition = Expression();
        return new DoWhileStatement(body, condition);
    }

    private Statement ForStatement()
    {
        Consume(TokenType.LParen, "Se esperaba '(' después de 'desde'.");
        Statement? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else
        {
            initializer = ExpressionStatement();
        }
        Expression? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = Expression();
        }
        Consume(TokenType.Semicolon, "Se esperaba ';' después de la condición.");
        Expression? increment = null;
        if (!Check(TokenType.RParen))
        {
            increment = Expression();
        }
        Consume(TokenType.RParen, "Se esperaba ')' después de las cláusulas del 'desde'.");
        Statement body = new BlockStatement(Block());
        ConsumeKeyword("fin", "Se esperaba 'fin' al terminar el 'desde'.");
        return new ForStatement(initializer, condition, increment, body);
    }

    private Statement ForeachStatement()
    {
        Token variable = Consume(TokenType.Identifier, "Se esperaba un nombre de variable después de 'para'.");
        ConsumeKeyword("en", "Se esperaba 'en' después de la variable.");
        Expression iterable = Expression();
        Statement body = new BlockStatement(Block());
        ConsumeKeyword("fin", "Se esperaba 'fin' al terminar el bucle 'para'.");
        return new ForeachStatement(variable, iterable, body);
    }

    private Statement SwitchStatement()
    {
        Expression value = Expression();
        List<CaseStatement> cases = new List<CaseStatement>();
        Statement? defaultBranch = null;
        while (!CheckKeyword("fin") && !IsAtEnd())
        {
            if (MatchKeywords("caso"))
            {
                Expression caseValue = Expression();
                Consume(TokenType.Colon, "Se esperaba ':' después del valor del caso.");
                List<Statement> caseBody = new List<Statement>();
                while (!CheckKeyword("caso") && !CheckKeyword("otro") && !CheckKeyword("defecto") && !CheckKeyword("fin") && !IsAtEnd())
                {
                    var stmt = Declaration();
                    if (stmt != null)
                        caseBody.Add(stmt);
                }
                cases.Add(new CaseStatement(caseValue, new BlockStatement(caseBody)));
            }
            else if (MatchKeywords("otro", "defecto"))
            {
                if (defaultBranch != null) throw Error(Previous(), "Solo puede haber una sección 'otro/defecto'.");
                Consume(TokenType.Colon, "Se esperaba ':' después de 'otro'.");
                List<Statement> defaultBody = new List<Statement>();
                while (!CheckKeyword("caso") && !CheckKeyword("fin") && !IsAtEnd())
                {
                    var stmt = Declaration();
                    if (stmt != null)
                        defaultBody.Add(stmt);
                }
                defaultBranch = new BlockStatement(defaultBody);
            }
            else
            {
                throw Error(Peek(), "Se esperaba 'caso' u 'otro' dentro de 'elegir'.");
            }
        }
        ConsumeKeyword("fin", "Se esperaba 'fin' al cerrar 'elegir'.");
        return new SwitchStatement(value, cases, defaultBranch);
    }

    private Statement TryCatchStatement()
    {
        Statement tryBranch = new BlockStatement(Block());
        Token? variable = null;
        if (MatchKeywords("atrapar"))
        {
            if (Match(TokenType.LParen))
            {
                variable = Consume(TokenType.Identifier, "Se esperaba nombre de variable de error.");
                Consume(TokenType.RParen, "Se esperaba ')' después de la variable.");
            }
        }
        else
        {
            throw Error(Peek(), "Se esperaba 'atrapar' después del bloque 'intentar'.");
        }
        Statement catchBranch = new BlockStatement(Block());
        ConsumeKeyword("fin", "Se esperaba 'fin' al cerrar la estructura intentar-atrapar.");
        return new TryCatchStatement(tryBranch, variable, catchBranch);
    }

    private Statement ReturnStatement()
    {
        Token keyword = Previous();
        Expression? value = null;
        if (!IsBlockEnd())
        {
            value = Expression();
        }
        Match(TokenType.Semicolon);
        return new ReturnStatement(keyword, value);
    }

    private Statement BreakStatement()
    {
        Token keyword = Previous();
        Match(TokenType.Semicolon);
        return new BreakStatement(keyword);
    }

    private Statement ContinueStatement()
    {
        Token keyword = Previous();
        Match(TokenType.Semicolon);
        return new ContinueStatement(keyword);
    }

    private Statement ExpressionStatement()
    {
        Expression expr = Expression();
        
        // NUEVA LÓGICA: Swap Genérico (Permite propiedades e índices)
        if (Match(TokenType.Swap))
        {
            Expression right = Expression();
            Match(TokenType.Semicolon);
            return new SwapStatement(expr, right);
        }
        
        Match(TokenType.Semicolon);
        return new ExpressionStatement(expr);
    }

    private void Synchronize() {
        Advance();
        while (!IsAtEnd()) {
            if (Previous().Type == TokenType.Semicolon) return;
            if (Check(TokenType.Keyword)) return; // Simplificado
            Advance();
        }
    }
}
