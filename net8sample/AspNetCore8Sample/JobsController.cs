using Microsoft.AspNetCore.Mvc;

namespace AspNetCore8Sample;

[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    [HttpGet("sync")]
    public IEnumerable<Jobs> JobsSync()
    {
        for (var i = 0; i < 10; i++)
        {
            Thread.Sleep(500);
            yield return new Jobs() { Id = i + 1, Title = $"job_{i} --- {DateTimeOffset.Now}" };
        }
    }
    
    [HttpGet]
    public async IAsyncEnumerable<Jobs> Jobs()
    {
        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(500);
            yield return new Jobs() { Id = i + 1, Title = $"job_{i} --- {DateTimeOffset.Now}" };
        }
    }
}

public class Jobs
{
    public required int Id { get; set; }
    public required string Title { get; set; }
}
