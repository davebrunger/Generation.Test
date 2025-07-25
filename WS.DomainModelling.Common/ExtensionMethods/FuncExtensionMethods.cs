namespace WS.DomainModelling.Common.ExtensionMethods;

public static class Func
{
    public static Func<T, V> Compose<T, U, V>(this Func<T, U> func1, Func<U, V> func2)
    {
        return t => func2(func1(t));
    }
}
