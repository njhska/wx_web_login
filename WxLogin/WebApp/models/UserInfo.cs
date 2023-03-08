namespace WebApp.models
{
    //微软推荐在反序列化时使用record类型
    //但是在config option中使用record类型 必须提供无参构造函数 所以还是回归使用简单的class
    public record UserInfo(string nickname, string headimg, int? sex,string openid);
}
