namespace WS.DomainModelling.Common.Test.TaskResultTests;

public class TaskResultCreationTests
{
    [Fact]
    public async Task ImplicitConversionFromSuccessResult_WrapsCorrectly()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(42);

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ImplicitConversionFromErrorResult_WrapsCorrectly()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("oops");

        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task ImplicitConversionFromTask_WrapsCorrectly()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Success(99));

        TaskResult<int, string> taskResult = task;

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
    }
}

public class TaskResultMatchTests
{
    [Fact]
    public async Task Match_SyncDelegates_Success_CallsSuccessFunc()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(10);

        var output = await taskResult.Match(
            value => $"got {value}",
            error => $"error: {error}");

        Assert.Equal("got 10", output);
    }

    [Fact]
    public async Task Match_SyncDelegates_Error_CallsErrorFunc()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("bad");

        var output = await taskResult.Match(
            value => $"got {value}",
            error => $"error: {error}");

        Assert.Equal("error: bad", output);
    }

    [Fact]
    public async Task Match_AsyncDelegates_Success_CallsSuccessFunc()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(5);

        var output = await taskResult.Match(
            async value => { await Task.Yield(); return value * 2; },
            async error => { await Task.Yield(); return -1; });

        Assert.Equal(10, output);
    }

    [Fact]
    public async Task Match_AsyncDelegates_Error_CallsErrorFunc()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("fail");

        var output = await taskResult.Match(
            async value => { await Task.Yield(); return value * 2; },
            async error => { await Task.Yield(); return -1; });

        Assert.Equal(-1, output);
    }
}

public class TaskResultSwitchTests
{
    [Fact]
    public async Task Switch_SyncDelegates_Success_CallsSuccessAction()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(7);
        int? captured = null;

        await taskResult.Switch(
            value => captured = value,
            error => captured = -1);

        Assert.Equal(7, captured);
    }

    [Fact]
    public async Task Switch_SyncDelegates_Error_CallsErrorAction()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("err");
        string? captured = null;

        await taskResult.Switch(
            value => captured = "success",
            error => captured = error);

        Assert.Equal("err", captured);
    }

    [Fact]
    public async Task Switch_AsyncDelegates_Success_CallsSuccessAction()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(3);
        int? captured = null;

        await taskResult.Switch(
            async value => { await Task.Yield(); captured = value; },
            async error => { await Task.Yield(); captured = -1; });

        Assert.Equal(3, captured);
    }

    [Fact]
    public async Task Switch_AsyncDelegates_Error_CallsErrorAction()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("async-err");
        string? captured = null;

        await taskResult.Switch(
            async value => { await Task.Yield(); captured = "success"; },
            async error => { await Task.Yield(); captured = error; });

        Assert.Equal("async-err", captured);
    }
}

public class TaskResultBindTests
{
    [Fact]
    public async Task Bind_AsyncDelegate_Success_ChainsOperation()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(4);

        var chained = taskResult.Bind(async value =>
        {
            await Task.Yield();
            return Result<string, string>.Success($"value:{value}");
        });

        var result = await chained.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal("value:4", result.Match(v => v, _ => ""));
    }

    [Fact]
    public async Task Bind_AsyncDelegate_Error_PassesThrough()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("upstream");
        var bindCalled = false;

        var chained = taskResult.Bind(async value =>
        {
            bindCalled = true;
            await Task.Yield();
            return Result<string, string>.Success($"value:{value}");
        });

        var result = await chained.AsTask();

        Assert.False(bindCalled);
        Assert.True(result.IsError);
        Assert.Equal("upstream", result.Match(_ => "", e => e));
    }

    [Fact]
    public async Task Bind_AsyncDelegate_InnerError_PropagatesError()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(1);

        var chained = taskResult.Bind(async value =>
        {
            await Task.Yield();
            return Result<string, string>.Error("inner error");
        });

        var result = await chained.AsTask();

        Assert.True(result.IsError);
        Assert.Equal("inner error", result.Match(_ => "", e => e));
    }

    [Fact]
    public async Task Bind_SyncDelegate_Success_ChainsOperation()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(6);

        var chained = taskResult.Bind(value => Result<int, string>.Success(value + 1));

        var result = await chained.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task Bind_SyncDelegate_Error_PassesThrough()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("err");
        var bindCalled = false;

        var chained = taskResult.Bind(value =>
        {
            bindCalled = true;
            return Result<int, string>.Success(value + 1);
        });

        var result = await chained.AsTask();

        Assert.False(bindCalled);
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task MultiStep_Chain_ShortCircuitsOnFirstError()
    {
        var step2Called = false;
        var step3Called = false;

        TaskResult<int, string> taskResult = Result<int, string>.Success(1);

        var chained = taskResult
            .Bind(async _ => { await Task.Yield(); return Result<int, string>.Error("step1 failed"); })
            .Bind(async v => { step2Called = true; await Task.Yield(); return Result<int, string>.Success(v + 1); })
            .Bind(v => { step3Called = true; return Result<string, string>.Success($"{v}"); });

        var result = await chained.AsTask();

        Assert.True(result.IsError);
        Assert.False(step2Called);
        Assert.False(step3Called);
        Assert.Equal("step1 failed", result.Match(_ => "", e => e));
    }

    [Fact]
    public async Task MultiStep_Chain_AllSucceed_ProducesCorrectValue()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(1);

        var result = await taskResult
            .Bind(async v => { await Task.Yield(); return Result<int, string>.Success(v + 1); })
            .Map(async v => { await Task.Yield(); return v * 10; })
            .Map(v => v + 5)
            .AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Match(v => v, _ => 0));
    }
}

public class TaskResultMapTests
{
    [Fact]
    public async Task Map_AsyncDelegate_Success_TransformsValue()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(3);

        var mapped = taskResult.Map(async value =>
        {
            await Task.Yield();
            return value * 10;
        });

        var result = await mapped.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(30, result.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task Map_AsyncDelegate_Error_PassesThrough()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("map-err");
        var mapCalled = false;

        var mapped = taskResult.Map(async value =>
        {
            mapCalled = true;
            await Task.Yield();
            return value * 10;
        });

        var result = await mapped.AsTask();

        Assert.False(mapCalled);
        Assert.True(result.IsError);
        Assert.Equal("map-err", result.Match(_ => "", e => e));
    }

    [Fact]
    public async Task Map_SyncDelegate_Success_TransformsValue()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Success(8);

        var mapped = taskResult.Map(value => value.ToString());

        var result = await mapped.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal("8", result.Match(v => v, _ => ""));
    }

    [Fact]
    public async Task Map_SyncDelegate_Error_PassesThrough()
    {
        TaskResult<int, string> taskResult = Result<int, string>.Error("orig-err");

        var mapped = taskResult.Map(value => value.ToString());

        var result = await mapped.AsTask();

        Assert.True(result.IsError);
        Assert.Equal("orig-err", result.Match(_ => "", e => e));
    }
}

public class ResultAsyncExtensionTests
{
    [Fact]
    public async Task Result_Bind_AsyncDelegate_Success_ProducesTaskResult()
    {
        var result = Result<int, string>.Success(10);

        var taskResult = result.Bind(async value =>
        {
            await Task.Yield();
            return Result<string, string>.Success($"n={value}");
        });

        var final = await taskResult.AsTask();

        Assert.True(final.IsSuccess);
        Assert.Equal("n=10", final.Match(v => v, _ => ""));
    }

    [Fact]
    public async Task Result_Bind_AsyncDelegate_Error_PassesThrough()
    {
        var result = Result<int, string>.Error("source error");
        var bindCalled = false;

        var taskResult = result.Bind(async value =>
        {
            bindCalled = true;
            await Task.Yield();
            return Result<string, string>.Success($"n={value}");
        });

        var final = await taskResult.AsTask();

        Assert.False(bindCalled);
        Assert.True(final.IsError);
    }

    [Fact]
    public async Task Result_Map_AsyncDelegate_Success_TransformsValue()
    {
        var result = Result<int, string>.Success(5);

        var taskResult = result.Map(async value =>
        {
            await Task.Yield();
            return value * 3;
        });

        var final = await taskResult.AsTask();

        Assert.True(final.IsSuccess);
        Assert.Equal(15, final.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task Result_Map_AsyncDelegate_Error_PassesThrough()
    {
        var result = Result<int, string>.Error("map-source-error");

        var taskResult = result.Map(async value =>
        {
            await Task.Yield();
            return value * 3;
        });

        var final = await taskResult.AsTask();

        Assert.True(final.IsError);
        Assert.Equal("map-source-error", final.Match(_ => "", e => e));
    }
}

public class TaskOfResultExtensionTests
{
    [Fact]
    public async Task TaskResult_Bind_SyncDelegate_Success_Chains()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Success(2));

        var taskResult = task.Bind(value => Result<int, string>.Success(value + 8));

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task TaskResult_Bind_SyncDelegate_Error_PassesThrough()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Error("t-err"));

        var taskResult = task.Bind(value => Result<int, string>.Success(value + 8));

        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task TaskResult_Bind_AsyncDelegate_Success_Chains()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Success(5));

        var taskResult = task.Bind(async value =>
        {
            await Task.Yield();
            return Result<string, string>.Success($"x{value}");
        });

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal("x5", result.Match(v => v, _ => ""));
    }

    [Fact]
    public async Task TaskResult_Bind_AsyncDelegate_Error_PassesThrough()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Error("async-t-err"));

        var taskResult = task.Bind(async value =>
        {
            await Task.Yield();
            return Result<string, string>.Success($"x{value}");
        });

        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task TaskResult_Map_SyncDelegate_Success_Transforms()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Success(4));

        var taskResult = task.Map(value => value * value);

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(16, result.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task TaskResult_Map_SyncDelegate_Error_PassesThrough()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Error("t-map-err"));

        var taskResult = task.Map(value => value * value);

        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task TaskResult_Map_AsyncDelegate_Success_Transforms()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Success(6));

        var taskResult = task.Map(async value =>
        {
            await Task.Yield();
            return value + 4;
        });

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task TaskResult_Map_AsyncDelegate_Error_PassesThrough()
    {
        Task<Result<int, string>> task = Task.FromResult(Result<int, string>.Error("t-async-map-err"));

        var taskResult = task.Map(async value =>
        {
            await Task.Yield();
            return value + 4;
        });

        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
    }
}

public class TryCatchAsyncTests
{
    [Fact]
    public async Task TryCatchAsync_Static_NoException_ReturnsSuccess()
    {
        var func = Result.TryCatchAsync<int, string, string>(
            async input => { await Task.Yield(); return $"result:{input}"; },
            ex => ex.Message);

        var taskResult = func(42);
        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal("result:42", result.Match(v => v, _ => ""));
    }

    [Fact]
    public async Task TryCatchAsync_Static_ExceptionThrown_ReturnsError()
    {
        var func = Result.TryCatchAsync<int, string, string>(
            async input => { await Task.Yield(); throw new InvalidOperationException("boom"); },
            ex => ex.Message);

        var taskResult = func(1);
        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
        Assert.Equal("boom", result.Match(_ => "", e => e));
    }

    [Fact]
    public async Task TryCatchAsync_Extension_NoException_ReturnsSuccess()
    {
        var taskResult = 7.TryCatchAsync(
            async input => { await Task.Yield(); return input * 2; },
            ex => ex.Message);

        var result = await taskResult.AsTask();

        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.Match(v => v, _ => 0));
    }

    [Fact]
    public async Task TryCatchAsync_Extension_ExceptionThrown_ReturnsError()
    {
        var taskResult = 7.TryCatchAsync<int, int, string>(
            async _ => { await Task.Yield(); throw new ArgumentException("arg bad"); },
            ex => ex.Message);

        var result = await taskResult.AsTask();

        Assert.True(result.IsError);
        Assert.Equal("arg bad", result.Match(_ => "", e => e));
    }

    [Fact]
    public async Task TryCatchAsync_ErrorConverter_ReceivesCorrectException()
    {
        Exception? captured = null;

        var taskResult = 0.TryCatchAsync<int, int, string>(
            async _ => { await Task.Yield(); throw new DivideByZeroException(); },
            ex => { captured = ex; return "divided by zero"; });

        await taskResult.AsTask();

        Assert.IsType<DivideByZeroException>(captured);
    }
}
