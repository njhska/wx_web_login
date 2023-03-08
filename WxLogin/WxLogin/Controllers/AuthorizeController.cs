using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using WxLogin.models;

namespace WxLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private static readonly string stateCode = "3d6be0a4035d839573b04816624a415e";
        private readonly IHttpClientFactory clientFactory;
        private readonly IOptions<WXOptions> wxOption;
        private readonly ILogger<AuthorizeController> logger;

        public AuthorizeController(IHttpClientFactory clientFactory,IOptions<WXOptions> wxOption,ILogger<AuthorizeController> logger)
        {
            this.clientFactory = clientFactory;
            this.wxOption = wxOption;
            this.logger = logger;
        }
        [HttpGet("{topage}")]
        public async Task<IActionResult> Get(string code,string state,string toPage)
        {
            var url = WebUtility.UrlDecode(toPage);

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state) || string.IsNullOrEmpty(toPage))
            {
                logger.LogDebug("get: code or state or topage is empty");
                return BadRequest("code or state or topage is empty");
            }

            if(state != stateCode)
            {
                logger.LogDebug("get: state 不匹配");
                return BadRequest("state 不匹配");
            }

            var userToken = await GetUserToken(code);

            if(string.IsNullOrEmpty(userToken.access_token)||string.IsNullOrEmpty(userToken.openid))
            {
                logger.LogDebug("get: code 未匹配");
                return NotFound("code 未匹配");
            }

            var userInfo = await GetUserInfo(userToken.access_token,userToken.openid);           
            

            return Redirect($"{url}?nickname={WebUtility.UrlEncode(userInfo.nickname)}&headimg={WebUtility.UrlEncode(userInfo.headimgurl)}");
        }

        private async Task<UserToken> GetUserToken(string code)
        {
            var httpClient = clientFactory.CreateClient("wx");
            
            var userToken = await httpClient
                .GetFromJsonAsync<UserToken>(
                    $"oauth2/access_token?appid={wxOption.Value.AppId}&secret={wxOption.Value.AppSecret}&code={code}&grant_type=authorization_code");
            return userToken;
        }
        private async Task<UserInfo> GetUserInfo(string access_token,string openid)
        {
            var httpClient = clientFactory.CreateClient("wx");
            var userInfo = await httpClient
                .GetFromJsonAsync<UserInfo> ($"userinfo?access_token={access_token}&openid={openid}");
            return userInfo;
        }
    }
}
