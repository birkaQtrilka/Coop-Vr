using StereoKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coop_Vr.Networking.ServerSide.Components
{
    public class GraphPoint : Component
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Dictionary<string, string> ExtraInfo { get; set; }

        //not serializable
        public ModelComponent model;

        // Factory method to create a GraphPoint from a CSV record
        public static GraphPoint FromCsvRecord(IDictionary<string, object> record)
        {
            var graphPoint = new GraphPoint
            {
                X = record.ContainsKey("X") ? float.Parse(record["X"].ToString(), CultureInfo.InvariantCulture) : 0,
                Y = record.ContainsKey("Y") ? float.Parse(record["Y"].ToString(), CultureInfo.InvariantCulture) : 0,
                Z = record.ContainsKey("Z") ? float.Parse(record["Z"].ToString(), CultureInfo.InvariantCulture) : 0,
                ExtraInfo = new Dictionary<string, string>()
            };

            foreach (var key in record.Keys)
            {
                if (key != "X" && key != "Y" && key != "Z")
                {
                    graphPoint.ExtraInfo[key] = record[key]?.ToString();
                }
            }

            return graphPoint;
        }

        public override void Start()
        {
            model = gameObject.GetComponent<ModelComponent>();
            gameObject.Transform.pose.position = new Vec3 (X, Y, Z);
        }

        private float CalculateScale()
        {
            return Math.Clamp(Y / 10.0f, 0.5f, 2.0f);
        }

        //render code
        public override void Update()
        {
            float scale = CalculateScale();
            model.color = Color.HSV((Z + 10) / 20.0f, 1.0f, 1.0f);
            PosComponent spherePose = gameObject.Transform;

            UI.Handle($"Sphere-{ExtraInfo.GetValueOrDefault("Country", "Unknown")}", ref spherePose.pose, new Bounds(Vec3.One * scale));

            Vec3 labelPosition = spherePose.pose.position + new Vec3(0, scale + 0.01f, 0);
            string label = ExtraInfo.GetValueOrDefault("Country", "Point");
            Text.Add(label, Matrix.TRS(labelPosition, Quat.FromAngles(0, 180, 0), 10f), TextAlign.TopCenter);

            Vec3 coordPosition = spherePose.pose.position + new Vec3(0, -scale - 0.01f, 0);
            string coordinates = $"({spherePose.pose.position.x:F1}, {spherePose.pose.position.y:F1}, {spherePose.pose.position.z:F1})";
            Text.Add(coordinates, Matrix.TRS(coordPosition, Quat.FromAngles(0, 180, 0), 5f), TextAlign.BottomCenter);
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(X);
            pPacket.Write(Y);
            pPacket.Write(Z);

            pPacket.Write(ExtraInfo.Count());
            foreach (var item in ExtraInfo)
            {
                pPacket.Write(item.Key);
                pPacket.Write(item.Value);
            }
        }

        public override void Deserialize(Packet pPacket)
        {
            X = pPacket.ReadFloat();
            Y = pPacket.ReadFloat();
            Z = pPacket.ReadFloat();
            int count = pPacket.ReadInt();
            ExtraInfo = new Dictionary<string, string>(count);
            for (int i = 0; i < count; i++)
            {
                ExtraInfo.Add(pPacket.ReadString(), pPacket.ReadString());
            }
        }
    }
}
