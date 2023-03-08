using WxLogin.models;

namespace WxLogin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<WXOptions>(builder.Configuration.GetSection("WXOptions"));
            builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<EncryptOption>(builder.Configuration.GetSection("Encrypt"));

            builder.Services.AddHttpClient("wx", client =>
            {
                client.BaseAddress = new Uri("https://api.weixin.qq.com/sns/");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}