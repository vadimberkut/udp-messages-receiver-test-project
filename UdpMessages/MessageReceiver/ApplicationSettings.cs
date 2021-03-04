using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver
{
    public class ApplicationSettings
    {
        public UdpMessageReceiverSettings UdpMessageReceiver { get; set; }
        public ConnectionStringsSettings ConnectionStrings { get; set; }
    }

    public class UdpMessageReceiverSettings
    {
        public int ListenPort { get; set; }
    }

    public class ConnectionStringsSettings
    {
        public string ApplicationDbContext { get; set; }
    }
}
