namespace WS.DomainModelling.DiscriminatedUnion;

using GetOptionResult = Common.Result<
    DiscriminatedUnionOption,
    IReadOnlyList<(
        DiscriminatedUnionGenerator.GetOptionError Error,
        string Message,
        Location Location
    )>>;

[Generator]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    private static readonly Regex OptionNamePattern = new("^[_a-zA-Z][_a-zA-Z0-9]*$");
    
    public enum GetOptionError
    {
        InvalidName = 1,
        BothTypeAndGenericSet,
        InvalidGenericType
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddOptionAttribute();
        });

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            "WS.DomainModelling.DiscriminatedUnion.OptionAttribute",
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) =>
            {
                var _class = context.TargetSymbol as INamedTypeSymbol;

                var genericParameters = _class!.TypeParameters.Select(p => p.Name).ToImmutableHashSet();

                var properties = _class.GetAttributes()
                    .Where(a => a.AttributeClass!.Name == "OptionAttribute") // Make this better by checking the namespace too
                    .Select(a => GetOption(a, genericParameters))
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
            var (properties, errors) = model.Properties.Aggregate(
                (Properties: new List<DiscriminatedUnionOption>(), Errors: new List<(GetOptionError Error, string Message, Location Location)>()),
                (a, r) => {
                    r.Switch(
                        property => a.Properties.Add(property),
                        errors => a.Errors.AddRange(errors));
                    return a;
                });

            if (errors.Count > 0)
            {
                foreach (var (optionError, message, location) in errors)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        $"WSDM{(int)optionError:000}",
                        optionError.ToString(),
                        message,
                        DiagnosticSeverity.Error,
                        DiagnosticSeverity.Error,
                        true,
                        0,
                        location: location));
                }

                return;
            }

            var matchParameters = properties.Select(p => p.Match(s => $"Func<TResult> {s}Func", t => $"Func<{t.Value}, TResult> {t.Key}Func"));
            var switchParameters = properties.Select(p => p.Match(s => $"Action {s}Action", t => $"Action<{t.Value}> {t.Key}Action"));

            var sourceText = SourceText.From($$"""
            using System;
            using System.Collections.Generic;
            
            namespace {{model.Namespace}};

            partial class {{model.ClassName}}
            {
                private enum {{model.FileName}}Option
                {
                    {{string.Join("\r\n        ", properties.Select(p => p.Match(s => $"{s},", t => $"{t.Key},")))}}
                }

                private readonly {{model.ClassName}}.{{model.FileName}}Option option;

                {{string.Join("\r\n    ", properties
                    .Where(p => p.IsTyped)
                    .Select(p => p.Match(_ => string.Empty, t => $$"""private {{t.Value}} {{t.Key}}_Value { get; init; }""")))}}

                {{string.Join("\r\n    ", properties.Select(p => $"public bool Is{p.Match(s => s, t => t.Key)} => option == {model.FileName}Option.{p.Match(s => s, t => t.Key)};"))}}
  
                private {{model.FileName}}({{model.ClassName}}.{{model.FileName}}Option option)
                {
                    this.option = option;
                    {{string.Join("\r\n        ", properties
                        .Where(p => p.IsTyped)
                        .Select(p => p.Match(_ => "", t => $"{t.Key}_Value = default!;")))}}
                }

                public TResult Match<TResult>({{string.Join(", ", matchParameters)}})
                {
                    return option switch
                    {
                        {{string.Join("\r\n            ", properties.Select(p => p.Match(
                            s => $"{model.ClassName}.{model.FileName}Option.{s} => {s}Func(),",
                            t => $"{model.ClassName}.{model.FileName}Option.{t.Key} => {t.Key}Func({t.Key}_Value),")))}}
                        _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
                    };
                }

                public void Switch({{string.Join(", ", switchParameters)}})
                {
                    switch (option)
                    {
                        {{string.Join("\r\n            ", properties.Select(p => p.Match(
                            s => $"case {model.ClassName}.{model.FileName}Option.{s}: {s}Action(); return;",
                            t => $"case {model.ClassName}.{model.FileName}Option.{t.Key}: {t.Key}Action({t.Key}_Value); return;")))}}
                        default: throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
                    };
                }
            
                {{string.Join("\r\n    ", properties.Select(p => p.Match(
                    s => $$"""public static {{model.ClassName}} {{s}} { get; } = new {{model.ClassName}}({{model.ClassName}}.{{model.FileName}}Option.{{s}});""",
                    t => $$"""public static {{model.ClassName}} {{t.Key}}({{t.Value}} {{t.Key}}) => new {{model.ClassName}}({{model.ClassName}}.{{model.FileName}}Option.{{t.Key}}) { {{t.Key}}_Value = {{t.Key}} };""")))}}

                {{string.Join("\r\n    ", properties.Select((p, i) => p.Match(
                    _ => "",
                    t => $"public WS.DomainModelling.Common.Option<{t.Value}> As{t.Key}() => Match<WS.DomainModelling.Common.Option<{t.Value}>>({GetMatch(properties, i)});")))}}

                public override string ToString()
                {
                    return Match(
                        {{string.Join(",\r\n            ", properties.Select(p => p.Match(
                            s => $$"""() => "{{s}}" """,
                            t => $$"""({{t.Key}}) => $"{{t.Key}} ({{{t.Key}}})" """)))}}
                    );
                }
            }
            """, Encoding.UTF8);

            context.AddSource($"{model.Namespace}.{model.FileName}.g.cs", sourceText);
        });
    }

    private static object GetMatch(List<DiscriminatedUnionOption> properties, int index)
    {
        var type = $"WS.DomainModelling.Common.Option<{properties[index].Match(s => s, t => t.Value)}>";

        return string.Join(", ", properties.Select((p, i) => i == index 
            ? $"({p.Match(s => s, t => t.Key)}) => {type}.Some({p.Match(s => s, t => t.Key)})"
            : $"({p.Match(_ => "", _ => "_")}) => {type}.None"));
    }

    private static GetOptionResult GetOption(AttributeData attributeData, IReadOnlyCollection<string> genericParameters)
    {
        var name = attributeData.ConstructorArguments.Single().Value!.ToString();
        var type = attributeData.NamedArguments.SingleOrDefault(na => na.Key == "OfType").Value.Value as INamedTypeSymbol;
        var genericType = attributeData.NamedArguments.SingleOrDefault(na => na.Key == "OfGeneric").Value.Value as string;
        var location = attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()!;

        var errors = new List<(GetOptionError, string, Location)>();

        if (type != null && genericType != null)
        {
            errors.Add((GetOptionError.BothTypeAndGenericSet, 
                $"The attribute {attributeData.AttributeClass?.ToDisplayString()} ({name}) cannot have both 'OfType' and 'OfGeneric' properties set at the same time.",
                location));
        }

        if (!OptionNamePattern.IsMatch(name))
        {
            errors.Add((GetOptionError.InvalidName,
                $"The name {name} does not meet the option name restrictions",
                location));
        }

        if (genericType != null && !genericParameters.Contains(genericType))
        {
            errors.Add((GetOptionError.InvalidGenericType,
                $"The generic type {genericType} must be one of {string.Join(", ", genericParameters)}",
                location));
        }

        if (errors.Count > 0)
        {
            return GetOptionResult.Error(errors);
        }

        if (type != null)
        {
            return GetOptionResult.Success(DiscriminatedUnionOption.Typed(name, GetFullType(type)));
        }

        if (genericType != null)
        {
            return GetOptionResult.Success(DiscriminatedUnionOption.Typed(name, genericType));
        }

        return GetOptionResult.Success(DiscriminatedUnionOption.Simple(attributeData.ConstructorArguments.First()!.Value!.ToString()));
    }

    private static string GetFullType(INamedTypeSymbol type)
    {
        if (type.IsTupleType)
        {
            var elements = type.TupleElements
                .Select(e => $"{GetFullType((e.Type as INamedTypeSymbol)!)} {e.Name}");
            return $"({string.Join(", ", elements)})";
        }

        return $"{type!.ContainingNamespace}.{type!.Name}";
    }
}
