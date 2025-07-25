namespace WS.DomainModelling.Common.ExtensionMethods;

public static class Object
{
    // Functional style methods for Tee
    public static Func<T, T> Tee<T>(Action<T> action)
    {
        return t =>
        {
            action(t);
            return t;
        };
    }

    // Idomatic C# style methods for Tee
    public static T Tee<T>(this T t, Action<T> action)
    {
        return Tee(action)(t);
    }
}
