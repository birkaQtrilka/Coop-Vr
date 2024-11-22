
namespace Coop_Vr.Networking.ServerSide
{
    public class CreateObjectResponse : IMessage
    {
        public SkObject NewObj;

        public void Deserialize(Packet pPacket)
        {
            NewObj.Deserialize(pPacket);
        }

        public void Serialize(Packet pPacket)
        {
            NewObj.Serialize(pPacket);
        }
    }
}
