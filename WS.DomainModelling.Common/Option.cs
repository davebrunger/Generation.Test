namespace WS.DomainModelling.Common;

public static class Option
{
    public sealed class NoneOption
    {
        internal NoneOption() { }
    }

    public static NoneOption None { get; } = new NoneOption();

    public static Option<T> Some<T>(T t) => Option<T>.Some(t);

    public static Option<T> Return<T>(this T t)
    {
        return Option<T>.Some(t);
    }

    // Functional style methods for Option

    public static Func<Option<T>, Option<U>> Bind<T, U>(Func<T, Option<U>> function)
    {
        return input => input.Match(t => function(t), () => None);
    }

    public static Func<Option<T>, Option<U>> Map<T, U>(Func<T, U> function)
    {
        return Bind(function.Compose(Option<U>.Some));
    }

    // Idomatic C# style methods for Option

    public static Option<U> Bind<T, U>(this Option<T> input, Func<T, Option<U>> function)
    {
        return Bind(function)(input);
    }

    public static Option<U> Map<T, U>(this Option<T> input, Func<T, U> function)
    {
        return Map(function)(input);
    }
}

public class Option<T>
{
    private enum OptionOption
    {
        Some,
        None
    }

    private readonly OptionOption option;

    private T Value { get; init; }

    public bool IsSome => option == OptionOption.Some;
    public bool IsNone => option == OptionOption.None;

    private Option(OptionOption option)
    {
        this.option = option;
        Value = default!;
    }

    public TResult Match<TResult>(Func<T, TResult> someFunc, Func<TResult> noneFunc)
    {
        return option switch
        {
            OptionOption.Some => someFunc(Value),
            OptionOption.None => noneFunc(),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

    public void Switch<TResult>(Action<T> someAction, Action noneAction)
    {
        switch (option)
        {
            case OptionOption.Some:
                someAction(Value);
                return;
            case OptionOption.None:
                noneAction();
                return;
            default:
                throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
        }
    }

    public static implicit operator Option<T>(Option.NoneOption _) => None;

    public static Option<T> Some(T some) => new(OptionOption.Some) { Value = some };
    public static Option<T> None { get; } = new(OptionOption.None);

    public override string ToString()
    {
        return Match(s => $"Some ({s})", () => "None");
    }
}