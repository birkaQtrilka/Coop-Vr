using Coop_Vr.Networking.ServerSide;
using StereoKit;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class LobbyView : Room<ClientStateMachine>
    {

        Pose windowPose = new(-.2f, 0, -0.6f, Quat.LookDir(0, 0, 1));

        //baller hotspot
        //string text = "192.168.144.33";
        //stefan house
        //string text = "192.168.178.75";
        //vr box
        //string text = "192.168.1.157";
        string text = "localhost";

        Task _connectingTask;
        bool _pressedConectToServer;
        bool _conectedToServer;

        //bool _test = true;

        public LobbyView(ClientStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
            _conectedToServer = false;
            _pressedConectToServer = false;
            _connectingTask = null;
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
                    //if(_test)
                        context.SendMessage(new PlayerJoinRequest());
                    //_test = false;
                }
            }

            if (_conectedToServer)
                DrawConnectionWindow();
            else
                DrawJoinWindow();

        }

        void DrawJoinWindow()
        {
            UI.WindowBegin("Window", ref windowPose, new Vec2(50, 50) * U.cm,UIWin.Normal);

            Vec2 inputSize = V.XY(20 * U.cm, 0);

            UI.Input("Sever IP", ref text, inputSize, TextContext.Text);

            if (/*UI.Button("Find Lobby") &&*/ !_pressedConectToServer)
            {
                _pressedConectToServer = true;
                _connectingTask = context.ConnectToServerAsync(text);
            }
            //if (UI.Button("Find Lobby") )
            //{
            //    _test = true;
            //}

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
