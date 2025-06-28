using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GenerationTest;

[Generator]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddEmbeddedAttributeDefinition();
            postInitializationContext.AddOptionAttribute();
            postInitializationContext.AddOption();
        });

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            "DiscriminatedUnion.OptionAttribute",
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) =>
            {
                var _class = context.TargetSymbol;

                var properties = _class.GetAttributes()
                    .Select(GetOption)
                    .ToList();

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
                    Properties = properties,
                };
            });


        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            var matchParameters = model.Properties.Select(p => p.Match(s => $"Func<TResult> {s}Func", t => $"Func<{t.Value}, TResult> {t.Key}Func"));
            var switchParameters = model.Properties.Select(p => p.Match(s => $"Action {s}Action", t => $"Action<{t.Value}> {t.Key}Action"));

            var sourceText = SourceText.From($$"""
            using System;
            using System.Collections.Generic;
            
            namespace {{model.Namespace}};

            partial class {{model.ClassName}}
            {
                private enum {{model.FileName}}Option
                {
                    {{string.Join("\r\n        ", model.Properties.Select(p => p.Match(s => $"{s},", t => $"{t.Key},")))}}
                }

                private readonly {{model.ClassName}}.{{model.FileName}}Option option;

                {{string.Join("\r\n    ", model.Properties
                    .Where(p => p.IsTyped)
                    .Select(p => p.Match(_ => string.Empty, t => $$"""private {{t.Value}} {{t.Key}}_Value { get; init; }""")))}}

                {{string.Join("\r\n    ", model.Properties.Select(p => $"public bool Is{p.Match(s => s, t => t.Key)} => option == {model.FileName}Option.{p.Match(s => s, t => t.Key)};"))}}
  
                private {{model.FileName}}({{model.ClassName}}.{{model.FileName}}Option option)
                {
                    this.option = option;
                    {{string.Join("\r\n        ", model.Properties
                        .Where(p => p.IsTyped)
                        .Select(p => p.Match(_ => "", t => $"{t.Key}_Value = default!;")))}}
                }

                public TResult Match<TResult>({{string.Join(", ", matchParameters)}})
                {
                    return option switch
                    {
                        {{string.Join("\r\n            ", model.Properties.Select(p => p.Match(
                            s => $"{model.ClassName}.{model.FileName}Option.{s} => {s}Func(),",
                            t => $"{model.ClassName}.{model.FileName}Option.{t.Key} => {t.Key}Func({t.Key}_Value),")))}}
                        _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
                    };
                }

                public void Switch({{string.Join(", ", switchParameters)}})
                {
                    switch (option)
                    {
                        {{string.Join("\r\n            ", model.Properties.Select(p => p.Match(
                            s => $"case {model.ClassName}.{model.FileName}Option.{s}: {s}Action(); return;",
                            t => $"case {model.ClassName}.{model.FileName}Option.{t.Key}: {t.Key}Action({t.Key}_Value); return;")))}}
                        default: throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
                    };
                }
            
                {{string.Join("\r\n    ", model.Properties.Select(p => p.Match(
                    s => $$"""public static {{model.ClassName}} {{s}} { get; } = new {{model.ClassName}}({{model.ClassName}}.{{model.FileName}}Option.{{s}});""",
                    t => $$"""public static {{model.ClassName}} {{t.Key}}({{t.Value}} {{t.Key}}) => new {{model.ClassName}}({{model.ClassName}}.{{model.FileName}}Option.{{t.Key}}) { {{t.Key}}_Value = {{t.Key}} };""")))}}

                {{string.Join("\r\n    ", model.Properties.Select((p, i) => p.Match(
                    _ => "",
                    t => $"public DiscriminatedUnion.Option<{t.Value}> As{t.Key}() => Match<DiscriminatedUnion.Option<{t.Value}>>({GetMatch(model.Properties, i)});")))}}
            }
            """, Encoding.UTF8);

            context.AddSource($"{model.Namespace}.{model.FileName}.g.cs", sourceText);
        });
    }

    private static object GetMatch(List<DiscriminatedUnionOption> properties, int index)
    {
        var type = $"DiscriminatedUnion.Option<{properties[index].Match(s => s, t => t.Value)}>";

        return string.Join(", ", properties.Select((p, i) => i == index 
            ? $"({p.Match(s => s, t => t.Key)}) => {type}.Some({p.Match(s => s, t => t.Key)})"
            : $"({p.Match(_ => "", _ => "_")}) => {type}.None"));
    }

    private static DiscriminatedUnionOption GetOption(AttributeData a)
    {
        var type = a.NamedArguments.SingleOrDefault(na => na.Key == "OfType").Value.Value as INamedTypeSymbol;
        var genericType = a.NamedArguments.SingleOrDefault(na => na.Key == "OfGeneric").Value.Value as string;

        if (type != null && genericType != null)
        {
            throw new InvalidOperationException($"The attribute {a.AttributeClass?.ToDisplayString()} cannot have both 'OfType' and 'OfGeneric' properties set at the same time.");
        }

        if (type != null)
        {
            return DiscriminatedUnionOption.Typed(
                a.ConstructorArguments.First()!.Value!.ToString(),
                GetFullType(type));
        }

        if (genericType != null)
        {
            return DiscriminatedUnionOption.Typed(
                a.ConstructorArguments.First()!.Value!.ToString(),
                genericType);
        }

        return DiscriminatedUnionOption.Simple(a.ConstructorArguments.First()!.Value!.ToString());
    }

    private static string GetFullType(INamedTypeSymbol type)
    {
        return $"{type!.ContainingNamespace}.{type!.Name}";
    }
}
