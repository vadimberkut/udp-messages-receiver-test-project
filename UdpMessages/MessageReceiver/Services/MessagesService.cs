using LinqKit;
using MessageReceiver.Contexts;
using MessageReceiver.Dtos;
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

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string ipAddress, DateTimeOffset? from, DateTimeOffset? to, int? offset, int? limit)
        {
            offset = offset ?? 0;
            limit = limit ?? 100;

            var query = _dbContext.Messages
                .OrderByDescending(x => x.CreatedAt) // NB: SQLite can't order by DateTimeOffset if it's stored as TEXT
                .Join(
                    _dbContext.Senders,
                    message => message.SenderId,
                    sender => sender.Id,
                    (message, sender) => new
                    {
                        Message = message,
                        Sender = sender,
                    }
                 );

            if (!string.IsNullOrEmpty(ipAddress))
            {
                query = query.Where(x => x.Sender.IpAddress == ipAddress);
            }

            if (from != null && to != null)
            {
                query = query.Where(x => x.Message.CreatedAt >= from.Value.ToUniversalTime() && x.Message.CreatedAt < to.Value.ToUniversalTime());
            }

            var queryResult = await query
                .Skip(offset.Value)
                .Take(limit.Value)
                .ToListAsync();

            var result = queryResult.Select(x => new MessageDto()
            {
                Id = x.Message.Id,
                Message = x.Message.Message,
                CreatedAt = x.Message.CreatedAt,
                SenderIp = x.Sender.IpAddress,
                SenderPort = x.Sender.Port,
            });
            return result;
        }

        public async Task SaveMessageAsync(string ipAddress, int port, string message)
        {
            // find or create sender
            var senderEntity = await _dbContext.Senders.FirstOrDefaultAsync(x => x.IpAddress == ipAddress && x.Port == port);
            if (senderEntity == null)
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
        }
    }
}
