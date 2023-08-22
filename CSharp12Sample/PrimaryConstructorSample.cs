﻿using WeihanLi.Common;

namespace CSharp12Sample;

public static class PrimaryConstructorSample
{
    public static void MainTest()
    {
        var mouse = new Animal("Jerry");
        Console.WriteLine(mouse.Name);
        var cat = new Cat("Tom");
        Console.WriteLine(cat.Name);
        var dog = new Dog("Spike", 3);
        Console.WriteLine(dog.Age);
        dog.OneYearPassed();

        var point = new Point(2, 2);
        Console.WriteLine(point);

        var helper = new CrudHelper<int>();
        var result = helper.Create(1);
        Console.WriteLine(result);
    }
}

file class Animal(string name) 
{
    public string Name => name;
}

file sealed class Cat(string name) : Animal(name){}

file sealed class Dog(string name, int age) : Animal(name)
{
    public int Age => age;

    public void OneYearPassed()
    {
        age++;
        Console.WriteLine("One year passed, now age is: {0}", age);
    }
}

file struct Point(int x, int y)
{
    public int X => x;
    public int Y => y;

    public override string ToString() => $"({X}, {Y})";
}

file sealed class CrudHelper<T>(IIdGenerator idGenerator)
{
    public CrudHelper(): this(GuidIdGenerator.Instance)
    {
    }
    public string Create(T t)
    {
        // Biu...
        return idGenerator.NewId();
    }
}
