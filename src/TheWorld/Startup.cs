using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using TheWorld.Services;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using TheWorld.Models;

namespace TheWorld
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        public static IConfigurationRoot Configuration { get; set; }

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
                // point to our .json file
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // For more information on how to configure your application,
        // visit http://go.microsoft.com/fwlink/?LinkID=398940
        // ConfigureServices configures dependency injection
        // NB: it accepts only ONE argument
        public void ConfigureServices(IServiceCollection services)
        {
            // Put Mvc services to the services container
            services.AddMvc();

            services.AddLogging();

            // First we add Entity Framework, then SQL Server
            // and finally our DbContext
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<WorldContext>();
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
        /// For more details see the John's blog at
        /// http://wildermuth.com/2015/3/2/A_Look_at_ASP_NET_5_Part_2_-_Startup
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="seeder"></param>
        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            WorldContextSeedData seeder)
        {
            // we have to choose: logging to console of debug window
            // or we could our own logging provider
            // here, we will add the debug window
            loggerFactory.AddDebug(LogLevel.Warning);

            app.UseStaticFiles();

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
            seeder.EnsureSeedData();
        }
    }
}
