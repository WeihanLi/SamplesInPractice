namespace CSharp12Sample
{
    public static class InterceptorSample
    {
        public static void MainTest()
        {
            var c = new C();
            c.InterceptableMethod();

            var a = new A();
            a.TestMethod();
        }
    }
    
    public class A
    {
        public void TestMethod()
        {
            Console.WriteLine("A.TestMethod");
        }
    }

    public class C
    {
        public void InterceptableMethod()
        {
            Console.WriteLine("interceptable");
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
#pragma warning disable CS9113 // Parameter is unread.
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
#pragma warning restore CS9113 // Parameter is unread.
    {
    }
}

namespace CSharp12Sample.Generated
{
     public static class Extensions
     {
         [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 11/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
         public static void TestMethodInterceptor(this A a)
         {
             Console.WriteLine($"Intercepted: {nameof(TestMethodInterceptor)}");
         }
         
//         // [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 8/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
//         // public static void InterceptorMethod(this C c)
//         // {
//         //     Console.WriteLine($"interceptor");
//         // }
//         
//         [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 8/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
//         public static void LoggingInterceptorMethod(this C c)
//         {
//             Console.WriteLine("Before...");
//             c.InterceptableMethod();
//             Console.WriteLine("After...");
//         }        
     }
}
