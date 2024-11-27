using System.Collections.Generic;

namespace Coop_Vr.Networking.ClientSide
{
    public class CreateObjectRequest : IMessage
    {
        public List<Component> Components;

        public void Deserialize(Packet pPacket)
        {
            Components = pPacket.ReadComponentsList();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.WriteComponentsList(Components);
        }
    }
}
