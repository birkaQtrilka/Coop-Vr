using StereoKit;
using System;
using System.Net;
using System.Net.Sockets;

namespace Coop_Vr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "Coop_Vr",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            
            Action stepper = new Server().Step;
            //Action stepper = new Client().Step;

            while (SK.Step(() =>
            {
                stepper?.Invoke();
            })) ;
            SK.Shutdown();
        }

        static Action Client()
        {
            //hard code a port that is available
            //var udpServer = bew UdpClient(port);
            //.enable broadcast
            //send message about host existing, 
            //send tcp host port over by finding an available code (withing message of server discovery)
            //build tcp connection between them
            //server is tcpListener, client is tcpClient
            //send data between them

            //other clients must know the port you chose
            //info about local host
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            //local host ip address
            IPAddress ip = ipEntry.AddressList[0];

            IPEndPoint endPoint = new(ip, 1234);

            //create client socket
            using Socket client = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try {
                client.Connect(endPoint);
            }
            catch (Exception)
            {
                Console.WriteLine(" didn't find server");

                return new Server().Step;
            }
            return null;
        }

        
    }
}
