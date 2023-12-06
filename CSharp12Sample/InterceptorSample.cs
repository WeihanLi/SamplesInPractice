using WeihanLi.Common.Models;

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
            
            Console.WriteLine("Hello world");
            
            c.GenericSampleMethod(1, Result.Success());
            
            C.StaticGenericSampleMethod(1, Result.Success());
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

        public void GenericSampleMethod<T1, T2>(T1 t1, T2? t2) 
            where T1: struct
            where T2: class
        {
            Console.WriteLine($"{nameof(GenericSampleMethod)}, t1: {t1}, t2: {t2}");
        }
        
        public static void StaticGenericSampleMethod<T1, T2>(T1 t1, T2? t2) 
            where T1: struct
            where T2: class
        {
            Console.WriteLine($"{nameof(GenericSampleMethod)}, t1: {t1}, t2: {t2}");
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute;
}

namespace CSharp12Sample.Generated
{
     public static class Extensions
     {
         // static method intercept sample
         // [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 13, character: 21)]
         // public static void ConsoleWriteLineInterceptor(string msg)
         // {
         //     Console.WriteLine($"Intercepted: {msg}");
         // }
         //
         // Error CS9153 : The indicated call is intercepted multiple times.
         // can not intercept the same invocation more than once
         // [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 13, character: 21)]
         // public static void ConsoleWriteLineInterceptor2(string msg)
         // {
         //     Console.WriteLine($"Intercepted2: {msg}");
         // }
         
        //  [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 11/*L1*/, character: 15/*C1*/)] // refers to the call at (L1, C1)
        //  public static void TestMethodInterceptor(this A a)
        //  {
        //      Console.WriteLine($"Intercepted: {nameof(TestMethodInterceptor)}");
        //  }
         
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

         // known bug: https://github.com/dotnet/roslyn/issues/70311
         // [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 17, character: 15)]
         // public static void GenericSampleMethodInterceptor<T1, T2>(this C c, T1 t1, T2? t2)
         //     where T1: struct
         //     where T2: class
         // {
         //     Console.WriteLine($"Intercepted: {nameof(GenericSampleMethodInterceptor)}");
         // }
         
         //
         // [System.Runtime.CompilerServices.InterceptsLocation(@"C:\projects\sources\SamplesInPractice\CSharp12Sample\InterceptorSample.cs", line: 19, character: 15)]
         // public static void StaticGenericSampleMethodInterceptor<T1, T2>(T1 t1, T2 t2)
         // {
         //     Console.WriteLine($"Intercepted: {nameof(StaticGenericSampleMethodInterceptor)}");
         // }
     }
}
