using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace InterceptorGenerators;

internal sealed class InterceptInvocation
{
    private readonly MemberAccessExpressionSyntax _memberAccessExpressionSyntax;

    private readonly IInvocationOperation _invocationOperation;

    public InterceptInvocation(IInvocationOperation invocationOperation)
    {
        _invocationOperation = invocationOperation;
        // The invocation expression consists of two properties:
        // - Expression: which is a `MemberAccessExpressionSyntax` that represents the method being invoked.
        // - ArgumentList: the list of arguments being invoked.
        // Here, we resolve the `MemberAccessExpressionSyntax` to get the location of the method being invoked.
        _memberAccessExpressionSyntax =
            (MemberAccessExpressionSyntax)((InvocationExpressionSyntax)_invocationOperation.Syntax)
            .Expression;
        // `MemberAccessExpressionSyntax`.Name: the name of the member being accessed
        MethodName = _memberAccessExpressionSyntax.Name.Identifier.Text;
        AssemblyName = _invocationOperation.TargetMethod.ContainingAssembly.MetadataName;
        ContainingNamespace = _invocationOperation.TargetMethod.ContainingNamespace.GetFullNamespace();
        ContainingTypeName = string.IsNullOrEmpty(ContainingNamespace)
                ? _invocationOperation.TargetMethod.ContainingType.Name
                : $"{ContainingNamespace}.{_invocationOperation.TargetMethod.ContainingType.Name}"
            ;
        IsStaticMethod = _invocationOperation.TargetMethod.IsStatic;
        IsExtensionMethod = _invocationOperation.TargetMethod.IsExtensionMethod;
        
        Location = GetLocation();
    }

    public string AssemblyName { get; }
    
    public string ContainingTypeName { get; }
    public string ContainingNamespace { get; }

    public bool IsExtensionMethod { get; }
    public bool IsStaticMethod { get; }

    public string MethodName { get; }

    public (string FilePath, int Line, int Column) Location { get; }

    private (string filePath, int line, int column) GetLocation()
    {
        // The `MemberAccessExpressionSyntax` in turn includes three properties:
        // - Expression: the expression that is being accessed.
        // - OperatorToken: the operator token, typically the dot separate.
        // - Name: the name of the member being accessed
        // Here, we resolve the `Name` to extract the location of the method being invoked.
        var invocationNameSpan = _memberAccessExpressionSyntax.Name.Span;
        // Resolve LineSpan associated with the name span so we can resolve the line and character number.
        var lineSpan = _invocationOperation.Syntax.SyntaxTree.GetLineSpan(invocationNameSpan);
        // Resolve the filepath of the invocation while accounting for source mapped paths.
        var filePath = _invocationOperation.Syntax.SyntaxTree.GetInterceptorFilePath(
            _invocationOperation.SemanticModel?.Compilation.Options.SourceReferenceResolver
        );
        // LineSpan.LinePosition is 0-indexed, but we want to display 1-indexed line and character numbers in the interceptor attribute.
        return (filePath, lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1);
    }
}

file static class Extensions
{
    // https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md
    // Utilize the same logic used by the interceptors API for resolving the source mapped
    // value of a path.
    // https://github.com/dotnet/roslyn/blob/f290437fcc75dad50a38c09e0977cce13a64f5ba/src/Compilers/CSharp/Portable/Compilation/CSharpCompilation.cs#L1063-L1064
    public static string GetInterceptorFilePath(this SyntaxTree tree, SourceReferenceResolver? resolver) =>
        resolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;

    public static string GetFullNamespace(this INamespaceSymbol symbol)
    {
        if (symbol.ContainingNamespace is null)
            return symbol.Name;

        return $"{GetFullNamespace(symbol.ContainingNamespace)}.{symbol.Name}".Trim('.');
    }
    
    public static string GetFullContainingTypeName(this ITypeSymbol symbol)
    {
        if (symbol.ContainingNamespace is null)
            return symbol.Name;

        return $"{GetFullNamespace(symbol.ContainingNamespace)}.{symbol.Name}".Trim('.');
    }

    public static string GetArgumentDeclaration(this IArgumentOperation argumentOperation)
    {
        var parameter = argumentOperation.Parameter;
        if (parameter is null) throw new ArgumentNullException(nameof(parameter));
        
        var fullTypeName = parameter.Type.GetFullContainingTypeName();
        var parameterName = parameter.Name;

        var simpleText = $"{fullTypeName} {parameterName}";

        if (parameter.HasExplicitDefaultValue)
        {
            simpleText = $"{simpleText} = {parameter.ExplicitDefaultValue}";
        }
        
        return simpleText;
    }
}
