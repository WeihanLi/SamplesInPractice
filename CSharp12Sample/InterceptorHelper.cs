using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace gen;

internal sealed class InterceptInvocation(IInvocationOperation invocationOperation, InvocationExpressionSyntax invocationExpressionSyntax)
{
    public string MethodName =>
        ((MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression).Name.Identifier.Text;

    public (string FilePath, int Line, int Column) Location { get; } = GetLocation(invocationOperation);

    private static (string filePath, int line, int column) GetLocation(IInvocationOperation operation)
    {
        // The invocation expression consists of two properties:
        // - Expression: which is a `MemberAccessExpressionSyntax` that represents the method being invoked.
        // - ArgumentList: the list of arguments being invoked.
        // Here, we resolve the `MemberAccessExpressionSyntax` to get the location of the method being invoked.
        var memberAccessorExpression = ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)operation.Syntax).Expression);
        // The `MemberAccessExpressionSyntax` in turn includes three properties:
        // - Expression: the expression that is being accessed.
        // - OperatorToken: the operator token, typically the dot separate.
        // - Name: the name of the member being accessed
        // Here, we resolve the `Name` to extract the location of the method being invoked.
        var invocationNameSpan = memberAccessorExpression.Name.Span;
        // Resolve LineSpan associated with the name span so we can resolve the line and character number.
        var lineSpan = operation.Syntax.SyntaxTree.GetLineSpan(invocationNameSpan);
        // Resolve the filepath of the invocation while accounting for source mapped paths.
        var filePath = operation.Syntax.SyntaxTree.GetInterceptorFilePath(operation.SemanticModel?.Compilation.Options.SourceReferenceResolver);
        // LineSpan.LinePosition is 0-indexed, but we want to display 1-indexed line and character numbers in the interceptor attribute.
        return (filePath, lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1);
    }   
}

internal static class Extensions
{
    // Utilize the same logic used by the interceptors API for resolving the source mapped
    // value of a path.
    // https://github.com/dotnet/roslyn/blob/f290437fcc75dad50a38c09e0977cce13a64f5ba/src/Compilers/CSharp/Portable/Compilation/CSharpCompilation.cs#L1063-L1064
    internal static string GetInterceptorFilePath(this SyntaxTree tree, SourceReferenceResolver? resolver) =>
        resolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;
}
