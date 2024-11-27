
using StereoKit;
using System.Collections.Generic;
using System.Linq;

namespace Coop_Vr.Networking
{
    public class SkObject : ISerializable
    {
        public int ID;
        public List<Component> Components;
        
        public PosComponent Transform { get; private set; }

        public void Init()
        {
            foreach (Component component in Components)
            {
                component.Init(this);
            }
            Transform = GetComponent<PosComponent>();
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            Components = pPacket.ReadComponentsList();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.WriteComponentsList(Components);
        }

        public T GetComponent<T>()
        {
            return Components.OfType<T>().First();
        }
    }
}
