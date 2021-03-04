using MessageReceiver.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageReceiver.HostedServices
{
    public class UdpMessageReceiverHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<UdpMessageReceiverHostedService> _logger;
        private readonly UdpMessageReceiverSettings _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly UdpClient _udpClient;
        private IAsyncResult _asyncResult;

        public UdpMessageReceiverHostedService(
            ILogger<UdpMessageReceiverHostedService> logger,
            IOptions<UdpMessageReceiverSettings> config    ,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _config = config.Value;
            _serviceProvider = serviceProvider;
            _udpClient = new UdpClient(_config.ListenPort);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting...");
            StartListening();
            _logger.LogInformation("Started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping...");
            _udpClient.Close();
            _logger.LogInformation("Stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }

        private void StartListening()
        {
            // receive in async manner
            _asyncResult = _udpClient.BeginReceive(Receive, new object());

            // receive in sync manner (blocking the method)
            //IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //byte[] receivedBytes = udpClient.Receive(ref remoteIpEndPoint);
            //string receivedMessage = Encoding.ASCII.GetString(receivedBytes);
            //Console.WriteLine("Received message from {0}: {1} ", remoteIpEndPoint.Address.ToString(), receivedMessage);
            //StartListening();
        }

        /// <summary>
        /// Handles received datagram
        /// </summary>
        private void Receive(IAsyncResult asyncResult)
        {
            try
            {
                // allow to read datagrams sent from any source
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, _config.ListenPort);

                byte[] bytes = _udpClient.EndReceive(asyncResult, ref remoteIpEndPoint);
                string message = Encoding.ASCII.GetString(bytes);
                _logger.LogInformation($"Received message from {remoteIpEndPoint.Address.ToString()}:{remoteIpEndPoint.Port}: {message}.");

                // save message
                using (var scope = _serviceProvider.CreateScope())
                {
                    var messagesService = scope.ServiceProvider.GetRequiredService<IMessagesService>();
                    messagesService.SaveMessageAsync(
                        remoteIpEndPoint.Address.ToString(),
                        remoteIpEndPoint.Port,
                        message
                    ).GetAwaiter().GetResult();
                }

                // continue receiving
                StartListening();
            }
            catch(ObjectDisposedException ex)
            {
                // ignore if disposed but log
                _logger.LogError(ex, "UdpClient disposed error.");
            }
        }
    }
}
