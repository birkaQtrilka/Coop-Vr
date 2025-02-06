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
        bool stoppedMoving;

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

            isMoving = UI.Handle(gameObject.ID.ToString(), ref gameObject.Transform.pose, modelComponent.bounds);

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
                return;
            }

            if (move.SenderID == ClientStateMachine.MessageSender.ID)
            {
                Log.Do("want to change pos but it is sender");
                MoverClientID = ClientStateMachine.MessageSender.ID;

                return;
            }
            //make a clock that calculates the refresh rate
            gameObject.Transform.QueueInterpolate(move.Position.pose);
            MoverClientID = move.SenderID;
        }
        
}
}
