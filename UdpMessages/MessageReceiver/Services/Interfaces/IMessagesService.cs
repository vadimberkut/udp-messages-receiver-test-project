using MessageReceiver.Dtos;
using MessageReceiver.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Services.Interfaces
{
    public interface IMessagesService
    {
        Task<IEnumerable<MessageDto>> GetMessagesAsync(string ipAddress, DateTimeOffset? from, DateTimeOffset? to, int? offset, int? limit);
        Task SaveMessageAsync(string ipAddress, int port, string message);
    }
}
