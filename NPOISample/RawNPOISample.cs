using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WeihanLi.Common.Helpers;
using WeihanLi.Npoi;
using WeihanLi.Npoi.Settings;

namespace NPOISample
{
    // ReSharper disable once InconsistentNaming
    public class RawNPOISample
    {
        public static void BasicTest()
        {
            var workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet();
            var cell = sheet.CreateRow(0).CreateCell(0);
            cell.SetCellType(CellType.String);
            cell.SetCellValue("Hello world");

            var cell2 = sheet.CreateRow(0).CreateCell(1);
            cell2.SetCellType(CellType.Numeric);
            cell2.SetCellValue(10);

            workbook.WriteToFile(ApplicationHelper.MapPath("raw_output.xlsx"));
        }

        public static void PrepareWorkbookTest()
        {
            var entities = new[]
            {
                new { Name = "Alice", Age = 10 },
                new { Name = "Mike", Age = 10 },
                new { Name = "Jane", Age = 10 },
            };

            // var workbook = ExcelHelper.PrepareWorkbook(ExcelFormat.Xlsx);
            var setting = new ExcelSetting();
            var workbook = new XSSFWorkbook();
            var props = workbook.GetProperties();
            props.CoreProperties.Creator = setting.Author;
            props.CoreProperties.Created = DateTime.Now;
            props.CoreProperties.Modified = DateTime.Now;
            props.CoreProperties.Title = setting.Title;
            props.CoreProperties.Subject = setting.Subject;
            props.CoreProperties.Category = setting.Category;
            props.CoreProperties.Description = setting.Description;
            props.ExtendedProperties.GetUnderlyingProperties().Company = setting.Company;
            props.ExtendedProperties.GetUnderlyingProperties().Application = "WeihanLi.Npoi";
            props.ExtendedProperties.GetUnderlyingProperties().AppVersion = "1.9.5";
            // props.ExtendedProperties.GetUnderlyingProperties().AppVersion = typeof(ExcelHelper).Assembly.GetName().Version?.ToString();

            workbook.ImportData(entities);

            workbook.WriteToFile(ApplicationHelper.MapPath("raw_output_0.xlsx"));
        }

        public static void ImportDataTest()
        {
            var entities = new[]
            {
                new { Name = "Alice", Age = 10 },
                new { Name = "Mike", Age = 10 },
                new { Name = "Jane", Age = 10 },
            };

            var workbook = new XSSFWorkbook();
            workbook.ImportData(entities);

            workbook.WriteToFile(ApplicationHelper.MapPath("raw_output_1.xlsx"));
        }
    }
}
