using StereoKit;
using System.Collections.Generic;

namespace Coop_Vr.Networking
{
    public class PosComponent : Component
    {
        public const int MAX_QUEUE_COUNT = 6;

        public Pose Pose
        {
            get => _modelMatrix.Pose;
            set
            {
                _modelMatrix.Decompose(out _, out _, out var s);
                _modelMatrix = Matrix.TRS(value.position, value.orientation, s);
                UpdateMatrix();
            }
        }

        public Vec3 Position
        {
            get => _modelMatrix.Translation;
            set
            {
                _modelMatrix.Translation = value;
                UpdateMatrix();
            }
        }

        public Vec3 Scale
        {
            get => _modelMatrix.Scale;
            set
            {
                _modelMatrix.Decompose(out var p, out var r, out _);
                _modelMatrix = Matrix.TRS(p, r, value);
                UpdateMatrix();
            }
        }

        public Quat Rotation
        {
            get => _modelMatrix.Rotation;
            set
            {
                _modelMatrix.Decompose(out var p, out _, out var s);
                _modelMatrix = Matrix.TRS(p, value, s);
                UpdateMatrix();
            }
        }

        readonly Queue<Pose> _interpolationQueue = new();
        readonly double _time = MySettings.FixedUpdateDelay / 1000.0;
        double _currTime = 0;
        Pose _startPose = Pose.Identity;
        bool _isPlaying = false;

        public Matrix ModelMatrix => _modelMatrix;
        Matrix _modelMatrix = Matrix.Identity;


        void UpdateMatrix(PosComponent parent = null)
        {
            parent ??= gameObject?.GetParent()?.Transform;
            if(parent == null)
            {
                _modelMatrix = Matrix.Identity;
                return;
            }
            _modelMatrix = parent.ModelMatrix * ModelMatrix;

            gameObject.ForEach(obj => obj.Transform.UpdateMatrix(this));
        }

        public void OnObjAdded()
        {
            UpdateMatrix();
        }

        public void OnObjRemoved(SkObject from)
        {
            _modelMatrix = from.Transform.ModelMatrix.Inverse * ModelMatrix;
        }

        public void QueueInterpolate(Pose p)
        {
            if (_interpolationQueue.Count > MAX_QUEUE_COUNT)
                _interpolationQueue.Dequeue();
            _interpolationQueue.Enqueue(p);
            _isPlaying = true;
            _startPose = Pose;
            _currTime = 0;

        }

        public bool QueueIsEmpty()
        {
            return _interpolationQueue.Count == 0;
        }

        public override void Update()
        {
            if (!_isPlaying) return;

            _currTime += Time.Step;
            if (_currTime < _time)
            {
                Pose = Pose.Lerp(_startPose, _interpolationQueue.Peek(), (float)(_currTime / _time));
                return;
            }
            _startPose = Pose;
            _currTime = 0;
            _interpolationQueue.Dequeue();
            _isPlaying = _interpolationQueue.Count > 0;
        }

        public override void Deserialize(Packet pPacket)
        {
            _modelMatrix = Matrix.TRS(
                new Vec3(
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat()
                    ),
                new Quat(
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat()
                    ),
                new Vec3(
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat(),
                    pPacket.ReadFloat()
                    )
                );

        }

        public override void Serialize(Packet pPacket)
        {
            var pos = Position;
            var rot = Rotation;
            var scale = Scale;

            pPacket.Write(pos.x);
            pPacket.Write(pos.y);
            pPacket.Write(pos.z);

            pPacket.Write(rot.x);
            pPacket.Write(rot.y);
            pPacket.Write(rot.z);
            pPacket.Write(rot.w);

            pPacket.Write(scale.x);
            pPacket.Write(scale.y);
            pPacket.Write(scale.z);
        }
        
    }
}

