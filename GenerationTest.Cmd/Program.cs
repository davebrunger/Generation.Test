using GenerationTest.Cmd;

var data = new[] { "1", "6745678", "7789.7867", "Hello", "678" };
var intData = data.Select(s => s.ToInt());

foreach (var item in intData)
{
    Console.WriteLine(item.Match(
        i => $"Parsed integer: {i}",
        () => "Failed to parse integer"
    ));
}

