using System.Collections.Generic;

namespace Coop_Vr.Networking.ClientSide
{
    public class CreateObjectRequest : IMessage
    {
        public List<ISerializable> Components;

        public void Deserialize(Packet pPacket)
        {
            Components = pPacket.ReadList();
        }

        public void Serialize(Packet pPacket)
        {
        }
    }
}
