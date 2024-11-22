using Coop_Vr.Networking.ServerSide;
using StereoKit;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class LobbyView : Room<ClientStateMachine>
    {
        //wrap this into own "gameObject" class
        string title = "Text Input";
        string description = "";

        Pose windowPose = Pose.Identity;
        string text = "Edit me";
        Task connectingTask;

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
            if(message is PlayerJoinResponse)
            {
                context.ChangeTo<GameView>();
            }
        }

        public override void Update()
        {
            if (connectingTask != null)
            {
                if (!connectingTask.IsCompleted)
                {
                    UI.WindowBegin("Text Input", ref windowPose);
                    UI.Text("Searching for lobby...");
                    UI.WindowEnd();
                }
                else
                {
                    connectingTask = null;
                    context.SendMessage(new PlayerJoinRequest() { ID = (int)Time.Total });
                }

                return;
            }


            UI.WindowBegin("Text Input", ref windowPose);

            UI.Text("You can specify whether or not a UI element will hide an active soft keyboard upon interaction.");
            UI.Button("Hides active soft keyboard");
            UI.SameLine();
            UI.PushPreserveKeyboard(true);
            UI.Button("Doesn't hide");
            UI.PopPreserveKeyboard();

            UI.HSeparator();

            UI.Text("Different TextContexts will surface different soft keyboards.");
            Vec2 inputSize = V.XY(20 * U.cm, 0);
            Vec2 labelSize = V.XY(8 * U.cm, 0);

            UI.Label("Normal Text", labelSize); UI.SameLine();
            UI.Input("TextText", ref text, inputSize, TextContext.Text);

            if (UI.Button("Find Lobby"))
            {
                connectingTask = context.ConnectToServerAsync(text);
            }


            UI.HSeparator();


            UI.SameLine();
            bool openKeyboard = Platform.KeyboardVisible;
            if (UI.Toggle("Show Keyboard", ref openKeyboard))
                Platform.KeyboardShow(openKeyboard);

            UI.WindowEnd();
        }
    }
}
