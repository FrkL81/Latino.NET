using System.Collections.Generic;
using Latino.Core.Runtime;

namespace Latino.Core.Types;

public interface ICallable
{
    int Arity { get; }
    LatinoObject Call(Interpreter interpreter, List<LatinoObject> arguments);
}
