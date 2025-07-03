using System.Diagnostics;
using GenerationTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Generation.Test.Test;

public class BasicWrapperGeneratorTest
{
    [Fact]
    public void Test1()
    {
        var source = """
            using System;

            namespace Tester

            [DiscriminatedUnion.BasicWrapper(typeof(string), "Validate")]
            public partial class TestType
            {
                private static bool Validate(string source)
                {
                    return true;
                }
            }
        """;

        var generator = new BasicWrapperGenerator();

        var compilation = CSharpCompilation.Create(nameof(BasicWrapperGeneratorTest))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var _);

        var result = driver.GetRunResult().Results.Single();

        Assert.Empty(result.Diagnostics);
    }
}
