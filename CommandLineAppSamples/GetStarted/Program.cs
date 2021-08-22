var command = new RootCommand("h");
var schemaOption = new Option<string>("--schema", "schema");
command.AddOption(new Option(new[] { "-v", "--verbose" }, "Verbose"));
command.AddOption(schemaOption);

var parseResult = command.Parse(args);

Console.WriteLine("----- Tokens ------");
foreach (var token in parseResult.Tokens)
{
    Console.WriteLine($"{token.Type}: {token.Value}");
}
Console.WriteLine("------------------");

Console.WriteLine($"Schema value: {parseResult.ValueForOption(schemaOption)}");
