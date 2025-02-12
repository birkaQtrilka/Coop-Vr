using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Coop_Vr.Networking.Messages;
using StereoKit;

namespace Coop_Vr.Networking.ServerSide.StateMachine
{
    class ClientHeart
    {
        public readonly TcpChanel Client;
        public double LastHeartBeat;

        public ClientHeart(TcpChanel client, double lastHeartBeat)
        {
            Client = client;
            LastHeartBeat = lastHeartBeat;
        }
    }

    public class ServerStateMachine
    {
        readonly Dictionary<Type, Room<ServerStateMachine>> _states;

        public Room<ServerStateMachine> CurrentRoom { get; private set; }

        readonly TcpListener _listener;
        bool _changedState;
        bool _canRunFixedUpdate = true;

        readonly List<ClientHeart> _clientHearts = new();
        Queue<IMessage> _messageQueue = new();
        Queue<Action> _afterFixedUpdate = new();

        
        public ServerStateMachine()
        {
            Log.Do("Server started on port 55555");

            _listener = new TcpListener(IPAddress.Any, 55555);
            _listener.Start();

            _states = new()
            {
                { typeof(LobbyRoom), new LobbyRoom(this)},
                { typeof(GameRoom), new GameRoom(this)}
            };

            CurrentRoom = _states[typeof(LobbyRoom)];
            CurrentRoom.OnEnter();
            _ = FixedUpdate();
            _ = HeartBeatLoop();

        }

        public void StopRunning()
        {
            _canRunFixedUpdate = false ;
        }

        void ProcessNewClients()
        {
            while (_listener.Pending())
            {
                var newClient = new TcpChanel(_listener.AcceptTcpClient());
                _states[typeof(LobbyRoom)].AddMember(newClient);
                Log.Do("Accepted new client.");
                //there is no client removing system
                _clientHearts.Add(new ClientHeart(newClient, Time.Total));
            }
        }

        public void Update()
        {
            ProcessNewClients();
            
            CurrentRoom.SafeForEachMember((client) =>
            {
                if (!client.HasMessage() || _changedState) return;

                IMessage message = client.GetMessage();
                if(message is HeartBeat heartBeat)
                {
                    ClientHeart clientHeart = _clientHearts.Find(h => h.Client == client);
                    clientHeart.LastHeartBeat = Time.Total;
                }
                else
                    CurrentRoom.ReceiveMessage(message, client);
            });

            if (_changedState)
            {
                _changedState = false;
                return;
            }

            CurrentRoom.Update();
        }

        public void SendMessage(IMessage msg)
        {
            _messageQueue.Enqueue(msg);
        }

        async Task FixedUpdate()
        {
            while (true)
            {
                await Task.Delay(MySettings.FixedUpdateDelay);

                CurrentRoom.FixedUpdate();

                while (_afterFixedUpdate.Count > 0)
                {
                    var action = _afterFixedUpdate.Dequeue();
                    try
                    {
                        action();

                    }
                    catch (Exception ex)
                    {
                        Log.Do("AAAAAAAAAAAAAAAaa");
                    }
                }

                while(_messageQueue.Count > 0)
                {
                    var msg = _messageQueue.Dequeue();
                    CurrentRoom.SafeForEachMember(c => c.SendMessage(msg));
                }

            }
        }

        async Task HeartBeatLoop()
        {
            while(_canRunFixedUpdate)
            {
                await Task.Delay(MySettings.HeartBeatDelay);

                if (_clientHearts.Count == 0) continue;

                HeartBeat heartBeatMsg = new();
                Log.Enabled = false;
                
                SendMessage(heartBeatMsg);

                Log.Enabled = true;
                CheckClientHeart();
            }
        }

        void CheckClientHeart()
        {
            double currentTime = Time.Total;
            for (int i = 0; i < _clientHearts.Count; i++)
            {
                ClientHeart client = _clientHearts[i];

                bool overdueHeartBeat = currentTime - client.LastHeartBeat > MySettings.MaxTimeForHeartBeat;
                bool clientedDisconnected = !client.Client.Connected;

                if (overdueHeartBeat || clientedDisconnected)
                {
                    _clientHearts.RemoveAt(i);
                    ResetServer();
                    return;
                }
                else
                    client.LastHeartBeat = currentTime;
            }
        }

        void ResetServer()
        {
            Log.Do("Reset!!!!!!!!!!!");
            SendMessage(new ResetMsg());
            //forgeting to re add members

            var referenceCopy = CurrentRoom;//gameRoom
            NextAction(() =>
            {
                ChangeTo(typeof(LobbyRoom));

                NextAction(() =>
                referenceCopy.SafeForEachMember(client =>
                {
                    referenceCopy.RemoveMember(client);
                }));


                NextAction(() =>
                    CurrentRoom.SafeForEachMember(client =>
                    {
                        CurrentRoom.AddMember(client);
                    })
                );

            });

            

        }

        void NextAction(Action action)
        {
            _afterFixedUpdate.Enqueue(action);
        }

        public Room<ServerStateMachine> GetRoom<T>() where T : Room<ServerStateMachine>
        {
            return _states[typeof(T)];
        }


        public void ChangeTo<T>()
        {
            _afterFixedUpdate.Enqueue(()=> ChangeTo(typeof(T)));
        }

        void ChangeTo(Type room)
        {
            _changedState = true;
            Room<ServerStateMachine> newState = _states[room];
            CurrentRoom.OnExit();
            CurrentRoom = newState;
            CurrentRoom.OnEnter();
        }
    }
}
