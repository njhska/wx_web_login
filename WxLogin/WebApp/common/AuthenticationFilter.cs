using HT.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net;
using WebApp.Entities;
using WebApp.models;

namespace WebApp.common
{
    public class AuthenticationFilter: IAsyncActionFilter
    {
        private readonly NpgsqlContext npgsqlContext;
        private readonly ILogger<AuthenticationFilter> logger;
        private readonly IOptions<EncryptOption> encryptOption;

        public AuthenticationFilter(NpgsqlContext npgsqlContext,
            ILogger<AuthenticationFilter> logger,
            IOptions<EncryptOption> encryptOption)
        {
            this.npgsqlContext = npgsqlContext;
            this.logger = logger;
            this.encryptOption = encryptOption;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var args = context.ActionArguments;
            var controller = context.RouteData.Values["Controller"];
            var action = context.RouteData.Values["Action"];

            if (args.ContainsKey("info"))
            {
                var userInfoArg = args["info"].ToString();
                var userInfoStr = userInfoArg.Decrypt(encryptOption.Value.key);
                if("error" == userInfoStr)
                {
                    //表示通过密钥解析失败 非法登陆
                    logger.LogWarning($"非法登陆{controller}/{action}");
                    context.Result = new BadRequestResult();
                    return;
                }
                else
                {
                    var userinfo = QueryHelpers.ParseQuery(userInfoStr);
                    var user = new User();
                    user.OpenId = userinfo["openid"][0];
                    user.NickName = userinfo["nickname"][0];
                    user.HeadImg = userinfo["headimg"][0];
                    user.LastLoginTime = DateTime.Now;


                    if(npgsqlContext.Users.Count(x=>x.OpenId == user.OpenId) == 0)
                    {
                        //首次登陆
                        logger.LogInformation($"{user.OpenId}首次登陆");
                        await npgsqlContext.Users.AddAsync(user);
                        await npgsqlContext.SaveChangesAsync();
                    }
                    else
                    {
                        var lastLoginTime = npgsqlContext.Users.Where(x=>x.OpenId==user.OpenId).Select(x=>x.LastLoginTime).FirstOrDefault();
                        if((user.LastLoginTime.Value - user.LastLoginTime.Value).TotalDays >= 7)
                        {
                            logger.LogInformation($"{user.OpenId}信息已过期");
                            var entry = npgsqlContext.Entry(user);
                            entry.Property(x=>x.NickName).IsModified= true;
                            entry.Property(x=>x.HeadImg).IsModified= true;
                            await npgsqlContext.SaveChangesAsync();
                        }
                    }
                    context.HttpContext.Items.Add("user", user.ToUserInfo());
                    await next();
                }
            }
            else
            {
                context.Result = new RedirectResult("http://localhost:5298/login.html");
                return;
            }
        }
    }
}
