using WS.DomainModelling.DiscriminatedUnion;
using WS.DomainModelling.BasicWrapper;
using WS.DomainModelling.Common;

namespace GenerationTest.Cmd;

[Option("Geoff")]
[Option("blobblobblob", OfType = typeof(string))]
[Option("Smith")]
[Option("Wobbly", OfGeneric = "Q")]
[Option("JammyWhammy", OfType = typeof((int AnInt, string)))]
public partial class Tester<Q>
{
    Option<Score> score { get; } = NaturalInteger.Create(5).Bind(home => NaturalInteger.Create(2).Map(away => new Score(home, away)));
}

[BasicWrapper(typeof(string), nameof(Validate))]
public partial class String50
{
    private static bool Validate(string source)
    {
        return source != null && source.Length <= 50;
    }
}

[BasicWrapper(typeof(int), nameof(Validate))]
public partial class NaturalInteger
{
    private static bool Validate(int value) => value >= 0;
}

public record Score(NaturalInteger Home, NaturalInteger Away);

[Option("Success", OfGeneric = "Q")]
[Option("Failed")]
[Option("FileNotFoundError", OfType = typeof(string))]
[Option("IndexOutOfBoundsError", OfType = typeof((int AnInt, string)))]
public partial class DetailedResult<Q>
{
}

DetailedResult<List<string>> DoSomething()
{
    var index = 9;
    if (DateTime.Now > DateTime.Now)
    {
        return DetailedResult<List<string>>.IndexOutOfBoundsError((index, "Message"));
    }
    if (DateTime.Now > DateTime.Now)
    {
        return DetailedResult<List<string>>.FileNotFoundError("Message");
    }
    var messages = new[] { "Hello" }.ToList();
    if (messages.Count > 0)
    {
        return DetailedResult<List<string>>.Failed;
    }
    return DetailedResult<List<string>>.Success(messages);
}
