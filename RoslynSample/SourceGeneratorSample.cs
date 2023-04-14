using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NuGet.Repositories;
using System.Reflection;
using System.Text.RegularExpressions;
using WeihanLi.Extensions;

namespace RoslynSample;

public static class SourceGeneratorSample
{
    public static void MainTest()
    {
        var code = @"
                using System;
                using System.Text.RegularExpressions;

                Console.WriteLine($""Hello is {RegexHelper.IsLowercase(""Hello"")}"");
                
                public partial class RegexHelper
                {
                    [System.Text.RegularExpressions.GeneratedRegex(""^[a-z]+$"")]
                    public static partial Regex LowercaseLettersRegex();

                    public static bool IsLowercase(string value)
                    {
                        return LowercaseLettersRegex().IsMatch(value);
                    }
                }";

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var assemblyName = $"MyApp.{Guid.NewGuid()}";
        IEnumerable<PortableExecutableReference> references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToArray();
        // Add a reference to the System.Text.RegularExpressions assembly
        var regexReference = MetadataReference.CreateFromFile(typeof(GeneratedRegexAttribute).Assembly.Location);
        Console.WriteLine(regexReference.FilePath);
        
        references = references.Append(regexReference);
        
        var generatorAssemblyPath = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.0-preview.3.23174.8\analyzers\dotnet\cs\System.Text.RegularExpressions.Generator.dll";
        
        var analyzerReference = new AnalyzerFileReference(generatorAssemblyPath, new AnalyzerAssemblyLoader());
        var generators = analyzerReference.GetGenerators(LanguageNames.CSharp);
        Console.WriteLine(generators.Select(x=>x.ToString()).StringJoin(", "));

        var generatorAssembly = Assembly.LoadFile(generatorAssemblyPath);

        var generator = generatorAssembly.GetType("System.Text.RegularExpressions.Generator.RegexGenerator");
        ArgumentNullException.ThrowIfNull(generator);

        var generatorInstance = generator.CreateInstance<IIncrementalGenerator>();

        ArgumentNullException.ThrowIfNull(generatorInstance);
        var inputCompilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generatorInstance);

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);


        using var ms = new MemoryStream();
        var result = outputCompilation.Emit(ms);

        if (result.Success)
        {
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            assembly.EntryPoint?.Invoke(null, new[] { new string[] { } });
        }
        else
        {
            Console.WriteLine($"Compilation errors:");
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine(diagnostic.ToString());
            }
        }
    }
}

file class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
{
    public Assembly LoadFromPath(string fullPath)
    {
        return Assembly.LoadFile(fullPath);
    }

    public void AddDependencyLocation(string fullPath)
    {
    }
}
