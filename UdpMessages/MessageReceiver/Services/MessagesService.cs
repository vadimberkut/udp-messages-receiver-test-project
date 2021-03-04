using MessageReceiver.Contexts;
using MessageReceiver.Entities;
using MessageReceiver.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly ApplicationDbContext _dbContext;

        public MessagesService(
            ApplicationDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<MessageEntity>> GetMessageAsync(string ipAddress, DateTimeOffset from, DateTimeOffset to)
        {
            _dbContext.Messages
                .Join(
                    _dbContext.Senders,
                    message => message.SenderId,
                    sender => sender.Id,
                    (message, sender) => new
                    {
                        Message = message,
                        Sender = sender,
                    }
                 )
                .Where(x => x.Sender.IpAddress == ipAddress && )

            throw new NotImplementedException();
        }

        public async Task<MessageEntity> SaveMessageAsync(string ipAddress, int port, string message)
        {
            // find or create sender
            var senderEntity = await _dbContext.Senders.FirstOrDefaultAsync(x => x.IpAddress == ipAddress && x.Port == port);
            if(senderEntity == null)
            {
                senderEntity = new SenderEntity()
                {
                    IpAddress = ipAddress,
                    Port = port,
                };

                _dbContext.Senders.Add(senderEntity);

                await _dbContext.SaveChangesAsync();
            }

            // add message
            var entity = new MessageEntity()
            {
                Message = message,
                Sender = senderEntity,
            };
            _dbContext.Messages.Add(entity);

            await _dbContext.SaveChangesAsync();

            return entity;
        }
    }
}
