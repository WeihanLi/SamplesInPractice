using System.Net;
using System.Text;

namespace CleanCSharpSamples;

public static class MiscSamples
{
    public static void ExceptionFilterSample()
    {
        // before
        {
            try
            {
                // ...
                WriteLine("success");
            }
            catch (Exception e)
            {
                var timeoutException = e as TimeoutException;
                var operationCanceledException = e as OperationCanceledException;
                if (timeoutException != null && operationCanceledException != null)
                {
                    throw;
                }
                
                WriteLine("Timeout");
            }
        }

        // after
        {
            try
            {
                // ...
                WriteLine("success");
            }
            // exception filter + pattern mapping
            catch (Exception e) when(e is TimeoutException or OperationCanceledException)
            {
                WriteLine("Timeout");
            }
        }
    }

    public static void Utf8StringLiteralSample()
    {
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-11.0/utf8-string-literals
        var lineEnd = "\r\n"u8;
        using var responseStream = new MemoryStream();
        responseStream.Write("id,name"u8);
        responseStream.Write(lineEnd);
        var list = new List<Student>()
        {
            new()
            {
                Id = 1,
                Name = "Ming"
            },
            new()
            {
                Id = 2,
                Name = "Mike"
            }
        };
        foreach (var student in list)
        {
            var lineBytes = Encoding.UTF8.GetBytes($"{student.Id},{student.Name}");
            responseStream.Write(lineBytes);
            responseStream.Write(lineEnd);
        }
    }

    public static void IndexRangeSample()
    {
        Student[] array =
        [
            new()
            {
                Id = 1,
                Name = "Ming"
            },
            new()
            {
                Id = 2,
                Name = "Mike"
            },
            new()
            {
                Id = 3,
                Name = "Jane"
            },
        ];
        Console.WriteLine(array[^1].Name);
        foreach (var student in array[1..])
        {
            Console.WriteLine($"{student.Id} {student.Name}");
        }

        var hello = "Hello World";
        var separator = hello.IndexOf(' ');
        if (separator > 0)
        {
            var first = hello[..separator];
            var second = hello[(separator + 1)..];
            Console.WriteLine($"{first} {second}");
        }
    }
}

file sealed class LocalTypesSample
{
    private IEnvironment? MockEnvironment { get; set; }
        
    private interface IEnvironment
    {
        string? GetEnvVal(string name);
    }
    
    private sealed class RuntimeEnvironment : IEnvironment
    {
        public string? GetEnvVal(string name) => Environment.GetEnvironmentVariable(name);
    }
    
    private sealed class Holder
    {
        private sealed class MockEnvironment : IEnvironment
        {
            public string? GetEnvVal(string name) => name;
        }
    }
}

// file types
file interface IEnvironment
{
    string? GetEnvVal(string name);
}

file sealed class RuntimeEnvironment : IEnvironment
{
    public string? GetEnvVal(string name) => Environment.GetEnvironmentVariable(name);
}

file sealed class MockEnvironment : IEnvironment
{
    public string? GetEnvVal(string name) => name;
}

file sealed class AppService1
{
    private readonly IEnvironment _environment;

    public AppService1(IEnvironment environment)
    {
        _environment = environment;
    }
    public void Test()
    {
        Console.WriteLine(_environment.GetEnvVal(nameof(Test)));
    }
}

// primary constructor
file sealed class AppService(IEnvironment environment)
{
    public void Test()
    {
        Console.WriteLine(environment.GetEnvVal(nameof(Test)));
    }
}

file sealed class Student
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
