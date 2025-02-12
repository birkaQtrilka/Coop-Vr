
using StereoKit;

namespace Coop_Vr.Networking.Messages
{
    public class ChangeColorMsg : IMessage
    {
        public int ColorCode;
        public int ObjectID;

        public void Deserialize(Packet pPacket)
        {
            ColorCode = pPacket.ReadInt();
            ObjectID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ColorCode);
            pPacket.Write(ObjectID);
        }

        public Color GetColor()
        {
            if (ColorCode == 0)
                return Color.Black;
            else if (ColorCode == 1)
                return Color.White;

            return Color.HSV(0,.5f,1);
        }
    }
}
