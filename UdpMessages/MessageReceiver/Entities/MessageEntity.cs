using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Entities
{
    public class MessageEntity : BaseEntity
    {
        public string SenderId { get; set; }
        public string Message { get; set; }

        public virtual SenderEntity Sender { get; set; }
    }
}
