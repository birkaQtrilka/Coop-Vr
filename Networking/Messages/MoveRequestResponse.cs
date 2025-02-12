
namespace Coop_Vr.Networking.Messages
{
    public class MoveRequestResponse : IMessage
    {
        public int ObjectID; // server
        public int SenderID; // Client
        public PosComponent Position;

        public bool stopped;
        public bool alsoMoveSender;//in case server wants to move an object as while assigning ownership to a player

        public void Deserialize(Packet pPacket)
        {
            ObjectID = pPacket.ReadInt();
            SenderID = pPacket.ReadInt();
            Position = new();
            Position.Deserialize(pPacket);
            stopped = pPacket.ReadBool();
            alsoMoveSender = pPacket.ReadBool();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ObjectID);
            pPacket.Write(SenderID);
            Position.Serialize(pPacket);
            pPacket.Write(stopped);
            pPacket.Write(alsoMoveSender);
        }
    }
}
