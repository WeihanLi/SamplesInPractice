using System.Runtime.CompilerServices;

namespace CSharp12Sample
{
    public static class InterceptorSample
    {
        public static void MainTest()
        {
            var c = new C();
            c.InterceptableMethod(123);
        }
    }

    public class C
    {
        public void InterceptableMethod(int param)
        {
            Console.WriteLine($"interceptable {param}");
        }
    }


    file class LoggingAttribute:Attribute { }
    
    public class LoggingGenerator
    {
        
    }
}

namespace CSharp12Sample.Generated
{
    public static class D
    {
        // [InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 10/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
        // public static void InterceptorMethod(this C c, int param)
        // {
        //     Console.WriteLine($"interceptor {param}");
        // }
        
        // [InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 10/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
        public static void LoggingInterceptorMethod(this C c, int param)
        {
            Console.WriteLine("before...");
            c.InterceptableMethod(param);
            Console.WriteLine("After...");
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
    {
    }
}

