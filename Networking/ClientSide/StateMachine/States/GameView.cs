using StereoKit;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class GameView : Room<ClientStateMachine>
    {
        Pose windowPos = Pose.Identity;

        public GameView(ClientStateMachine context) : base(context)
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
            UI.WindowBegin("GameWindow", ref windowPos);
            UI.Label("GameView");
            UI.WindowEnd();
        }
    }
}
