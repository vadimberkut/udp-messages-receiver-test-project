using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Dtos
{
    public class SenderDto
    {
        public string Id { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

    }
}
