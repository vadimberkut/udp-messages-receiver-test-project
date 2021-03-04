using MessageReceiver.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Services.Interfaces
{
    public interface IMessagesService
    {
        Task<IEnumerable<MessageEntity>> GetMessageAsync(string ipAddress, DateTimeOffset from, DateTimeOffset to);
        Task<MessageEntity> SaveMessageAsync(string ipAddress, int port, string message);
    }
}
