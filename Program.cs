using Coop_Vr.Networking.ClientSide.StateMachine;
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
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            ClientStateMachine clientSetup = new();

            while (SK.Step(() =>
            {
                clientSetup.Update();
            })) ;
            SK.Shutdown();
        }

        
    }
}
