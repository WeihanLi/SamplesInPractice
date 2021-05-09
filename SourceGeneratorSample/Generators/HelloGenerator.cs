using Microsoft.CodeAnalysis;

namespace Generators
{
[Generator]
public class HelloGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // for debugging
        // if (!Debugger.IsAttached) Debugger.Launch();
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var code = @"namespace HelloGenerated
{
  public class HelloGenerator
  {
    public static void Test() => System.Console.WriteLine(""Hello Generator"");
  }
}";
        context.AddSource(nameof(HelloGenerator), code);
    }
}
}
