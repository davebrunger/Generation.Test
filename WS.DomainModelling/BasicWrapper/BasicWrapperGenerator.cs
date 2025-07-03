using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WS.DomainModelling.BasicWrapper;

[Generator]
public class BasicWrapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddBasicWrapperAttribute();
        });

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            "WS.DomainModelling.BasicWrapper.BasicWrapperAttribute",
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) =>
            {
                var _class = (context.TargetSymbol as INamedTypeSymbol)!;
                var attribute = context.Attributes.Single();
                var wrappedType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
                var validateMemberName = attribute.ConstructorArguments[1].Value!.ToString();
                var validateMethod = _class.GetMembers(validateMemberName)
                    .OfType<IMethodSymbol>()
                    .SingleOrDefault();

#pragma warning disable IDE0270 // Use coalesce expression
                if (validateMethod == null)
                {
                    throw new InvalidOperationException(
                        $"Class '{_class.Name}' must have a method named '{validateMemberName}' to validate the wrapper source.");
                }
#pragma warning restore IDE0270 // Use coalesce expression

                if (validateMethod.DeclaredAccessibility != Accessibility.Private)
                {
                    throw new InvalidOperationException(
                        $"Method '{validateMemberName}' in class '{_class.Name}' must be private");
                }

                if (!validateMethod.IsStatic)
                {
                    throw new InvalidOperationException(
                        $"Method '{validateMemberName}' in class '{_class.Name}' must be static");
                }

                if (validateMethod.ReturnType.SpecialType != SpecialType.System_Boolean)
                {
                    throw new InvalidOperationException(
                        $"Method '{validateMemberName}' in class '{_class.Name}' must return a boolean");
                }

                if (validateMethod.Parameters.Length != 1 
                    || !SymbolEqualityComparer.Default.Equals(validateMethod.Parameters[0].Type, wrappedType))
                {
                    throw new InvalidOperationException(
                        $"Method '{validateMemberName}' in class '{_class.Name}' must have a single {wrappedType!.Name} parameter");
                }

                return new
                {
                    Namespace = _class.ContainingNamespace?.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat
                            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                    ClassName = _class.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                            .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters))
                        .Split('.')
                        .Last(),
                    FileName = _class.Name,
                    WrappedType = wrappedType,
                    ValidateMemberName = validateMemberName

                };
            });

        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            var source = SourceText.From($$"""
            namespace {{model.Namespace}};

            sealed partial class {{model.ClassName}}
            {
                public {{model.WrappedType.Name}} Value { get; }

                private {{model.FileName}}({{model.WrappedType.Name}} value)
                {
                    Value = value;
                }

                public static WS.DomainModelling.Common.Option<{{model.ClassName}}> Create({{model.WrappedType.Name}} source)
                {
                    return {{model.ValidateMemberName}}(source)
                        ? WS.DomainModelling.Common.Option.Some(new {{model.ClassName}}(source))
                        : WS.DomainModelling.Common.Option.None;
                }

                public override string ToString()
                {
                    return Value.ToString();
                }
            }
            """, Encoding.UTF8);

            context.AddSource($"{model.Namespace}.{model.FileName}.g.cs", source);
        });
    }
}
