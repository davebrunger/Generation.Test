namespace WS.DomainModelling.BasicWrapper;

[Generator]
public class BasicWrapperGenerator : IIncrementalGenerator
{
    private enum BasicWrapperError
    {
        MissingValidateMethod = 1,
        ValidateMethodNotPrivate,
        ValidateMethodNotStatic,
        ValidateMethodReturnTypeNotBoolean,
        ValidateMethodParameterTypeMismatch
    }

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
                var location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()!;

                var errors = new System.Collections.Generic.List<(BasicWrapperError Error, string Message, Location Location)>();
                if (validateMethod == null)
                {
                    errors.Add((
                        BasicWrapperError.MissingValidateMethod,
                        $"Class '{_class.Name}' must have a method named '{validateMemberName}' to validate the wrapper source.",
                        location));
                }
                else
                {
                    if (validateMethod.DeclaredAccessibility != Accessibility.Private)
                    {
                        errors.Add((
                            BasicWrapperError.ValidateMethodNotPrivate,
                            $"Method '{validateMemberName}' in class '{_class.Name}' must be private",
                            location));
                    }
                    if (!validateMethod.IsStatic)
                    {
                        errors.Add((
                            BasicWrapperError.ValidateMethodNotStatic,
                            $"Method '{validateMemberName}' in class '{_class.Name}' must be static",
                            location));
                    }
                    if (validateMethod.ReturnType.SpecialType != SpecialType.System_Boolean)
                    {
                        errors.Add((
                            BasicWrapperError.ValidateMethodReturnTypeNotBoolean,
                            $"Method '{validateMemberName}' in class '{_class.Name}' must return a boolean",
                            location));
                    }
                    if (validateMethod.Parameters.Length != 1
                        || !SymbolEqualityComparer.Default.Equals(validateMethod.Parameters[0].Type, wrappedType))
                    {
                        errors.Add((
                            BasicWrapperError.ValidateMethodParameterTypeMismatch,
                            $"Method '{validateMemberName}' in class '{_class.Name}' must have a single {wrappedType!.Name} parameter",
                            location));
                    }
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
                    ValidateMemberName = validateMemberName,
                    Errors = errors
                };
            });

        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            if (model.Errors.Count > 0)
            {
                foreach (var (optionError, message, location) in model.Errors)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        $"WSBW{(int)optionError:000}",
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

            var source = SourceText.From($$"""
            namespace {{model.Namespace}};

            sealed partial class {{model.ClassName}} : IEquatable<{{model.ClassName}}>
            {
                public {{model.WrappedType!.Name}} Value { get; }

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
                    return $"{{model.FileName}} ({Value})";
                }

                public bool Equals({{model.ClassName}}? other)
                {
                    if (other is null) return false;
                    return Value.Equals(other.Value);
                }

                public override bool Equals(object? obj)
                {
                    return obj is {{model.ClassName}} other && Equals(other);
                }

                public override int GetHashCode()
                {
                    return Value.GetHashCode();
                }

                public static implicit operator {{model.WrappedType!.Name}}({{model.ClassName}} input) {
                    return input.Value;
                }
            }
            """, Encoding.UTF8);

            context.AddSource($"{model.Namespace}.{model.FileName}.g.cs", source);
        });
    }
}
