using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using ReferenceResolver;
using WeihanLi.Common;
using WeihanLi.Extensions;

namespace RoslynSample;

// https://github.com/filipw/Strathweb.Samples.Roslyn.Completion/blob/master/CompletionDemo/CompletionDemo/Program.cs
public static class CodeCompletionSample
{
    public static async Task MainTest()
    {
        Console.WriteLine($"{new string('-', 20)} general completion {new string('-', 20)}");
        var code = """
using System;

public class MyClass
{
    public static void MyMethod(int value)
    {
        Guid.
    }
}
""";
        
        using var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

        // IReferenceResolver frameworkReferenceResolver = new FrameworkReferenceResolver();
        // var references = await frameworkReferenceResolver.
        //     ResolveMetadataReferences(FrameworkReferenceResolver.FrameworkNames.Default,
        //     "net8.0");
        
        var references = new MetadataReference[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            nameof(CodeCompletionSample),
            nameof(CodeCompletionSample),
            LanguageNames.CSharp)
            .WithMetadataReferences(references)
            ;
        var project = workspace.AddProject(projectInfo);
        var document = workspace.AddDocument(project.Id, "__script.cs", SourceText.From(code));
        var completionService = CompletionService.GetService(document);
        Guard.NotNull(completionService);
        var completions = await completionService.GetCompletionsAsync(document,  code.LastIndexOf("Guid.", StringComparison.Ordinal) + 5);
        foreach (var completionItem in completions.ItemsList)
        {
            Console.WriteLine(completionItem.DisplayText);
            Console.WriteLine(completionItem.ToJson());
        }
        
        //
        Console.WriteLine($"{new string('-', 20)} script completion {new string('-', 20)}");
        var scriptCode = "System.DateTime.";
        var scriptProjectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                nameof(CodeCompletionSample),
                nameof(CodeCompletionSample),
                LanguageNames.CSharp, isSubmission: true)
            .WithMetadataReferences(references)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var scriptProject = workspace.AddProject(scriptProjectInfo);
        var scriptDocumentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(scriptProject.Id), "Script",
            sourceCodeKind: SourceCodeKind.Script,
            loader: new PlainTextLoader(scriptCode, VersionStamp.Create()));
        var scriptDocument = workspace.AddDocument(scriptDocumentInfo);
        var scriptCompletionService = CompletionService.GetService(scriptDocument);
        Guard.NotNull(scriptCompletionService);
        var scriptCompletions = await scriptCompletionService.GetCompletionsAsync(scriptDocument, scriptCode.Length);
        foreach (var completionItem in scriptCompletions.ItemsList)
        {
            Console.WriteLine(completionItem.DisplayText);
            Console.WriteLine(completionItem.ToJson());
        }
    }
}

public sealed class PlainTextLoader(string text, VersionStamp? versionStamp = null) : TextLoader
{
    private readonly TextAndVersion _textAndVersion = TextAndVersion.Create(SourceText.From(text), versionStamp.GetValueOrDefault(VersionStamp.Default));

    public override Task<TextAndVersion> LoadTextAndVersionAsync(LoadTextOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(_textAndVersion);
    }
}
