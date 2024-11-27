
namespace Coop_Vr.Networking.ServerSide
{
    public class PlayerJoinResponse : IMessage
    {
        public int ID;

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
        }
    }
}
