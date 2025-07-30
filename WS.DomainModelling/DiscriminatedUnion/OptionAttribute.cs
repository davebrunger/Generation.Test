namespace WS.DomainModelling.DiscriminatedUnion;

public static class OptionAttribute
{
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

