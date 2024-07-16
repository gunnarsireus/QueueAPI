using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Repositories;

public interface IQueueRepository
{
    Task<ClientQueueEntity> GetMessageFromClientQueue();
    Task<ServerQueueEntity> GetMessageFromServerQueueByCorrelationId(Guid destinationServerQueueItemId);
    Task<int> AddClientQueueItemAsync(ClientQueueEntity entity);
    Task<int> AddServerQueueItemAsync(ServerQueueEntity entity);
}
