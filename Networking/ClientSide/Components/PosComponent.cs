using StereoKit;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coop_Vr.Networking
{
    public class PosComponent : Component
    {
        public Pose pose;
        public Vec3 scale = new(1, 1, 1);

        readonly Queue<Pose> _interpolationQueue = new();
        readonly double _time = .2f;
        double _currTime = 0;
        Pose _startPose = Pose.Identity;
        bool _isPlaying = false;

        public override void Deserialize(Packet pPacket)
        {
            pose = new(
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                new Quat(
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat()
                    )
                );
            scale = new(
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat()
                );
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(pose.position.x);
            pPacket.Write(pose.position.y);
            pPacket.Write(pose.position.z);

            pPacket.Write(pose.orientation.x);
            pPacket.Write(pose.orientation.y);
            pPacket.Write(pose.orientation.z);
            pPacket.Write(pose.orientation.w);

            pPacket.Write(scale.x);
            pPacket.Write(scale.y);
            pPacket.Write(scale.z);
        }

        public void QueueInterpolate(Pose p)
        {
            _interpolationQueue.Enqueue(p);
            _isPlaying = true;
            _startPose = pose;
            _currTime = 0;
        }

        public override void Update()
        {
            if (!_isPlaying) return;

            _currTime += Time.Step;
            if (_currTime < _time)
            {
                pose = Pose.Lerp(_startPose, _interpolationQueue.Peek(), (float)(_currTime / _time));
                return;
            }
            _startPose = pose;
            _currTime = 0;
            _interpolationQueue.Dequeue();
            _isPlaying = _interpolationQueue.Count > 0;
        }
    }
}

