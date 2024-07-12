using System;
using System.Threading.Tasks;

namespace Client.Interfaces
{
    public interface IClientMessageHub
    {
        Task SendToServerMessage(object message, Guid correlationId);
        Task<TResponse> ReceiveFromServerMessage<TResponse>(Guid correlationId);
    }
}
