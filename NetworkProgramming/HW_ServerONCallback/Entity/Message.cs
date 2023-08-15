using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HW_Server.Entity
{
    enum MessageType
    {
        LOGIN,
        MESSAGE
    }

    [Serializable]
    class Message
    {
        public MessageType MT;

        [OptionalField]
        public string? payload;
        [OptionalField]
        public string? channel;
    }
}
