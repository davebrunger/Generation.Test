using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GenerationTest;

public static class OptionAttribute
{
    public static void AddOptionAttribute(this IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("DiscriminatedUnion.OptionAttribute.g.cs", SourceText.From("""
            using System;
            using Microsoft.CodeAnalysis;
                
            namespace DiscriminatedUnion
            {
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
            }
            """, Encoding.UTF8));
    }
}

