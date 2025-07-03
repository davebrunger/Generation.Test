using System.Runtime.CompilerServices;

namespace Generation.Test.Test;

public class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
    }
}
