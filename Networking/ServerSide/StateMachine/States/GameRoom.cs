﻿using Coop_Vr.Networking.ClientSide;
using Coop_Vr.Networking.ServerSide.Components;
using Coop_Vr.Networking.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class GameRoom : Room<ServerStateMachine>
    {
        Graph Graph;
        public int CurrentId { get; private set; }

        Dictionary<int, SkObject> _objects = new();
        SkObject _root;

        public GameRoom(ServerStateMachine context) : base(context)
        {
            _root = new SkObject() { ID = -1 };
            _objects.Add(-1, _root);
            Graph = new Graph();
        }

        public override void OnEnter()
        {
            Log.Do("Entered game room\nMember count: " + MemberCount());
            EventBus<SKObjectAdded>.Event += OnObjectCreated;
            EventBus<SKObjectRemoved>.Event += OnObjRemoved;

            string filePath = "Assets\\Documents\\europe_data.csv";
            
            // Create a FileHandler instance
            var fileHandler = new FileHandler(filePath);

            // Set the original records
            Graph.SetOrigialRecords(fileHandler.Records);

            // Read graph points from the CSV file
            var graphPoints = fileHandler.ReadGraphPointsFromCsv();

            // Scale the graph points
            fileHandler.ScaleGraphPoints(graphPoints, config.Config.SCALE); 
            
            Graph.SetGraphPoints(graphPoints);

            SkObject graphHolder = new(
                parentID: -1,
                new List<Component>() { new PosComponent(), Graph }
            );

            Graph.GenerateGraphPoints();

            var response = new CreateObjectResponse() { NewObj = graphHolder, ParentID = -1};
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                SafeForEachMember((m) => m.SendMessage(response));

            });
            Log.Do("Object ID: " + response.ToString());
        }

        void OnObjRemoved(SKObjectRemoved evnt)
        {
            //removing from parent
            _objects[evnt.ParentID].RemoveChild(evnt.Obj);
            //remove from pool
            _objects.Remove(evnt.Obj.ID);
        }

        public override void OnExit()
        {
            EventBus<SKObjectAdded>.Event -= OnObjectCreated;
            EventBus<SKObjectRemoved>.Event -= OnObjRemoved;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender"></param>
        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectRequest objCreate)//SHOULD HAVE PARENT ID
            {
                SkObject newObject = new(objCreate.ParentID, objCreate.Components); 
                
                var response = new CreateObjectResponse() { NewObj = newObject, ParentID = objCreate.ParentID };
                context.CurrentRoom.SafeForEachMember((m) => m.SendMessage(response));
            }
            else if( message is ChangePositionRequest changePositionRequest)
            {
                _objects[changePositionRequest.ObjectID].Transform.pose = changePositionRequest.position.pose;

                var response = new ChangePositionResponse() { 
                    ObjectID = changePositionRequest.ObjectID,
                    PosComponent = changePositionRequest.position,
                    SenderID = changePositionRequest.SenderID
                };
                Log.Do("Member count: " + MemberCount() + "   ");
                context.CurrentRoom.SafeForEachMember((m) => m.SendMessage(response));

            }
            else if (message is MoveRequestResponse move)
            {
                SkObject obj = _objects[move.ObjectID];
                obj.Transform.pose = move.Position.pose;
                var component = obj.GetComponent<Move>();
                bool hasNoOwner = component.MoverClientID == -1;
                bool isMovedBySender = move.SenderID == component.MoverClientID;
                //in case two clients grab the object "at once", this prevents from juggling between owners
                if (hasNoOwner || isMovedBySender)
                {
                    if(move.stopped)//terminating ownership
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
                    Log.Do("Object ID: " + response.ObjectID.ToString());
                    component.OnMove?.Invoke(component, response);

                    context.CurrentRoom.SafeForEachMember((m) => m.SendMessage(response));
                } 
            }
        }

        void OnObjectCreated(SKObjectAdded evnt)
        {
            var obj = evnt.Obj;
            obj.ID = CurrentId++;
            _objects.Add(obj.ID, obj);
            _objects[evnt.ParentID].AddChild(obj);
            obj.Init();
        }

        public override void Update()
        {
            //_root.Update();
        }
    }
}
