using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.common;
using WebApp.models;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("index")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public IActionResult Index(string info)
        {
            HttpContext.Items.TryGetValue("user", out var result);

            return new ContentResult { Content = ((UserInfo)result).nickname };
        }
    }
}
