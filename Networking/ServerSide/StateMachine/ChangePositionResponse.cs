
namespace Coop_Vr.Networking.ServerSide.StateMachine
{
    public class ChangePositionResponse : IMessage
    {
        int id;
        PosComponent position;

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
