using Microsoft.Extensions.Hosting;
using Server.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Services
{
    public class MessageHubService : BackgroundService
    {
        private readonly IServerMessageHub _messageHub;

        public MessageHubService(IServerMessageHub messageHub)
        {
            _messageHub = messageHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _messageHub.CheckForNewClientMessage();

                    await Task.Delay(100, stoppingToken);
                }
                catch (Exception e)
                {
                    // Log any errors
                    Console.WriteLine("Error in package service loop: " + e.Message + " " + e.StackTrace);
                }
            }
        }
    }

}
