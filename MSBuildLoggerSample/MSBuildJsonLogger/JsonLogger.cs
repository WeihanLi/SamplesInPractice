using Microsoft.Build.Framework;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MSBuildJsonLogger;

// https://learn.microsoft.com/en-us/visualstudio/msbuild/build-loggers?view=vs-2022
public sealed class JsonErrorLogger : ILogger
{
    private const string ErrorLogFileName = "json-error-logger.json";
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    private readonly List<BuildError> _errors = new(), _warnings = new();
    public void Initialize(IEventSource eventSource)
    {
        eventSource.WarningRaised += EventSourceOnWarningRaised;
        eventSource.ErrorRaised += EventSourceOnErrorRaised;
    }

    private void EventSourceOnWarningRaised(object sender, BuildWarningEventArgs e)
    {
        _warnings.Add(new BuildError(e.Subcategory, e.Code, e.File, e.LineNumber, e.ColumnNumber, e.EndLineNumber, e.EndColumnNumber, e.Message, e.HelpKeyword, e.SenderName));
    }

    private void EventSourceOnErrorRaised(object sender, BuildErrorEventArgs e)
    {
        _errors.Add(new BuildError(e.Subcategory, e.Code, e.File, e.LineNumber, e.ColumnNumber, e.EndLineNumber, e.EndColumnNumber, e.Message, e.HelpKeyword, e.SenderName));
    }

    public void Shutdown()
    {
        using var fs = File.Create(ErrorLogFileName);
        JsonSerializer.Serialize(fs, new
        {
            warnings = _warnings,
            errors = _errors
        }, JsonSerializerOptions);
        
        _errors.Clear();
    }

    public LoggerVerbosity Verbosity { get; set; }
    public string? Parameters { get; set; }
}


public sealed record BuildError(
    string Subcategory,
    string Code,
    string File,
    int LineNumber,
    int ColumnNumber,
    int EndLineNumber,
    int EndColumnNumber,
    string? Message,
    string? HelpKeyword,
    string? SenderName);
