namespace WS.DomainModelling.BasicWrapper;

public static class BasicWrapperAttribute
{
    public static void AddBasicWrapperAttribute(this IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("WS.DomainModelling.BasicWrapper.BasicWrapperAttribute.g.cs", SourceText.From("""
            using System;
            using Microsoft.CodeAnalysis;
                
            namespace WS.DomainModelling.BasicWrapper
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