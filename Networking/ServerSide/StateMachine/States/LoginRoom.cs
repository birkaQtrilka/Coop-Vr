using Coop_Vr.Networking.ClientSide;
using System;
using System.Net.Sockets;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class LoginRoom : Room<ServerStateMachine>
    {
        //struct playerJoinData
        //{
        //    int id;
        //    bool _isReady;
        //}

        //const int minPlayerCount = 2;

        public LoginRoom(ServerStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if(message is PlayerJoinRequest)
            {
                RemoveMember(sender);
                context.ChangeTo<LobbyRoom>();
                
                sender.SendMessage(new PlayerJoinResponse());
            }

        }

        public override void Update()
        {


        }
    }
}
