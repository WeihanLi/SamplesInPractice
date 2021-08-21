var command = new RootCommand("h");
command.AddOption(new Option(new[] { "-v", "--verbose" }, "Verbose"));
command.AddOption(new Option("--schema", "schema"));

var parseResult = command.Parse(args);

Console.WriteLine("----- Tokens ------");
foreach (var token in parseResult.Tokens)
{
    Console.WriteLine($"{token.Type}: {token.Value}");
}
Console.WriteLine("------------------");

Console.WriteLine($"Schema value: {parseResult.ValueForOption("--schema")}");
