using Coop_Vr.Networking.Messages;
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
        readonly SkObject _root;

        //AnchorManager _anchorManager = new();

        public GameView(ClientStateMachine context) : base(context)
        {
            //the root
            _root = new SkObject() { ID = -1, Components = new() { new PosComponent() } };
            _objects.Add(-1, _root);
            _root.Init();
        }

        public override void OnEnter()
        {
            //_anchorManager.Initialize();
            EventBus<SKObjectCreated>.Event += OnSkObjectCreated;
            EventBus<SKObjectGetter>.Event += ObjectGet;

        }

        public override void OnExit()
        {
            EventBus<SKObjectCreated>.Event -= OnSkObjectCreated;
            EventBus<SKObjectGetter>.Event -= ObjectGet;

            ResetData();
        }

        void ResetData()
        {
            _objects.Clear();

            _root.ForEach(c => {
                _root.RemoveChild(c, false);
            });

            _objects.Add(-1, _root);
        }

        void OnSkObjectCreated(SKObjectCreated evnt)
        {
            var obj = evnt.Obj;
            int temporaryID = Random.Shared.Next(0, int.MaxValue);
            _objects[obj.ParentID].AddChild(obj, false);
            obj.ID = temporaryID;

            _objects.Add(obj.ID, obj);
            obj.Init();
            context.SendMessage(new CreateObjectMsg()
            {
                NewObj = evnt.Obj,
                ParentID = evnt.ParentID,
                SenderID = context.ID
            });
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectMsg createdObject)
            {
                //adding it outside the objectCreated callback because 
                //the object has already set its children in the deserialization loop
                if (createdObject.SenderID == context.ID) return;

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
                obj.Transform.Pose = changePosition.PosComponent.Pose;
            }
            else if (message is MoveRequestResponse move)
            {

                SkObject obj = _objects[move.ObjectID];
                obj.GetComponent<Move>().HandleResponese(move);
            }
        }

        void ObjectGet(SKObjectGetter getter)
        {
            getter.ReturnedObj = () => _objects[getter.ID];
        }

        void OnObjectCreated(SkObject obj)
        {
            obj.Init();
            _objects[obj.ParentID].AddChild(obj, false);
            _objects.Add(obj.ID, obj);
            obj.ForEach(child => OnObjectCreated(child));
        }

        public override void Update()
        {
            //_anchorManager.Step();

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
                var newObj =
                    new SkObject(components: new()
                    {
                        new PosComponent() { Pose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "sphere"},
                        new Move()
                    });
                
                var newObj2 =
                    new SkObject(components: new()
                    {
                        new PosComponent() { Pose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "cube"},
                        new Move()
                    });

                
                newObj.AddChild(newObj2, true);
            }
            UI.HSeparator();

            if (UI.Button("Add object cube"))
            {
                var newObj =
                    new SkObject(components: new()
                    {
                        new PosComponent() { Pose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "cube"},
                        new Move()
                    });

            }

            UI.WindowEnd();
        }
    }
}
