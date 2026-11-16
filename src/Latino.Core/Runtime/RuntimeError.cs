using System;
using Latino.Core.Analysis;

namespace Latino.Core.Runtime;

public class RuntimeError : Exception
{
    public Token Token { get; }

    public RuntimeError(Token token, string message) : base(message)
    {
        Token = token;
    }
}
