using DiscriminatedUnion;

namespace GenerationTest.Cmd;

[Option("Some", OfGeneric = "T")]
[Option("None")]
public partial class Result<T>
{
}

public static class IntUtilities
{
    public static Result<int> ToInt(this string source)
    {
        return int.TryParse(source, out int integer)
            ? Result<int>.Some(integer)
            : Result<int>.None;
    }
}
