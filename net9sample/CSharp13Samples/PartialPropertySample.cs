using System.Text.RegularExpressions;

namespace CSharp13Samples;
public partial class PartialPropertySample
{
    [GeneratedRegex(@"^1[1-9]\d{9}$")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"^1[1-9]\d{9}$")]
    private static partial Regex PhoneNumberRegex { get; }

    public static void MainTest()
    {
        // partial class property
        var a = new PartialPropertyClass();
        Console.WriteLine(a.Num);

        // partial struct property
        var b = new PartialPropertyStruct();
        Console.WriteLine(b.Num);

        // partial index
        var c = new PartialIndexer();
        Console.WriteLine(c[1]);

        // phone
        var phone = "12312341234";
        Console.WriteLine(PhoneRegex().IsMatch(phone));
        Console.WriteLine(PhoneNumberRegex.IsMatch(phone));
    }
}


// partial property, https://github.com/dotnet/csharplang/issues/6420
file partial class PartialPropertyClass
{
    // CS9252: Property accessor 'PartialPropertyClass.Num.set' must be implemented because it is declared on the definition part
    // CS9253: Property accessor 'PartialPropertyClass.Num.set' does not implement any accessor declared on the definition part
    public partial int Num { get; set; }
}

file partial class PartialPropertyClass
{
    private int _num = 1;
    public partial int Num { get => _num; set => _num = value; }
}

file partial struct PartialPropertyStruct
{
    public partial int Num { get; }
}

file partial struct PartialPropertyStruct
{
    public partial int Num => 2;
}

file partial class PartialIndexer
{
    public partial string this[int index] { get; }
}

file partial class PartialIndexer
{
    public partial string this[int index] { get => index.ToString(); }
}
