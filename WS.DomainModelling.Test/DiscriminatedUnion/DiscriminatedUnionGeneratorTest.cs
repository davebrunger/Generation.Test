using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WS.DomainModelling.DiscriminatedUnion;

namespace WS.DomainModelling.Test.DiscriminatedUnion;

public class DiscriminatedUnionGeneratorTest
{
    [Fact]
    public void TestInvalidOptionName()
    {
        var source = """
        namespace Tester;

        [WS.DomainModelling.DiscriminatedUnion.Option("Jammy Dodger")]
        public partial class Tester<Q>
        {
        }
        """;

        var generator = new DiscriminatedUnionGenerator();

        var compilation = CSharpCompilation.Create(nameof(DiscriminatedUnionGeneratorTest))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var _);

        var result = driver.GetRunResult().Results.Single();

        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void TestInvalidGenricOption()
    {
        var source = """
        namespace Tester;

        [WS.DomainModelling.DiscriminatedUnion.Option("JammyDodger", OfGeneric="R")]
        public partial class Tester<Q>
        {
        }
        """;

        var generator = new DiscriminatedUnionGenerator();

        var compilation = CSharpCompilation.Create(nameof(DiscriminatedUnionGeneratorTest))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var _);

        var result = driver.GetRunResult().Results.Single();

        Assert.Single(result.Diagnostics);
        Assert.Equal("The generic type R must be one of Q", result.Diagnostics[0].GetMessage());
    }
}
