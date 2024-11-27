
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using Coop_Vr.Networking.ClientSide.StateMachine.States;
using System.Threading.Tasks;

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
            _scenes = new()
            {
                { typeof(LoginView), new LoginView(this)},
                { typeof(LobbyView), new LobbyView(this)},
                { typeof(GameView), new GameView(this)}
            };


            _current = _scenes[typeof(LobbyView)];
            _current.OnEnter();
        }

        public void SendMessage(IMessage msg)
        {
            _server.SendMessage(msg);
        }

        public void ConnectToServer(string Ip)
        {
            try
            {
                var client = new TcpClient();
                client.Connect(Ip, 55555);
                _server = new TcpChanel(client);
                Console.WriteLine("Connected to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task ConnectToServerAsync(string Ip)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(Ip, 55555);
                _server = new TcpChanel(client);
                Console.WriteLine("Connected to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            await Task.Delay(10);
        }

        public void Update()
        {

            if (_server != null && _server.HasMessage()) 
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
