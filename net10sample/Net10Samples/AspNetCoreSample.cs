using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        builder.Services.AddValidation();
        var app = builder.Build();
        app.MapGet("/hello", ([Required] string name) => $"Hello {name}");
        app.MapPost("/students", (Student student) => Results.Ok(student));
        app.MapPost("/teachers", (Teacher teacher) => Results.Ok(teacher));
        await app.RunAsync();
    }
}

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
