using ComplaintSystem.Data;
using ComplaintSystem.Helper;
using ComplaintSystem.Helpers;
using ComplaintSystem.Repositories;
using ComplaintSystem.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Diagnostics;
using System.Text;

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
            builder.Services.AddSingleton<IStatus, StatusRepo>();
            builder.Services.AddSingleton<EmailService>();
            builder.Services.AddSingleton<MFAService>();
            builder.Services.AddSingleton<PasswordHelper>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<TokenHelper>();
            builder.Services.AddSingleton<IUser, UserRepo>();
            builder.Services.AddSingleton<IRefreshToken, RefreshTokenRepo>();

            //Registering Serilog
            builder.Host.UseSerilog((context, services, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            #region [ Configure Jwt ]
            //Jwt configuration starts here
            var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
            var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
            //Jwt configuration ends here 
            #endregion



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

//TO DO - TEST USER CRUD OPERATIONS AND WHY SERILOG IS NOT WORKING
