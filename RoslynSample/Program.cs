using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReferenceResolver;
using System.Reflection;
using WeihanLi.Extensions;

const string targetFramework = "net8.0";

var resolverFactory = new ReferenceResolverFactory(null);
var frameworkReferencePaths = await FrameworkReferenceResolver.ResolveDefaultReferences(targetFramework, true);
var frameworkReference = frameworkReferencePaths.Select(x => MetadataReference.CreateFromFile(x));
var serilogPackageReference =
    await resolverFactory.ResolveMetadataReferences("nuget: WeihanLi.Common.Logging.Serilog", targetFramework);
var httpExtensionPackageReference =
    await resolverFactory.ResolveMetadataReferences("nuget: Microsoft.Extensions.Http, 8.0.0-preview.2.23128.3", targetFramework);

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

var metadataReferences = frameworkReference
    .Concat(serilogPackageReference)
    .Concat(httpExtensionPackageReference);
var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication, 
    optimizationLevel: OptimizationLevel.Debug, nullableContextOptions: NullableContextOptions.Annotations,
            allowUnsafe: true);
EnableReferencesSupersedeLowerVersions(compilationOptions);
var compilation = CSharpCompilation.Create($"dynamic-{Guid.NewGuid()}", 
    new[] { scriptSyntaxTree },
    metadataReferences,
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
        if (diagnostic.Severity != DiagnosticSeverity.Error) ;
        Console.WriteLine(CSharpDiagnosticFormatter.Instance.Format(diagnostic));
    }
}



static void EnableReferencesSupersedeLowerVersions(CompilationOptions compilationOptions)
{
    // https://github.com/dotnet/roslyn/blob/a51b65c86bb0f42a79c47798c10ad75d5c343f92/src/Compilers/Core/Portable/Compilation/CompilationOptions.cs#L183
    typeof(CompilationOptions)
        .GetProperty("ReferencesSupersedeLowerVersions", BindingFlags.Instance | BindingFlags.NonPublic)!
        .SetMethod!
        .Invoke(compilationOptions, new object[] { true });
}
