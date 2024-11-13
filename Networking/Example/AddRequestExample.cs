
namespace Coop_Vr.Networking
{
    public class AddRequestExample : IMessage
    {
        public Score score;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(score);
        }

        public void Deserialize(Packet pPacket)
        {
            score = pPacket.Read<Score>();
        }
    }
}
