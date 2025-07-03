using DiscriminatedUnion;

namespace GenerationTest.Cmd;

[Option("Geoff")]
[Option("blobblobblob", OfType = typeof(string))]
[Option("Smith")]
[Option("Wobbly", OfGeneric = "Q")]
public partial class Tester<Q>
{
}

[BasicWrapper(typeof(string), nameof(Validate))]
public partial class String50
{
    private static bool Validate(string source)
    {
        return source != null && source.Length <= 50;
    }
}