
using Coop_Vr.Networking.ClientSide;
using System;
using System.Collections.Generic;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class LobbyRoom : Room<ServerStateMachine>
    {
        const int MIN_PLAYERS = 1;
        List<TcpChanel> readyPlayers = new();

        public LobbyRoom(ServerStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
            Console.WriteLine("exited lobby room");

        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is PlayerJoinRequest request)
            {
                Console.WriteLine($"PlayerJoinRequest: {sender}, id: {request.ID}");
                readyPlayers.Add(sender);
                if(readyPlayers.Count >= MIN_PLAYERS)
                {
                    Room<ServerStateMachine> gameRoom = context.GetRoom<GameRoom>();
                    foreach (TcpChanel player in readyPlayers)
                    {
                        RemoveMember(player);
                        gameRoom.AddMember(player);
                        player.SendMessage(new PlayerJoinResponse());
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
