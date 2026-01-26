namespace WS.DomainModelling.DiscriminatedUnion;

/// <summary>
/// A discriminated union type representing an option that can be either a simple string or a typed key-value pair.
/// </summary>
public class DiscriminatedUnionOption
{
    private enum Option
    {
        Simple,
        Typed
    }

    private readonly Option option;

    private string Simple_Value { get; init; }
    private KeyValuePair<string, string> Typed_Value { get; init; }

    /// <summary>
    /// Indicates whether the current option is a simple string.
    /// </summary>
    public bool IsSimple => option == Option.Simple;
    /// <summary>
    /// Indicates whether the current option is a typed key-value pair.
    /// </summary>
    public bool IsTyped => option == Option.Typed;

    private DiscriminatedUnionOption(Option option)
    {
        this.option = option;
        Simple_Value = default!;
        Typed_Value = default!;
    }

    /// <summary>
    /// Matches the current option and invokes the corresponding function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="simpleFunc"></param>
    /// <param name="typedFunc"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public TResult Match<TResult>(Func<string, TResult> simpleFunc, Func<KeyValuePair<string, string>, TResult> typedFunc)
    {
        return option switch
        {
            Option.Simple => simpleFunc(Simple_Value),
            Option.Typed => typedFunc(Typed_Value),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

    /// <summary>
    /// Switches on the current option and invokes the corresponding action.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="simpleAction"></param>
    /// <param name="typedAction"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void Switch<TResult>(Action<string> simpleAction, Action<KeyValuePair<string, string>> typedAction)
    {
        switch (option)
        {
            case Option.Simple:
                simpleAction(Simple_Value);
                return;
            case Option.Typed:
                typedAction(Typed_Value);
                return;
            default:
                throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
        }
    }

    /// <summary>
    /// Creates a DiscriminatedUnionOption of type Simple.
    /// </summary>
    /// <param name="simple"></param>
    /// <returns></returns>
    public static DiscriminatedUnionOption Simple(string simple) => new(Option.Simple) {Simple_Value = simple };
    
    /// <summary>
    /// Creates a DiscriminatedUnionOption of type Typed.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static DiscriminatedUnionOption Typed(string name, string type) => new(Option.Typed) { Typed_Value = new KeyValuePair<string, string>(name, type) };
}
