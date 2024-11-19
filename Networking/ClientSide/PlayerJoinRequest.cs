
namespace Coop_Vr.Networking.ClientSide
{
    class PlayerJoinRequest : IMessage
    {
        //int _id;
        //bool _isReady;

        public void Deserialize(Packet pPacket)
        {
            //_id = pPacket.ReadInt();
            //_isReady = pPacket.ReadBool();
        }

        public void Serialize(Packet pPacket)
        {
            //pPacket.Write(_id);
            //pPacket.Write(_isReady);
        }
    }
}
