using HT.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WxLogin.common;
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
        private readonly IOptions<JWTOptions> jwtOption;
        private readonly IOptions<EncryptOption> encryptOption;
        private readonly ILogger<AuthorizeController> logger;

        public AuthorizeController(IHttpClientFactory clientFactory,
            IOptions<WXOptions> wxOption,
            IOptions<JWTOptions> jwtOption,
            IOptions<EncryptOption> encryptOption,
            ILogger<AuthorizeController> logger)
        {
            this.clientFactory = clientFactory;
            this.wxOption = wxOption;
            this.jwtOption = jwtOption;
            this.encryptOption = encryptOption;
            this.logger = logger;
        }
        [HttpGet("{topage}")]
        public async Task<IActionResult> Get(string code,string state,string toPage)
        { 
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

            var userToken = await GetUserTokenAsync(code);

            if(string.IsNullOrEmpty(userToken.access_token)||string.IsNullOrEmpty(userToken.openid))
            {
                logger.LogDebug("get: code 未匹配");
                return NotFound("code 未匹配");
            }

            var userInfo = await GetUserInfoAsync(userToken.access_token,userToken.openid);
            var url = WebUtility.UrlDecode(toPage);

            return Redirect($"{url}?info={EncryptUserInfo(userInfo)}");
        }

        private async Task<UserToken> GetUserTokenAsync(string code)
        {
            var httpClient = clientFactory.CreateClient("wx");
            
            var userToken = await httpClient
                .GetFromJsonAsync<UserToken>(
                    $"oauth2/access_token?appid={wxOption.Value.AppId}&secret={wxOption.Value.AppSecret}&code={code}&grant_type=authorization_code");
            return userToken;
        }
        private async Task<UserInfo> GetUserInfoAsync(string access_token,string openid)
        {
            var httpClient = clientFactory.CreateClient("wx");
            var userInfo = await httpClient
                .GetFromJsonAsync<UserInfo> ($"userinfo?access_token={access_token}&openid={openid}");
            return userInfo;
        }

        private string EncryptUserInfo(UserInfo userInfo)
        {
            var str = $"nickname={userInfo.nickname}&headimg={userInfo.headimg}&openid={userInfo.openid}";
            return str.Encrypt(encryptOption.Value.key);
        }

        private string BuildJwtToken(UserInfo userInfo)
        {
            var claims = new List<Claim>
            {
                new Claim("nickname",userInfo.nickname),
                new Claim("headimg",userInfo.headimg),
                new Claim("openid",userInfo.openid)
            };
            return JWTHelper.BuildToken(claims, jwtOption.Value);
        }
    }
}
