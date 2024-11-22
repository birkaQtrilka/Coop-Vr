
using StereoKit;
using System.Collections.Generic;
using System.Linq;

namespace Coop_Vr.Networking
{
    public class SkObject : ISerializable
    {
        public int ID;
        public List<ISerializable> Components;

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            Components = pPacket.ReadList();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.WriteList(Components);
        }

        public T GetComponent<T>()
        {
            return Components.OfType<T>().First();
        }
    }
}
