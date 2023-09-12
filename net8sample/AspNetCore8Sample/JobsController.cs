using Microsoft.AspNetCore.Mvc;

namespace AspNetCore8Sample;

[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    [HttpGet]
    public async IAsyncEnumerable<Jobs> Jobs()
    {
        for (var i = 0; i < 100; i++)
        {
            await Task.Delay(200);
            yield return new Jobs() { Id = i + 1, Title = $"job_{i}" };
        }
    }
}


public class Jobs
{
    public required int Id { get; set; }
    public required string Title { get; set; }
}
