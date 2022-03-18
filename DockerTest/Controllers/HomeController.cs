using Microsoft.AspNetCore.Mvc;

namespace DockerTest.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/")]
        [HttpGet("[controller]/")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
