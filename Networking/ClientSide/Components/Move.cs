using Coop_Vr.Networking.ClientSide.StateMachine;
using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class Move : Component
    {
        public float speed;
        private ModelComponent modelComponent;
        bool moving;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(speed);
        }
        
        public override void Deserialize(Packet pPacket)
        {
            speed = pPacket.ReadFloat();
        }

        public override void Start()
        {
            modelComponent = gameObject.GetComponent<ModelComponent>();
        }

        public override void Update()
        {
            moving = UI.Handle(gameObject.ID.ToString(), ref gameObject.Transform.pose, modelComponent.bounds);
            
        }

        public override void FixedUpdate()
        {
            if (!moving) return;
            //gameObject.Transform.pose.position += Vec3.Forward * speed;

            ClientStateMachine.MessageSender.SendMessage(
                new ChangePositionRequest()
                {
                    ObjectID = gameObject.ID,
                    position = gameObject.Transform,
                    SenderID = ClientStateMachine.MessageSender.ID
                });
        }

    }
}
