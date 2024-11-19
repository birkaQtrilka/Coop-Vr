using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class ChangePositionRequest : IMessage
    {
        Pose pos;

        public void Deserialize(Packet pPacket)
        {
            pPacket.Write(pos.position.x);
            pPacket.Write(pos.position.y);
            pPacket.Write(pos.position.z);

            pPacket.Write(pos.orientation.x);
            pPacket.Write(pos.orientation.y);
            pPacket.Write(pos.orientation.z);
            pPacket.Write(pos.orientation.w);

        }

        public void Serialize(Packet pPacket)
        {
            pos = new(pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                new Quat(
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat())
                );
        }
    }
}
