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

        public Pose WorldPose
        {
            get => GetWorldMatrix().Pose;
            set
            {
                var parent = gameObject.GetParent()?.Transform;
                if (parent == null)
                {
                    _localPosition = value.position; // No parent, so world position is the local position.
                    _localRotation = value.orientation; 
                }
                else
                {
                    // Transform the world position to local space.
                    Matrix worldInverse = parent.GetWorldMatrixInverse();
                    _localPosition = worldInverse.Transform(value.position);
                    _localRotation = worldInverse.Rotation * value.orientation;

                }
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
                    _localPosition = parent.GetWorldMatrixInverse().Transform(value);
                }
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();
            }
        }

        public Quat WorldRotation
        {
            get => GetWorldMatrix().Rotation;
            set
            {
                var parent = gameObject.GetParent()?.Transform;
                if (parent == null)
                {
                    _localRotation = value; // No parent, so world position is the local rotation.
                }
                else
                {
                    _localRotation = parent.GetWorldMatrixInverse().Rotation * value;
                }
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();
            }
        }

        public Vec3 WorldScale
        {
            get => GetWorldMatrix().Scale;
            set
            {
                var parent = gameObject.GetParent()?.Transform;
                if (parent == null)
                {
                    _localScale = value; // No parent, so world position is the local scale.
                }
                else
                {
                    _localScale = value / parent.GetWorldMatrix().Scale;
                }
                _modelMatrixDirty = true;
                PropagateWorldMatrixDirty();
            }
        }

        readonly Queue<Pose> _interpolationQueue = new();
        readonly double _time = MySettings.FixedUpdateDelay / 1000.0;
        double _currTime = 0;
        Pose _startPose = Pose.Identity;
        bool _isPlaying = false;
        
        Matrix _modelMatrix = Matrix.Identity;
        Matrix _cachedWorldMatrix = Matrix.Identity;
        Matrix _cachedInverseWorldMatrix = Matrix.Identity;
        bool _modelMatrixDirty = true;
        bool _worldMatrixDirty = true;
        bool _inverseWorldMatrixDirty = true;

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

        public Matrix GetWorldMatrixInverse()
        {
            if (!_inverseWorldMatrixDirty) return _cachedInverseWorldMatrix;
            _inverseWorldMatrixDirty = false;

            _cachedInverseWorldMatrix = GetWorldMatrix().Inverse;
            return _cachedInverseWorldMatrix;
        }

        void PropagateWorldMatrixDirty()
        {
            _worldMatrixDirty = true;
            _inverseWorldMatrixDirty = true;

            gameObject?.ForEach(child => child.Transform.PropagateWorldMatrixDirty());
        }
        public override void Start()
        {
            OnObjAdded();
        }

        public void OnObjAdded()
        {
            //sometimes this method is called right after the object is allocated with 'new'
            //the method is called in start to still allow proper initialization
            if (gameObject == null) return;

            var copy = WorldPosition;
            _worldMatrixDirty = true;
            WorldPosition = copy;
            GetWorldMatrixInverse();
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

