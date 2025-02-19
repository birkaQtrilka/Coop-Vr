using StereoKit;

namespace Coop_Vr.Networking.ClientSide
{
    public class ClientMoveObj : Component
    {
        bool TryGetInputVal(out Vec3 pos)
        {
            Hand hand = Input.Hand(Handed.Right);
            pos = Vec3.Zero;

            if (!hand.IsTracked) return false;

            Vec3 thumbTip = hand[FingerId.Thumb, JointId.Tip].position;
            Vec3 ringTip = hand[FingerId.Ring, JointId.Tip].position;

            float distance = Vec3.Distance(thumbTip, ringTip);

            float touchThreshold = 0.02f; // 2 cm
                //Text.Add("Thumb is not touching the ring finger", Matrix.TS(0, 0, 0, 0.4f));

            if (distance > touchThreshold) return false;
                //Text.Add("Thumb is touching the ring finger", Matrix.TS(0, 0, 0, 0.4f));
            pos = ringTip;
            return true;
        }
        int times;
        public override void FixedUpdate()
        {
            //if(times++ <= 100)
            //{
            //    Log.Do("Pinch");
            //    gameObject.Transform.LocalPosition += Vec3.Right * .001f;
            //}

            if (!TryGetInputVal(out Vec3 pos)) return;

            gameObject.Transform.LocalPosition = pos;
        }

        //not gonna serialize
        public override void Deserialize(Packet pPacket)
        {
        }

        public override void Serialize(Packet pPacket)
        {
        }
    }
}
