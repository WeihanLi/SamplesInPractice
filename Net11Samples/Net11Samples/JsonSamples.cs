using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;

namespace Net11Samples;

internal static class JsonSamples
{
    public static async Task Run()
    {
        {
            // Serialize an IAsyncEnumerable<T> to a stream as JSON lines
            var logs = GetLogsAsync();
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsyncEnumerable(ms, logs, true);
            ms.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(ms);
            var output = await reader.ReadToEndAsync();
            Console.WriteLine(output);
        }

        {
            // Serialize an IAsyncEnumerable<T> to a PipeWriter as JSON lines
            var logs = GetLogsAsync();
            var pipe = new Pipe();
            var writerTask = JsonSerializer.SerializeAsyncEnumerable(pipe.Writer, logs, true)
                .ContinueWith(_ => pipe.Writer.Complete());
            var readerTask = ReadFromPipeAsync(pipe.Reader);
            await Task.WhenAll(writerTask, readerTask);

            async Task ReadFromPipeAsync(PipeReader reader)
            {
                while (true)
                {
                    // Wait for data from the writer
                    var result = await reader.ReadAsync();
                    var buffer = result.Buffer;
                    // Process whatever data is available
                    if (!buffer.IsEmpty)
                    {
                        // Note: In real networking code, you must handle incomplete messages here.
                        // For this example, we just read everything available.
                        var text = Encoding.UTF8.GetString(buffer);
                        Console.Write($"[{DateTimeOffset.Now}]: {text}");
                    }
                    // Tell the reader how much data we consumed so it can free the memory
                    reader.AdvanceTo(buffer.End);
                    // Stop if the writer called Complete()
                    if (result.IsCompleted)
                    {
                        await reader.CompleteAsync();
                        break;
                    }
                }
            }
        }
    }

    public static async Task JsonLinesApiSample()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();
        app.MapGet("/logs", (HttpContext context) =>
        {
            var logs = GetLogsAsync();
            return Results.JsonLines(logs);
        });
        await app.RunAsync("http://localhost:5100");
    }

    private static async IAsyncEnumerable<LogEntry> GetLogsAsync()
    {
        yield return new("xxx begin");
        await Task.Delay(2000);
        yield return new("xxx processing");
        await Task.Delay(2000);
        yield return new("xxx end");
    }
}

internal record LogEntry(string Message)
{
    public DateTimeOffset Time { get; init; } = DateTimeOffset.Now;
}

internal sealed class JsonLinesResult<TValue>(IAsyncEnumerable<TValue> result) : IResult
{
    private const string ContentType = "application/jsonl;charset=utf-8";
    private readonly IAsyncEnumerable<TValue> _result = result;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.ContentType = ContentType;
        var jsonOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<JsonOptions>>().Value;
        await JsonSerializer.SerializeAsyncEnumerable(httpContext.Response.BodyWriter, _result,
            true, jsonOptions.SerializerOptions, httpContext.RequestAborted);
    }
}

public static class JsonLinesExtension
{
    extension(Results)
    {
        public static IResult JsonLines<TValue>(IAsyncEnumerable<TValue> result)
        {
            return new JsonLinesResult<TValue>(result);
        }
    }
}
