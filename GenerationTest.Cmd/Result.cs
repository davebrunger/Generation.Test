using DiscriminatedUnion;

namespace GenerationTest.Cmd;

[Option("Success", OfGeneric = "T")]
[Option("Failure")]
public partial class Result<T>
{
}

public static class IntUtilities
{
    public static Result<int> ToInt(this string source)
    {
        return int.TryParse(source, out int integer)
            ? Result<int>.Success(integer)
            : Result<int>.Failure;
    }
}
