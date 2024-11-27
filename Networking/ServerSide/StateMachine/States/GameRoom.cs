using Coop_Vr.Networking.ClientSide;
using System;
using System.Collections.Generic;
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
            Console.WriteLine("Entered game room");
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

                sender.SendMessage(new CreateObjectResponse() { NewObj = newObject});
            }
        }

        public override void Update()
        {
        }
    }
}
