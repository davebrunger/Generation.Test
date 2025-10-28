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
- Mark the wrapping class <code>partial</code>, this allows the code generator to place the generated code in a different file
- Create a <code>private static</code> method that accepts a value of the wrapped type as an argument and returns a <code>bool</code>
- Add the <code>BasicWrapper</code> attribute, and specify the wrapped type and the name of the validating method 
```csharp
[BasicWrapper(typeof(int), nameof(Validate))]
public partial class NaturalInteger
{
    private static bool Validate(int value) => value >= 0;
}
```
### Usage
- Use the generated <code>Create</code> static method to generate a new value, an <code>Option</code> instance is returned
- For convenience, use the <code>Bind</code> and <code>Map</code> methods to access the value.
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
- Mark the union class <code>partial</code>, this allows the code generator to place the generated code in a different file
- For each option add an instance of the <code>Option</code> attribute
- The type of any data associated with an option can be specified by the <code>OfGeneric</code> and <code>OfType</code> attribute constructor parameters
- An option can specify either <code>OfGeneric</code> or <code>OfType</code>, or neither, but not both
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
- Static methods and properties are generated to create new instances.
- Access the option using the <code>Switch</code> and <code>Match</code> methods
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
