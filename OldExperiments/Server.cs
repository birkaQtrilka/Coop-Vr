using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Coop_Vr
{

    internal class Server
    {
        //info about local host
        //local host ip address
        IPAddress ip;

        IPEndPoint iPEndPoint;
        Action step;

        public Server()
        {
            Console.WriteLine("server created");

            //info about local host
            //local host ip address
            var hostname = Dns.GetHostName();
            var entry = Dns.GetHostEntry(hostname);
            ip = entry.AddressList[1];

            iPEndPoint = new(ip, 50160);
            //create client socket
            using Socket server = new(
                iPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            server.Bind(iPEndPoint);
            server.Listen();
            //String data = null;
            var handler = server.Accept();
            Console.WriteLine("accepted");

            step = () =>
            {
                Byte[] buffer = new Byte[1024];

                int received = handler.Receive(buffer,SocketFlags.None);
                string msg = Encoding.UTF8.GetString(buffer, 0, received);
                //Material.Default.SetColor(Color.Black);
                if (msg != null) {
                    //Console.WriteLine("server received msg: " + msg);
                    if(msg.Contains("num:"))
                    {
                        if(int.Parse(msg.Split(':')[1]) < 5)
                        {
                            byte[] response = Encoding.UTF8.GetBytes("OK");
                            handler.Send(response, SocketFlags.None);

                        }
                        else
                        {
                            byte[] response = Encoding.UTF8.GetBytes("Bad");
                            handler.Send(response, SocketFlags.None);
                        }
                    }
                }
            };
        }

        public void Step()
        {
            step.Invoke();
        }
    }
}
