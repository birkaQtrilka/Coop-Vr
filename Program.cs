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
            //serverSettings
            SKSettings settings = new SKSettings
            {
                appName = "Coop_Vr",
                assetsFolder = "Assets",
                disableUnfocusedSleep = true,
                disableFlatscreenMRSim = true,
                flatscreenHeight = 1,
                flatscreenWidth = 1,
                disableDesktopInputWindow = true,
                
            };
            //SKSettings settings = new SKSettings
            //{
            //    appName = "Coop_Vr",
            //    assetsFolder = "Assets",
            //    disableUnfocusedSleep = true,
            //    flatscreenHeight = 700,
            //    flatscreenWidth = 700,
            //};
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            ServerStateMachine setup = new();

            while (SK.Step(() =>
            {
                if (Input.Key(Key.P) == BtnState.JustActive)
                    Log.Enabled = !Log.Enabled;
                setup.Update();
            })) ;
            SK.Shutdown();
            setup.StopRunning();
        }

        
    }
}
