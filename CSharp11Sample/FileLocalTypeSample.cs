namespace CSharp11Sample;
class FileLocalTypeSample
{
    public static void MainTest()
    {
        Console.WriteLine("file local type, file scoped type");

        // error CS0246: The type or namespace name 'FileLocalTypeSample2' could not be found
        // var test = new FileLocalTypeSample2();

        // CSharp11Sample.<FileLocalTypeSample2>F1__FileLocalTypeSample2 

        var currentType = typeof(FileLocalTypeSample);
        //var typeName
        //    = "CSharp11Sample.<FileLocalTypeSample2>F1__FileLocalTypeSample2";
        //// = "CSharp11Sample.FileLocalTypeSample2";
        //var localType2TypeInfo = currentType.Assembly.GetType(typeName);

        var localType2TypeInfo = currentType.Assembly.DefinedTypes.First(x => x.Name.EndsWith("FileLocalTypeSample2"));
        ArgumentNullException.ThrowIfNull(localType2TypeInfo);
        var localType2Instance = Activator.CreateInstance(localType2TypeInfo);
        Console.WriteLine(localType2Instance);
        var ageValue = localType2TypeInfo.GetProperty("Age")?.GetValue(localType2Instance);
        Console.WriteLine(ageValue);

        var dog = new Dog();
        Console.WriteLine(dog);
        Console.WriteLine(string.Join(Environment.NewLine, typeof(Dog).GetInterfaces()
                                     .Select(x => x.FullName)));
    }
}

