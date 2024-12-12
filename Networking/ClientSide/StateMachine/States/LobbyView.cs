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
        //string text = "192.168.144.33";
        string text = "192.168.178.75";
        Task _connectingTask;
        bool _pressedConectToServer;
        bool _conectedToServer;

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
            if (message is PlayerJoinResponse join)
            {
                //set id
                context.SetID(join.ID);
                context.ChangeTo<GameView>();
            }
        }

        public override void Update()
        {
            if (_connectingTask != null)
            {
                _conectedToServer = true;
                if (_connectingTask.IsCompleted)
                {
                    _connectingTask = null;
                    context.SendMessage(new PlayerJoinRequest());
                }
            }

            if (_conectedToServer)
                DrawConnectionWindow();
            else
                DrawJoinWindow();

        }

        void DrawJoinWindow()
        {
            UI.WindowBegin("Window", ref windowPose, new Vec2(50, 50) * U.cm, showHeader ? UIWin.Normal : UIWin.Body);

            Vec2 inputSize = V.XY(20 * U.cm, 0);

            UI.Input("Sever IP", ref text, inputSize, TextContext.Text);

            if (/*UI.Button("Find Lobby") &&*/ !_pressedConectToServer)
            {
                _pressedConectToServer = true;
                _connectingTask = context.ConnectToServerAsync(text);
            }


            UI.HSeparator();


            UI.SameLine();
            bool openKeyboard = Platform.KeyboardVisible;
            if (UI.Toggle("Show Keyboard", ref openKeyboard))
                Platform.KeyboardShow(openKeyboard);

            UI.WindowEnd();
        }

        void DrawConnectionWindow()
        {
            UI.WindowBegin("Text Input", ref windowPose);
            UI.Text("Searching for lobby...");
            UI.WindowEnd();
        }
    }
}
