var command = new RootCommand("h");
var methodArgument = new Argument<HttpMethod>("method", ()=> HttpMethod.Get, "Request method");
methodArgument.Arity = ArgumentArity.ZeroOrOne;
command.AddArgument(methodArgument);
var urlArgument = new Argument<string>("url", "Request url");
urlArgument.Arity = ArgumentArity.ExactlyOne;
command.AddArgument(urlArgument);

var schemaOption = new Option<string>("--schema", ()=> "http", "schema");
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
Console.WriteLine($"Request：{parseResult.ValueForArgument(methodArgument)} {parseResult.ValueForArgument(urlArgument)}");
