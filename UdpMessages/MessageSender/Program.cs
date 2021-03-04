using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Configuration;
using UdpMessages.Shared.Helpers;

namespace MessageSender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = GetConfiguration();
            var config = configuration.Get<ApplicationSettings>();

            // Log parameters
            Console.WriteLine("\n");
            Console.WriteLine("Parameters:");
            Console.WriteLine($"Environment: {HostingEnvironmentHelper.Environment}");
            Console.WriteLine($"RemoteIp: {config.UdpMessageSender.RemoteIp}");
            Console.WriteLine($"RemoteHost: {config.UdpMessageSender.RemoteHost}");
            Console.WriteLine($"RemotePort: {config.UdpMessageSender.RemotePort}");
            Console.WriteLine("\n");

            const int concurrencyLimit = 3;
            var tokenSource = new CancellationTokenSource();

            var task = StartSendingTestMessagesAsync(
                concurrencyLimit, 
                config.UdpMessageSender.RemoteIp, 
                config.UdpMessageSender.RemoteHost, 
                config.UdpMessageSender.RemotePort,
                tokenSource.Token
            );

            // wait for key press
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // cancel and await task
            Console.WriteLine($"\nStopping {concurrencyLimit} tasks...");
            tokenSource.Cancel();
            await task;
            Console.WriteLine("Done.");
        }

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

        private static Task StartSendingTestMessagesAsync(
            int concurrencyLimit, 
            string remoteIp, 
            string remoteHost,
            int remotePort, 
            CancellationToken cancellationToken
        )
        {
            var faker = new Faker();

            return Task.WhenAll(Enumerable.Range(0, concurrencyLimit).Select(async (x, i) =>
            {
                // create UDP client with default remote host specified
                using UdpClient udpClient = new UdpClient();
                if(!string.IsNullOrEmpty(remoteIp))
                {
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
                    udpClient.Connect(remoteIpEndPoint);
                }
                else if(!string.IsNullOrEmpty(remoteHost))
                {
                    udpClient.Connect(remoteHost, remotePort);
                }

                int messageIndex = 0;

                while (true)
                {
                    // stop when cancellation requested
                    if(cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // send another message
                    string message = $"Sender {i}. Message {messageIndex}. {faker.Lorem.Sentence(wordCount: 5)}.";
                    Console.WriteLine($"Sender {i}. Sending message {messageIndex}...");
                    byte[] bytes = Encoding.ASCII.GetBytes(message);
                    udpClient.Send(bytes, bytes.Length);
                    messageIndex += 1;

                    // wait
                    await Task.Delay(5000 + i * 1000);
                }
            }));
        }
    }
}
