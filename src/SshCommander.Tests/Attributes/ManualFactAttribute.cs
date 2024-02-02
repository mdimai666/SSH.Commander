using System.Diagnostics;

namespace SSH.Commander.Tests.Attributes;

public class ManualFactAttribute : FactAttribute
{
    public ManualFactAttribute()
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Only running in interactive mode.";
        }
    }
}

