using StereoKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coop_Vr.Networking.ServerSide.Components
{
    //bad part about it is that it has duplicate memory (XYZ here  and position in PosComponent)
    //can be optimized by storing position data in posComponent
    public class GraphPoint : Component
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Dictionary<string, string> ExtraInfo { get; set; } = new Dictionary<string, string>();

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

            gameObject.Transform.pose = new Pose(new Vec3(X, Y, Z));
            gameObject.Transform.scale = new Vec3(.1f);
        }

        private float CalculateScale()
        {
            return Math.Clamp(Y / 50.0f, 0.1f, 2.0f);
        }

        //render code
        // Class-level variables to track popup state
        private bool showPopup = false;
        //private float popupTimer = 0f;
        //private const float POPUP_DURATION = 5.0f; // Show popup for 5 seconds

        public override void Update()
        {
            // Get the sphere's position and transform
            PosComponent spherePose = gameObject.Transform;
            Vec3 spherePosition = spherePose.pose.position;
            Vec3 sphereScale = spherePose.scale;

            // Display label and coordinates
            Vec3 labelPosition = spherePosition + new Vec3(0, sphereScale.y, 0);
            string label = ExtraInfo.GetValueOrDefault("Project1", "Project1");
            Text.Add(label, Matrix.TR(labelPosition, Quat.FromAngles(0, 180, 0)), TextAlign.TopCenter);

            Vec3 coordPosition = spherePosition + new Vec3(0, -sphereScale.y, 0);
            string coordinates = $"({spherePosition.x:F1}, {spherePosition.y:F1}, {spherePosition.z:F1})";
            Text.Add(coordinates, Matrix.TR(coordPosition, Quat.FromAngles(0, 180, 0)), TextAlign.BottomCenter);

            // Define the position of the button relative to the sphere
            Vec3 buttonPosition = spherePosition + new Vec3(0.2f, -0.1f, -0.2f);

            // Use consistent rotation for the button
            Quat buttonRotation = Quat.FromAngles(0, 180, 0); // Rotate 180 degrees to face backward

            // Define the size of the button and scale it
            Vec2 buttonSize = new Vec2(0.1f, 0.05f); // Width and Height
            Vec3 buttonScale = new Vec3(buttonSize.x, buttonSize.y, 1.0f); // Scale in 3D space

            Matrix buttonTransform = Matrix.TRS(buttonPosition, buttonRotation, buttonScale);

            // Handle button click

            // Handle button click
            if (UI.ButtonAt("More Info", buttonPosition, buttonSize))
            {
                showPopup = true; // Start showing popup
            }


            // Handle popup display with timer
            if (showPopup)
            {
                // Position the popup in front of the sphere
                Vec3 popupPosition = spherePosition + new Vec3(0.5f, sphereScale.y + 0.5f, -0.5f);
                // Use consistent rotation for popup
                Quat popupRotation = Quat.FromAngles(0, 180, 0);
                Pose popupPose = new Pose(popupPosition, popupRotation);

                UI.WindowBegin("Object Information", ref popupPose);
                UI.Label($"Max Capacity: {ExtraInfo.GetValueOrDefault("MaxCapcity", "")}");
                UI.Label($"X - Capacity Project 1: {ExtraInfo.GetValueOrDefault("Capacity1", "Unknown")}");
                UI.Label($"Y - Influence Score: {ExtraInfo.GetValueOrDefault("Score", "Unknown")}");
                UI.Label($"Z - Investment Project 1: {ExtraInfo.GetValueOrDefault("Investment1", "Unknown")}");
                UI.HSeparator();
                UI.Label($"Project 1: {ExtraInfo.GetValueOrDefault("Project1", "Unknown")}");
                UI.Label($"Capacity Project 1: {ExtraInfo.GetValueOrDefault("Capacity1", "Unknown")}");
                UI.Label($"Investment Project 1: {ExtraInfo.GetValueOrDefault("Investment1", "Unknown")}");
                UI.Label($"Date Project 1: {ExtraInfo.GetValueOrDefault("Date1", "Unknown")}");
                UI.Label($"Technology Project 1: {ExtraInfo.GetValueOrDefault("Technology1", "Unknown")}");
                UI.HSeparator();
                UI.Label($"Project 2: {ExtraInfo.GetValueOrDefault("Project2", "Unknown")}");
                UI.Label($"Capacity Project 2: {ExtraInfo.GetValueOrDefault("Capacity2", "Unknown")}");
                UI.Label($"Investment Project 2: {ExtraInfo.GetValueOrDefault("Investment2", "Unknown")}");
                UI.Label($"Date Project 2: {ExtraInfo.GetValueOrDefault("Date2", "Unknown")}");
                UI.Label($"Technology Project 2: {ExtraInfo.GetValueOrDefault("Technology2", "Unknown")}");

                // Add a close button to the popup
                if (UI.Button("Close"))
                {
                    showPopup = false; // Stop showing popup
                }
                UI.WindowEnd();
            }

            // Change the color of the model based on some condition
            X = spherePosition.x;
            Y = spherePosition.y;
            Z = spherePosition.z;
            model.color = Color.HSV((Z + 10) / 20.0f, 1.0f, 1.0f);
        }




        //public override void Update()
        //{
        //    //float scale = CalculateScale();

        //    model.color = Color.HSV((Z + 10) / 20.0f, 1.0f, 1.0f);
        //    PosComponent spherePose = gameObject.Transform;

        //    Vec3 labelPosition = spherePose.pose.position + new Vec3(0, 1f * spherePose.scale.y,0) ;
        //    string label = ExtraInfo.GetValueOrDefault("Country1", "Point");
        //    Text.Add(label, Matrix.TR(labelPosition, Quat.FromAngles(0, 180, 0)), TextAlign.TopCenter);

        //    Vec3 coordPosition = spherePose.pose.position + new Vec3(0,  -1f * spherePose.scale.y, 0);
        //    string coordinates = $"({spherePose.pose.position.x:F1}, {spherePose.pose.position.y:F1}, {spherePose.pose.position.z:F1})";
        //    Text.Add(coordinates, Matrix.TR(coordPosition, Quat.FromAngles(0, 180, 0)), TextAlign.BottomCenter);

        //    //changing data

        //    Vec3 modelSpace = spherePose.pose.position;
        //    //modelMatrix.Translation = modelSpace;
        //    X = modelSpace.x;
        //    Y = modelSpace.y;
        //    Z = modelSpace.z;
        //}

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
