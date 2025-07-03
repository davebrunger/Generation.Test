using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GenerationTest;

public static class BasicWrapperAttribute
{
    public static void AddBasicWrapperAttribute(this IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("DiscriminatedUnion.BasicWrapperAttribute.g.cs", SourceText.From("""
            using System;
            using Microsoft.CodeAnalysis;
                
            namespace DiscriminatedUnion
            {
                [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), Embedded]
                public sealed class BasicWrapperAttribute : Attribute
                {
                    public Type WrappedType { get; }
                    public string ValidateMethodName { get; }
                        
                    public BasicWrapperAttribute(Type wrappedType, string validateMethodName)
                    {
                        ValidateMethodName = validateMethodName;
                    }
               }
            }
            """, Encoding.UTF8));
    }
}