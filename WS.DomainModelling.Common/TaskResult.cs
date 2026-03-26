namespace WS.DomainModelling.Common;

/// <summary>
/// Wraps a <see cref="Task{TResult}"/> of <see cref="Result{T, TError}"/> and provides
/// async-friendly Bind, Map and Switch operations that mirror those on <see cref="Result{T, TError}"/>.
/// </summary>
public class TaskResult<T, TError>
{
    private readonly Task<Result<T, TError>> task;

    internal TaskResult(Task<Result<T, TError>> task)
    {
        this.task = task;
    }

    /// <summary>Returns the underlying task.</summary>
    public Task<Result<T, TError>> AsTask() => task;

    /// <summary>Awaits the result and projects it using the provided functions.</summary>
    public async Task<TResult> Match<TResult>(Func<T, Task<TResult>> successFunc, Func<TError, Task<TResult>> errorFunc)
    {
        var result = await task.ConfigureAwait(false);
        return await result.Match(successFunc, errorFunc).ConfigureAwait(false);
    }

    /// <summary>Awaits the result and projects it using the provided functions.</summary>
    public async Task<TResult> Match<TResult>(Func<T, TResult> successFunc, Func<TError, TResult> errorFunc)
    {
        var result = await task.ConfigureAwait(false);
        return result.Match(successFunc, errorFunc);
    }

    /// <summary>Awaits the result and performs a side-effecting action.</summary>
    public async Task Switch(Func<T, Task> successAction, Func<TError, Task> errorAction)
    {
        var result = await task.ConfigureAwait(false);
        await result.Match(
            async t => { await successAction(t).ConfigureAwait(false); return true; },
            async e => { await errorAction(e).ConfigureAwait(false); return false; }
        ).ConfigureAwait(false);
    }

    /// <summary>Awaits the result and performs a side-effecting action.</summary>
    public async Task Switch(Action<T> successAction, Action<TError> errorAction)
    {
        var result = await task.ConfigureAwait(false);
        result.Switch(successAction, errorAction);
    }

    /// <summary>Chains an async operation on success; errors pass through.</summary>
    public TaskResult<U, TError> Bind<U>(Func<T, Task<Result<U, TError>>> function)
    {
        return new TaskResult<U, TError>(BindAsync(function));
    }

    /// <summary>Chains a synchronous operation on success; errors pass through.</summary>
    public TaskResult<U, TError> Bind<U>(Func<T, Result<U, TError>> function)
    {
        return Bind(t => Task.FromResult(function(t)));
    }

    /// <summary>Transforms the success value with an async function; errors pass through.</summary>
    public TaskResult<U, TError> Map<U>(Func<T, Task<U>> function)
    {
        return Bind(async t => Result<U, TError>.Success(await function(t).ConfigureAwait(false)));
    }

    /// <summary>Transforms the success value with a synchronous function; errors pass through.</summary>
    public TaskResult<U, TError> Map<U>(Func<T, U> function)
    {
        return Bind(t => Result<U, TError>.Success(function(t)));
    }

    private async Task<Result<U, TError>> BindAsync<U>(Func<T, Task<Result<U, TError>>> function)
    {
        var result = await task.ConfigureAwait(false);
        return await result.Match(
            function,
            e => Task.FromResult(Result<U, TError>.Error(e))
        ).ConfigureAwait(false);
    }

    public static implicit operator TaskResult<T, TError>(Task<Result<T, TError>> task) => new(task);
    public static implicit operator TaskResult<T, TError>(Result<T, TError> result) => new(Task.FromResult(result));
}
