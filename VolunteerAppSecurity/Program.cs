using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VolunteerAppSecurity.DataAccess;
using VolunteerAppSecurity.Interfaces;
using VolunteerAppSecurity.Models;
using VolunteerAppSecurity.Services;
using VolunteerAppSecurity.Exceptions;
using VolunteerAppSecurity.ValidatorsDTO;
using FluentValidation;
using Serilog;

namespace VolunteerAppSecurity
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ExceptionResponseFilter));
            });

            builder.Services.AddValidatorsFromAssemblyContaining<EmailDTOValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<LoginDTOValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterDTOValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordDTOValidator>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<SecurityDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr"));
            });

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<SecurityDBContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigins", policy =>
                {
                    policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            builder.Services.Configure<AuthenticationSetup>(builder.Configuration.GetSection("JwtOptions"));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JwtOptions:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JwtOptions:Audience"],

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtOptions:SecretKey"])),
                    };
                });


            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            app.UseCors("AllowMyOrigins");

            app.UseSerilogRequestLogging();


            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                var roles = new[] { "Admin", "Volunteer", "Organization" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }


            app.Run();
        }
    }
}
