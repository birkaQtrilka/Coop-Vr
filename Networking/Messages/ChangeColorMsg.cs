
using System.Drawing;

namespace Coop_Vr.Networking.Messages
{
    public class ChangeColorMsg : IMessage
    {
        public int ColorCode;
        public int ObjID;

        public void Deserialize(Packet pPacket)
        {
            ColorCode = pPacket.ReadInt();
            ObjID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ColorCode);
            pPacket.Write(ObjID);
        }

        public Color GetColor()
        {
            if (ColorCode == 0)
                return Color.Red;
            else if (ColorCode == 1)
                return Color.Green;

            return Color.Blue;
        }
    }
}
