// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/experimental-attribute?WT.mc_id=DT-MVP-5004222
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/ref-readonly-parameters?WT.mc_id=DT-MVP-5004222
// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12?WT.mc_id=DT-MVP-5004222#ref-readonly-parameters

namespace CSharp12Sample;

public class RefReadOnlySample
{
    public static void MainTest()
    {
        
    }


    private void UpdateValue()
    {
        var a = 1;
        UpdateValueInternal(a);
        Console.WriteLine(a);
        
        UpdateValueInternalWithRef(ref a);
        Console.WriteLine(a);
        
        UpdateValueInternalWithIn(in a);
        UpdateValueInternalWithIn(in a);
        // warning CS9191: The 'ref' modifier for argument 1 corresponding to 'in' parameter is equivalent to 'in'. Consider using 'in' instead.
        // UpdateValueInternalWithIn(ref a);
        Console.WriteLine(a);
        
        UpdateValueInternalWithRefReadOnly(in a);
        UpdateValueInternalWithRefReadOnly(ref a);
        Console.WriteLine(a);
    }

    private void UpdateValueInternal(int a)
    {
        a = 2;
        Console.WriteLine($"{a}");
    }
    
    private void UpdateValueInternalWithRef(ref int a)
    {
        a = 2;
        var b = a;
        Console.WriteLine($"{a} {b}");
    }
    
    
    private void UpdateValueInternalWithIn(in int a)
    {
        // error CS8331: Cannot assign to variable 'a' or use it as the right hand side of a ref assignment because it is a readonly variable
        // a = 2;
        var b = a;
        Console.WriteLine($"{a} {b}");
    }
    
    private void UpdateValueInternalWithRefReadOnly(ref readonly int a)
    {
        // error CS8331: Cannot assign to variable 'a' or use it as the right hand side of a ref assignment because it is a readonly variable
        // a = 2;
        var b = a;
        Console.WriteLine($"{a} {b}");
    }
}
