using System.Runtime.CompilerServices;
using WebApp.Entities;

namespace WebApp.models
{
    public static class UserExtension
    {
        public static UserInfo ToUserInfo(this User user)
        {
            return new UserInfo(user.NickName, user.HeadImg, user.Gender, user.OpenId,"somerole");
        }
    }
}
