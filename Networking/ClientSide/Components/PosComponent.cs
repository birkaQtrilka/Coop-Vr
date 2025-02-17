using StereoKit;
using System.Collections.Generic;

namespace Coop_Vr.Networking
{
    public class PosComponent : Component
    {
        public const int MAX_QUEUE_COUNT = 6;

        public Pose LocalPose
        {
            get => new(_localPosition, _localRotation);
            set
            {
                _localPosition = value.position;
                _localRotation = value.orientation;
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();

            }
        }

        Vec3 _localPosition = Vec3.Zero;
        public Vec3 LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();
            }
        }

        Vec3 _localScale = Vec3.One;
        public Vec3 LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value;
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();

            }
        }

        Quat _localRotation = Quat.Identity;
        public Quat LocalRotation
        {
            get => _localRotation;
            set
            {
                _localRotation = value;
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();

            }
        }

        public Vec3 WorldPosition
        {
            get => GetWorldMatrix().Translation;
            set
            {
                var parent = gameObject.GetParent()?.Transform;
                if (parent == null)
                {
                    _localPosition = value; // No parent, so world position is the local position.
                }
                else
                {
                    // Transform the world position to local space.
                    _localPosition = parent.GetWorldMatrix().Inverse.Transform(value);
                }
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();
            }
        }

        bool _modelMatrixDirty = true;
        bool _worldMatrixDirty = true;

        readonly Queue<Pose> _interpolationQueue = new();
        readonly double _time = MySettings.FixedUpdateDelay / 1000.0;
        double _currTime = 0;
        Pose _startPose = Pose.Identity;
        bool _isPlaying = false;
        
        Matrix _modelMatrix = Matrix.Identity;
        Matrix _cachedWorldMatrix = Matrix.Identity;

        public Matrix ModelMatrix
        {
            get
            {
                if(!_modelMatrixDirty) return _modelMatrix;
                
                _modelMatrix = Matrix.TRS(_localPosition, _localRotation, _localScale);
                _modelMatrixDirty = false;
                return _modelMatrix;
            }
        }

        public Matrix GetWorldMatrix()
        {
            if (!_worldMatrixDirty) return _cachedWorldMatrix;
            _worldMatrixDirty = false;

            var parent = gameObject.GetParent()?.Transform;

            _cachedWorldMatrix = ModelMatrix;
            if (parent == null) return ModelMatrix;
            _cachedWorldMatrix = ModelMatrix * parent.GetWorldMatrix();

            return _cachedWorldMatrix;
        }

        void PropagateWorldMatrixDirty()
        {
            _worldMatrixDirty = true;
            gameObject.ForEach(child => child.Transform.PropagateWorldMatrixDirty());
        }

        public override void Start()
        {
            OnObjAdded();
        }

        public void OnObjAdded()
        {
            var copy = WorldPosition;
            _worldMatrixDirty = true;
            WorldPosition = copy;

            gameObject.ForEach(child => child.Transform.OnObjAdded());

        }

        public void OnObjRemoved(SkObject from)
        {
            _worldMatrixDirty = true;
        }

        public override void Deserialize(Packet pPacket)
        {
            _localPosition = new Vec3(
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat()
            );
            _localRotation = new Quat(
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat()
            );
            _localScale = new Vec3(
                pPacket.ReadFloat(),
                pPacket.ReadFloat(),
                pPacket.ReadFloat()
            );
            _modelMatrixDirty = true;
            _worldMatrixDirty = true;
        }

        public override void Serialize(Packet pPacket)
        {
            var pos = LocalPosition;
            var rot = LocalRotation;
            var scale = LocalScale;

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

        public void QueueInterpolate(Pose p)
        {
            if(_interpolationQueue.Count > MAX_QUEUE_COUNT)
                _interpolationQueue.Dequeue();
            _interpolationQueue.Enqueue(p);
            _isPlaying = true;
            _startPose = LocalPose;
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
                LocalPose = Pose.Lerp(_startPose, _interpolationQueue.Peek(), (float)(_currTime / _time));
                return;
            }
            _startPose = LocalPose;
            _currTime = 0;
            _interpolationQueue.Dequeue();
            _isPlaying = _interpolationQueue.Count > 0;
        }
    }
}

