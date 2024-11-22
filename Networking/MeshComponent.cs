
namespace Coop_Vr.Networking
{
    public class MeshComponent : ISerializable
    {
        public string MeshName;

        public void Deserialize(Packet pPacket)
        {
            MeshName = pPacket.ReadString();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(MeshName);
        }

        //public void Update()
        //{

        //}
    }
}
