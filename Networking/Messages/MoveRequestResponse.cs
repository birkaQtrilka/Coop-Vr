
namespace Coop_Vr.Networking.Messages
{
    public class MoveRequestResponse : IMessage
    {
        public int ObjectID; // server
        public int SenderID; // Client
        public PosComponent Position;

        public bool stopped;

        public void Deserialize(Packet pPacket)
        {
            ObjectID = pPacket.ReadInt();
            SenderID = pPacket.ReadInt();
            Position = new();
            Position.Deserialize(pPacket);
            stopped = pPacket.ReadBool();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ObjectID);
            pPacket.Write(SenderID);
            Position.Serialize(pPacket);
            pPacket.Write(stopped);
        }
    }
}
