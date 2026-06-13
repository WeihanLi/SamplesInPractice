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
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                Console.WriteLine($"[{DateTimeOffset.Now}]: {line}");
            }
        }

        {
            // Serialize an IAsyncEnumerable<T> to a stream as JSON lines
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
                        break;
                    }
                }
            }

        }

        async IAsyncEnumerable<LogEntry> GetLogsAsync()
        {
            yield return new("xxx begin");
            await Task.Delay(1000);
            yield return new("xxx processing");
            await Task.Delay(2000);
            yield return new("xxx end");
        }
    }
}


file record LogEntry(string Message)
{
    public DateTimeOffset Time { get; init; } = DateTimeOffset.Now;
}
