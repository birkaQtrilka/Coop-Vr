using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.ServerSide.StateMachine;
using StereoKit;
using System;
using System.Collections.Generic;
using Coop_Vr.Networking.ServerSide.Components;

namespace Coop_Vr
{

    internal class Program
    {
        

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

            var visualizer = new Graph(graphPoints);
            visualizer.Render();

            SK.Shutdown();
        }
    }
}
