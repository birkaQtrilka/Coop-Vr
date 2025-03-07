using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.Messages
{
    public struct RemoveMsg : IMessage
    {
        public int ID;
        public int ParentID;

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            ParentID = pPacket.ReadInt();
        }

        public readonly void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.Write(ParentID);
        }
    }
}
