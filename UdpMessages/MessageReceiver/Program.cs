using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UdpMessages.Shared.Helpers;

namespace MessageReceiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ShowEnvironmentInfo();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                       // UseConfiguration must be first to work properly
                       .UseConfiguration(GetConfiguration());

                    if (HostingEnvironmentHelper.IsHerokuAny())
                    {
                        webBuilder.UseUrls($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
                    }
                    else
                    {
                        webBuilder.UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://+:80");
                    }

                    webBuilder.UseStartup<Startup>();
                });

        private static IConfiguration GetConfiguration()
        {
            // load env variables from .env file
            string envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envFilePath))
            {
                DotNetEnv.Env.Load(envFilePath);
            }

            // check environment is set (ASPNETCORE_ENVIRONMENT or Environment might be set in .env file)
            // exception will be thrown by helper in case of error
            string temp = HostingEnvironmentHelper.Environment;

            // load env variables from specific .env file
            string envFileSpecificPath = Path.Combine(Directory.GetCurrentDirectory(), $".env__{HostingEnvironmentHelper.Environment}");
            if (File.Exists(envFileSpecificPath))
            {
                DotNetEnv.Env.Load(envFileSpecificPath);
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{HostingEnvironmentHelper.Environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return configuration;
        }

        private static void ShowEnvironmentInfo()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Environment info:");
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            Console.WriteLine($"ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");
            Console.WriteLine($"PORT: {Environment.GetEnvironmentVariable("PORT")}");
            Console.WriteLine(Environment.NewLine);
        }
    }
}
