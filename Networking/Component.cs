
namespace Coop_Vr.Networking
{
    public abstract class Component : ISerializable
    {
        public bool Enabled;

        public SkObject gameObject;

        public void Init(SkObject pGameObject)
        {
            gameObject = pGameObject;
            Start();
        }

        public abstract void Serialize(Packet pPacket);
        public abstract void Deserialize(Packet pPacket);


        public virtual void Start()
        {

        }

        public virtual void Update()
        {

        }
    }
}
