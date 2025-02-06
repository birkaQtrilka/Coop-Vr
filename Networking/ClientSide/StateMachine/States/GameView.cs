using Coop_Vr.Networking.Messages;
using StereoKit;
using System.Collections.Generic;
using Coop_Vr.Networking.ServerSide;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class GameView : Room<ClientStateMachine>
    {
        Pose windowPos = Pose.Identity;
        readonly Dictionary<int, SkObject> _objects = new();
        readonly SkObject _root;

        AnchorManager _anchorManager = new();

        public GameView(ClientStateMachine context) : base(context)
        {
            //the root
            _root = new SkObject();
            _objects.Add(-1, _root);
        }

        public override void OnEnter()
        {
            _anchorManager.Initialize();

        }


        public override void OnExit()
        {

        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectResponse createdObject)
            {
                //adding it outside the objectCreated callback because 
                //the object has already set its children in the deserialization loop
                _objects[createdObject.ParentID].AddChild(createdObject.NewObj);

                OnObjectCreated(createdObject.NewObj);
                Log.Do("received object: " + createdObject.NewObj.ID);
            }
            else if (message is ChangePositionResponse changePosition)
            {
                if (changePosition.SenderID == context.ID)
                {
                    Log.Do("want to change pos but it is sender");
                    return;
                }
                SkObject obj = _objects[changePosition.ObjectID];
                obj.Transform.pose = changePosition.PosComponent.pose;
            }
            else if (message is MoveRequestResponse move)
            {

                SkObject obj = _objects[move.ObjectID];
                obj.GetComponent<Move>().HandleResponese(move);
            }
        }

        void OnObjectCreated(SkObject obj)
        {
            _objects.Add(obj.ID, obj);
            obj.Init();
            obj.ForEach(child => OnObjectCreated(child));
        }

        public override void Update()
        {
            _anchorManager.Step();

            DrawWindow();

            _root.Update();

        }

        public override void FixedUpdate()
        {
            _root.FixedUpdate();
        }

        void DrawWindow()
        {
            UI.WindowBegin("GameWindow", ref windowPos);
            UI.Label("GameView");
            UI.HSeparator();

            if (UI.Button("Add object sphere"))
            {
                Log.Do("request to add sphere");
                context.SendMessage(new CreateObjectRequest()
                {
                    Components = new List<Component>()
                    {
                        new PosComponent() { pose = new Pose(2,0,0)},
                        new ModelComponent() { MeshName = "sphere"},
                        new Move()
                    }

                });
            }
            UI.HSeparator();

            if (UI.Button("Add object cube"))
            {
                context.SendMessage(new CreateObjectRequest()
                {
                    Components = new List<Component>()
                    {
                        new PosComponent() { pose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "cube"},
                        new Move()

                    }

                });
            }

            UI.WindowEnd();
        }
    }
}
