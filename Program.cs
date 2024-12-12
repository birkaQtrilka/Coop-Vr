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
            SKSettings SKSettings = new SKSettings
            {
                appName = "3D Plot Demo",
                assetsFolder = "Assets",
                flatscreenWidth = 700

            };

            if (!SK.Initialize(SKSettings))
                Environment.Exit(1);

            ServerStateMachine setup = new();

            while (SK.Step(() =>
            {
                if (Input.Key(Key.P) == BtnState.JustActive)
                    Log.Enabled = !Log.Enabled;
                setup.Update();
            })) ;

            SK.Shutdown();
        }
    }
}
