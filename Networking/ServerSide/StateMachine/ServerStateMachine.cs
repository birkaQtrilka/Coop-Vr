using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Coop_Vr.Networking.Messages;
using StereoKit;
using System.Collections.Concurrent;
using Coop_Vr.Networking.ClientSide.StateMachine;

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
        double _lastHeartBeat;

        readonly List<ClientHeart> _clientHearts = new();
        readonly ConcurrentQueue<IMessage> _messageQueue = new();
        readonly ConcurrentQueue<Action> _afterFixedUpdate = new();
        double _lastFixedUpdate;
        public static MessageSender MessageSender { get; private set; }


        public ServerStateMachine()
        {
            Log.Do("Server started on port 55555");
            MessageSender ??= new(SendMessage, -1);

            _listener = new TcpListener(IPAddress.Any, 55555);
            _listener.Start();

            _states = new()
            {
                { typeof(LobbyRoom), new LobbyRoom(this)},
                { typeof(GameRoom), new GameRoom(this)}
            };

            CurrentRoom = _states[typeof(LobbyRoom)];
            CurrentRoom.OnEnter();

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
                _clientHearts.Add(new ClientHeart(newClient, Time.Total));
                _states[typeof(LobbyRoom)].AddMember(newClient);
                Log.Do("Accepted new client.");
                //there is no client removing system
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
            try
            {
                HeartBeatLoop();
            }
            catch (Exception ex)
            {
                Log.Do(ex);
            }
            FixedUpdate();

        }

        void HeartBeatLoop()
        {
            if (_clientHearts.Count == 0 || Time.Total - _lastHeartBeat < MySettings.HeartBeatDelay) return;
            _lastHeartBeat = Time.Total;

            HeartBeat heartBeatMsg = new();

            SendMessage(heartBeatMsg);
            //Log.Do("HearBeat");
            CheckClientHeart();
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
            //the message will happen after removing and adding clients to the new room
            //I need to skip a frame, so the message is first sent to the clients of current room, then move the clients
            //into the lobby where there can potentially be more clients
            _afterFixedUpdate.Enqueue(() =>
                _afterFixedUpdate.Enqueue(() =>
                {
                    ChangeTo(typeof(LobbyRoom), true);
                })
            );
        }

        public void SendMessage(IMessage msg)
        {
            _messageQueue.Enqueue(msg);
        }
        

        void FixedUpdate()
        {
            if (Time.Total - _lastFixedUpdate < 0.05f) return;
            _lastFixedUpdate = Time.Total;
            try
            {
                CurrentRoom.FixedUpdate();

                while (!_afterFixedUpdate.IsEmpty)
                {
                    if (!_afterFixedUpdate.TryDequeue(out var action)) continue;
                    action();

                }

                while (!_messageQueue.IsEmpty)
                {
                    if (!_messageQueue.TryDequeue(out var msg)) continue;

                    CurrentRoom.SafeForEachMember(c => { if (c.Connected) c.SendMessage(msg); });
                }
            }
            catch (Exception ex)
            {
                Log.Do(ex);
            }
            
        }

        public Room<ServerStateMachine> GetRoom<T>() where T : Room<ServerStateMachine>
        {
            return _states[typeof(T)];
        }


        public void ChangeTo<T>()
        {
            _afterFixedUpdate.Enqueue(()=> ChangeTo(typeof(T)));
        }

        void ChangeTo(Type room, bool moveClients = false)
        {
            _changedState = true;
            Room<ServerStateMachine> newState = _states[room];
            CurrentRoom.OnExit();

            if(moveClients)
                CurrentRoom.SafeForEachMember(client =>
                {
                    newState.AddMember(client);
                    CurrentRoom.RemoveMember(client);

                });

            CurrentRoom = newState;
            CurrentRoom.OnEnter();
        }

        
    }
}
