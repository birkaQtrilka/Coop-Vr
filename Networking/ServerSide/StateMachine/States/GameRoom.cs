using Coop_Vr.Networking.ClientSide;
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
            Console.WriteLine("Entered game room\nMember count: " + MemberCount());

            
        }

        public override void OnExit()
        {
        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if (message is CreateObjectRequest objCreate)
            {
                Console.WriteLine("create object response");
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
                Console.Write("Member count: " + MemberCount() + "   ");
                context.CurrentRoom.SafeForEachMember((m) => m.SendMessage(response));

            }
        }

        public override void Update()
        {
        }
    }
}
