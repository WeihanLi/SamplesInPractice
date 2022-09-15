namespace CSharp11Sample;
file class FileLocalTypeSample
{
    public static void MainTest()
    {
        Console.WriteLine("file local type, file scoped type");

        // error CS0246: The type or namespace name 'FileLocalTypeSample2' could not be found
        // var test = new FileLocalTypeSample2();
    }
}


file struct FileLocalStruct
{
}
