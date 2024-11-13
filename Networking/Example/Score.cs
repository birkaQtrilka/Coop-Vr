using System;
using System.Collections.Generic;
using System.Linq;

namespace Coop_Vr.Networking
{
    public class Score : ISerializable
    {
        public string name;
        public int score;

        public Score() { }

        public Score(string pName, int pScore)
        {
            name = pName;
            score = pScore;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(name);
            pPacket.Write(score);
        }

        public void Deserialize(Packet pPacket)
        {
            name = pPacket.ReadString();
            score = pPacket.ReadInt();
        }
    }
}
