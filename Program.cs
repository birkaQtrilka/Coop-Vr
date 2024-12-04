using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.ServerSide.StateMachine;
using Coop_Vr.Networking.ClientSide.Components ;
using StereoKit;
using System;

namespace Coop_Vr
{
    internal class Program
    {
       

        static void Main(string[] args)
        {
            string filePath = "D:\\saxion\\ACS\\2nd\\Honour\\Coop-Vr\\Assets\\Documents\\sample_data_3.csv";
            FileHandler fileHandler = new();
            
            // Read data from the file
            var graphPoints = fileHandler.ReadGraphPointsFromCsv(ref filePath);

            // Scale the graph points
            ScaleGraphPoints(graphPoints);

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
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "Coop_Vr",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            ClientStateMachine setup = new();

            while (SK.Step(() =>
            {
                setup.Update();
            })) ;
            SK.Shutdown();
            setup.StopRunning();
        }
    }
}
