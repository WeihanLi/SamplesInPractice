using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReferenceResolver;
using WeihanLi.Extensions;

const string targetFramework = "net8.0";

var resolverFactory = new ReferenceResolverFactory(null);

var frameworkReference = await resolverFactory.ResolveMetadataReferences("framework: default", targetFramework);
var nugetPackageReference =
    await resolverFactory.ResolveMetadataReferences("nuget: WeihanLi.Common.Logging.Serilog", targetFramework);

var codeText = """
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Common.Logging.Serilog;

var serviceCollection = new ServiceCollection()
        .AddLogging(builder => builder.AddSerilog())
    ;
await using var provider = serviceCollection.BuildServiceProvider();
provider.GetRequiredService<ILoggerFactory>()
    .CreateLogger("test")
    .LogInformation("Hello 1234");
""";
var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
var scriptSyntaxTree = CSharpSyntaxTree.ParseText(codeText, parseOptions);

var metadataReferences = frameworkReference.Concat(nugetPackageReference);
var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
var compilation = CSharpCompilation.Create($"dynamic-{Guid.NewGuid()}", new[] { scriptSyntaxTree }, metadataReferences,
    compilationOptions);
var usedReference = compilation.GetUsedAssemblyReferences();
Console.WriteLine("Used references: {0}", usedReference.Select(x => x.Display).StringJoin(Environment.NewLine));

using var ms = new MemoryStream();
var emitResult = compilation.Emit(ms);
if (emitResult.Success)
{
    Console.WriteLine("Success");
}
else
{
    Console.WriteLine("Compile failed");
    foreach (var diagnostic in emitResult.Diagnostics)
    {
        Console.WriteLine(CSharpDiagnosticFormatter.Instance.Format(diagnostic));
    }
}
