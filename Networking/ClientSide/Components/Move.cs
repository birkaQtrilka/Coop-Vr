using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide
{
    public class Move : Component
    {
        public float speed;
        private ModelComponent modelComponent;
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(speed);
        }
        public override void Start()
        {
            modelComponent = gameObject.GetComponent<ModelComponent>();
        }
        public override void Deserialize(Packet pPacket)
        {
            speed = pPacket.ReadFloat();
        }

        public override void Update()
        {
            UI.Handle(gameObject.ID.ToString(), ref gameObject.Transform.pose, modelComponent.bounds);
            gameObject.Transform.pose.position += Vec3.Forward * speed;
        }

    }
}
