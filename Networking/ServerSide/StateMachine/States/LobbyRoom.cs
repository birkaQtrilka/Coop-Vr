using System.Collections.Generic;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class LobbyRoom : Room<ServerStateMachine>
    {
        const int MIN_PLAYERS = 2;
        readonly List<TcpChanel> _readyPlayers = new();
        static int playerId;

        public LobbyRoom(ServerStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {
            Log.Do("entered lobby room");
        }

        public override void OnExit()
        {
            Log.Do("exited lobby room");
            _readyPlayers.Clear();

        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is PlayerJoinRequest)
            {
                _readyPlayers.Add(sender);
                Log.Do($"PlayerJoinRequest: {sender}");

                if (_readyPlayers.Count >= MIN_PLAYERS)
                {
                    Room<ServerStateMachine> gameRoom = context.GetRoom<GameRoom>();
                    foreach (TcpChanel player in _readyPlayers)
                    {
                        RemoveMember(player);
                        gameRoom.AddMember(player);
                        player.SendMessage(new PlayerJoinResponse() { ID = playerId++ });
                    }
                    context.ChangeTo<GameRoom>();
                }
            }
        }

        public override void Update()
        {

        }
    }
}
