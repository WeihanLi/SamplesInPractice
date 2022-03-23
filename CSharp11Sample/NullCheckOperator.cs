// https://github.com/dotnet/csharplang/blob/main/proposals/param-nullchecking.md
public class NullCheckOperator
{    
    public static void MainTest()
    {
        Hello("World");

        try
        {
            Hello(null!);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }

    static void Hello(string name!!)
    {
        Console.WriteLine($"Hello, {name}!");
    }

    // void Hello1(string name!!) => Console.WriteLine($"Hello, {name}!");

    // warning CS8995: Nullable type 'string?' is null-checked and will throw if null.
    // void Hello2(string? name!!) => Console.WriteLine($"Hello, {name}!");

    // error CS8992: Parameter 'int' is a non-nullable value type and cannot be null-checked.
    // void Hello3(int name!!) => Console.WriteLine($"Hello, {name}!");

    // error CS8994: 'out' parameter 'name' cannot be null-checked.
    // void Hello4(out string name!!) => name = "World";

}

