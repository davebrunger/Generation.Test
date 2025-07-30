using System.Collections.Generic;

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


    public static Result<T, TError> Success(T Success) => new SuccessResult(Success);

    public static Result<T, TError> Error(TError Error) => new ErrorResult(Error);

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


public static class Result
{
    // Functional style methods for Result

    public static Func<Result<T, TError>, Result<U, TError>> Bind<T, U, TError>(Func<T, Result<U, TError>> function)
    {
        return input => input.Match(t => function(t), e => Result<U, TError>.Error(e));
    }

    public static Func<Result<(T Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>, Result<(U Value, IReadOnlyList<TMessage>Messages), IReadOnlyList<TError>>> Bind<T, U, TMessage, TError>(
        Func<T, Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>> function)
    {
        return input => input.Match(
            t => function(t.Value).Match(
                u => (u.Value, t.Messages.Concat(u.Messages).ToImmutableList()),
                Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>.Error),
            Result<(U Value, IReadOnlyList<TMessage> Messages), IReadOnlyList<TError>>.Error);
    }

    public static Func<Result<T, TError>, Result<U, TError>> Map<T, U, TError>(Func<T, U> function)
    {
        return Bind(function.Compose(Result<U, TError>.Success));
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
}
