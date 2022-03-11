using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreSample.Controllers;

[Route("api/health")]
public class HealthController: ControllerBase
{
    [HttpGet]
    public IActionResult Health() => Ok();
}