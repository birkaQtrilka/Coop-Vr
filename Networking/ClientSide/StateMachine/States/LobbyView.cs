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

        Pose windowPose = new Pose(-.2f, 0, -0.6f, Quat.LookDir(0, 0, 1));

        bool showHeader = true;
        float slider = 0.5f;
        string text = "192.168.138.33";
        Task connectingTask;
        bool conecedToServer;

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
            if(message is PlayerJoinResponse join)
            {
                //set id
                context.SetID(join.ID);
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
                    context.SendMessage(new PlayerJoinRequest());
                }

                return;
            }
            

            //UI.WindowBegin("Text Input", ref windowPose);
            UI.WindowBegin("Window", ref windowPose, new Vec2(20, 0) * U.cm, showHeader ? UIWin.Normal : UIWin.Body);

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

            if (UI.Button("Find Lobby") && !conecedToServer)
            {
                conecedToServer = true;
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
