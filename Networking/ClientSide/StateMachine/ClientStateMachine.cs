
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using Coop_Vr.Networking.ClientSide.StateMachine.States;
using System.Threading.Tasks;
using Coop_Vr.Networking.ServerSide;

namespace Coop_Vr.Networking.ClientSide.StateMachine
{
    public class MessageSender
    {
        public readonly int ID;
        readonly Action<IMessage> _message;

        public MessageSender(Action<IMessage> method, int id)
        {
            _message = method;
            ID = id;
        }

        public void SendMessage(IMessage msg)
        {
            _message?.Invoke(msg);
        }
    }

    public class ClientStateMachine
    {
        public static MessageSender MessageSender { get; private set; }
        public int ID { get; private set; }

        readonly Dictionary<Type, Room<ClientStateMachine>> _scenes;

        Room<ClientStateMachine> _current;

        bool _changedScene;

        TcpChanel _server;

        public ClientStateMachine()
        {
            
            _scenes = new()
            {
                { typeof(LobbyView), new LobbyView(this)},
                { typeof(GameView), new GameView(this)}
            };


            _current = _scenes[typeof(LobbyView)];
            _current.OnEnter();
        }

        public void SetID(int id)
        {
            ID = id;
            MessageSender ??= new(SendMessage, ID);

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
            { 
                _current.ReceiveMessage(_server.GetMessage(), _server);
            }

            
            if (_changedScene)
            {
                _changedScene = false;
                return;
            }

            _current.Update();
        }

        public async Task FixedUpdate()
        {
            while(true)
            {
                await Task.Delay(200);

                _current.FixedUpdate();
            }
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
