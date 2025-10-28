# WS.DomainModelling
Test out dotnet code generation to create a genearator for F# style discriminated unions in C#

## Inspiration
- [*Domain Modeling Made Functional*](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) - [Scott Wlaschin](https://scottwlaschin.com/)

## BasicWrapper

Basic Wrapper generates a class that wraps a single type with validation logic. The class has a Create method that
takes a value of the wrapped type, and returns an <code>Option</code> of the wrapping type. This has two advantages.
The caller is forced to deal with an invalid value when an instance of the wrapping type is created and the caller 
also knows that once created the wrapper instance can be passed to other methods and will always hold a valid value.

### Definition
```csharp
[BasicWrapper(typeof(int), nameof(Validate))]
public partial class NaturalInteger
{
    private static bool Validate(int value) => value >= 0;
}
```
### Usage
```csharp
public record Score(NaturalInteger Home, NaturalInteger Away);
...
Option<Score> score = NaturalInteger
    .Create(5)
    .Bind(home => NaturalInteger
        .Create(2)
        .Map(away => new Score(home, away)));
```

## Discriminated Union

### Definition
```csharp
[Option("Success", OfGeneric = "Q")]
[Option("Failed")]
[Option("FileNotFoundError", OfType = typeof(string))]
[Option("IndexOutOfBoundsError", OfType = typeof((int AnInt, string)))]
public partial class DetailedResult<Q>
{
}
```
### Usage
```csharp
DetailedResult<List<string>> DoSomething()
{
    var index = ...
    if (...)
    {
        return DetailedResult<List<string>>.IndexOutOfBoundsError((index, "Message"));
    }
    if (...)
    {
        return DetailedResult<List<string>>.FileNotFoundError("Message");
    }
    var messages = ...
    if (...)
    {
        return DetailedResult<List<string>>.Failed;
    }
    return DetailedResult<List<string>>.Success(messages);
}
...
var result = DoSomething();
result.Switch(
    messages => Console.WriteLine(string.Join(", ", messages)),
    () => Console.WriteLine("Failed"),
    message => Console.WriteLine($"File not found {message}"),
    result => Console.WriteLine($"Index {result.AnInt} out of range: {result.Item2}")
);
```
