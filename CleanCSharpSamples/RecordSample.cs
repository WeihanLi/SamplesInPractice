namespace CleanCSharpSamples;


public static class RecordSample
{
    public static void Run()
    {
        var student = new Student { Id = 1, Name = "Mike" };
        var student2 = new Student { Id = 1, Name = "Mike" };
        // output true
        Console.WriteLine(student == student2);

        Console.WriteLine(student);
        var updatedStudent = student with { Name = "Ming" };
        Console.WriteLine(updatedStudent);

        Console.WriteLine(new Point(1, 2));
    }
}

file record Student
{
    // required members
    // init only setter
    public required int Id { get; init; }
    public required string Name { get; init; }
}

file record struct Point(int X, int Y);
