using Coop_Vr.Networking.ClientSide;
using Coop_Vr.Networking.Messages;
using Coop_Vr.Networking.Scenes;
using System.Collections.Generic;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class GameRoom : Room<ServerStateMachine>
    {
        public int CurrentId { get; private set; } = 1;

        Dictionary<int, SkObject> _objects = new();
        SkObject _root;
        Scene _currentScene;

        public GameRoom(ServerStateMachine context) : base(context)
        {
            _currentScene = new AnchorScene(this);
            _root = new SkObject() { ID = -1, Components = new() { new PosComponent() } };
            _objects.Add(-1, _root);
            _root.Init();
        }

        public override void OnEnter()
        {
            EventBus<SKObjectCreated>.Event += OnObjectCreated;
            EventBus<SKObjectAdded>.Event += OnObjectAdded;
            EventBus<SKObjectRemoved>.Event += OnObjectRemoved;
            EventBus<SKObjectGetter>.Event += ObjectGet;
            _currentScene.OnStart();
        }

        public override void OnExit()
        {
            _currentScene.OnStop();
            EventBus<SKObjectCreated>.Event -= OnObjectCreated;
            EventBus<SKObjectAdded>.Event -= OnObjectAdded;
            EventBus<SKObjectRemoved>.Event -= OnObjectRemoved;
            EventBus<SKObjectGetter>.Event -= ObjectGet;
            _objects.Clear();
            
            _root.ForEach(c => { 
                _root.RemoveChild(c, false);
            });

            _objects.Add(-1, _root);
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectMsg objCreate)
            {
                EventBus<SKObjectCreated>.Publish
                (
                    new SKObjectCreated(objCreate.NewObj, objCreate.ParentID, objCreate.SenderID)
                );
            }
            else if (message is ChangePositionRequest changePositionRequest)
            {
                _objects[changePositionRequest.ObjectID].Transform.Pose = changePositionRequest.position.Pose;

                var response = new ChangePositionResponse()
                {
                    ObjectID = changePositionRequest.ObjectID,
                    PosComponent = changePositionRequest.position,
                    SenderID = changePositionRequest.SenderID
                };
                Log.Do("Member count: " + MemberCount() + "   ");
                context.SendMessage(response);

            }
            else if (message is MoveRequestResponse move)
            {
                SkObject obj = _objects[move.ObjectID];
                obj.Transform.Pose = move.Position.Pose;
                var component = obj.GetComponent<Move>();
                bool hasNoOwner = component.MoverClientID == -1;

                //will probably bug out if owner leaves the game
                if (hasNoOwner || move.SenderID == component.MoverClientID)//is same Owner
                {
                    if (move.stopped)//terminating ownership
                        component.MoverClientID = -1;
                    else//claiming / continuing ownership 
                        component.MoverClientID = move.SenderID;

                    var response = new MoveRequestResponse()
                    {
                        ObjectID = move.ObjectID,
                        SenderID = move.SenderID,
                        Position = move.Position,
                        stopped = move.stopped,
                    };
                    Log.Do("Member count: " + MemberCount() + "   ");
                    context.SendMessage(response);
                }

            }
        }

        void OnObjectCreated(SKObjectCreated evnt)
        {
            var obj = evnt.Obj;
            
            obj.ID = obj.ID == 0 ? CurrentId++ : obj.ID;

            _objects.Add(obj.ID, obj);
            obj.Init();
            _objects[evnt.ParentID].AddChild(obj, false);

            var response = new CreateObjectMsg()
            {
                NewObj = obj,
                ParentID = evnt.ParentID,
                SenderID = evnt.SenderID,
            };
            context.SendMessage(response);
        }

        void OnObjectAdded(SKObjectAdded evnt)
        {
            _objects[evnt.OldParentID].RemoveChild(evnt.AddedObj);
            //send response
        }

        void OnObjectRemoved(SKObjectRemoved evnt)
        {
            //_objects[evnt.RemovedObj.ParentID].RemoveChild(evnt.RemovedObj);
            _root.AddChild(evnt.RemovedObj);
            //send response
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
        }
    }
}
