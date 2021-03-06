using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Entities
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
