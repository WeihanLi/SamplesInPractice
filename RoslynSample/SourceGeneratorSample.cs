using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

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

        // Add a reference to the System.Text.RegularExpressions assembly
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.RegularExpressions.Regex).Assembly.Location),
        };

        // Add a reference to the project that contains the source generator
        var generatorProjectPath = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.0-preview.3.23174.8\analyzers\dotnet\cs\System.Text.Json.SourceGeneration.dll";
        var projectReference = MetadataReference.CreateFromFile(generatorProjectPath);

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references.Append(projectReference),
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (result.Success)
        {
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            assembly.EntryPoint?.Invoke(null, Array.Empty<object>());
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
