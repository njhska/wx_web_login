using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using WebApp.models;

namespace WebApp.common
{
    /// <summary>
    /// 判断用户权限
    /// </summary>
    public class SessionFilter : IAsyncAuthorizationFilter
    {
        private readonly IConfiguration configuration;
        private readonly string roles;

        public SessionFilter(IConfiguration configuration,string roles)
        {
            this.configuration = configuration;
            this.roles = roles;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userJsonStr = context.HttpContext.Session.GetString(configuration.GetValue<string>("LoginUserSessionKey"));
            if (string.IsNullOrEmpty(userJsonStr))
            {
                context.Result = new RedirectResult("http://localhost:5298/login.html");
                return;
            }

            var userInfo = JsonSerializer.Deserialize<UserInfo>(userJsonStr);
            if (userInfo == null)
            {
                context.Result = new RedirectResult("http://localhost:5298/login.html");
                return;
            }
            if(!roles.Contains(userInfo.role))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
