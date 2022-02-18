Hello("World");

try
{
    Hello(null!);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

void Hello(string name!!)
{
    Console.WriteLine($"Hello, {name}!");
}
