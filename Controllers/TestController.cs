using Microsoft.AspNetCore.Mvc;

namespace NetReach.Api.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                ok = true,
                message = "API is working",
                time = DateTime.UtcNow
            });
        }

        [HttpPost]
        public IActionResult Post([FromBody] object data)
        {
            return Ok(new
            {
                ok = true,
                received = data
            });
        }
    }
}
