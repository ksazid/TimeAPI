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
using TimeAPI.Domain.Repositories;
using TimeAPI.Data.Repositories;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using TimeAPI.API.Filters;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Hangfire;
using Hangfire.MemoryStorage;
using TimeAPI.API.HangfireJobs;
//using Hangfire;
//using Hangfire.MemoryStorage;

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


            services.AddHealthChecks();
            ////Cross Platform Enabled
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder => builder.WithOrigins("*", "*", "*", "http://208.109.13.140:8080/", "http://208.109.13.140:9090/", "http://localhost:4200/", "https://enforcesolutions.com/", "http://enforcesolutions.com/")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed((hosts) => true));
                //            .AllowCredentials()
                //                .SetIsOriginAllowed((hosts) => true));


                //options.AddPolicy("CorsPolicy",
                //        builder =>
                //        {
                //            builder.AllowAnyOrigin();
                //        });
            });

            //services.AddSignalR();


            services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = "TimeAPI.API";
            });

            services.AddAuthentication();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            //services.AddMvc().AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddMvc().AddNewtonsoftJson();
            //services.AddMvc().AddJsonOptions(o =>
            //{
            //    o.JsonSerializerOptions.PropertyNamingPolicy = null;
            //    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            //    o.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
            //});
            //services.AddControllers().AddJsonOptions(o =>
            //{
            //    o.JsonSerializerOptions.PropertyNamingPolicy = null;
            //    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            //    o.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
            //});


            services.AddControllers().AddNewtonsoftJson();
            services.AddMvcCore().AddApiExplorer();


            //Inject appsetting.json for controllers.
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<StorageSettings>(Configuration.GetSection("StorageSettings"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
              .AddCustomStores()
              .AddDefaultTokenProviders();
            //.AddDefaultTokenProviders(provider => new LifetimeValidator(TimeSpan.FromDays(2)));


            // Add application services.
            services.AddScoped<IUnitOfWork, DapperUnitOfWork>(provider => new DapperUnitOfWork(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmsSender, SmsSender>();
            services.AddSingleton<IJobVerification, JobVerification>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Time API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "token",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                        //new string[] { }
                        Array.Empty<string>()
                    }
                  });
            });

            //Asp.net Identity Password Config
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 1;
            });

            //JWT Auth for Token Based Authentication
            var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                //x.RequireHttpsMetadata = false;
                //x.SaveToken = false;
                //x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                //{
                //    ValidateIssuer = true,
                //    ValidateAudience = true,
                //    ValidAudience = "https://enforce.azurewebsites.net/",
                //    ValidIssuer = "https://enforce.azurewebsites.net/",

                //    ValidateIssuerSigningKey = false,
                //    IssuerSigningKey = new SymmetricSecurityKey(key),
                //    //ValidateIssuer = false,
                //    //ValidateAudience = false,
                //    ClockSkew = TimeSpan.Zero
                //};
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromDays(5));

            //services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfire(config =>
                    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage());

            services.AddHangfireServer();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, 
                            IRecurringJobManager recurringJobManager, 
                            IServiceProvider  serviceProvider )
        {
            //app.UseHangfireDashboard();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDeveloperExceptionPage();
            app.UseCors("CorsPolicy");




            //app.UseSignalR((options) =>
            //{

            //});

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCookiePolicy();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapHealthChecks("/health");
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHealthChecks("/health");
            });


            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Time API");

            });

            app.UseHangfireDashboard();
            //recurringJobManager.AddOrUpdate("run every minute", 
            //    ()=> serviceProvider.GetService<IJobVerification>().SendVerficationEmailConfirmationAsync(),
            //    "* * * * *"
                
            //    );


        }
    }
}
