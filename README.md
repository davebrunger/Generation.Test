# WS.DomainModelling
Test out dotnet code generation to create a genearator for F# style discriminated unions in C#

## Inspiration
- [*Domain Modeling Made Functional*](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) - [Scott Wlaschin](https://scottwlaschin.com/)

## Usage

### BasicWrapper

Basic Wrapper generates a class that wraps a single type with validation logic.

#### Definition

```csharp
[BasicWrapper(typeof(int), nameof(Validate))]
public partial class PositiveInterger

```