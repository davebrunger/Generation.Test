namespace WS.DomainModelling.Common;

public abstract class Option<T>
{
    public bool IsSome => Match(_ => true, () => false);
    public bool IsNone => Match(_ => false, () => true);

    private Option()
    {
    }

    public abstract TResult Match<TResult>(Func<T, TResult> someFunc, Func<TResult> noneFunc);

    public abstract void Switch<TResult>(Action<T> someAction, Action noneAction);

    private sealed class SomeOption : Option<T>
    {
        private readonly T value;

        internal SomeOption(T value)
        {
            this.value = value;
        }

        public override TResult Match<TResult>(Func<T, TResult> someFunc, Func<TResult> noneFunc)
        {
            return someFunc(value);
        }

        public override void Switch<TResult>(Action<T> someAction, Action noneAction)
        {
            someAction(value);
        }
    }

    private sealed class NoneOption : Option<T>
    {
        internal NoneOption()
        {
        }

        public override TResult Match<TResult>(Func<T, TResult> someFunc, Func<TResult> noneFunc)
        {
            return noneFunc();
        }
        public override void Switch<TResult>(Action<T> someAction, Action noneAction)
        {
            noneAction();
        }
    }

    public static implicit operator Option<T>(Option.NoneOption _) => None;

    public static Option<T> Some(T some) => new SomeOption(some);

    public static Option<T> None { get; } = new NoneOption();

    public override string ToString()
    {
        return Match(s => $"Some ({s})", () => "None");
    }
}

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

