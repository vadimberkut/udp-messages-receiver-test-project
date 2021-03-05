using MessageReceiver.Contexts;
using MessageReceiver.HostedServices;
using MessageReceiver.Services;
using MessageReceiver.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdpMessages.Shared.Config;
using UdpMessages.Shared.Helpers;

namespace MessageReceiver
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
            var config = Configuration.Get<ApplicationSettings>();

            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration);
            services.Configure<UdpMessageReceiverSettings>(Configuration.GetSection("UdpMessageReceiver"));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(config.ConnectionStrings.ApplicationDbContext);

                // enable detailed logs
                //if(HostingEnvironmentHelper.IsDevelopmentAny())
                //{
                //    options.LogTo(Console.WriteLine);
                //    options.EnableSensitiveDataLogging();
                //    options.EnableDetailedErrors();
                //}
            });

            // Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore provides middleware for Entity Framework Core error pages.
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddHostedService<UdpMessageReceiverHostedService>();

            services.AddTransient<IMessagesService, MessagesService>();

            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    SerializationConfig.GetDefaultJsonSerializerSettings(options.SerializerSettings);
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessageReceiver", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // apply migrations automatically
            using(var scope = app.ApplicationServices.CreateScope())
            {
                var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                applicationDbContext.Database.Migrate();
            }

            if (HostingEnvironmentHelper.IsDevelopmentAny())
            {
                app.UseDeveloperExceptionPage();
            }

            if (!HostingEnvironmentHelper.IsProductionAny())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessageReceiver v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
