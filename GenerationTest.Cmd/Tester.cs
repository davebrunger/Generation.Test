using DiscriminatedUnion;

namespace GenerationTest.Cmd;

[Option("Geoff")]
[Option("blobblobblob", OfType = typeof(string))]
[Option("Smith")]
public partial class Tester
{
}
