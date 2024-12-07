﻿
using System;
using StereoKit;

namespace Coop_Vr.Networking
{
    public class ModelComponent : Component
    {
        public string MeshName;
        public Mesh mesh;
        public Material material;
        public Bounds bounds;


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
            if(MeshName == "sphere")
                mesh = Mesh.GenerateSphere(1.0f);
            
            else if (MeshName == "cube")
                mesh = Mesh.GenerateCube(new Vec3(1));

            bounds = mesh.Bounds;
            material = Material.Default;
        }

        public override void Update()
        {
            mesh.Draw(material, gameObject.Transform.pose.ToMatrix());
        }
    }
}
