using Coop_Vr.Networking.ServerSide;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class GameView : Room<ClientStateMachine>
    {
        Pose windowPos = Pose.Identity;
        readonly Dictionary<int, SkObject> _objects = new();

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
            if (message is CreateObjectResponse createdObject)
            {
                _objects.Add(createdObject.NewObj.ID, createdObject.NewObj);
                Log.Do("received object: " + createdObject.NewObj.ID);
                createdObject.NewObj.Init();
            }
            else if (message is ChangePositionResponse changePosition)
            {
                if (changePosition.SenderID == context.ID)
                {
                    Log.Do("want to change pos but it is sender");
                    return;
                }
                _objects[changePosition.ObjectID].Transform.QueueInterpolate(changePosition.PosComponent.pose);
            }
        }

        public override void Update()
        {
            DrawWindow();

            foreach (var kv in _objects)
            {
                SkObject obj = kv.Value;
                foreach (Component component in obj.Components)
                    if (component.Enabled)
                        component.Update();
            }

        }

        public override void FixedUpdate()
        {
            foreach (var kv in _objects)
            {
                SkObject obj = kv.Value;
                foreach (Component component in obj.Components)
                    if (component.Enabled)
                        component.FixedUpdate();
            }
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
                        new Move() { }
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
                        new Move() { }

                    }

                });
            }

            UI.WindowEnd();
        }
    }
}
