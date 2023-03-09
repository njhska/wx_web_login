using HT.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
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
                    var user = new UserInfo(userinfo["nickname"][0], userinfo["headimg"][0],1, userinfo["openid"][0],"somerole",DateTime.Now);

                    try
                    {
                        using ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("redis"));
                        var userId = $"user:{user.openid}";
                        IDatabase database = redis.GetDatabase(0);
                        var userJson = await database.StringGetAsync(userId);
                        if (string.IsNullOrEmpty(userJson))
                        {
                            //首次登陆
                            await database.StringSetAsync(userId, JsonSerializer.Serialize(user));
                        }
                        var userObj = JsonSerializer.Deserialize<UserInfo>(userJson);
                        if((user.lastvisittime.Value - userObj.lastvisittime.Value).TotalDays >= 7)
                        {
                            await database.StringSetAsync(userId, JsonSerializer.Serialize(user));
                        }

                        context.HttpContext.Session.SetString(configuration.GetValue<string>("LoginUserSessionKey"), database.StringGet(userId));
                        await next();
                    }
                    catch(RedisConnectionException)
                    {
                        logger.LogError("redis连接失败");
                        throw;
                    }
                    catch (RedisServerException ex)
                    {
                        logger.LogError("redis命令执行失败", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.ToString());
                    } 
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
