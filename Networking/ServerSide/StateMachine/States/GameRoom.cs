using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class GameRoom : Room<ServerStateMachine>
    {
        public GameRoom(ServerStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {
            Console.WriteLine("Entered game room");
        }

        public override void OnExit()
        {
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
        }

        public override void Update()
        {
        }
    }
}
