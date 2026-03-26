# WS.DomainModelling.Common

Common types used by the [WS.DomainModelling](https://github.com/davebrunger/Generation.Test) source generator package. These types are also available for direct use in your own projects.

## Option

`Option<T>` represents a value that may or may not be present, similar to F#'s `Option` type.

### Creating Options
```csharp
var some = Option<int>.Some(42);
var none = Option<int>.None;

// Using the static helper
var some = Option.Some(42);
var none = Option.None;

// Using the extension method
var some = 42.Return();
```

### Accessing Values
Use `Match` to extract the value by providing handlers for both cases:
```csharp
string message = option.Match(
    value => $"Got {value}",
    () => "No value"
);
```

Use `Switch` for side effects:
```csharp
option.Switch(
    value => Console.WriteLine($"Got {value}"),
    () => Console.WriteLine("No value")
);
```

### Composing Options
Use `Bind` and `Map` to chain operations:
```csharp
Option<int> result = Option.Some(5)
    .Bind(x => x > 0 ? Option.Some(x) : Option.None)
    .Map(x => x * 2);
```

## Result

`Result<T, TError>` represents the outcome of an operation that can either succeed or fail.

### Creating Results
```csharp
var success = Result<int, string>.Success(42);
var error = Result<int, string>.Error("Something went wrong");

// Implicit conversions are supported
Result<int, string> result = 42;            // Success
Result<int, string> result = "Error";       // Error
```

### Accessing Values
```csharp
string message = result.Match(
    value => $"Got {value}",
    error => $"Failed: {error}"
);
```

### Composing Results
Use `Bind` and `Map` to chain operations, errors short-circuit automatically:
```csharp
Result<int, string> result = Result<int, string>.Success(5)
    .Bind(x => x > 0
        ? Result<int, string>.Success(x)
        : Result<int, string>.Error("Must be positive"))
    .Map(x => x * 2);
```

### Railway-Oriented Programming
`Result` supports railway-oriented programming patterns with `Plus` for combining parallel validations and `TryCatch` for wrapping exception-throwing code:
```csharp
var validate = Result.Plus<Input, Name, Age, Person, IReadOnlyList<string>>(
    (name, age) => new Person(name, age),
    (errors1, errors2) => [.. errors1, .. errors2],
    input => ValidateName(input),
    input => ValidateAge(input)
);
```

## TaskResult

`TaskResult<T, TError>` wraps a `Task<Result<T, TError>>` and provides async-friendly versions of
`Bind`, `Map`, `Match` and `Switch`. It allows async and synchronous operations to be chained
fluently without awaiting at each step.

### Creating a TaskResult
```csharp
// From an existing Result (implicit conversion)
TaskResult<int, string> tr = Result<int, string>.Success(42);

// From a Task<Result<T, TError>> (implicit conversion)
TaskResult<int, string> tr = SomeAsyncMethodReturningResult();

// Or via the async extension methods on Result
TaskResult<int, string> tr = Result<int, string>.Success(5)
    .Bind(async x => await LookupAsync(x));
```

### Composing async pipelines
`Bind` and `Map` accept both synchronous and asynchronous delegates. Errors short-circuit automatically:
```csharp
TaskResult<Order, string> result = await Result<int, string>.Success(orderId)
    .Bind(async id => await LoadOrderAsync(id))       // async Bind
    .Map(order => order with { Status = "Processed" }) // sync Map
    .Bind(async order => await SaveOrderAsync(order)); // async Bind
```

### Accessing the value
```csharp
string message = await taskResult.Match(
    value => $"Got {value}",
    error => $"Failed: {error}"
);

await taskResult.Switch(
    value => Console.WriteLine($"Got {value}"),
    error => Console.WriteLine($"Failed: {error}")
);
```

### Async TryCatch
Wrap exception-throwing async code using `TryCatchAsync`:
```csharp
TaskResult<string, string> result = await orderId
    .TryCatchAsync(
        async id => await FetchFromApiAsync(id),
        ex => ex.Message
    );
```

## Extension Methods

### FuncExtensionMethods
- `Compose` — Composes two functions together (`f.Compose(g)` produces `x => g(f(x))`)

### ObjectExtensionMethods
- `Pipe` — Pipes a value into a function (`value.Pipe(f)` is equivalent to `f(value)`)

### ListExtensionMethods
- `Sequence` — Converts a list of `Option<T>` into an `Option<IReadOnlyList<T>>` (returns `None` if any element is `None`)
- `Sequence` — Converts a list of `Result<T, TError>` into a `Result<IReadOnlyList<T>, IReadOnlyList<TError>>` (collects all errors)
