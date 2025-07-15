using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using AspIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using IPNetwork = System.Net.IPNetwork;

namespace Net10Samples;

public class AspNetCoreSample
{
    public static async Task MainTest()
    {
        Console.WriteLine(AspIPNetwork.TryParse("127.0.0.1/8", out _));
        Console.WriteLine(IPNetwork.TryParse("127.0.0.1/8", out _));
    }

    public static async Task RouteConvention()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();
        await app.RunAsync();
    }

    public static async Task MinimalApiValidation()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();
        app.MapGet("/hello", ([Required] string name) => $"Hello {name}");
        app.MapPost("/students", (Student student) => Results.Ok(student));
        await app.RunAsync();
    }
}

public class Student
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
