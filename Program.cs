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


            //Action stepper = new Server().Step;
            //Action stepper = new ClientAsync().Step;
            //var stepper = new ServerAsync().Step;
            Action stepper = new ServerBuff().Step;

            while (SK.Step(() =>
            {
                stepper?.Invoke();
            })) ;
            SK.Shutdown();
        }

        
    }
}
