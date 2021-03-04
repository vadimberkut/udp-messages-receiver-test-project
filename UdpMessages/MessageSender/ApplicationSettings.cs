using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageSender
{
    public class ApplicationSettings
    {
        public UdpMessageSenderSettings UdpMessageSender { get; set; }
    }

    public class UdpMessageSenderSettings
    {
        public string RemoteIp { get; set; }
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
    }
}
