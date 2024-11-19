using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class LobbyView : Room<ClientStateMachine>
    {
        public LobbyView(ClientStateMachine context) : base(context)
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
        }

        public override void Update()
        {
        }
    }
}
