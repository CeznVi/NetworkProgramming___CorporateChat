using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncClient_004
{
    class ClientState
    {
        public Socket Socket { get; set; }
        public string Message { get; set; }



    }
}
