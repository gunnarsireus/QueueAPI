using Client.Interfaces;
using Shared.Helpers;
using Shared.Models;
using Shared.Repositories;
using System;
using System.Threading.Tasks;

namespace Client.Hub
{
    public class ClientMessageHub:IClientMessageHub
    {
        private readonly IQueueRepository _queueRepository;

        public ClientMessageHub(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        public async Task SendToServerMessage(object message, Guid correlationId)
        {
            var result = Helpers.ConvertObjectToJson(message);
            var entity = new ClientQueueEntity
            {
                CorrelationId = correlationId,
                Content = result.Item1,
                TypeName = result.Item2.ToString(),
                Created = DateTime.Now,
                StatusDate = DateTime.Now,
                QueueStatus = QueueStatus.New
            };

            await _queueRepository.AddClientQueueItemAsync(entity);
        }

        public async Task<TResponse> ReceiveFromServerMessage<TResponse>(Guid correlationId)
        {
            var response = await ReceiveServerMessage(correlationId);
            return (TResponse)Helpers.ConvertJsonToObject(response.Content, Helpers.GetType(response.TypeName));
        }

        private async Task<ServerQueueEntity> ReceiveServerMessage(Guid correlationId)
        {
            var response = await _queueRepository.GetMessageFromServerQueueByCorrelationId(correlationId);
            while (response == null)
            {
                await Task.Delay(100);
                response = await _queueRepository.GetMessageFromServerQueueByCorrelationId(correlationId);
            }

            return response;
        }
    }
}
