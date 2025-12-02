namespace WS.DomainModelling.Common;

public abstract class Result<T, TError>
{
    public bool IsSuccess => Match(_ => true, _ => false);
    public bool IsError => Match(_ => false, _ => false);

    private Result() { }

    public abstract TResult Match<TResult>(Func<T, TResult> successFunc, Func<TError, TResult> errorFunc);

    public abstract void Switch(Action<T> successAction, Action<TError> errorAction);

    private sealed class SuccessResult : Result<T, TError>
    {
        private readonly T value;

        internal SuccessResult(T value)
        {
            this.value = value;
        }

        public override TResult Match<TResult>(Func<T, TResult> successFunc, Func<TError, TResult> errorFunc)
        {
            return successFunc(value);
        }

        public override void Switch(Action<T> successAction, Action<TError> errorAction)
        {
            successAction(value);
        }
    }

    private sealed class ErrorResult : Result<T, TError>
    {
        private readonly TError error;

        internal ErrorResult(TError error)
        {
            this.error = error;
        }

        public override TResult Match<TResult>(Func<T, TResult> successFunc, Func<TError, TResult> errorFunc)
        {
            return errorFunc(error);
        }

        public override void Switch(Action<T> successAction, Action<TError> errorAction)
        {
            errorAction(error);
        }
    }


    public static Result<T, TError> Success(T success) => new SuccessResult(success);

    public static Result<T, TError> Error(TError error) => new ErrorResult(error);

    public Option<T> AsSuccess() => Match(Option<T>.Some, (_) => Option<T>.None);

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


public static class Result
{
    // Functional style methods for Result

    public static Func<Result<T, TError>, Result<U, TError>> Bind<T, U, TError>(Func<T, Result<U, TError>> function)
    {
        return input => input.Match(
            t => function(t), 
            e => Result<U, TError>.Error(e)
        );
    }

    public static Func<Result<(T Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>, Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> Bind<T, U, TMessage, TError>(
        Func<T, Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function)
    {
        return input => input.Match(
            t => function(t.Value).Match(
                u => (u.Value, [.. t.Messages, .. u.Messages]),
                Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>.Error),
            Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>.Error);
    }

    public static Func<Result<T, TError>, Result<U, TError>> Map<T, U, TError>(Func<T, U> function)
    {
        return Bind(function.Compose(Result<U, TError>.Success));
    }

    public static Func<T, Result<U, TError>> TryCatch<T, U, TError>(Func<T, U> function, Func<Exception, TError> error)
    {
        return input =>
        {
            try
            {
                return function(input);
            }
            catch (Exception ex)
            {
                return error(ex);
            }
        };
    }

    public static Func<Result<T, TError>, Result<U, TError>> DoubleMap<T, U, TError>(Func<T, U> success, Func<TError, TError> error)
    {
        return input => input.Match<Result<U, TError>>(
            t => success(t),
            e => error(e)
        );
    }

    public static Func<T, Result<W, TError>> Plus<T, U, V, W, TError>(
        Func<U, V, W> combineSuccess,
        Func<TError, TError, TError> combineError,
        Func<T, Result<U, TError>> function1,
        Func<T, Result<V, TError>> function2)
    {
        return input => function1(input).Match(
            success1 => function2(input).Match<Result<W, TError>>(
                success2 => combineSuccess(success1, success2),
                error2 => error2
            ),
            error1 => function2(input).Match<Result<W, TError>>(
                _ => error1,
                error2 => combineError(error1, error2)
            )
        );
    }

    public static Func<T, Result<(W Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> Plus<T, U, V, W, TMessage, TError>(
        Func<U, V, W> combineSuccess,
        Func<T, Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function1,
        Func<T, Result<(V Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function2)
    {
        return Plus<T, (U Value, IReadOnlyList<TMessage> Messages), (V Value, IReadOnlyList<TMessage> Messages), (W Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>(
            (success1, success2) => (combineSuccess(success1.Value, success2.Value), [.. success1.Messages, .. success2.Messages]),
            (error1, error2) => [.. error1, .. error2],
            function1,
            function2
        );
    }

    // Idomatic C# style methods for Result

    public static Result<U, TError> Bind<T, U, TError>(this Result<T, TError> input, Func<T, Result<U, TError>> function)
    {
        return Bind(function)(input);
    }
    public static Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>> Bind<T, U, TMessage, TError>(
        this Result<(T Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>> input, Func<T, Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function)
    {
        return Bind(function)(input);
    }

    public static Result<U, TError> Map<T, U, TError>(this Result<T, TError> input, Func<T, U> function)
    {
        return Map<T, U, TError>(function)(input);
    }

    public static Result<U, TError> TryCatch<T, U, TError>(this T input, Func<T, U> function, Func<Exception, TError> convert)
    {
        return TryCatch(function, convert)(input);
    }

    public static Result<U, TError> DoubleMap<T, U, TError>(this Result<T, TError> input, Func<T, U> success, Func<TError, TError> error)
    {
        return DoubleMap(success, error)(input);
    }

    public static Result<W, TError> Plus<T, U, V, W, TError>(
        this T input,
        Func<U, V, W> combineSuccess,
        Func<TError, TError, TError> combineError,
        Func<T, Result<U, TError>> function1,
        Func<T, Result<V, TError>> function2)
    {
        return Plus(combineSuccess, combineError, function1, function2)(input);
    }

    public static Result<(W Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>> Plus<T, U, V, W, TMessage, TError>(
        this T input,
        Func<U, V, W> combineSuccess,
        Func<T, Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function1,
        Func<T, Result<(V Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function2)
    {
        return Plus(combineSuccess, function1, function2)(input);
    }
}
