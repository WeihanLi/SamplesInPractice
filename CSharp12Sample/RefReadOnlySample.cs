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
        Console.WriteLine(a);
        
        
    }

    private void UpdateValueInternal(int a)
    {
        a = 2;
        Console.WriteLine(a);
    }
    
    private void UpdateValueInternalWithRef(ref int a)
    {
        a = 2;
        Console.WriteLine(a);
    }
    
    
    private void UpdateValueInternalWithIn(in int a)
    {
        // error CS8331: Cannot assign to variable 'a' or use it as the right hand side of a ref assignment because it is a readonly variable
        // a = 2;
        Console.WriteLine(a);
    }
    
    private void UpdateValueInternalWithRefReadOnly(ref readonly int a)
    {
        // error CS8331: Cannot assign to variable 'a' or use it as the right hand side of a ref assignment because it is a readonly variable
        // a = 2;
        Console.WriteLine(a);
    }
}
