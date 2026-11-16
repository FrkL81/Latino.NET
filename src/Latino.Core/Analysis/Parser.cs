using System;
using System.Collections.Generic;
using Latino.Core.Analysis.Nodes;
using Latino.Core.Types;

namespace Latino.Core.Analysis;

public partial class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<ParseError> Errors { get; } = new();

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        Token current = Peek();

        // Lógica de Error Amigable
        if (type == TokenType.Identifier)
        {
            // Caso específico para 'y' (el error más común)
            if (current.Text == "y")
            {
                throw Error(current, "'y' es un operador reservado (AND). Intente usar 'Y' mayúscula.");
            }

            // Caso general para otras palabras reservadas (si, mientras, o, no...)
            // Verificamos si es una Keyword o un Operador Lógico con texto
            if (current.Type == TokenType.Keyword || 
                current.Type == TokenType.LogicAnd || 
                current.Type == TokenType.LogicOr || 
                current.Type == TokenType.LogicNot)
            {
                throw Error(current, $"'{current.Text}' es una palabra reservada, no puede usarse como variable.");
            }
        }

        throw Error(current, message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EndOfFile;

    private Token Peek() => _tokens[_current];

    private Token Previous() => _tokens[_current - 1];

    private Token PeekNext()
    {
        if (_current + 1 >= _tokens.Count) return _tokens[_tokens.Count - 1]; // EOF
        return _tokens[_current + 1];
    }

    private Exception Error(Token token, string message)
    {
        return new ParseError(token, message);
    }
}
