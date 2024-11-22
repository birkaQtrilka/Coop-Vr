using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class ChangePositionRequest : IMessage
    {
        public int id;
        public PosComponent position;

        public void Deserialize(Packet pPacket)//find the obj with id and change component data
        {
            id = pPacket.ReadInt();
            position.Deserialize(pPacket);
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(id);
            position.Serialize(pPacket);
        }
    }
}
