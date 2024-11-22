using StereoKit;

namespace Coop_Vr.Networking
{
    public struct PosComponent : ISerializable
    {
        public Pose pose;

        public void Deserialize(Packet pPacket)
        {
            pose = new(
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                new Quat(
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat()
                    )
                );
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(pose.position.x);
            pPacket.Write(pose.position.y);
            pPacket.Write(pose.position.z);
            pPacket.Write(pose.orientation.x);
            pPacket.Write(pose.orientation.y);
            pPacket.Write(pose.orientation.z);
            pPacket.Write(pose.orientation.w);
        }
    }
}
