#:sdk Microsoft.Net.Sdk.Web

using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/sse", (CancellationToken cancellationToken, int duration = 10) =>
{
    async IAsyncEnumerable<string> GetLines(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var rand = Random.Shared.Next(60, 100);
            yield return $"date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}, rand: {rand}";
            await Task.Delay(1000, cancellationToken);
        }
    }

    return TypedResults.ServerSentEvents(GetLines(cancellationToken));
});

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head><title>SSE Demo</title></head>
    <body>
    <h1>SSE Demo from ASP.NET Core</h1>
    <div id=""messages""></div>
    <script>
        const source = new EventSource('/sse');
        source.onmessage = (event) => {
            document.getElementById('messages').innerHTML += '<p>' + event.data + '</p>';
        };
        source.onerror = (err) => console.error('SSE error:', err);
    </script>
    </body>
    </html>
    """, "text/html"));

app.Run();
