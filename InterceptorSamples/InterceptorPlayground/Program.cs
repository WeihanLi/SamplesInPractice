Console.WriteLine("Hello, World!");

var test = new Test();
Console.WriteLine(test.Hello(10));
Console.WriteLine(test.AgePlusPlus(10));

Console.WriteLine("Amazing Interceptor");
Console.WriteLine("Amazing .NET Conf China 2023");

await InterceptorPlayground.ActivityScopeSample.MainTest();

public class Test
{
    public string Hello(int age) => $"Hello, I'm {age} years old";
}

public static class Extensions
{
    public static int AgePlusPlus(this Test test, int age)
    {
        return age++;
    }
}

namespace InterceptorPlayground.Generated
{
    public static class Generators
    {
        // [System.Runtime.CompilerServices.InterceptsLocation(
        //     @"C:\projects\sources\SamplesInPractice\InterceptorSamples\InterceptorPlayground\Program.cs",
        //     1,
        //     9
        // )]
        public static void ConsoleWriteLineInterceptor(string? text)
        {
            Console.WriteLine($"Intercepted: {text}");
        }

        // [System.Runtime.CompilerServices.InterceptsLocation(
        //     @"C:\projects\sources\SamplesInPractice\InterceptorSamples\InterceptorPlayground\Program.cs", 
        //     8, 
        //     9
        //     )]
        // [System.Runtime.CompilerServices.InterceptsLocation(
        //     @"C:\projects\sources\SamplesInPractice\InterceptorSamples\InterceptorPlayground\Program.cs", 
        //     7, 
        //     9
        //     )]
        public static void AmazingConsoleWriteLineInterceptor(string? text)
        {
            Console.WriteLine($"Amazing .NET, {text}");
        }

        // [System.Runtime.CompilerServices.InterceptsLocation(
        //     @"C:\projects\sources\SamplesInPractice\InterceptorSamples\InterceptorPlayground\Program.cs", 
        //     4, 
        //     24
        //     )]
        public static string InstanceMethodInterceptor(this Test test, int age)
        {
            return $"Intercepted: {test.Hello(age)}";
        }

        // [System.Runtime.CompilerServices.InterceptsLocation(
        //     @"C:\projects\sources\SamplesInPractice\InterceptorSamples\InterceptorPlayground\Program.cs", 
        //     5, 
        //     24
        //     )]
        public static int ExtensionMethodInterceptor(this Test test, int age)
        {
            return ++age;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute;
}
