using Microsoft.EntityFrameworkCore;

namespace WebApp.Entities
{
    public class NpgsqlContext:DbContext
    {
        public NpgsqlContext(DbContextOptions<NpgsqlContext> dbContextOptions):base(dbContextOptions)
        {

        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfig());
        }
    }
}
