using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Entities
{
    public class SenderEntity : BaseEntity
    {
        public string IpAddress { get; set; }

        public virtual IList<SenderEntity> Messages { get; set; }
    }
}
