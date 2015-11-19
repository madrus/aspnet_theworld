using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using TheWorld.Services;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using Newtonsoft.Json.Serialization;
using TheWorld.Models;
using TheWorld.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TheWorld
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        public static IConfigurationRoot Configuration { get; set; }

        public class MyOptions
        {
            public string BingKey { get; set; }
        }

        /// <summary>
        /// Application environment knows where our application
        /// is being executed
        /// </summary>
        /// <param name="appEnv"></param>
        /// <param name="env"></param>
        public Startup(
            IApplicationEnvironment appEnv,
            IHostingEnvironment env)
        {
            _env = env;
            // environment variables are very important
            // especially with the Cloud Services
            // because we need to know certain things
            // about the hosting environment
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        // For more information on how to configure your application,
        // visit http://go.microsoft.com/fwlink/?LinkID=398940
        // ConfigureServices configures dependency injection
        // NB: it accepts only ONE argument
        public void ConfigureServices(IServiceCollection services)
        {
            // add the Configuration to our own MyOptions
            services.Configure<MyOptions>(Configuration);

            // Put Mvc services to the services container
            services.AddMvc()
                .AddJsonOptions(opt =>
                {
                    // to convert pascal-case property name to camel-case
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddIdentity<WorldUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.Password.RequiredLength = 8;
                // we define the default path if the user is not authenticated
                config.Cookies.ApplicationCookie.LoginPath = "/Auth/Login";
            })
            .AddEntityFrameworkStores<WorldContext>();

            services.AddLogging();

            // First we add Entity Framework, then SQL Server
            // and finally our DbContext
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<WorldContext>();

            // once per request => AddScoped
            services.AddScoped<GeoService>();

            // we could add the seeder as scoped, in this way an instance of this class
            // would be added every time anybody needs it
            //services.AddScoped<WorldContextSeedData>();

            // but in our case, we would only need it once during configuration
            // so, we will go another way - stateless
            services.AddTransient<WorldContextSeedData>();

            // this could also be a possibility if we wanted only one single instance to exist
            //services.AddSingleton<WorldContextSeedData>();

            // finally, we could use AddInstance and the service will no longer be responsible
            // for creation and cleanup of this object but it could be helpful in some
            // situations when the services are not quite friendly or don't work well with dependency injection
            //services.AddInstance<WorldContextSeedData>("some contructed object");

            // for the repository we will use AddScoped
            services.AddScoped<IWorldRepository, WorldRepository>();

            if (_env.IsDevelopment())
            {
                services.AddScoped<IMailService, DebugMailService>();
            }
            else
            {
                services.AddScoped<IMailService, MailService>();
            }
        }

        /// <summary>
        /// This is the entry point of the app, just a void method
        /// that calls the async ConfigureAsync method
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="seeder"></param>
        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            WorldContextSeedData seeder)
        {
            ConfigureAsync(app, loggerFactory, seeder).Wait();
        }

        /// <summary>
        /// For more details see the John's blog at
        /// http://wildermuth.com/2015/3/2/A_Look_at_ASP_NET_5_Part_2_-_Startup
        ///
        /// VERY IMPORTANT: the sequential order of Use directive matters!!!
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="seeder"></param>
        public async Task ConfigureAsync(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            WorldContextSeedData seeder)
        {
            // we have to choose: logging to console of debug window
            // or we could our own logging provider
            // here, we will add the debug window
            loggerFactory.AddDebug(LogLevel.Warning);

            app.UseStaticFiles();

            app.UseIdentity();

            // AutoMapper with the ReverseMap because
            // we have to be able to map in both directions
            Mapper.Initialize(config =>
            {
                config.CreateMap<Trip, TripViewModel>().ReverseMap();
                config.CreateMap<Stop, StopViewModel>().ReverseMap();
            });

            app.UseMvc(config =>
            {
                config.MapRoute(
                    name: "Default",
                    // in our case we have AppController.Index()
                    // but no 'id', so it will not be looking
                    // for the parameter of the Index method
                    template: "{controller}/{action}/{id?}",
                    // basically we are telling here that // localhost:8000
                    // should be same as localhost:8000/app/index.html
                    defaults: new { controller = "App", action = "Index" }
                );
            });

            // we want to inject it here because its constructor
            // has access to the DbContext and the necessary configuration information
            await seeder.EnsureSeedDataAsync();
        }
    }
}
