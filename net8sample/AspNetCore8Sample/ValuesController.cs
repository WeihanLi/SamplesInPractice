using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore8Sample;

[Route("api/values")]
public class ValuesController : ControllerBase
{
    [HttpGet, RequestTimeout(1000)]
    public async Task<string[]> Values()
    {
        await Task.Delay(2000, HttpContext.RequestAborted);
        return new[] { "test" };
    }
    
    [HttpGet("value"), DisableRequestTimeout]
    public async Task<string[]> Value()
    {
        await Task.Delay(20000, HttpContext.RequestAborted);
        return new[] { "test" };
    }
}
