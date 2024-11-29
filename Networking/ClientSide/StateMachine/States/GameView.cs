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
        Dictionary<int, SkObject> objects = new();

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
                objects.Add(createdObject.NewObj.ID, createdObject.NewObj);
                Console.WriteLine("received object: " + createdObject.NewObj.ID);
                createdObject.NewObj.Init();
            }
            else if (message is ChangePositionResponse changePosition)
            {
                if (changePosition.SenderID == context.ID)
                {
                    Console.WriteLine("want to change pos but it is sender");
                    return;
                }
                Console.WriteLine("fuck, this is bad");
                objects[changePosition.ObjectID].Transform.pose = changePosition.PosComponent.pose;
            }
        }

        public override void Update()
        {
            UI.WindowBegin("GameWindow", ref windowPos);
            UI.Label("GameView");
            UI.HSeparator();

            if (UI.Button("Add object sphere"))
            {
                Console.WriteLine("request to add sphere");
                context.SendMessage(new CreateObjectRequest()
                {
                    Components = new List<Component>()
                    {
                        new PosComponent() { pose = new Pose(2,0,0)},
                        new ModelComponent() { MeshName = "sphere"},
                        new Move() { speed = 0.5f}
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
                        new Move() { speed =  0.5f}

                    }

                });
            }

            UI.WindowEnd();
            
            foreach (var kv in objects)
            {
                SkObject obj = kv.Value;
                foreach (Component component in obj.Components)
                    if (component.Enabled)
                        component.Update();
            }

        }

        public override void FixedUpdate()
        {
            foreach (var kv in objects)
            {
                SkObject obj = kv.Value;
                foreach (Component component in obj.Components)
                    if (component.Enabled)
                        component.FixedUpdate();
            }
        }


    }
}
