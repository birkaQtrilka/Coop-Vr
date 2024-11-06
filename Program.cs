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


            // Create assets used by the app
            Pose cubePose = new Pose(0, 0, -0.5f, Quat.Identity);
            Model cube = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Default.MaterialUI);

            Matrix floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
            Material floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;

            Action stepper = Client();
            // Core application loop
            while (SK.Step(() =>
            {
                if (SK.System.displayType == Display.Opaque)
                    Default.MeshCube.Draw(floorMaterial, floorTransform);

                UI.Handle("Cube", ref cubePose, cube.Bounds);
                cube.Draw(cubePose.ToMatrix());

                Pose inputPos = Input.Hand(Handed.Right)[FingerId.Index, JointId.Tip].Pose;
                Text.Add(inputPos.position.ToString(), inputPos.ToMatrix());

                stepper?.Invoke();
            })) ;
            SK.Shutdown();
        }

        static Action Client()
        {
            //info about local host
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            //local host ip address
            IPAddress ip = ipEntry.AddressList[0];//

            IPEndPoint endPoint = new(ip, 1234);

            //create client socket
            using Socket client = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try {
                client.Connect(endPoint);
            }
            catch (Exception)
            {
                Console.WriteLine(" didn't find server");

                return new Server().Step;
            }
            return null;
        }

        
    }
}
