using System.Collections.Generic;

namespace Coop_Vr.Networking
{
    public class CreateObjectMsg : IMessage
    {
        public SkObject NewObj;
        public int ParentID = -1;
        public int SenderID = -1;
        public void Deserialize(Packet pPacket)
        {
            NewObj = new();
            NewObj.Deserialize(pPacket);
            ParentID = pPacket.ReadInt();
            SenderID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            NewObj.Serialize(pPacket);
            pPacket.Write(ParentID);
            pPacket.Write(SenderID);
        }
    }
}
