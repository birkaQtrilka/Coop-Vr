
namespace Coop_Vr.Networking.Messages
{
    public struct RemoveMsg : IMessage
    {
        public int ID;
        public int ParentID;

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            ParentID = pPacket.ReadInt();
        }

        public readonly void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(ParentID);
        }
    }
}
