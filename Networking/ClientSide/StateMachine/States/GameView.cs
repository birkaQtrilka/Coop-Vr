using Coop_Vr.Networking.Messages;
using Coop_Vr.Networking.ServerSide.StateMachine.States;
using StereoKit;
using System;
using System.Collections.Generic;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class GameView : Room<ClientStateMachine>
    {
        Pose windowPos = Pose.Identity;
        readonly Dictionary<int, SkObject> _objects = new();
        SkObject _root;

        readonly Dictionary<int, SKObjectCreated> _sendingObjects = new();

        public GameView(ClientStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {
            _root = new SkObject(parentID: -98127) //id is random negative number so it's cannot have a parent 
            {
                ID = -1,
                Components = new() {
                    new PosComponent(),
                    new ClientMoveObj()
                }
            };

            _objects.Add(-1, _root);
            _root.Init();

            EventBus<SKObjectCreated>.Event += OnLocalObjectCreated;
            EventBus<SKObjectGetter>.Event += ObjectGet;
            EventBus<SKObjectAdded>.Event += OnObjectAdded;
            _root.Start();
        }

        public override void OnExit()
        {
            EventBus<SKObjectCreated>.Event -= OnLocalObjectCreated;
            EventBus<SKObjectGetter>.Event -= ObjectGet;
            EventBus<SKObjectAdded>.Event -= OnObjectAdded;

            ResetData();
        }

        void ResetData()
        {
            _objects.Clear();

            _root.ForEach(c => {
                _root.RemoveChild(c, false);
            });

            _root = null;
            _sendingObjects.Clear();
        }

        void OnLocalObjectCreated(SKObjectCreated evnt)
        {
            var obj = evnt.Obj;
            int temporaryID = Random.Shared.Next(0, int.MaxValue);
            obj.ID = temporaryID;

            _objects.Add(obj.ID, obj);
            obj.Init();
            _objects[obj.ParentID].AddChild(obj, false);

            obj.Start();

            AddToSendObjectsQueue(obj, evnt);
        }

        void AddToSendObjectsQueue(SkObject obj, SKObjectCreated evnt)
        {
            _sendingObjects.Add(obj.ID, evnt);
        }

        void ProcessSendingObjects()
        {
            foreach (SKObjectCreated data in _sendingObjects.Values)
            {
                bool hasParent = _sendingObjects.ContainsKey(data.Obj.ParentID);

                if (hasParent) continue;

                var response = new CreateObjectMsg()
                {
                    NewObj = data.Obj,
                    ParentID = data.Obj.ParentID,
                    SenderID = context.ID,
                };
                Log.Do("Send create object, id: " + response.NewObj.ID);

                context.SendMessage(response);
            }
            _sendingObjects.Clear();
        }

        void OnRemoteObjectCreated(SkObject obj)
        {
            OnRemoteObjectCreatedRecursive(obj);
            _objects[obj.ParentID].AddChild(obj, false);
        }

        void OnRemoteObjectCreatedRecursive(SkObject obj, bool addToPool = true)
        {
            if(addToPool)
                _objects.Add(obj.ID, obj);
            obj.Init();
            obj.ForEach(child => OnRemoteObjectCreatedRecursive(child));
            obj.Start();
        }

        void OnObjectAdded(SKObjectAdded evnt)
        {
            _objects[evnt.OldParentID].RemoveChild(evnt.AddedObj);
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectMsg createdObject)
            {
                //adding it outside the objectCreated callback because 
                //the object has already set its children in the deserialization loop
                if (createdObject.SenderID == context.ID) return;

                OnRemoteObjectCreated(createdObject.NewObj);
                Log.Do("received object: " + createdObject.NewObj.ID);
            }
            else if (message is ChangePositionResponse changePosition)
            {
                if (changePosition.SenderID == context.ID)
                {
                    Log.Do("want to change pos but it is sender");
                    return;
                }
                Log.Do("Change position obj id: " + changePosition.ObjectID);

                SkObject obj = _objects[changePosition.ObjectID];
                obj.Transform.LocalPose = changePosition.PosComponent.LocalPose;
            }
            else if (message is MoveRequestResponse move)
            {
                Log.Do("move, obj id: " + move.ObjectID);

                SkObject obj = _objects[move.ObjectID];
                obj.GetComponent<Move>().HandleResponese(move);
            }
        }

        void ObjectGet(SKObjectGetter getter)
        {
            getter.ReturnedObj = () => {
                if (!_objects.ContainsKey(getter.ID)) return null;
                return _objects[getter.ID];
            };
        }

        public override void Update()
        {
            //_anchorManager.Step();
            _root.Update();

            DrawWindow();
            ProcessSendingObjects();
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
                //Change this to the server similar system. Better yet. Make, CreationManager and use it in both endpoints

                var newObj =
                    new SkObject(components: new()
                    {
                        new PosComponent() { LocalPose = new Pose(3,0,0), LocalScale = new Vec3(.3f,.3f,.3f)},
                        new ModelComponent() { MeshName = "sphere"},
                        new Move()
                    });
                
                var newObj2 =
                    new SkObject(parentID: newObj.ID,
                    components: new()
                    {
                        new PosComponent() { LocalPose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "cube"},
                        new Move()
                    });
                var newObj3 =
                    new SkObject(
                    components: new()
                    {
                        new PosComponent() { LocalPose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "cube"},
                        new Move()
                    });
                //no need to send another message to server since it will the object will already have the child 
                //when the message exits the queue
                newObj2.AddChild(newObj3,false);
            }
            UI.HSeparator();

            if (UI.Button("Add object cube"))
            {
                var newObj =
                    new SkObject(components: new()
                    {
                        new PosComponent() { LocalPose = new Pose(3,0,0)},
                        new ModelComponent() { MeshName = "cube"},
                        new Move()
                    });

            }

            UI.WindowEnd();
        }
    }
}
