namespace WS.DomainModelling.Common;

/// <summary>
/// An incremental source generator that provides common functionality for domain modelling.
/// </summary>
[Generator]
public class CommonGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator.
    /// </summary>
    /// <param name="context">The context for the incremental generator initialization.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddEmbeddedAttributeDefinition();
        });
    }
}
