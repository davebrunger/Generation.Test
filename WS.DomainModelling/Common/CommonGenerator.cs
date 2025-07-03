using Microsoft.CodeAnalysis;

namespace WS.DomainModelling.Common;

[Generator]
public class CommonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddEmbeddedAttributeDefinition();
            postInitializationContext.AddOption();
        });
    }
}
