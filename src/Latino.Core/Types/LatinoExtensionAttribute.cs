using System;

namespace Latino.Core.Types;

[AttributeUsage(AttributeTargets.Method)]
public class LatinoExtensionAttribute : Attribute
{
    public string FunctionName { get; }
    public string Help { get; set; } = "";
    public string Args { get; set; } = "";
    public string Returns { get; set; } = "nulo";

    public LatinoExtensionAttribute(string functionName)
    {
        FunctionName = functionName;
    }
}
