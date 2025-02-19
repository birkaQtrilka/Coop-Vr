
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using Coop_Vr.Networking.ClientSide.StateMachine.States;
using System.Threading.Tasks;
using Coop_Vr.Networking.ServerSide;
using Coop_Vr.Networking.Messages;
using Coop_Vr.Networking.ServerSide.StateMachine;
using StereoKit;

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

        readonly Queue<IMessage> _mesageQueue = new();
        public bool IsConnected => _server.Connected;
        double _lastHeartBeat;
        double _lastFixedUpdate;

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

        public async Task ConnectToServerAsync(string Ip)
        {
            while (_server == null)
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
        }

        public void Update()
        {
            if (_server != null && _server.HasMessage())
            {
                IMessage msg = _server.GetMessage();
                if (msg is HeartBeat beat)
                {
                    _lastHeartBeat = Time.Total;
                    SendMessage(beat);
                }
                else if (msg is ResetMsg)
                {
                    Disconnect();
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

            HeartBeatLoop();
            FixedUpdate();

        }

        public void FixedUpdate()
        {
            if (Time.Total - _lastFixedUpdate < 0.05f) return;
            _lastFixedUpdate = Time.Total;

            try
            {
                if (_changedScene)
                {
                    _changedScene = false;
                    return;
                }

                _current.FixedUpdate();
                if (_server == null || !IsConnected) return;

                while (_mesageQueue.Count > 0)
                    _server.SendMessage(_mesageQueue.Dequeue());
            }
            catch (Exception e)
            {
                Log.Do(e);
            }
            
        }
        

        void HeartBeatLoop()
        {
            if(_server == null)
            {
                _lastHeartBeat = Time.Total;
                return;
            }

            if (Time.Total - _lastHeartBeat < MySettings.MaxTimeForHeartBeat) return;
            _lastHeartBeat = Time.Total;
            Disconnect();
        }

        public void Disconnect()
        {
            _server = null;
            ChangeTo<LobbyView>();
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
