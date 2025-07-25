namespace WS.DomainModelling.Common;

public class Result<T, TError>
{
    private enum ResultOption
    {
        Success,
        Error,
    }

    private readonly ResultOption option;

    private T Success_Value { get; init; }
    private TError Error_Value { get; init; }

    public bool IsSuccess => option == ResultOption.Success;
    public bool IsError => option == ResultOption.Error;

    private Result(ResultOption option)
    {
        this.option = option;
        Success_Value = default!;
        Error_Value = default!;
    }

    public TResult Match<TResult>(Func<T, TResult> SuccessFunc, Func<TError, TResult> ErrorFunc)
    {
        return option switch
        {
            ResultOption.Success => SuccessFunc(Success_Value),
            ResultOption.Error => ErrorFunc(Error_Value),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

    public void Switch(Action<T> SuccessAction, Action<TError> ErrorAction)
    {
        switch (option)
        {
            case ResultOption.Success: SuccessAction(Success_Value); return;
            case ResultOption.Error: ErrorAction(Error_Value); return;
            default: throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
        };
    }

    public static Result<T, TError> Success(T Success) => new(ResultOption.Success) { Success_Value = Success };
    public static Result<T, TError> Error(TError Error) => new(ResultOption.Error) { Error_Value = Error };

    public Option<T> AsSuccess() => Match((Success) => Option<T>.Some(Success), (_) => Option<T>.None);
    public Option<TError> AsError() => Match((_) => Option<TError>.None, (Error) => Option<TError>.Some(Error));

    public override string ToString()
    {
        return Match(
            (Success) => $"Success ({Success})",
            (Error) => $"Error ({Error})"
        );
    }

    public static implicit operator Result<T, TError>(T value) => Success(value);

    public static implicit operator Result<T, TError>(TError value) => Error(value);
}

public class Result<T, TMessage, TError>
{
    private enum ResultOption
    {
        Success,
        Error,
    }

    private readonly ResultOption option;

    private (T Value, IReadOnlyList<TMessage> Messages) successValue { get; init; }
    private TError errorValue { get; init; }

    public bool IsSuccess => option == ResultOption.Success;
    public bool IsError => option == ResultOption.Error;

    private Result(ResultOption option)
    {
        this.option = option;
        successValue = default!;
        errorValue = default!;
    }

    public TResult Match<TResult>(Func<(T Value, IReadOnlyList<TMessage> Messages), TResult> successFunc, Func<TError, TResult> errorFunc)
    {
        return option switch
        {
            ResultOption.Success => successFunc(successValue),
            ResultOption.Error => errorFunc(errorValue),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

    public void Switch(Action<(T Value, IReadOnlyList<TMessage> Messages)> successAction, Action<TError> errorAction)
    {
        switch (option)
        {
            case ResultOption.Success: successAction(successValue); return;
            case ResultOption.Error: errorAction(errorValue); return;
            default: throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
        }
        ;
    }

    public static Result<T, TMessage, TError> Success(T value) => new(ResultOption.Success) { successValue = (value, ImmutableList<TMessage>.Empty) };
    public static Result<T, TMessage, TError> Success(T value, IReadOnlyList<TMessage> messages) => new(ResultOption.Success) { successValue = (value, messages) };
    public static Result<T, TMessage, TError> Success((T Value, IReadOnlyList<TMessage> Messages) value) => new(ResultOption.Success) { successValue = value };
    public static Result<T, TMessage, TError> Error(TError errors) => new(ResultOption.Error) { errorValue = errors };

    public Option<(T Value, IReadOnlyList<TMessage> Messages)> AsSuccess() => Match(Option<(T Value, IReadOnlyList<TMessage> Messages)>.Some, (_) => Option.None);
    public Option<TError> AsError() => Match((_) => Option.None, Option<TError>.Some);

    public override string ToString()
    {
        return Match(
            success => $"Success ({success})",
            errors => $"Error ({errors})"
        );
    }

    public static implicit operator Result<T, TMessage, TError>(T value) => Success(value);
    public static implicit operator Result<T, TMessage, TError>((T Value, IReadOnlyList<TMessage> Messages) value) => Success(value);
    public static implicit operator Result<T, TMessage, TError>(TError value) => Error(value);
}

public static class Result
{
    // Functional style methods for Result

    public static Func<Result<T, TError>, Result<U, TError>> Bind<T, U, TError>(Func<T, Result<U, TError>> function)
    {
        return input => input.Match(t => function(t), e => Result<U, TError>.Error(e));
    }

    public static Func<Result<T, TError>, Result<U, TError>> Map<T, U, TError>(Func<T, U> function)
    {
        return Bind(function.Compose(Result<U, TError>.Success));
    }

    public static Func<Result<T, TMessage, TError>, Result<U, TMessage, TError>> Bind<T, U, TMessage, TError>(Func<T, Result<U, TMessage, TError>> function)
    {
        return input => input.Match(
            t => function(t.Value).Match(
                u => (u.Value, t.Messages.Concat(u.Messages).ToImmutableList()),
                Result<U, TMessage, TError>.Error),
            Result<U, TMessage, TError>.Error);
    }
    
    public static Func<Result<T, TMessage, TError>, Result<U, TMessage, TError>> Map<T, U, TMessage, TError>(Func<T, U> function)
    {
        return Bind(function.Compose(Result<U, TMessage, TError>.Success));
    }

    // Idomatic C# style methods for Result

    public static Result<U, TError> Bind<T, U, TError>(this Result<T, TError> input, Func<T, Result<U, TError>> function)
    {
        return Bind(function)(input);
    }

    public static Result<U, TError> Map<T, U, TError>(this Result<T, TError> input, Func<T, U> function)
    {
        return Map<T, U, TError>(function)(input);
    }

    public static Result<U, TMessage, TError> Bind<T, U, TMessage, TError>(this Result<T, TMessage, TError> input, Func<T, Result<U, TMessage, TError>> function)
    {
        return Bind(function)(input);
    }

    public static Result<U, TMessage, TError> Map<T, U, TMessage, TError>(this Result<T, TMessage, TError> input, Func<T, U> function)
    {
        return Map<T, U, TMessage, TError>(function)(input);
    }
}
