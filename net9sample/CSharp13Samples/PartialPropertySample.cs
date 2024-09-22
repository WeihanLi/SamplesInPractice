using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CSharp13Samples;
// https://github.com/dotnet/csharplang/issues/6420
// https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/partial-properties.md
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

file partial class PartialPropertyClass
{
    // CS9252: Property accessor 'PartialPropertyClass.Num.set' must be implemented because it is declared on the definition part
    // CS9253: Property accessor 'PartialPropertyClass.Num.set' does not implement any accessor declared on the definition part
    //  CS9250: a partial property cannot be automatically implemented

    /// <summary>
    /// Number comment on declaration
    /// </summary>
    public partial int Num { get; set; }

    private int _num = 1;
    /// <summary>
    /// Number comment on implementation
    /// </summary>
    public partial int Num { get => _num; set => _num = value; }
}

file partial struct PartialPropertyStruct
{
    [DisplayName("Number")]
    public partial int Num { get; }

    [JsonPropertyName("num")]
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
