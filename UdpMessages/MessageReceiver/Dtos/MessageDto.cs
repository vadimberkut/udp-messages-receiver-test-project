using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Dtos
{
    public class MessageDto
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string SenderIp { get; set; }
        public int SenderPort { get; set; }
    }
}
