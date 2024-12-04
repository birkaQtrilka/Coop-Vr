using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace Coop_Vr.Networking.ServerSide.StateMachine
{

    public class ServerStateMachine
    {
        readonly Dictionary<Type, Room<ServerStateMachine>> _states;

        public Room<ServerStateMachine> CurrentRoom { get; private set; }

        readonly TcpListener _listener;
        bool _changedState;
        bool _canRunFixedUpdate = true;

        public int FixedUpdateDelay = 50;

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

            CurrentRoom = _states[typeof(LobbyRoom)];
            CurrentRoom.OnEnter();
            _ = FixedUpdate();

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
                Console.WriteLine("Accepted new client.");

            }
        }

        public void Update()
        {
            ProcessNewClients();
            
            CurrentRoom.SafeForEachMember((client) =>
            {
                if (!client.HasMessage() || _changedState) return;

                CurrentRoom.ReceiveMessage(client.GetMessage(), client);
            });

            if (_changedState)
            {
                _changedState = false;
                return;
            }

            CurrentRoom.Update();
        }

        public async Task FixedUpdate()
        {
            while (_canRunFixedUpdate)
            {
                await Task.Delay(FixedUpdateDelay);

                CurrentRoom.FixedUpdate();
            }
        }

        public Room<ServerStateMachine> GetRoom<T>() where T : Room<ServerStateMachine>
        {
            return _states[typeof(T)];
        }


        public void ChangeTo<T>()
        {
            _changedState = true;
            Room<ServerStateMachine> newState = _states[typeof(T)];
            CurrentRoom.OnExit();
            CurrentRoom = newState;
            newState.OnEnter();
        }
    }
}
