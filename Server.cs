using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr
{

    internal class Server
    {
        //info about local host
        IPHostEntry ipEntry;
        //local host ip address
        IPAddress ip;

        IPEndPoint iPEndPoint;
        Action step;

        public Server()
        {
            Console.WriteLine("server created");

            //info about local host
            ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            //local host ip address
            ip = ipEntry.AddressList[0];

            iPEndPoint = new(ip, 1234);
            //create client socket
            using Socket server = new(
                iPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            server.Bind(iPEndPoint);
            server.Listen();
            Byte[] bytes = new Byte[256];
            //String data = null;
            var handler = server.Accept();
            Console.WriteLine("accepted");

            step = () =>
            {
                int received = handler.Receive(bytes,SocketFlags.None);
                string msg = Encoding.UTF8.GetString(bytes, 0, received);

                if (msg != null) {
                    Console.WriteLine("server received msg: " + msg);

                    byte[] response = Encoding.UTF8.GetBytes("OK");
                    handler.Send(response,SocketFlags.None);
                }
            };
        }

        public void Step()
        {
            step.Invoke();
        }
    }
}
