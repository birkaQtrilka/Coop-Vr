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
        public const float INVERSE_POSITION_SCALE = .1f;
        public const float POSITION_SCALE = 10f;
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

            gameObject.Transform.pose = new Pose(new Vec3(X, Y, Z) * INVERSE_POSITION_SCALE);
            gameObject.Transform.scale = new Vec3(.1f);
        }

        private float CalculateScale()
        {
            return Math.Clamp(Y / 50.0f, 0.1f, 2.0f);
        }

        // Class-level variables to track popup state
        private bool showPopup = false;

        public override void Update()
        {
            // Get the sphere's position and transform information
            PosComponent spherePose = gameObject.Transform;
            Vec3 spherePosition = spherePose.pose.position;
            Vec3 sphereScale = spherePose.scale;

            // Display label and coordinates with positions based on sphere scale
            RenderLabels(spherePosition, sphereScale);

            // Handle the information button
            HandleInfoButton(spherePosition);

            // Handle popup display if needed
            if (showPopup)
            {
                DisplayPopup(spherePosition, sphereScale);
            }

            // Update model color based on position
            UpdateModelColor(spherePosition);
        }

        private void RenderLabels(Vec3 position, Vec3 scale)
        {
            // Project name label above the sphere
            string label = ExtraInfo.GetValueOrDefault("Project1", "Project1");
            Text.Add(label,
                     Matrix.TR(position + new Vec3(0, scale.y, 0), Quat.FromAngles(0, 180, 0)),
                     TextAlign.TopCenter);

            // Coordinates label below the sphere
            string coordinates = $"({position.x:F1}, {position.y:F1}, {position.z:F1})";
            Text.Add(coordinates,
                     Matrix.TR(position + new Vec3(0, -scale.y, 0), Quat.FromAngles(0, 180, 0)),
                     TextAlign.BottomCenter);
        }

        private void HandleInfoButton(Vec3 position)
        {
            // Define consistent button parameters
            Vec3 buttonPosition = position + new Vec3(0.001f, -0.02f, 0.001f);
            Vec2 buttonSize = new Vec2(0.05f, 0.05f);
            Quat buttonRotation = Quat.FromAngles(0, 180, 0);

            // Create a transformation matrix that includes position and rotation
            Matrix buttonTransform = Matrix.TR(buttonPosition, buttonRotation);

            // Begin a UI area with the transformation
            UI.PushSurface(new Pose(buttonPosition, buttonRotation));

            // Create the button
            bool clicked = UI.Button("Info", buttonSize);

            // End the UI area
            UI.PopSurface();

            // Handle button click
            if (clicked)
            {
                showPopup = true;
            }
        }

        private void DisplayPopup(Vec3 position, Vec3 scale)
        {
            // Position the popup in front of the sphere
            Vec3 popupPosition = position + new Vec3(0.15f, scale.y + 0.5f, 0.15f);
            Quat popupRotation = Quat.FromAngles(0, 180, 0);
            Pose popupPose = new Pose(popupPosition, popupRotation);

            UI.WindowBegin("Object Information", ref popupPose);

            // Project coordinates information
            UI.Label($"Max Capacity: {ExtraInfo.GetValueOrDefault("MaxCapcity", "")}");
            UI.Label($"X - Capacity Project 1: {ExtraInfo.GetValueOrDefault("Capacity1", "Unknown")}");
            UI.Label($"Y - Influence Score: {ExtraInfo.GetValueOrDefault("Score", "Unknown")}");
            UI.Label($"Z - Investment Project 1: {ExtraInfo.GetValueOrDefault("Investment1", "Unknown")}");
            UI.HSeparator();

            // Project 1 details
            DisplayProjectDetails("1");
            UI.HSeparator();

            // Project 2 details
            DisplayProjectDetails("2");

            // Close button
            if (UI.Button("Close"))
            {
                showPopup = false;
            }

            UI.WindowEnd();
        }

        private void DisplayProjectDetails(string projectNum)
        {
            UI.Label($"Project {projectNum}: {ExtraInfo.GetValueOrDefault($"Project{projectNum}", "Unknown")}");
            UI.Label($"Capacity Project {projectNum}: {ExtraInfo.GetValueOrDefault($"Capacity{projectNum}", "Unknown")}");
            UI.Label($"Investment Project {projectNum}: {ExtraInfo.GetValueOrDefault($"Investment{projectNum}", "Unknown")}");
            UI.Label($"Date Project {projectNum}: {ExtraInfo.GetValueOrDefault($"Date{projectNum}", "Unknown")}");
            UI.Label($"Technology Project {projectNum}: {ExtraInfo.GetValueOrDefault($"Technology{projectNum}", "Unknown")}");
        }

        private void UpdateModelColor(Vec3 position)
        {
            // Update position variables and set color based on Z position
            X = position.x * POSITION_SCALE;
            Y = position.y * POSITION_SCALE;
            Z = position.z * POSITION_SCALE;
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
