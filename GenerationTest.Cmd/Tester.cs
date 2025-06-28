using DiscriminatedUnion;

namespace GenerationTest.Cmd;

[Option("Geoff")]
[Option("blobblobblob", OfType = typeof(string))]
[Option("Smith")]
[Option("Wobbly", OfGeneric = "Q")]
public partial class Tester<Q>
{
}
