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
        //filter attribute以这种方式注入 可以在ctor中注入服务
        [ServiceFilter(typeof(AuthenticationFilter))]
        public IActionResult Index(string info)
        {
            HttpContext.Items.TryGetValue("user", out var result);

            return new ContentResult { Content = ((UserInfo)result).nickname };
        }
    }
}
