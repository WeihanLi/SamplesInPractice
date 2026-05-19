using System.Runtime.CompilerServices;

namespace CSharp15Samples;

internal static class UnionSample
{
    public static void Run()
    {

        {
            Pet pet = new Cat("Whiskers");
            Console.WriteLine(pet switch
            {
                Cat c => $"Cat: {c.Name}",
                Dog d => $"Dog: {d.Name}"
            });
        }

        {
            IntegerOrString union1 = 42;
            IntegerOrString union2 = "Hello";
            Console.WriteLine($"Union1: {union1.Value}");
            Console.WriteLine($"Union2: {union2.Value}");

            Action func = union1 switch
            {
                int i => () => Console.WriteLine($"Union1 is an integer: {i}"),
                string s => () => Console.WriteLine($"Union1 is a string: {s}")
            };
            func();

            IntegerOrStringCustomized union3 = 42;
            if (union3.TryGetValue(out int? num))
            {
                Console.WriteLine($"Union3 integer value is {num}");
            }
        }
    }
}

public record class Cat(string Name);
public record class Dog(string Name);
public union Pet(Cat, Dog);

public union IntegerOrString(int, string);

[Union]
public record struct IntegerOrStringCustomized
{
    private readonly string? _value;
    private readonly int? _num = null;
    public IntegerOrStringCustomized(int num)
    {
        _num = num;
    }
    public IntegerOrStringCustomized(string value)
    {
        _value = value;
    }

    // public object? Value => _num is null ? _value : $"{_num}";
    public bool HasValue => _value is not null && _num is not null;

    public bool TryGetValue(out int? value)
    {
        if (_num is int intNum)
        {
            value = intNum;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValue(out string? value)
    {
        if (_value is string stringValue)
        {
            value = stringValue;
            return true;
        }

        value = null;
        return false;
    }
}

[Union]
public class CustomIntegerOrString : IUnion
{
    public object? Value { get; private set; }
}
