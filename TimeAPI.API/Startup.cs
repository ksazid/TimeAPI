
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using TimeAPI.API.Identity;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using TimeAPI.Data;
using TimeAPI.Domain;

namespace TimeAPI.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowOrigin",
            //        builder =>
            //        {
            //            builder.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString(),
            //                        "http://localhost:4200",
            //                        "https://enforce.azurewebsites.net",
            //                        "https://*.azurewebsites.net")
            //                .SetIsOriginAllowedToAllowWildcardSubdomains()
            //                .AllowAnyOrigin()
            //                .SetIsOriginAllowed((host) => true)
            //                .AllowAnyMethod().WithOrigins("GET, POST, PUT, DELETE, OPTIONS")
            //                .AllowAnyHeader().WithOrigins("origin, accept, content-Type")
            //                .AllowCredentials();
            //        });

            //});
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("https://enforce.azurewebsites.net")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddMvc();
            services.AddMvcCore().AddApiExplorer();

            //Inject appsetting.json for controllers.
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
              .AddCustomStores()
              .AddDefaultTokenProviders();

            // Add application services.
            services.AddScoped<IUnitOfWork, DapperUnitOfWork>(provider => new DapperUnitOfWork(Configuration.GetConnectionString("DefaultConnection").ToString()));
            services.AddTransient<IEmailSender, EmailSender>();

            //services.AddControllersWithViews();
            //services.AddSwaggerGen(c =>
            //{
            //    c.EnableAnnotations();
            //});

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Time API", Version = "v1" });
            });


            //Asp.net Identity Password Config
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            });

            //Cross Platform Enabled
            services.AddCors();

            //JWT Auth for Token Based Authentication
            var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"].ToString());
            services.AddAuthentication(x =>
                        {
                            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                        }).AddJwtBearer(x =>
                        {
                            x.RequireHttpsMetadata = false;
                            x.SaveToken = false;
                            x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(key),
                                ValidateIssuer = false,
                                ValidateAudience = false,
                                ClockSkew = TimeSpan.Zero
                            };
                        });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("CorsPolicy");

            //app.UseCors();
            ////app.UseOptions();
            //app.UseCors("AllowOrigin");
            //app.UseCors(builder =>
            //    builder.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString(),
            //                        "http://localhost:4200",
            //                        "https://enforce.azurewebsites.net/",
            //                        "https://*.azurewebsites.net")
            //                        .SetIsOriginAllowed((host) => true)
            //                        .AllowAnyHeader().WithOrigins("origin, accept, content-Type")
            //                        .AllowAnyMethod().WithOrigins("GET, POST, PUT, DELETE, OPTIONS")
            //                        .SetIsOriginAllowedToAllowWildcardSubdomains()
            //                        .AllowCredentials()
            //            );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });


            //WebAPI Hosted URL
            //app.UseCors(builder =>
            //    builder.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString())
            //    .AllowAnyHeader()
            //    .AllowAnyMethod()
            //);
          

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Time API");
            });
        }




    }
}
