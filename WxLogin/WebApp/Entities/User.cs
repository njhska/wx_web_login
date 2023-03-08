namespace WebApp.Entities
{
    public class User
    {
        public string OpenId { get; set; }
        public string NickName { get; set; }

        public string HeadImg { get; set; }
        public int? Gender { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
}
