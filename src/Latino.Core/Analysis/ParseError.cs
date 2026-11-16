using System;

namespace Latino.Core.Analysis;

public class ParseError : Exception
{
    public Token Token { get; }
    public ParseError(Token token, string message) : base(message)
    {
        Token = token;
    }
}
