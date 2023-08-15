using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HW_Server.Entity
{
    class ServerContext
    {
        public string? nickname;
        public TcpClient Client;
        public Stream Stream;
        public byte[] Buffer = new byte[1024];
    }
}
