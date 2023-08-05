using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HW_Server.Entity
{
    public class ConnectedClients
    {
        public TcpClient Client { get; set; }
        public string Nick { get; set; }

        public ConnectedClients(TcpClient client, string nick)
        {
            Client = client;
            Nick = nick;
        }

    }
}
