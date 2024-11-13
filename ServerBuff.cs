using Coop_Vr.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Coop_Vr
{
    public class ServerBuff
    {
        TcpListener _listener;
        List<TcpClient> _clients = new();
        List<Score> _scores = new();
        Action _step;

        public ServerBuff()
        {
            Console.WriteLine("Server started on port 55555");

            _listener = new TcpListener(IPAddress.Any, 55555);
            _listener.Start();
        }

        public void Step()
        {
            _step?.Invoke();
            ProcessNewClients();
            ProcessExistingClients();

            //Although technically not required, now that we are no longer blocking, 
            //it is good to cut your CPU some slack
            Thread.Sleep(100);
        }

        void ProcessNewClients()
        {
            while (_listener.Pending())
            {
                _clients.Add(_listener.AcceptTcpClient());
                Console.WriteLine("Accepted new client.");
            }
        }

        void ProcessExistingClients()
        {
            foreach (TcpClient client in _clients)
            {
                if (client.Available == 0) continue;

                byte[] inBytes = StreamUtil.Read(client.GetStream());
                Packet inPacket = new Packet(inBytes);
                ISerializable inObject = inPacket.ReadObject();
                Console.WriteLine("Received:" + inObject);

                if (inObject is AddRequestExample addRequest) { HandleAddRequest(client, addRequest); }
                else if (inObject is GetRequestExample getRequest) { HandleGetRequest(client, getRequest); }
            }
        }

        void HandleAddRequest(TcpClient pClient, AddRequestExample pAddRequest)
        {
            _scores.Add(pAddRequest.score);
        }

        void HandleGetRequest(TcpClient pClient, GetRequestExample pGetRequest)
        {
            //construct a reply 
            GetScoresExample highscoreUpdate = new();
            highscoreUpdate.scores = _scores;
            SendObject(pClient, highscoreUpdate);
        }

        void SendObject(TcpClient pClient, ISerializable pOutObject)
        {
            Console.WriteLine("Sending:" + pOutObject);
            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);
            StreamUtil.Write(pClient.GetStream(), outPacket.GetBytes());
        }
    }
}
