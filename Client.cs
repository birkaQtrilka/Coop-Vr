using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr
{
    internal class Client
    {
        //info about local host
        IPHostEntry ipEntry;
        //local host ip address
        IPAddress ip;

        IPEndPoint iPEndPoint;
        Action step;

        public Client()
        {
            Console.WriteLine("Client trying to connect");

            //info about local host
            ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            //local host ip address
            ip = ipEntry.AddressList[0];

            iPEndPoint = new(ip, 1234);
            //create client socket
            using Socket client = new(
                iPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            client.Connect(iPEndPoint);

            //client.Bind(iPEndPoint);
            //client.Listen();
            Byte[] bytes = Encoding.UTF8.GetBytes("client message");
            //String data = null;
            Console.WriteLine("accepted");

            step = () =>
            {
                Console.WriteLine("client sending message!");
                client.Send(bytes, SocketFlags.None);

                byte[] buffer = new byte[1024];

                int reveived = client.Receive(buffer,SocketFlags.None);
                Console.WriteLine("client received message: " + Encoding.UTF8.GetString(buffer)); 
            };
        }

        public void Step()
        {
            step.Invoke();
        }
    }
}
