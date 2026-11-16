using System;

namespace Latino.Core.Runtime;

public class GotoException : Exception
{
    public string TargetLabel { get; }
    public GotoException(string targetLabel)
    {
        TargetLabel = targetLabel;
    }
}
