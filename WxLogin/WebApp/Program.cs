using Microsoft.EntityFrameworkCore;
using WebApp.common;
using WebApp.Entities;
using WebApp.models;

namespace WebApp
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

            builder.Services.AddDbContext<NpgsqlContext>(x =>
            {
                var constr = builder.Configuration.GetConnectionString("npgsql");
                x.UseNpgsql(constr);
            });

            //filter attribute以这种方式注入 可以在ctor中注入服务
            builder.Services.AddScoped<AuthenticationFilter>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(option =>
            {
                option.Cookie.Name = "webapp";
                option.IdleTimeout = TimeSpan.FromSeconds(600);
                option.Cookie.IsEssential = true;
            });

            var app = builder.Build();
            app.UseStaticFiles();
            app.UseSession();
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