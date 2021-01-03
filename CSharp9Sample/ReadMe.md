# CSharp 9 New Features

## Intro

随着 dotnet 5 的发布，一系列的 C# 9 新特性也正式发布了。

## `ForEach` extensions

``` csharp
private static IEnumerator<char> GetEnumerator(this int num)
{
    return num.ToString().GetEnumerator();
}

public static void MainTest()
{
    var num = 123;
    foreach (var i in num)
    {
        Console.WriteLine(i);
    }
}
```

## init-only setter

``` csharp
public static void MainTest()
{
    var p1 = new Person
    {
        Name = "Michael",
        Age = 10
    };

    // compiler error
    //p1.Age = 12;

    Console.WriteLine(p1);

    Person p2 = new()
    {
        Name = "Jane",
        Age = 10,
    }, p3 = new()
    {
        Name = "Alice"
    };
    Console.WriteLine(p2);
    Console.WriteLine(p3);

    ReadOnlyPerson p4 = new("Tom", 10);
    Console.WriteLine(p4);

    var model = new TestInitModel() { Name = "Test" };
    var model1 = model.ToJson().JsonToObject<TestInitModel>();
    Console.WriteLine(model1.Name);
}

private class TestInitModel
{
    private readonly string _name;

    public string Name
    {
        get => _name;
        init => _name = value;
    }
}

public class ReadOnlyPerson
{
    public int Age { get; }

    public string Name { get; }

    public ReadOnlyPerson(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public override string ToString()
    {
        return $"(ReadOnlyPerson)Name:{Name}(Age:{Age})";
    }
}

public class Person
{
    public int Age { get; init; }

    public string Name { get; init; }

    public string Description { get; set; }

    public override string ToString()
    {
        return $"Name:{Name}(Age:{Age})";
    }
}
```



 ## `record`

``` csharp
record RecordPerson
{
    public string Name { get; init; }

    public int Age { get; init; }
}

record RecordPerson2(string Name, int Age);

public static void MainTest()
{
    var p1 = new RecordPerson()
    {
        Name = "Tom",
        Age = 12,
    };
    Console.WriteLine(p1);

    var p2 = p1 with { Age = 10 };
    Console.WriteLine(p2);

    var p3 = new RecordPerson() { Name = "Tom", Age = 12 };
    Console.WriteLine($"p1 Equals p3 =:{p1 == p3}");

    RecordPerson2 p4 = new("Tom", 12);
}
```



## Improved pattern-matching

``` csharp
var person = new Person();

// or
// person.Description == null || person.Description.Length = 0
if (person.Description is null or { Length: 0 })
{
    Console.WriteLine($"{nameof(person.Description)} is IsNullOrEmpty");
}

// and
// !string.IsNullOrEmpty(person.Name)
if (person.Name is not null and { Length: > 0 })
{
    if (person.Name[0] is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '.' or ',')
    {
    }
}

// not
if (person.Name is not null)
{
}
```



## Others

### Top-Level Statement

``` csharp
using static System.Console;

WriteLine("Hello World!");
```

### Improved discards in lambda input parameter

``` csharp
Func<int, int, int> constant = (_, _) => 42;
```

## Attributes for local function

``` csharp
public static void MainTest()
{
    InnerTest();

    [MethodImpl(MethodImplOptions.Synchronized)]
    void InnerTest()
    {
        Console.WriteLine(nameof(InnerTest));
    }
}
```

### Partition methods

``` csharp
partial class PartialMethod
{
    public static partial void MainTest();

    static partial void Test1();
}

partial class PartialMethod
{
    public static partial void MainTest()
    {
        Test1();
        Console.WriteLine("Partial method works");
    }
}
```

### `ModuleInitializer`

```csharp
internal static class ModuleInitializerSample
{
    /// <summary>
    /// Initializer for specific module
    /// 
    /// Must be static
    /// Must be parameter-less
    /// Must return void
    /// Must not be a generic method
    /// Must not be contained in a generic class
    /// Must be accessible from the containing module
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        Console.WriteLine($"{nameof(ModuleInitializerAttribute)} works");
    }
}
```
## Reference

- https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9
- https://github.com/WeihanLi/SamplesInPractice/tree/master/CSharp9Sample