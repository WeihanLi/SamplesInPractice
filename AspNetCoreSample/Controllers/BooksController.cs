using AspNetCoreSample.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.IO.Compression;

namespace AspNetCoreSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IEnumerable<Book> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5)
                .Select(i => new Book
                {
                    Id = i,
                    Author = (i % 2).ToString(),
                    Name = Summaries[rng.Next(Summaries.Length)]
                });
        }

        [HttpPost]
        public Book Post([FromBody] Book book)
        {
            return book;
        }

        [HttpPost("download")]
        public async Task<IActionResult> DownloadCsv()
        {
            var books = Get();
            var memoryStream = new MemoryStream();            
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var group in books
                            .GroupBy(t => new
                            {
                                t.Author
                            }))
                {
                    var entryName = $"{group.Key.Author}.csv";
                    var entry = zip.CreateEntry(entryName);

                    await using var writer = new StreamWriter(entry.Open());
                    await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false
                    });

                    // register class map if necessary
                    // csv.Context.RegisterClassMap<ClassMap>();

                    await csv.WriteRecordsAsync(group);
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "application/zip", "books.zip");
        }
    }
}
