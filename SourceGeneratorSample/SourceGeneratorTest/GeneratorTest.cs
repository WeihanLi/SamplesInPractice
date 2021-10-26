using Generators;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using WeihanLi.Common;
using Xunit;

namespace SourceGeneratorTest;

public class GeneratorTest
{
    [Fact]
    public async Task HelloGeneratorTest()
    {
        var code = string.Empty;
        var generatedCode = @"namespace HelloGenerated
{
  public class HelloGenerator
  {
    public static void Test() => System.Console.WriteLine(""Hello Generator"");
  }
}";
        var tester = new CSharpSourceGeneratorTest<HelloGenerator, XUnitVerifier>()
        {
            TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(HelloGenerator), $"{nameof(HelloGenerator)}.cs", SourceText.From(generatedCode, Encoding.UTF8)),
                    }
                },
        };

        await tester.RunAsync();
    }

    [Fact]
    public async Task ModelGeneratorTest()
    {
        var code = @"
public class TestModel123{}
";
        var generatedCode = @"
using System;
using WeihanLi.Extensions;

namespace Generated
{
  public class ModelGenerator
  {
    public static void Test()
    {
      Console.WriteLine(""-- ModelGenerator --"");
      ""TestModel123 Generated"".Dump();
    }
  }
}
";
        var tester = new CSharpSourceGeneratorTest<ModelGenerator, XUnitVerifier>()
        {
            TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(ModelGenerator), $"{nameof(ModelGenerator)}.cs", SourceText.From(generatedCode, Encoding.UTF8)),
                    }
                },
        };
        // references
        // TestState.AdditionalReferences
        tester.TestState.AdditionalReferences.Add(typeof(DependencyResolver).Assembly);

        // ReferenceAssemblies
        //    WithAssemblies
        //tester.ReferenceAssemblies = tester.ReferenceAssemblies
        //    .WithAssemblies(ImmutableArray.Create(new[] { typeof(DependencyResolver).Assembly.Location.Replace(".dll", "", System.StringComparison.OrdinalIgnoreCase) }))
        //    ;
        //    WithPackages
        //tester.ReferenceAssemblies = tester.ReferenceAssemblies
        //    .WithPackages(ImmutableArray.Create(new PackageIdentity[] { new PackageIdentity("WeihanLi.Common", "1.0.46") }))
        //    ;

        await tester.RunAsync();
    }
}
