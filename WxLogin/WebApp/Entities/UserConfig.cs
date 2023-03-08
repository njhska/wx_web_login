using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Entities
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("t_users");
            builder.HasKey(x => x.OpenId);
            builder.Property(x => x.LastLoginTime).HasColumnType("timestamp");
        }
    }
}
