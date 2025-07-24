using GenerationTest.Cmd;

var data = new[] { "1", "6745678", "7789.7867", "Hello", "678" };
var intData = data.Select(s => s.ToInt());
var optionData = intData.Select(r => r.AsSuccess());

foreach (var item in optionData)
{
    Console.WriteLine(item.Match(
       i => $"Parsed integer: {i}",
       () => "Failed to parse integer"
    ));
}

var checkedString = String50.Create("Hello");
var badString = String50.Create("Hels;,lg';fag';fg'fg';kdsh-0thigdflkhbn,/.gfdm,bXBMl;vmx;cbmc;vbmmg;mhmsf;fdh;gfnm;dlo");

Console.WriteLine(checkedString);
Console.WriteLine(badString);

var hello1 = checkedString.Match(s => s, () => throw new Exception("This shouldn't happen"));
var hello2 = String50.Create("Hello").Match(s => s, () => throw new Exception("This shouldn't happen"));
var goodbye = String50.Create("Goodbye").Match(s => s, () => throw new Exception("This shouldn't happen"));

Console.WriteLine(hello1 == hello2);
Console.WriteLine(hello1 != hello2);
Console.WriteLine(hello1 == "Hello");
Console.WriteLine(hello1 != "Hello");
Console.WriteLine(hello1 == goodbye);
Console.WriteLine(hello1 != goodbye);
Console.WriteLine(hello1 == "Goodbye");
Console.WriteLine(hello1 != "Goodbye");

