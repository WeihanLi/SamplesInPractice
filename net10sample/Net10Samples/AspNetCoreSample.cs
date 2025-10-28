using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using AspIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using IPNetwork = System.Net.IPNetwork;

namespace Net10Samples;

public class AspNetCoreSample
{
    public static async Task MainTest()
    {
#pragma warning disable ASPDEPR005
        Console.WriteLine(AspIPNetwork.TryParse("127.0.0.1/8", out _));
#pragma warning restore ASPDEPR005
        Console.WriteLine(IPNetwork.TryParse("127.0.0.1/8", out _));
    }

    public static async Task RouteConvention()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();
        await app.RunAsync();
    }

    public static async Task ServerSentEventSample()
    {
        var app = WebApplication.Create();
        app.MapGet("/", (string? eventSource) => 
            Results.Content($$"""
                            <!DOCTYPE html>
                            <html lang="en">
                            <head>
                              <meta charset="UTF-8" />
                              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                              <title>SSE Demo</title>
                              <style>
                                body {
                                  font-family: system-ui, sans-serif;
                                  margin: 2rem;
                                  background: #f8f9fb;
                                }
                                h1 {
                                  color: #333;
                                }
                                #status {
                                  margin-bottom: 1rem;
                                  font-weight: 500;
                                }
                                #messages {
                                  background: #fff;
                                  border-radius: 8px;
                                  padding: 1rem;
                                  box-shadow: 0 2px 5px rgba(0,0,0,0.1);
                                  max-height: 400px;
                                  overflow-y: auto;
                                }
                                .msg {
                                  border-bottom: 1px solid #eee;
                                  padding: 0.5rem 0;
                                  font-size: 0.95rem;
                                }
                                .msg:last-child {
                                  border-bottom: none;
                                }
                                .timestamp {
                                  color: #888;
                                  font-size: 0.8rem;
                                  float: right;
                                }
                                .heartbeat {
                                  color: blue;
                                }
                              </style>
                            </head>
                            <body>
                              <h1>Server-Sent Events Demo</h1>
                              <div id="status">Connecting...</div>
                              <div id="messages"></div>
                            
                              <script>
                                const statusEl = document.getElementById('status');
                                const messagesEl = document.getElementById('messages');
                                let evtSource;
                            
                                function connect() {
                                  evtSource = new EventSource('/{{eventSource ?? "sse"}}');
                            
                                  evtSource.onopen = () => {
                                    statusEl.textContent = '✅ Connected to /{{eventSource ?? "sse"}}';
                                    statusEl.style.color = 'green';
                                  };
                            
                                  evtSource.onmessage = (event) => {
                                    const msgEl = document.createElement('div');
                                    msgEl.className = 'msg';
                                    const time = new Date().toLocaleTimeString();
                                    msgEl.innerHTML = `<span>Message: ${event.data}</span><span class="timestamp">${time}</span>`;
                                    messagesEl.appendChild(msgEl);
                                    messagesEl.scrollTop = messagesEl.scrollHeight;
                                  };
                            
                                  evtSource.onerror = () => {
                                    statusEl.textContent = '⚠️ Disconnected. Retrying...';
                                    statusEl.style.color = 'red';
                                    // Automatic reconnection is built-in, but we can handle custom logic too
                                  };
                                  
                                  evtSource.addEventListener("heartbeat", (event) => {
                                        const msgEl = document.createElement('div');
                                        msgEl.className = 'heartbeat';
                                        const time = new Date().toLocaleTimeString();
                                        msgEl.innerHTML = `<span>Heartbeat: ${event.data}</span><span class="timestamp">${time}</span>`;
                                        messagesEl.appendChild(msgEl);
                                        messagesEl.scrollTop = messagesEl.scrollHeight;
                                  });
                                }
                            
                                connect();
                              </script>
                            </body>
                            </html>
                            """, "text/html"));

        app.MapGet("/sse", (CancellationToken cancellationToken) => 
            Results.ServerSentEvents(GetSseData(cancellationToken)));
        app.MapGet("/sse1", (CancellationToken cancellationToken) => 
            Results.ServerSentEvents(GetSseTypedData(cancellationToken), "heartbeat"));
        app.MapGet("/sse2", (CancellationToken cancellationToken) => 
            Results.ServerSentEvents(GetSseItem(cancellationToken), "heartbeat"));

        await app.RunAsync();

        
        static async IAsyncEnumerable<string> GetSseData(
            [EnumeratorCancellation]CancellationToken cancellationToken
            )
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                yield return $"date: {DateTimeOffset.Now}";
            }
        }
        
        static async IAsyncEnumerable<EchoDataModel> GetSseTypedData(
            [EnumeratorCancellation]CancellationToken cancellationToken
        )
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                yield return new EchoDataModel("test", DateTimeOffset.Now);
            }
        }
        
        static async IAsyncEnumerable<SseItem<DateTimeOffset>> GetSseItem(
            [EnumeratorCancellation]CancellationToken cancellationToken
        )
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                yield return new SseItem<DateTimeOffset>(DateTimeOffset.Now)
                {
                    EventId = Guid.CreateVersion7().ToString("N"),
                    // ReconnectionInterval = TimeSpan.FromSeconds(10)
                };
            }
        }
    }

    public static async Task MinimalApiValidation()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddValidation();
        var app = builder.Build();
        app.MapGet("/hello", ([Required] string name) => $"Hello {name}");
        app.MapPost("/students", (Student student) => Results.Ok(student));
        app.MapPost("/teachers", (Teacher teacher) => Results.Ok(teacher));
        await app.RunAsync();
    }
}

public record EchoDataModel(string Data, DateTimeOffset Date);

public class Student
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public record Teacher([Required]string Name, int Grade, int? ClassNo = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ClassNo.HasValue && Grade <= 0)
        {
            yield return new ValidationResult("Grade must be greater than or equal to 0 when classNo exists.", [nameof(Grade)]);
        }
    }
}

[Route("api/[controller]")]
public class TestController : ControllerBase
{
    public IResult Sse()
    {
        static async IAsyncEnumerable<string> GetSseData(
            [EnumeratorCancellation]CancellationToken cancellationToken
        )
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                yield return $"date: {DateTimeOffset.Now}";
            }
        }
        
        return TypedResults.ServerSentEvents(GetSseData(CancellationToken.None));
    }
}
