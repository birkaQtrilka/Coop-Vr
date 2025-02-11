
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using Coop_Vr.Networking.ClientSide.StateMachine.States;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide.StateMachine
{

    public class ClientStateMachine
    {
        public static MessageSender MessageSender { get; private set; }
        public int ID { get; private set; }

        readonly Dictionary<Type, Room<ClientStateMachine>> _scenes;

        Room<ClientStateMachine> _current;

        bool _changedScene;

        TcpChanel _server;

        bool _canFixedUpdate = true;

        readonly Queue<IMessage> _messageQueue = new();


        public ClientStateMachine()
        {
            _scenes = new()
            {
                { typeof(LobbyView), new LobbyView(this)},
                { typeof(GameView), new GameView(this)}
            };


            _current = _scenes[typeof(LobbyView)];
            _current.OnEnter();
            _ = FixedUpdate();

        }

        public void StopRunning()
        {
            _canFixedUpdate = false;
        }

        public void SetID(int id)
        {
            ID = id;
            MessageSender ??= new(SendMessage, ID);
            Log.Do("Setting ID");
        }

        public void SendMessage(IMessage msg)
        {
            _messageQueue.Enqueue(msg);
        }

        public async Task ConnectToServerAsync(string Ip)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(Ip, 55555);
                _server = new TcpChanel(client);
                Log.Do("Connected to server.");
            }
            catch (Exception e)
            {
                Log.Do(e.Message);
            }
            await Task.Delay(100);
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
            while (_canFixedUpdate)
            {
                await Task.Delay(MySettings.FixedUpdateDelay);

                _current.FixedUpdate();

                while (_messageQueue.Count > 0)
                {
                    var msg = _messageQueue.Dequeue();
                    _server.SendMessage(msg);
                }
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
