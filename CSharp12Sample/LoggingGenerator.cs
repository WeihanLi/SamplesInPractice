using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics;
using System.Text;

namespace gen;

[Generator(LanguageNames.CSharp)]
public sealed class LoggingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodCalls = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is 
                InvocationExpressionSyntax 
                { 
                    Expression: MemberAccessExpressionSyntax 
                    { 
                        Name:
                        {
                            Identifier:
                            {
                                ValueText: "InterceptableMethod"
                            }
                        }
                    }
                }
            ,
            transform: static (context, token) =>
            {
                var operation = context.SemanticModel.GetOperation(context.Node, token);
                if (operation is IInvocationOperation targetOperation 
                    )
                {
                    return new InterceptInvocation(targetOperation);
                }
                return null;
            })
            .Where(static invocation => invocation != null);

        var interceptors = methodCalls.Collect()
            .Select((invocations, _) =>
            {
                var stringBuilder = new StringBuilder();
                foreach (var invocation in invocations)
                {
                    Debug.Assert(invocation != null);
                    var definition = $$"""
        [System.Runtime.CompilerServices.InterceptsLocationAttribute(@"{{invocation.Location.FilePath}}", {{invocation.Location.Line}}, {{invocation.Location.Column}})]
        public static void LoggingInterceptorMethod(this CSharp12Sample.C c)
        {
            System.Console.WriteLine("logging before...");
            c.InterceptableMethod();
            System.Console.WriteLine("logging after...");
        }
""";
                    stringBuilder.Append(definition);
                    stringBuilder.AppendLine();
                }
                return stringBuilder.ToString();
            });

        context.RegisterSourceOutput(interceptors, (ctx, sources) =>
        {
            var code = $$"""
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute;
}

namespace CSharp12Sample.Generated
{
    public static partial class GeneratedLogging
    {
{{sources}}
    }
}
""";
            ctx.AddSource("GeneratedLoggingInterceptor.g.cs", code);
        });
    }
}
