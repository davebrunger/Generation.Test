using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace WS.DomainModelling.Internal;

public class Result<T, TError>
{
    private enum ResultOption
    {
        Success,
        Failure
    }

    private readonly ResultOption option;

    private T successValue { get; init; }
    private ImmutableList<TError> failureValue { get; init; }

    public bool IsSuccess => option == ResultOption.Success;
    public bool IsFailure => option == ResultOption.Failure;

    private Result(ResultOption option)
    {
        this.option = option;
        successValue = default!;
        failureValue = default!;
    }

    public TResult Match<TResult>(Func<T, TResult> successFunc, Func<ImmutableList<TError>, TResult> failureFunc)
    {
        return option switch
        {
            ResultOption.Success => successFunc(successValue),
            ResultOption.Failure => failureFunc(failureValue),
            _ => throw new IndexOutOfRangeException($"{nameof(option)} is out of range")
        };
    }

    public void Switch(Action<T> successAction, Action<ImmutableList<TError>> failureAction)
    {
        switch (option)
        {
            case ResultOption.Success:
                successAction(successValue);
                return;
            case ResultOption.Failure:
                failureAction(failureValue);
                return;
            default:
                throw new IndexOutOfRangeException($"{nameof(option)} is out of range");
        }
    }

    public static Result<T, TError> Success(T successValue) => new(ResultOption.Success) { successValue = successValue };
    public static Result<T, TError> Failure(IEnumerable<TError> failureValue) => new(ResultOption.Failure) { failureValue = [.. failureValue] };
    public static Result<T, TError> Failure(params TError[] failureValue) => new(ResultOption.Failure) { failureValue = [.. failureValue] };

    public override string ToString()
    {
        return Match(s => $"Success ({s})", (f) => $"Failure:\n  {string.Join("\n  ", f)}");
    }
}