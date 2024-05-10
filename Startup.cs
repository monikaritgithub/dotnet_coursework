using DotNetCourse.Implementation;
using DotNetCourse.Interfaces;
using DotNetCourse.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DotNetCourse.Data;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Builder.Extensions;
using DotNetCourse.Controllers;

namespace DotNetCourse
{
    public class Startup

    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSession();
            services.AddSignalR();

            // Add JWT authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });

            // Add DbContext using AddDbContext
            services.AddDbContext<DotNetCourseDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Register other services
            services.AddHttpContextAccessor();

            services.AddScoped<IUserInterface, UserImplementation>();
            services.Configure<SmtpOptions>(Configuration.GetSection("SmtpOptions"));
        
    }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
          
            app.UseAuthorization();
            //app.UseEndpoints(endPoints =>
            //{
            //    endPoints.MapHub<AlertHub>("/notificationHub");
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Blog}/{action=Index}/{id?}");
            });
        }
    }
}
