using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWebDemo.Controllers
{
    public partial class UsersController
    {
        [HttpGet("test")]
        public string Test() => "test";
    }
}
