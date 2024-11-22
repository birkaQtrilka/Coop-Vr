using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class ChangePositionRequest : IMessage
    {
        public int ObjID;
        public Pose Pos;

        public void Deserialize(Packet pPacket)
        {
            pPacket.Write(ObjID);
            pPacket.Write(Pos.position.x);
            pPacket.Write(Pos.position.y);
            pPacket.Write(Pos.position.z);

            pPacket.Write(Pos.orientation.x);
            pPacket.Write(Pos.orientation.y);
            pPacket.Write(Pos.orientation.z);
            pPacket.Write(Pos.orientation.w);
        }

        public void Serialize(Packet pPacket)
        {
            ObjID = pPacket.ReadInt();
            Pos = new(pPacket.ReadFloat(),
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
