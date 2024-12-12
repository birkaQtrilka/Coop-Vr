using Coop_Vr.Networking.ClientSide;
using Coop_Vr.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace Coop_Vr.Networking.ServerSide.StateMachine.States
{
    public class GameRoom : Room<ServerStateMachine>
    {
        public int currentId;

        Dictionary<int, SkObject> objects = new(); 

        public GameRoom(ServerStateMachine context) : base(context)
        {
        }

        public override void OnEnter()
        {
            Log.Do("Entered game room\nMember count: " + MemberCount());

            
        }

        public override void OnExit()
        {
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectRequest objCreate)
            {
                SkObject newObject = new() 
                {
                    ID = currentId++,
                    Components = objCreate.Components,
                };
                newObject.Init();
                objects.Add(newObject.ID, newObject);
                var response = new CreateObjectResponse() { NewObj = newObject };
                context.CurrentRoom.SafeForEachMember((m) => m.SendMessage(response));
            }
            else if( message is ChangePositionRequest changePositionRequest)
            {
                objects[changePositionRequest.ObjectID].Transform.pose = changePositionRequest.position.pose;

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
                SkObject obj = objects[move.ObjectID];
                obj.Transform.pose = move.Position.pose;
                var component = obj.GetComponent<Move>();
                bool hasNoOwner = component.MoverClientID == -1;

                //will probably bug out if owner leaves the game
                if (hasNoOwner || move.SenderID == component.MoverClientID)//is same Owner
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
                    context.CurrentRoom.SafeForEachMember((m) => m.SendMessage(response));
                } 

            }
        }

        public override void Update()
        {
        }
    }
}
