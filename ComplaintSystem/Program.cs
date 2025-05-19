using ComplaintSystem.Data;
using ComplaintSystem.Repositories;
using Scalar.AspNetCore;
using Serilog;

namespace ComplaintSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //Registering Services
            builder.Services.AddSingleton<IDapperContext, DapperContext>();
            builder.Services.AddSingleton<IDepartment, DepartmentRepo>();

            //Registering Serilog
            builder.Host.UseSerilog((context, services, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));


            builder.Host.UseSerilog();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
