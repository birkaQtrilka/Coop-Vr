
using Coop_Vr.Networking.ServerSide.StateMachine.States;
using Coop_Vr.Networking.ServerSide.StateMachine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using Coop_Vr.Networking.ClientSide.StateMachine.States;
using System.Diagnostics;

namespace Coop_Vr.Networking.ClientSide.StateMachine
{
    public class ClientStateMachine
    {
        readonly Dictionary<Type, Room<ClientStateMachine>> _scenes;

        Room<ClientStateMachine> _current;

        bool _changedScene;

        TcpChanel _server;

        public ClientStateMachine()
        {
            ConnectToServer();

            _scenes = new()
            {
                { typeof(LoginView), new LoginView(this)},
                { typeof(LobbyView), new LobbyView(this)},
                { typeof(GameView), new GameView(this)}
            };


            _current = _scenes[typeof(LoginView)];
            _current.OnEnter();
        }

        void ConnectToServer()
        {
            try
            {
                var client = new TcpClient();
                client.Connect("localHost", 55555);
                _server = new TcpChanel(client);
                Console.WriteLine("Connected to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Update()
        {
            if (_server.HasMessage()) 
                _current.ReceiveMessage(_server.GetMessage(), _server);

            
            if (_changedScene)
            {
                _changedScene = false;
                return;
            }

            _current.Update();
        }

        public void ChangeTo<T>()
        {
            _changedScene = true;
            var newState = _scenes[typeof(T)];
            _current.OnExit();
            _current = newState;
            newState.OnEnter();
        }
    }
}
