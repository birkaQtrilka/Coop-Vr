using System.Collections.Generic;

namespace Coop_Vr.Networking
{
    public class CreateObjectRequest : IMessage
    {
        //-1 is the ID of the root
        public int ParentID = -1;
        public List<Component> Components;

        public void Deserialize(Packet pPacket)
        {
            Components = pPacket.ReadComponentsList();
            ParentID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.WriteComponentsList(Components);
            pPacket.Write(ParentID);
        }
    }
}
