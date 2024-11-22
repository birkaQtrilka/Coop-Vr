using StereoKit;
using System;
using System.Collections.Generic;

namespace StereoKitDataPlotter
{
    public class DataPoint
    {
        public string Label { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // Constructor to initialize DataPoint from CSV values
        public DataPoint(string label, float x, float y, float z)
        {
            Label = label;
            X = x;
            Y = y;
            Z = z;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize the StereoKit application
            if (!SK.Initialize(new SKSettings
            {
                appName = "3D Plot Demo",
                assetsFolder = "Assets",
            }))
            {
                Environment.Exit(1);
            }

            // Load the sample data
            var dataPoints = GenerateSampleData();

            // Main application loop
            while (SK.Step(() =>
            {
                // Plot each point in 3D space
                foreach (var point in dataPoints)
                {
                    // Create position from DataPoint coordinates
                    Vec3 position = new Vec3(point.X, point.Y, point.Z);

                    // Example: Color based on Y value (this is optional)
                    Color color = Color.HSV((point.Y + 5) / 10.0f, 1.0f, 1.0f);

                    // Create a sphere model and draw it at the data point's position
                    Model sphere = Model.FromMesh(Mesh.GenerateSphere(1f), Default.MaterialUnlit);
                    sphere.Draw(Matrix.TS(position, 1f), color);

                    // Draw the label using the Text class
                    if (!string.IsNullOrEmpty(point.Label))
                    {
                        // Draw the label near the point's coordinates
                        //UI.Label(point.Label);
                    }
                }
            }))
            {
            }

            // Shutdown the application
            SK.Shutdown();
        }

        private static List<DataPoint> GenerateSampleData()
        {
            var dataPoints = new List<DataPoint>
    {
        new DataPoint("P0", 0.0f, 0.0f, 0.0f),
        new DataPoint("P1", 1.5f, 0.5f, 0.5f),
        new DataPoint("P2", 1.0f, 1.0f, 2.0f),
        new DataPoint("P3", 1.5f, 0.5f, 3.0f),
        new DataPoint("P4", 2.0f, 0.0f, 4.0f),
        new DataPoint("P5", 1.5f, -0.5f, 5.0f),
        new DataPoint("P6", 1.0f, -1.0f, 6.0f),
        new DataPoint("P7", 0.5f, -0.5f, 7.0f),
        new DataPoint("P8", 0.0f, 0.0f, 8.0f),
        new DataPoint("P9", -0.5f, 0.5f, 9.0f),
    };

            return dataPoints;
        }


    }
}
