using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace Coop_Vr.Networking.ServerSide.StateMachine
{

    public class ServerStateMachine
    {
        readonly Dictionary<Type, Room<ServerStateMachine>> _states;

        Room<ServerStateMachine> _current;

        readonly TcpListener _listener;
        bool _changedState;

        public ServerStateMachine()
        {
            Console.WriteLine("Server started on port 55555");

            _listener = new TcpListener(IPAddress.Any, 55555);
            _listener.Start();

            _states = new()
            {
                { typeof(LobbyRoom), new LobbyRoom(this)},
                { typeof(GameRoom), new GameRoom(this)}
            };

            _current = _states[typeof(LobbyRoom)];
            _current.OnEnter();
        }

        void ProcessNewClients()
        {
            while (_listener.Pending())
            {
                var newClient = new TcpChanel(_listener.AcceptTcpClient());
                _states[typeof(LobbyRoom)].AddMember(newClient);
                Console.WriteLine("Accepted new client.");

            }
        }



        public void Update()
        {
            ProcessNewClients();
            
            _current.SafeForEachMember((client) =>
            {
                if (!client.HasMessage() || _changedState) return;

                _current.ReceiveMessage(client.GetMessage(), client);
            });

            if (_changedState)
            {
                _changedState = false;
                return;
            }

            _current.Update();
        }


        public Room<ServerStateMachine> GetRoom<T>() where T : Room<ServerStateMachine>
        {
            return _states[typeof(T)];
        }

        public void ChangeTo<T>()
        {
            _changedState = true;
            Room<ServerStateMachine> newState = _states[typeof(T)];
            _current.OnExit();
            _current = newState;
            newState.OnEnter();
        }
    }
}
