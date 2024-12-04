using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.ServerSide.StateMachine;
using StereoKit;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Coop_Vr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "Coop_Vr",
                assetsFolder = "Assets",
                flatscreenWidth = 10
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            ServerStateMachine setup = new();

            while (SK.Step(() =>
            {
                setup.Update();
            })) ;
            SK.Shutdown();
            setup.StopRunning();
        }

        
    }
}
