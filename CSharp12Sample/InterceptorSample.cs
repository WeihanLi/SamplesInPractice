namespace CSharp12Sample
{
    public static class InterceptorSample
    {
        public static void MainTest()
        {
            var c = new C();
            c.InterceptableMethod();
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

// namespace System.Runtime.CompilerServices
// {
//     [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
//     file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
//     {
//     }
// }

// namespace CSharp12Sample.Generated
// {
//     public static class D
//     {
//         // [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 10/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
//         // public static void InterceptorMethod(this C c)
//         // {
//         //     Console.WriteLine($"interceptor");
//         // }
//         
//         [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 10/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
//         public static void LoggingInterceptorMethod(this C c)
//         {
//             Console.WriteLine("Before...");
//             c.InterceptableMethod();
//             Console.WriteLine("After...");
//         }        
//     }
// }
