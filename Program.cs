using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.ServerSide.StateMachine;
using Coop_Vr.Networking.ClientSide.Components;
using StereoKit;
using System;
using System.Collections.Generic;

namespace Coop_Vr
{

    internal class Program
    {
        public class Visualizer
        {
            private List<GraphPoint> graphPoints;

            public Visualizer(List<GraphPoint> points)
            {
                graphPoints = points;
            }

            public void Render()
            {
                var sphereTransforms = new Dictionary<GraphPoint, Pose>();
                foreach (var point in graphPoints)
                {
                    sphereTransforms[point] = new Pose(new Vec3(point.X, point.Y, point.Z), Quat.Identity);
                }
                Mesh mesh = Mesh.GenerateSphere(1f);
                Model sphere = Model.FromMesh(mesh, Default.MaterialUnlit);


                while (SK.Step(() =>
                {
                    Program.DrawAxes(20f);

                    foreach (var point in graphPoints)
                    {
                        Pose spherePose = sphereTransforms[point];
                        float scale = Math.Clamp(point.Y / 10.0f, 0.5f, 2.0f);
                        Color color = Color.HSV((point.Z + 10) / 20.0f, 1.0f, 1.0f);

                        //UI.Handle($"Sphere-{point.Label}", ref spherePose, new Bounds(Vec3.One * scale));
                        
                        sphere.Draw(Matrix.TRS(spherePose.position, Quat.Identity, scale), color);

                        //Vec3 labelPosition = spherePose.position + new Vec3(0, scale + 0.01f, 0);
                        //Text.Add(point.Label, Matrix.TRS(labelPosition, Quat.FromAngles(0, 180, 0), 10f), TextAlign.TopCenter);

                        //Vec3 coordPosition = spherePose.position + new Vec3(0, -scale - 0.01f, 0);
                        //string coordinates = $"({spherePose.position.x:F1}, {spherePose.position.y:F1}, {spherePose.position.z:F1})";
                        //Text.Add(coordinates, Matrix.TRS(coordPosition, Quat.FromAngles(0, 180, 0), 5f), TextAlign.BottomCenter);
                    }
                })) { }
            }
        }

        static void Main(string[] args)
        {
            string filePath = "D:\\saxion\\ACS\\2nd\\Honour\\Coop-Vr\\Assets\\Documents\\sample_data_3.csv";

            // Create a FileHandler instance
            var fileHandler = new FileHandler(filePath);

            // Read graph points from the CSV file
            var graphPoints = fileHandler.ReadGraphPointsFromCsv();

            // Scale the graph points
            fileHandler.ScaleGraphPoints(graphPoints, 100f);

            // Output scaled data
            foreach (var point in graphPoints)
            {
                Console.WriteLine($"X: {point.X}, Y: {point.Y}, Z: {point.Z}");
                Console.WriteLine("Extra Info:");
                foreach (var info in point.ExtraInfo)
                {
                    Console.WriteLine($"{info.Key}: {info.Value}");
                }
            }

            if (!SK.Initialize(new SKSettings { appName = "3D Plot Demo", assetsFolder = "Assets" }))
                Environment.Exit(1);

            var visualizer = new Visualizer(graphPoints);
            visualizer.Render();

            SK.Shutdown();
        }
        private static void DrawAxes(float scale)
        {
            Lines.Add(Vec3.Zero, new Vec3(scale, 0, 0), Color.White, 0.01f); // X-axis
            Text.Add("X", Matrix.TRS(new Vec3(scale, 0, 0), Quat.Identity, 5f), TextAlign.BottomCenter);

            Lines.Add(Vec3.Zero, new Vec3(0, scale, 0), Color.White, 0.01f); // Y-axis
            Text.Add("Y", Matrix.TRS(new Vec3(0, scale, 0), Quat.Identity, 5f), TextAlign.BottomCenter);

            Lines.Add(Vec3.Zero, new Vec3(0, 0, scale), Color.White, 0.01f); // Z-axis
            Text.Add("Z", Matrix.TRS(new Vec3(0, 0, scale), Quat.Identity, 5f), TextAlign.BottomCenter);
        }
    }
}
