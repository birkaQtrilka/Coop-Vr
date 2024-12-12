﻿using Coop_Vr.Networking.ServerSide;
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
        SkObject _root;

        public GameView(ClientStateMachine context) : base(context)
        {
            //the root
            _root = new SkObject();
            _objects.Add(-1, _root);
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

                _objects[createdObject.ParentID].AddChild(createdObject.NewObj);

                createdObject.NewObj.Init();

                Log.Do("received object: " + createdObject.NewObj.ID);
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
