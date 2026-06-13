using System.Runtime.CompilerServices;

namespace CSharp15Samples;

// https://github.com/dotnet/csharplang/blob/main/proposals/unions.md
internal static class UnionSample
{
    public static void Run()
    {

        {
            Pet pet = new Cat("A");
            Console.WriteLine(pet switch
            {
                Cat c => $"Cat: {c.Name}",
                Dog d => $"Dog: {d.Name}"
            });

            Animal animal = (Pet)new Cat("test");
            Console.WriteLine(animal);
            //Console.WriteLine(animal switch
            //{
            //    Cat c => $"Cat: {c.Name}",
            //    Dog d => $"Dog: {d.Name}",
            //    _ => throw new NotImplementedException()
            //});
        }

        {
            Result<int> result = new ArgumentException("Invalid argument");
            Console.WriteLine(result.Value);
            result = 1;
            Console.WriteLine(result.Value);
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

            union3 = "Hello";
            if (union3.TryGetValue(out string? str))
            {
                Console.WriteLine($"Union3 string value is {str}");
            }

            // Missing compiler required member 'IntegerOrStringCustomized.Value'
            var stringValue = union3 switch
            {
                int i => i.ToString(),
                string s => s
            };
            Console.WriteLine(stringValue);
        }
    }
}

public record class Cat(string Name);
public record class Dog(string Name);
public union Pet(Cat, Dog);
public union Animal(Pet);

public union Result<T>(T, Exception);

public record User(int Id, string Name);
public record NotFoundError(string Message);
public record ValidationError(string Message);
public union GetUserResult(User, NotFoundError, ValidationError);

public union IntegerOrString(int, string);

// CS0029: Cannot implicitly convert type 'int' to 'CSharp15Samples.IntegerOrStringCustomized'
[Union]
public readonly struct IntegerOrStringCustomized
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

    public object? Value => _num is null ? _value : $"{_num}";
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

public record struct IntOrBool
{
    private readonly bool _isBool;
    private readonly int _value;

    public IntOrBool(int value) => (_isBool, _value) = (false, value);
    public IntOrBool(bool value) => (_isBool, _value) = (true, value ? 1 : 0);

    public object Value => _isBool ? _value is 1 : _value;

    public bool HasValue => true;
    public bool TryGetValue(out int value)
    {
        value = _value;
        return !_isBool;
    }
    public bool TryGetValue(out bool value)
    {
        value = _isBool && _value is 1;
        return _isBool;
    }
}

//public record class Result<T> : Result<T>.IUnionMembers
//{
//    private readonly object? _value;

//    public interface IUnionMembers
//    {
//        static Result<T> Create(T value) => new() { _value = value };
//        static Result<T> Create(Exception value) => new() { _value = value };

//        object? Value { get; }
//    }

//    object? IUnionMembers.Value => _value;
//}
