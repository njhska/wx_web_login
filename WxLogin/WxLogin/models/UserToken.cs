namespace WxLogin.models
{
    public record UserToken(string access_token, string openid, string unionid, string errcode, string errmsg);
}
