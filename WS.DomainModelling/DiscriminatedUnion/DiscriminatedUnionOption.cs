using System;
using System.Collections.Generic;

namespace WS.DomainModelling.DiscriminatedUnion;

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

    public bool IsSimple => option == Option.Simple;
    public bool IsTyped => option == Option.Typed;

    private DiscriminatedUnionOption(Option option)
    {
        this.option = option;
        Simple_Value = default!;
        Typed_Value = default!;
    }

    public TResult Match<TResult>(Func<string, TResult> simpleFunc, Func<KeyValuePair<string, string>, TResult> typedFunc)
    {
        return option switch
        {
            Option.Simple => simpleFunc(Simple_Value),
            Option.Typed => typedFunc(Typed_Value),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

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

    public static DiscriminatedUnionOption Simple(string simple) => new(Option.Simple) {Simple_Value = simple };
    public static DiscriminatedUnionOption Typed(string name, string type) => new(Option.Typed) { Typed_Value = new KeyValuePair<string, string>(name, type) };
}
