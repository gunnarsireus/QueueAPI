using System.Threading.Tasks;

namespace Server.Interfaces
{
    public interface IServerMessageHub
    {
        Task CheckForNewClientMessage();
    }
}
