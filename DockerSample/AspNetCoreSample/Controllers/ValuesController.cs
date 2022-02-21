using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace AspNetCoreSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(Dns.GetHostAddresses(Dns.GetHostName())
            .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
            .Select(ip => ip.ToString())
            .ToArray());
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test(string ip, [FromServices] HttpClient httpClient)
    {
        var url = $"http://{ip}/ap/values";
        try
        {
            var result = await httpClient.GetFromJsonAsync<string[]>(url);
            return Ok(new { url, result });
        }
        catch (Exception e)
        {
            return BadRequest(new { url, Error = e.Message });
        }
    }
}
