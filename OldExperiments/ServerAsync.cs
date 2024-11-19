using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr
{
    internal class ServerAsync
    {
        IPAddress ip;

        IPEndPoint iPEndPoint;
        Func<Task> step;

        public ServerAsync()
        {
            _ = Init();
        }

        async Task Init()
        {
            var hostname = Dns.GetHostName();
            var entry =  await Dns.GetHostEntryAsync(hostname);
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

            Task<Socket> acceptanceTask = server.AcceptAsync();

            step = async () =>
            {
                if (!acceptanceTask.IsCompleted) return;

                var handler = acceptanceTask.Result;

                Byte[] buffer = new Byte[1024];
                int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                string msg = Encoding.UTF8.GetString(buffer, 0, received);
                
                if (string.IsNullOrEmpty(msg)) return;
                
                Console.WriteLine("server received msg: " + msg);

                await HandleMessage(msg, handler);
            };
            //purely for debugging
            while(!acceptanceTask.IsCompleted)
            {
                await Task.Delay(200);
                Console.WriteLine("Searching for Client!");

            }
        }

        async Task HandleMessage(string msg, Socket handler)
        {
            if (msg.Contains("num:"))
            {
                string responseStr = int.Parse(msg.Split(':')[1]) < 5 ? "OK" : "Bad";

                byte[] response = Encoding.UTF8.GetBytes(responseStr);
                await handler.SendAsync(response, SocketFlags.None);
                
            }
        }

        public void Step()
        {
            _ = step?.Invoke();
        }
    }
}
