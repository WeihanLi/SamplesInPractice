using WeihanLi.Extensions;
using WeihanLi.Npoi;

namespace NPOISample
{
    public class NPOIImageSample
    {
        public static void MainTest()
        {
            var excelPath = @"C:\Users\Weiha\Documents\WeChat Files\wxid_htdd44acmqq252\FileStorage\File\2021-04\t.xlsx";
            var workbook = ExcelHelper.LoadExcel(excelPath);

            foreach (var pictureData in
                workbook.GetSheetAt(0).GetPicturesAndPosition())
            {
                pictureData.Dump();
            }
        }
    }
}
