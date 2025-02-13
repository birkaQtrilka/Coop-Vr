﻿using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.Messages;
using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class Move : Component
    {
        private ModelComponent modelComponent;
        public int MoverClientID = -1;
        bool isMoving;
        bool stoppedMoving = true;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(MoverClientID);
        }

        public override void Deserialize(Packet pPacket)
        {
            MoverClientID = pPacket.ReadInt();
        }

        public override void Start()
        {
            modelComponent = gameObject.GetComponent<ModelComponent>();
        }

        public override void Update()
        {
            if (MoverClientID != -1 && ClientStateMachine.MessageSender.ID != MoverClientID)
                return;

            Pose copy = gameObject.Transform.LocalPose;

            isMoving = UI.Handle(gameObject.ID.ToString(), ref copy, modelComponent.bounds * gameObject.Transform.LocalScale);

            gameObject.Transform.LocalPose = copy;
        }

        public override void FixedUpdate()
        {
            if (isMoving)
            {
                stoppedMoving = false;
                ClientStateMachine.MessageSender.SendMessage
                (
                   new MoveRequestResponse()
                   {
                       ObjectID = gameObject.ID,
                       Position = gameObject.Transform,
                       SenderID = ClientStateMachine.MessageSender.ID,
                       stopped = false,
                   }
                );
                return;
            }

            if (!stoppedMoving && gameObject.Transform.QueueIsEmpty())
            {
                stoppedMoving = true;
                ClientStateMachine.MessageSender.SendMessage
                (
                    new MoveRequestResponse()
                    {
                        ObjectID = gameObject.ID,
                        Position = gameObject.Transform,
                        SenderID = ClientStateMachine.MessageSender.ID,
                        stopped = true,
                    }
                );
            }
        }

        public void HandleResponese(MoveRequestResponse move)
        {
            if (move.stopped)
            {
                MoverClientID = -1;
                return;
            }

            if (move.SenderID == ClientStateMachine.MessageSender.ID)
            {
                Log.Do("want to change pos but it is sender");
                MoverClientID = ClientStateMachine.MessageSender.ID;

                return;
            }

            gameObject.Transform.QueueInterpolate(move.Position.LocalPose);
            MoverClientID = move.SenderID;
        }

    }
}