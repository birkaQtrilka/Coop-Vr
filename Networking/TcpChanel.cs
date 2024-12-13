using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Coop_Vr.Networking
{
    public class TcpChanel
    {
        const int HEADER_SIZE = 4;
        readonly byte[] sizeHeader = new byte[HEADER_SIZE];

        readonly TcpClient _client;

        public TcpChanel(TcpClient client)
        {
            _client = client;
        }

        public void SendMessage(IMessage msg)
        {
            Log.Do("Sending:" + msg);
            Packet outPacket = new();
            outPacket.Write(msg);
            Log.Do("Bytes: " + outPacket.GetBytes().Length);
            StreamUtil.Write(_client.GetStream(), outPacket.GetBytes());
        }

        public IMessage GetMessage()
        {
            byte[] inBytes = StreamUtil.Read(_client.GetStream());
            Packet inPacket = new(inBytes);
            ISerializable inObject = inPacket.ReadObject();
            Log.Do("receiving " + inObject.ToString());
            if (inObject is IMessage msg) return msg;

            Console.WriteLine("received packet with wrong format (not inheriting IMessage)");
            return null;
        }

        //public bool HasMessage()
        //{
        //    if (_client.Available < HEADER_SIZE) return false;
        //    _client.Client.Receive(sizeHeader, HEADER_SIZE, SocketFlags.Peek);
        //    int messageSize = BitConverter.ToInt32(sizeHeader, 0);
        //    return _client.Available >= HEADER_SIZE + messageSize;
        //}

        public bool HasMessage()
        {
            return _client.Available >= HEADER_SIZE;
        }
    }
}
