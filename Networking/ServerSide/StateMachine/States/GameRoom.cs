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

        //related to sending objects
        //int is obj id, bool is representing if it will be sent or not 
        readonly Dictionary<int, SendingObjectsData> _sendingObjects = new();
        readonly struct SendingObjectsData
        {
            public readonly bool HasParent;
            public readonly SKObjectCreated EventData;

            public SendingObjectsData(bool hasParent, SKObjectCreated eventData)
            {
                HasParent = hasParent;
                EventData = eventData;
            }
        }

        public GameRoom(ServerStateMachine context) : base(context)
        {
            _currentScene = new AnchorScene(this);
            //id is random negative number so it's cannot have a parent 
            _root = new SkObject(-974327) { ID = -1, Components = new() { new PosComponent() } };
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
                Log.Do("Receive create object, id: " + objCreate.NewObj.ID);

                EventBus<SKObjectCreated>.Publish
                (
                    new SKObjectCreated(objCreate.NewObj, objCreate.ParentID, objCreate.SenderID)
                );

            }
            else if (message is ChangePositionRequest changePositionRequest)
            {
                _objects[changePositionRequest.ObjectID].Transform.LocalPose = changePositionRequest.position.LocalPose;

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
                obj.Transform.LocalPose = move.Position.LocalPose;
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
                    Log.Do("Receive, send Move. ID: " + move.ObjectID);
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

            //has parent refers to a parent in the sendingObject queue, it doesn't care if it has a parent outside it
            //because that parent was already sent through the network
            //do object create loop. Find root, if there is one. Only send the root to reduce send count
            bool hasParent = _sendingObjects.ContainsKey(obj.ParentID);
            
            _sendingObjects.Add(obj.ID, new SendingObjectsData(hasParent, evnt));
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

        void ProcessSendingObjects()
        {
            foreach (SendingObjectsData data in _sendingObjects.Values)
            {
                if (data.HasParent) continue;

                var response = new CreateObjectMsg()
                {
                    NewObj = data.EventData.Obj,
                    ParentID = data.EventData.ParentID,
                    SenderID = data.EventData.SenderID,
                };
                Log.Do("Send create object, id: " + response.NewObj.ID);

                context.SendMessage(response);
            }
            _sendingObjects.Clear();
        }

        public override void Update()
        {
            ProcessSendingObjects();
        }
    }
}
