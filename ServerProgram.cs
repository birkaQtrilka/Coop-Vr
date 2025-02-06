using Coop_Vr.Networking.ClientSide.StateMachine;
using Coop_Vr.Networking.ServerSide.StateMachine;
using StereoKit;
using StereoKit.Framework;
using System;

namespace Coop_Vr
{

    internal class ServerProgram
    {
        public static PassthroughFBExt _passthroughFBExt;

        static void Main(string[] args)
        {
            ServerOrClientSetup(isClient: false);
        }

        static void ServerOrClientSetup(bool isClient)
        {
            if (isClient)
            {
                _passthroughFBExt = SK.AddStepper(new PassthroughFBExt());

                SKSettings settings = new SKSettings
                {
                    appName = "Coop_Vr Client",
                    assetsFolder = "Assets",
                    disableUnfocusedSleep = true,
                    flatscreenHeight = 700,
                    flatscreenWidth = 700,
                    mode = AppMode.XR

                };

                if (!SK.Initialize(settings))
                    Environment.Exit(1);

                ClientStateMachine setup = new();

                while (SK.Step(() =>
                {
                    if (!_passthroughFBExt.EnabledPassthrough)
                        _passthroughFBExt.EnabledPassthrough = true;

                    if (Input.Key(Key.P) == BtnState.JustActive)
                        Log.Enabled = !Log.Enabled;
                    setup.Update();
                })) ;

                SK.Shutdown();
                setup.StopRunning();
            }
            else
            {
                //serverSettings
                SKSettings settings = new SKSettings
                {
                    appName = "Server",
                    assetsFolder = "Assets",
                    disableUnfocusedSleep = true,
                    disableFlatscreenMRSim = true,
                    flatscreenHeight = 10,
                    flatscreenWidth = 10,
                    disableDesktopInputWindow = true,

                };

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
}
