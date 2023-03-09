using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.common;
using WebApp.models;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        IConfiguration configuration;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        [HttpGet("index")]
        [TypeFilter(typeof(SessionFilter), Arguments = new object[] { "somerole" })]
        public IActionResult Index()
        {
            var json = HttpContext.Session.GetString(configuration.GetValue<string>("LoginUserSessionKey"));
            var userInfo = JsonSerializer.Deserialize<UserInfo>(json);

            return Content(userInfo.nickname);
        }

        //活动网址的登录页->登陆后要跳转到网站首页 可以是登录页-中转页-首页
        //中转页
        [HttpGet("Transit")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public IActionResult Transit(string info = null)
        {
            return RedirectToAction("index");
        }
    }
}
