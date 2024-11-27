using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ClientSide
{
    internal class Move : Component
    {
        public float speed;
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(speed);
        }
        public override void Deserialize(Packet pPacket)
        {
            speed = pPacket.ReadFloat();
        }

        public override void Update()
        {
            gameObject.Transform.pose.position += Vec3.Forward * speed;
        }

    }
}
