using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreSample.Models;
using Microsoft.AspNetCore.Mvc;

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
                    Name = Summaries[rng.Next(Summaries.Length)]
                });
        }

        [HttpPost]
        public Book Post([FromBody] Book book)
        {
            return book;
        }
    }
}
