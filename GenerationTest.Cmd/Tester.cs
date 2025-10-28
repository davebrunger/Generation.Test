using WS.DomainModelling.DiscriminatedUnion;
using WS.DomainModelling.BasicWrapper;

namespace GenerationTest.Cmd;

[Option("Geoff")]
[Option("blobblobblob", OfType = typeof(string))]
[Option("Smith")]
[Option("Wobbly", OfGeneric = "Q")]
[Option("JammyWhammy", OfType = typeof((int AnInt, string)))]
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