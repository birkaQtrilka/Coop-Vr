
namespace Coop_Vr.Networking.ServerSide
{
    public class ChangePositionResponse : IMessage
    {
        public int ObjectID;
        public int SenderID;

        public PosComponent PosComponent;

        public void Deserialize(Packet pPacket)//find the obj with id and change component data
        {
            ObjectID = pPacket.ReadInt();
            SenderID = pPacket.ReadInt();
            PosComponent = new();
            PosComponent.Deserialize(pPacket);
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ObjectID);
            pPacket.Write(SenderID);
            PosComponent.Serialize(pPacket);
        }
    }
}
