using System;
using Latino.Core.Types;

namespace Latino.Core.Runtime;

// No son errores, son señales de tráfico para el Intérprete

public class BreakException : Exception { }

public class ContinueException : Exception { }

public class ReturnException : Exception
{
    public LatinoObject Value { get; }

    public ReturnException(LatinoObject value)
    {
        Value = value;
    }
}
