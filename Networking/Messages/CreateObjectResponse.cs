
namespace Coop_Vr.Networking
{
    public class CreateObjectResponse : IMessage
    {
        public SkObject NewObj;
        public int ParentID;

        public void Deserialize(Packet pPacket)
        {
            NewObj = new();
            NewObj.Deserialize(pPacket);
            ParentID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            NewObj.Serialize(pPacket);
            pPacket.Write(ParentID);
        }
    }
}
