using WeihanLi.Npoi;
using WeihanLi.Npoi.Attributes;

namespace NPOISample;

public class MultiSheetsSample
{
    public static void MainTest()
    {
        // configure settings for sheet
        var settings = FluentSettings.For<TestEntity2>();
        settings.HasSheetSetting(sheet => sheet.SheetName = "TestEntity2", 1);
        settings.Property(x => x.Id)
            .HasColumnIndex(0)
            .HasColumnOutputFormatter(v => v.ToString("#0000"))
            ;
        settings.Property(x => x.Title)
            .HasColumnIndex(1)
            ;
        settings.Property(x => x.Description)
            .HasColumnIndex(2)
            ;

        var collection1 = new[]
        {
            new TestEntity1() { Id = 1, Title = "test1" },
            new TestEntity1() { Id = 2, Title = "test2" }
        };
        var collection2 = new[]
        {
            new TestEntity2() { Id = 1, Title = "test1", Description = "description"},
            new TestEntity2() { Id = 2, Title = "test2" }
        };
        var workbook = ExcelHelper.PrepareWorkbook(ExcelFormat.Xlsx);
        workbook.ImportData(collection1);
        workbook.ImportData(collection2, 1);
        workbook.WriteToFile("multi-sheets.xlsx");
    }
}

[Sheet(SheetName = "TestSheet", SheetIndex = 0)]
file sealed class TestEntity1
{
    [Column("ID", Index = 0)]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}


file sealed class TestEntity2
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; }
}
