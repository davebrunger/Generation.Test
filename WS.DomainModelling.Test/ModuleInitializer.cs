using System.Runtime.CompilerServices;

namespace WS.DomainModelling.Test;

public class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
    }
}
