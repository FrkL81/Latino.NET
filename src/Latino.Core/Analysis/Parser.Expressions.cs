using System.Collections.Generic;
using Latino.Core.Analysis.Nodes;
using Latino.Core.Types;

namespace Latino.Core.Analysis;

public partial class Parser
{
    // 1. Entrada Principal
    private Expression Expression() => Assignment();

    // 2. Asignación ( = , +=, -=, etc )
    private Expression Assignment()
    {
        Expression expr = Ternary();

        if (Match(TokenType.Equal, TokenType.PlusEqual, TokenType.MinusEqual,
                  TokenType.StarEqual, TokenType.SlashEqual, TokenType.ModEqual, TokenType.ConcatEqual, TokenType.NullCoalescingEqual))
        {
            Token oper = Previous();
            Expression value = Assignment(); // Recursivo

            if (expr is VariableExpression v)
            {
                // PROTECCIÓN CONTRA REASIGNACIÓN (solo si es asignación directa, no acceso a propiedades)
                if ((v.Name.Text == "mi" || v.Name.Text == "este" || v.Name.Text == "esta") && 
                    oper.Type == TokenType.Equal)
                {
                    throw Error(v.Name, $"'{v.Name.Text}' es una referencia de solo lectura. No puedes asignarle valor directamente.");
                }
                
                if (oper.Type == TokenType.Equal) return new AssignExpression(v.Name, value);

                TokenType binaryType = GetBinaryTypeForEqual(oper);
                if (binaryType == TokenType.Error) throw Error(oper, "Operador de asignación compuesta no soportado.");

                Token opToken = new Token(binaryType, oper.Text.TrimEnd('='), null, oper.Line, oper.Column);
                return new AssignExpression(v.Name, new BinaryExpression(v, opToken, value));
            }
            else if (expr is GetExpression get)
            {
                if (oper.Type == TokenType.Equal) return new SetExpression(get.Object, get.Name, value);
                
                TokenType binaryType = GetBinaryTypeForEqual(oper);
                if (binaryType != TokenType.NullCoalescing) 
                    throw Error(oper, "Solo se permite asignación simple '=' o fusión nula '??=' en propiedades por ahora.");

                Token opToken = new Token(binaryType, "??", null, oper.Line, oper.Column);
                return new SetExpression(get.Object, get.Name, new BinaryExpression(get, opToken, value));
            }
            else if (expr is IndexExpression idx)
            {
                if (oper.Type == TokenType.Equal) return new IndexSetExpression(idx.Object, idx.Index, value);
                
                TokenType binaryType = GetBinaryTypeForEqual(oper);
                if (binaryType != TokenType.NullCoalescing) 
                    throw Error(oper, "Solo se permite asignación simple '=' o fusión nula '??=' en índices por ahora.");

                Token opToken = new Token(binaryType, "??", null, oper.Line, oper.Column);
                return new IndexSetExpression(idx.Object, idx.Index, new BinaryExpression(idx, opToken, value));
            }

            throw Error(oper, "Destino de asignación inválido. Solo se puede asignar a variables, propiedades o índices.");
        }

        return expr;
    }

    private TokenType GetBinaryTypeForEqual(Token oper)
    {
        return oper.Type switch {
            TokenType.PlusEqual => TokenType.Plus,
            TokenType.MinusEqual => TokenType.Minus,
            TokenType.StarEqual => TokenType.Star,
            TokenType.SlashEqual => TokenType.Slash,
            TokenType.ModEqual => TokenType.Modulo,
            TokenType.ConcatEqual => TokenType.Concat,
            TokenType.NullCoalescingEqual => TokenType.NullCoalescing,
            _ => TokenType.Error
        };
    }

    // 2.5 Operador Ternario ( ? : )
    private Expression Ternary()
    {
        Expression expr = Pipeline();  // CAMBIO: antes era LogicOr()

        if (Match(TokenType.Question))
        {
            Expression thenBranch = Expression(); // Permitimos anidamiento
            Consume(TokenType.Colon, "Se esperaba ':' en el operador ternario.");
            Expression elseBranch = Ternary();    // Asociatividad derecha
            return new TernaryExpression(expr, thenBranch, elseBranch);
        }

        return expr;
    }

    // 2.6 Operador Pipeline ( |> )
    private Expression Pipeline()
    {
        Expression expr = NullCoalescing();

        while (Match(TokenType.Pipe))
        {
            Token oper = Previous();
            Expression right = NullCoalescing();
            expr = new BinaryExpression(expr, oper, right);
        }

        return expr;
    }

    // 2.7 Fusión Nula ( ?? )
    private Expression NullCoalescing()
    {
        Expression expr = LogicOr();
        
        while (Match(TokenType.NullCoalescing))
        {
            Token oper = Previous();
            Expression right = LogicOr();
            expr = new BinaryExpression(expr, oper, right);
        }
        
        return expr;
    }

    // 3. Lógica O ( || )
    private Expression LogicOr()
    {
        Expression expr = LogicAnd();
        while (Match(TokenType.LogicOr))
        {
            Token oper = Previous();
            Expression right = LogicAnd();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 4. Lógica Y ( && )
    private Expression LogicAnd()
    {
        Expression expr = Equality();
        while (Match(TokenType.LogicAnd))
        {
            Token oper = Previous();
            Expression right = Equality();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 5. Igualdad ( ==, !=, ~= )
    private Expression Equality()
    {
        Expression expr = Concatenation();
        while (Match(TokenType.EqualEqual, TokenType.NotEqual, TokenType.RegexMatch))
        {
            Token oper = Previous();
            Expression right = Concatenation();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 6. Concatenación ( .. )
    private Expression Concatenation()
    {
        Expression expr = Comparison();
        while (Match(TokenType.Concat))
        {
            Token oper = Previous();
            Expression right = Comparison();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 7. Comparación ( >, >=, <, <= )
    private Expression Comparison()
    {
        Expression expr = Term();
        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token oper = Previous();
            Expression right = Term();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 8. Término ( +, - )
    private Expression Term()
    {
        Expression expr = Factor();
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            Token oper = Previous();
            Expression right = Factor();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 9. Factor ( *, /, % )
    private Expression Factor()
    {
        Expression expr = Power();
        while (Match(TokenType.Star, TokenType.Slash, TokenType.Modulo))
        {
            Token oper = Previous();
            Expression right = Power();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 10. Potencia ( ^ )
    private Expression Power()
    {
        Expression expr = Unary();
        while (Match(TokenType.Power))
        {
            Token oper = Previous();
            Expression right = Unary();
            expr = new BinaryExpression(expr, oper, right);
        }
        return expr;
    }

    // 11. Unario ( !, -, ++, -- ) - Prefijo
    private Expression Unary()
    {
        if (Match(TokenType.LogicNot, TokenType.Minus, TokenType.Increment, TokenType.Decrement))
        {
            Token oper = Previous();
            Expression right = Unary();
            return new UnaryExpression(oper, right);
        }
        return Call();
    }

    // 12. Llamadas y Accesos ( . , (), [] )
    private Expression Call()
    {
        Expression expr = Primary();

        while (true)
        {
            if (Match(TokenType.LParen))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.Dot))
            {
                Token name;
                
                // Permitimos Identificadores
                if (Match(TokenType.Identifier))
                {
                    name = Previous();
                }
                // Permitimos CUALQUIER Keyword, Operador Lógico o New como nombre de propiedad
                // Esto permite: obj.y, obj.si, obj.crear, obj.fin, obj.nuevo
                else if (Match(TokenType.Keyword) || 
                         Match(TokenType.LogicAnd) || 
                         Match(TokenType.LogicOr) || 
                         Match(TokenType.LogicNot) ||
                         Match(TokenType.New)) // Captura nueva, crear, nuevo
                {
                    Token keyword = Previous();
                    // Lo convertimos a Identificador para el AST
                    name = new Token(TokenType.Identifier, keyword.Text, null, keyword.Line, keyword.Column);
                }
                else
                {
                    throw Error(Peek(), "Se esperaba el nombre de una propiedad o método después del punto.");
                }

                expr = new GetExpression(expr, name);
            }
            else if (Match(TokenType.LBracket))
            {
                Expression index = Expression();
                Token bracket = Consume(TokenType.RBracket, "Se esperaba ']' después del índice.");
                expr = new IndexExpression(expr, bracket, index);
            }
            else if (Match(TokenType.Increment, TokenType.Decrement))
            {
                Token oper = Previous();
                expr = new PostfixExpression(expr, oper);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expression FinishCall(Expression callee)
    {
        List<Expression> arguments = new List<Expression>();
        if (!Check(TokenType.RParen))
        {
            do {
                if (arguments.Count >= 255) Error(Peek(), "No se pueden tener más de 255 argumentos.");
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }
        Token paren = Consume(TokenType.RParen, "Se esperaba ')' después de los argumentos.");
        return new CallExpression(callee, paren, arguments);
    }

    // 13. Primario (Literales, Variables, Agrupación)
    private Expression Primary()
    {
        if (Match(TokenType.Keyword))
        {
            string txt = Previous().Text;
            if (txt == "verdadero" || txt == "cierto") return new LiteralExpression(new LatinoBoolean(true));
            if (txt == "falso" || txt == "espurio") return new LiteralExpression(new LatinoBoolean(false));
            if (txt == "nulo") return new LiteralExpression(LatinoNull.Instance);
            
            // SOPORTE PARA EL TRÍO
            if (txt == "mi" || txt == "este" || txt == "esta") 
            {
                return new VariableExpression(Previous());
            }
            
            // Soporte para Funciones Anónimas
            if (txt == "funcion" || txt == "fun")
            {
                var parts = ParseFunctionBody();
                ConsumeKeyword("fin", "Se esperaba 'fin' después del cuerpo de la función anónima.");
                return new AnonymousFunctionExpression(parts.Params, parts.Body);
            }
            
            // Importante: Si llegamos aquí y es una keyword reservada (ej: 'si'), lanzamos error.
            throw Error(Previous(), "Palabra clave inesperada en este contexto.");
        }

        if (Match(TokenType.Number)) return new LiteralExpression(Previous().Value);
        if (Match(TokenType.String)) return new LiteralExpression(Previous().Value);
        if (Match(TokenType.Identifier)) return new VariableExpression(Previous());
        
        // Soporte para Instanciación (nueva/crear/nuevo)
        if (Match(TokenType.New)) return Primary();

        if (Match(TokenType.LParen))
        {
            if (Match(TokenType.RParen)) return new TupleExpression(new List<Expression>());
            
            Expression expr = Expression();
            if (Match(TokenType.Comma)) {
                List<Expression> elements = new() { expr };
                if (!Check(TokenType.RParen)) {
                    do { elements.Add(Expression()); } while (Match(TokenType.Comma) && !Check(TokenType.RParen));
                }
                Consume(TokenType.RParen, "Se esperaba ')' para cerrar la tupla.");
                return new TupleExpression(elements);
            }
            Consume(TokenType.RParen, "Se esperaba ')' después de la expresión.");
            return new GroupingExpression(expr);
        }

        if (Match(TokenType.LBracket))
        {
            List<Expression> elements = new List<Expression>();
            if (!Check(TokenType.RBracket))
            {
                do { elements.Add(Expression()); } while (Match(TokenType.Comma) && !Check(TokenType.RBracket));
            }
            Consume(TokenType.RBracket, "Se esperaba ']' al final de la lista.");
            return new ListLiteralExpression(elements);
        }

        if (Match(TokenType.LBrace))
        {
             List<Expression> keys = new List<Expression>();
             List<Expression> values = new List<Expression>();
             if (!Check(TokenType.RBrace))
             {
                 do {
                     keys.Add(Expression());
                     Consume(TokenType.Colon, "Se esperaba ':' en el diccionario.");
                     values.Add(Expression());
                 } while (Match(TokenType.Comma) && !Check(TokenType.RBrace));
             }
             Consume(TokenType.RBrace, "Se esperaba '}' al final del diccionario.");
             return new DictLiteralExpression(keys, values);
        }

        Token token = Peek();
        if (token.Text == "y")
            throw Error(token, "'y' es un operador reservado (AND). Intente usar 'Y' mayúscula.");
            
        if (token.Type == TokenType.Keyword || token.Type == TokenType.LogicAnd || token.Type == TokenType.LogicOr || token.Type == TokenType.LogicNot)
            throw Error(token, $"'{token.Text}' es una palabra reservada, no puede usarse como variable.");

        throw Error(token, "Se esperaba una expresión.");
    }
}
