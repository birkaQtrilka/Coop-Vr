
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using Coop_Vr.Networking.ClientSide.StateMachine.States;
using System.Threading.Tasks;
using Coop_Vr.Networking.ServerSide;
using Coop_Vr.Networking.Messages;

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

        bool _canFixedUpdate = true;

        Queue<IMessage> _mesageQueue = new();

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
            //Console.WriteLine(msg.ToString());
            _mesageQueue.Enqueue(msg);
        }

        public void ConnectToServer(string Ip)
        {
            try
            {
                var client = new TcpClient();
                client.Connect(Ip, 55555);
                _server = new TcpChanel(client);
                Log.Do("Connected to server.");
            }
            catch (Exception e)
            {
                Log.Do(e.Message);
            }
        }

        public async Task ConnectToServerAsync(string Ip)
        {
            //while(_server == null)
            //{
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
            //}
            await Task.Delay(100);
        }

        public void Update()
        {

            if (_server != null && _server.HasMessage())
            {
                IMessage msg = _server.GetMessage();
                if (msg is HeartBeat beat)
                {
                    SendMessage(beat);
                }
                else if (msg is ResetMsg)
                {
                    //ResetClient();
                    Log.Do("Reset!!");
                }
                else
                    _current.ReceiveMessage(msg, _server);
            }


            if (_changedScene)
            {
                _changedScene = false;
                return;
            }

            _current.Update();
        }

        void ResetClient()
        {
            _current.SafeForEachMember(member =>
            {
                _current.RemoveMember(member);
            });
            ChangeTo<LobbyView>();
        }

        public async Task FixedUpdate()
        {
            while (_canFixedUpdate)
            {
                await Task.Delay(MySettings.FixedUpdateDelay);

                _current.FixedUpdate();

                while (_mesageQueue.Count > 0)
                    _server.SendMessage(_mesageQueue.Dequeue());

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
