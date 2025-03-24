using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.Messages;
using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class Move : Component
    {
        public int MoverClientID { get; set; } = -1;

        ModelComponent modelComponent;
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
            bool hasOwner = MoverClientID != -1;
            bool moverIsNotOwnedByMe = hasOwner && ClientStateMachine.MessageSender.ID != MoverClientID;

            if (moverIsNotOwnedByMe)
                return;

            isMoving = UI.Handle(gameObject.ID.ToString(), ref gameObject.Transform.pose, modelComponent.bounds *.2f);
        }

        public override void FixedUpdate()
        {
            if (isMoving)
            {
                stoppedMoving = false;
                SendMoveRequest(stoppedMoving);
            }
            else if (!stoppedMoving && gameObject.Transform.QueueIsEmpty())
            {
                stoppedMoving = true;
                SendMoveRequest(stoppedMoving);
            }
        }

        void SendMoveRequest(bool stoppedMoving)
        {
            ClientStateMachine.MessageSender.SendMessage
                (
                    new MoveRequestResponse()
                    {
                        ObjectID = gameObject.ID,
                        Position = gameObject.Transform,
                        SenderID = ClientStateMachine.MessageSender.ID,
                        stopped = stoppedMoving,
                    }
                );
        }

        public void HandleResponese(MoveRequestResponse move)
        {
            if(move.stopped)
            {
                MoverClientID = -1;
                stoppedMoving = true;
                return;
            }

            bool movedByMe = move.SenderID == ClientStateMachine.MessageSender.ID;
            MoverClientID = move.SenderID;

            if (movedByMe && !move.alsoMoveSender)
            {
                Log.Do("want to change pos but it is sender");
                return;
            }
            //make a clock that calculates the refresh rate?
            gameObject.Transform.QueueInterpolate(move.Position.pose);
        }
        
}
}
