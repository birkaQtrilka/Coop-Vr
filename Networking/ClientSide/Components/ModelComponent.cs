
using System;
using System.Collections.Generic;
using StereoKit;

namespace Coop_Vr.Networking
{
    public class ModelComponent : Component
    {

        public static readonly Dictionary<string, Mesh> _cachedMeshes = new();
        //serializable
        public string MeshName;
        //not serializable
        public Mesh mesh;
        public Material material;
        public Bounds bounds;
        public Color color = Color.White;

        public override void Deserialize(Packet pPacket)
        {
            MeshName = pPacket.ReadString();
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(MeshName);
        }

        public override void Start()
        {
            if (MeshName == "sphere")
            {
                if (!_cachedMeshes.ContainsKey(MeshName))
                    _cachedMeshes.Add(MeshName, Mesh.GenerateSphere(1.0f));

                mesh = _cachedMeshes[MeshName];

            }
            else if (MeshName == "cube")
            {
                if (!_cachedMeshes.ContainsKey(MeshName))
                    _cachedMeshes.Add(MeshName, Mesh.GenerateCube(new Vec3(1)));

                mesh = _cachedMeshes[MeshName];
            }


            bounds = mesh.Bounds;
            material = Material.Default;
        }

        public override void Update()
        {
            Pose p = gameObject.Transform.pose;
            Vec3 s = gameObject.Transform.scale;

            mesh.Draw(material, Matrix.TRS(p.position, p.orientation, s), color);
        }
    }
}
