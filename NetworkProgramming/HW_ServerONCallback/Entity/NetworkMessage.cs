using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HW_Server.Entity
{
    delegate void NetworkHandlerFunc(NetworkMessage message, NetworkContext context);

    enum NetworkMessageType
    {
        LOGIN,
        REJECT_LOGIN,
        LOGOUT,
        NEW_MESSAGE,
        UPDATE_USER_STATUS
    }


    [Serializable]
    class NetworkMessage
    {
        public NetworkMessage(NetworkMessageType MT, string? Owner = null, string? MessageText = null, string? Channel = null, List<string>? Users = null)
        {
            this.MT = MT;
            this.Owner = Owner;
            this.MessageText = MessageText;
            this.Channel = Channel;
            this.Users = Users;
        }

        public NetworkMessageType MT;
        [OptionalField]
        public string? Owner;

        [OptionalField]
        public string? MessageText;
        [OptionalField]
        public string? Channel;

        [OptionalField]
        public List<string>? Users;

        public static NetworkMessage FromBinary(byte[] Buffer)
        {
            MemoryStream memStream = new MemoryStream(Buffer);
            BinaryFormatter formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Danger: BinaryFormatter.Deserialize is insecure for untrusted input
            return (NetworkMessage)formatter.Deserialize(memStream);
#pragma warning restore SYSLIB0011
        }

        public static MemoryStream ToStream(NetworkMessage message)
        {
            BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
            MemoryStream memStream = new MemoryStream();
            formatter.Serialize(memStream, message);
#pragma warning restore SYSLIB0011

            return memStream;
        }
    }

}
