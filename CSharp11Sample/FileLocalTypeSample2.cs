namespace CSharp11Sample;
file class FileLocalTypeSample2
{
    public int Age { get; set; } = 10;
}

file record FileLocalRecord();

file struct FileLocalStruct
{
    public string RecordName => nameof(FileLocalRecord);
}

file record struct FileLocalRecordStruct { }
