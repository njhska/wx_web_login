using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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

        [HttpGet("demo")]
        [TypeFilter(typeof(SessionFilter), Arguments = new object[] { "somerole" })]
        public IActionResult Demo()
        {
            return Content("demo");
        }

        //活动网址的登录页->登陆后要跳转到网站首页 可以是登录页-中转页-首页
        //中转页
        [HttpGet("Transit/{topage?}")]
        [ServiceFilter(typeof(AuthenticationFilter))]
        public async Task<IActionResult> Transit(string toPage = null,string info = null,CancellationToken cancellationToken = default)
        {
            //进入网站时，进入的是登录页
            if(string.IsNullOrEmpty(toPage))
            {
                return RedirectToAction("index");
            }
            //在其他位置点的登陆，或要跳转的页面session过期
            return Redirect(WebUtility.UrlDecode(toPage));
        }
    }
}
