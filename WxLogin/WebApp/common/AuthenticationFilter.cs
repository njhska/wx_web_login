using HT.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;
using WebApp.Entities;
using WebApp.models;

namespace WebApp.common
{
    /// <summary>
    /// 单一职责，这个只用来做登陆
    /// </summary>
    public class AuthenticationFilter : IAsyncActionFilter
    {
        private readonly NpgsqlContext npgsqlContext;
        private readonly ILogger<AuthenticationFilter> logger;
        private readonly IConfiguration configuration;

        public AuthenticationFilter(NpgsqlContext npgsqlContext,
            ILogger<AuthenticationFilter> logger,
            IConfiguration configuration)
        {
            this.npgsqlContext = npgsqlContext;
            this.logger = logger;
            this.configuration = configuration;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cancellationToken = context.HttpContext.RequestAborted;

            var args = context.ActionArguments;

            if (args.ContainsKey("info"))
            {
                var userInfoArg = args["info"].ToString();
                var userInfoStr = userInfoArg.Decrypt(configuration.GetValue<string>("EncryptKey"));
                if ("error" == userInfoStr)
                {
                    //表示通过密钥解析失败 非法登陆
                    logger.LogWarning($"非法登陆");
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


                    try
                    {
                        if (npgsqlContext.Users.Count(x => x.OpenId == user.OpenId) == 0)
                        {
                            //首次登陆
                            logger.LogInformation($"{user.OpenId}首次登陆");
                            await npgsqlContext.Users.AddAsync(user, cancellationToken);
                            await npgsqlContext.SaveChangesAsync(cancellationToken);
                        }
                        else
                        {
                            var lastLoginTime = npgsqlContext.Users.Where(x => x.OpenId == user.OpenId).Select(x => x.LastLoginTime).FirstOrDefault();
                            if ((user.LastLoginTime.Value - lastLoginTime.Value).TotalDays >= 7)
                            {
                                logger.LogInformation($"{user.OpenId}信息已过期");
                                var entry = npgsqlContext.Entry(user);
                                entry.Property(x => x.NickName).IsModified = true;
                                entry.Property(x => x.HeadImg).IsModified = true;
                                entry.Property(x => x.LastLoginTime).IsModified = true;
                                await npgsqlContext.SaveChangesAsync(cancellationToken);
                            }
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        logger.LogError("数据库更新失败" + ex.Message);
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        //用户中断了请求
                    }
                    catch (Exception ex)
                    {

                    }
                    finally 
                    { 
                    
                    }
                    context.HttpContext.Session.SetString(configuration.GetValue<string>("LoginUserSessionKey"), JsonSerializer.Serialize<UserInfo>(user.ToUserInfo()));
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
