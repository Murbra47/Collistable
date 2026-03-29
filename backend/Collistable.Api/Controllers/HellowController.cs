using Microsoft.AspNetCore.Mvc;

namespace Collistable.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHello()
        {
            return Ok("Hello from Collistable!");
        }
    }
}