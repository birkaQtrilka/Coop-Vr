
namespace Coop_Vr.Networking.ClientSide
{
    public class ChangePositionRequest : IMessage
    {
        public int ObjectID;
        public int SenderID;
        public PosComponent position;

        public void Deserialize(Packet pPacket)//find the obj with id and change component data
        {
            ObjectID = pPacket.ReadInt();
            SenderID = pPacket.ReadInt();
            position = new();
            position.Deserialize(pPacket);
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ObjectID);
            pPacket.Write(SenderID);
            position.Serialize(pPacket);
        }


    }
}
