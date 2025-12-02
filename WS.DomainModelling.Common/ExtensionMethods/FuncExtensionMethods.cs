namespace WS.DomainModelling.Common.ExtensionMethods;

public static class Func
{
    // Functional style methods for Func

    public static Func<T, V> Compose<T, U, V>(this Func<T, U> func1, Func<U, V> func2)
    {
        return t => func2(func1(t));
    }

    public static Func<T, T> Tee<T>(this Action<T> action)
    {
        return t =>
        {
            action(t);
            return t;
        };
    }

    // Idomatic C# style methods for Func

    public static T Tee<T>(this T input, Action<T> action)
    {
        return Tee(action)(input);
    }
}
