namespace WS.DomainModelling.DiscriminatedUnion;

/// <summary>
/// Provides the Option attribute source code to be added during source generation.
/// </summary>
public static class OptionAttribute
{
    /// <summary>
    /// Adds the Option attribute source code to the generator context.
    /// </summary>
    /// <param name="context">The context for the incremental generator initialization.</param>
    public static void AddOptionAttribute(this IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("WS.DomainModelling.DiscriminatedUnion.OptionAttribute.g.cs", SourceText.From("""
            using System;
            using Microsoft.CodeAnalysis;
                
            namespace WS.DomainModelling.DiscriminatedUnion;
            
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true), Embedded]
            internal sealed class OptionAttribute : Attribute
            {
                public string PropertyName { get; }
            
                public OptionAttribute(string propertyName)
                {
                    PropertyName = propertyName;
                }

                public Type OfType { get; set; }
                public string OfGeneric { get; set; }
            }
            """, Encoding.UTF8));
    }
}

