using System;
using WeihanLi.Npoi;

namespace NPOISample
{
    public class NPOIExtensionSample
    {
        public static void MainTest()
        {
            var workbook = ExcelHelper.LoadExcel(excelPath: null);

            if (workbook is null)
            {
                Console.WriteLine("workbook is null");
            }
        }
    }
}
