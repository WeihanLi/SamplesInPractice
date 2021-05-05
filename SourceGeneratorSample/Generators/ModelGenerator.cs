using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestModels;

namespace Generators
{
    [Generator]
    public class ModelGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new CustomSyntaxReceiver(typeof(User).Assembly.GetName()));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var codeBuilder = new StringBuilder(@"
using System;
using WeihanLi.Extensions;

namespace Generated
{
  public class ModelGenerator
  {
    public static void Test()
    {
      Console.WriteLine(""-- ModelGenerator --"");
");

            if (context.SyntaxReceiver is CustomSyntaxReceiver syntaxReceiver)
            {
                foreach (var model in syntaxReceiver.Models)
                {
                    codeBuilder.AppendLine($@"      ""{model.Identifier.ValueText} Generated"".Dump();");
                }
            }

            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("  }");
            codeBuilder.AppendLine("}");
            var code = codeBuilder.ToString();
            context.AddSource(nameof(ModelGenerator), code);
        }
    }

    internal class CustomSyntaxReceiver : ISyntaxReceiver
    {
        private readonly string _assemblyName;

        public CustomSyntaxReceiver(AssemblyName assemblyName)
        {
            _assemblyName = assemblyName.Name;
        }

        public List<ClassDeclarationSyntax> Models { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                Models.Add(classDeclarationSyntax);
            }
        }
    }
}
