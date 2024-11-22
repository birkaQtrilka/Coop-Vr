
namespace Coop_Vr.Networking.ClientSide
{
    class PlayerJoinRequest : IMessage
    {
        public int ID;
        //bool _isReady;

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            //_isReady = pPacket.ReadBool();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            //pPacket.Write(_isReady);
        }
    }
}
