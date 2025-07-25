namespace WS.DomainModelling.Common.ExtensionMethods;

public static class ListExtensionMethods
{
    public static Option<(T, IReadOnlyList<T>)> TryTakeOne<T>(this IReadOnlyList<T> source)
    {
        return source.TryTake(1).Bind(r => (r.Item1.Single(), r.Item2).Return());
    }

    public static Option<(IReadOnlyList<T>, IReadOnlyList<T>)> TryTake<T>(this IReadOnlyList<T> source, int numberToTake)
    {
        if (source.Count < numberToTake)
        {
            return Option.None;
        }
        return Option<(IReadOnlyList<T>, IReadOnlyList<T>)>.Some(([.. source.Take(numberToTake)], [.. source.Skip(numberToTake)]));
    }
}
