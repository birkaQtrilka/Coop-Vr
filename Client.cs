using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StereoKit;

namespace Coop_Vr
{
    internal class Client
    {
        //info about local host
        IPHostEntry ipEntry;
        //local host ip address
        IPAddress ip;

        IPEndPoint iPEndPoint;
        Action step;
        Socket client;

        public Client()
        {
            Console.WriteLine("Client trying to connect");

            //info about local host
            //ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            //local host ip address
            ip = IPAddress.Parse("192.168.1.157");
            // 192.168.1.236
            iPEndPoint = new(ip, 50160);
            //create client socket
            client = new(
               iPEndPoint.AddressFamily,
               SocketType.Stream,
               ProtocolType.Tcp
            );
            client.Connect(iPEndPoint);

            //client.Bind(iPEndPoint);
            //client.Listen();
            Random rnd = new Random();
            int month = rnd.Next(1, 13);
            Byte[] bytes = Encoding.UTF8.GetBytes("num:" + month.ToString());
            //String data = null;
            Console.WriteLine("accepted");

            step = () =>
            {
                //Console.WriteLine("client sending message!");
                client.Send(bytes, SocketFlags.None);

                byte[] buffer = new byte[1024];
                int received = client.Receive(buffer, SocketFlags.None);
                string msg = Encoding.UTF8.GetString(buffer, 0, received);
                Pose handlePose = new Pose(0, 0, 0, Quat.Identity);
                float scale = .20f;
                Mesh roundedCubeMesh = Mesh.GenerateRoundedCube(Vec3.One * 0.4f, 0.05f);
                Model roundedCubeModel = Model.FromMesh(roundedCubeMesh, Default.Material);

                Mesh sphereMesh = Mesh.GenerateSphere(1, 1);
                Model sphereModel = Model.FromMesh(sphereMesh, Default.Material);
                if (msg == "OK")
                {
                    UI.HandleBegin("roundedCubeModel", ref handlePose, roundedCubeModel.Bounds * 2);
                    roundedCubeModel.Draw(Matrix.S(scale));
                    UI.HandleEnd();
                }
                else if (msg == "Bad")
                {

                    UI.HandleBegin("sphereModel", ref handlePose, sphereModel.Bounds * 2);
                    sphereModel.Draw(Matrix.S(scale));
                    UI.HandleEnd();
                }

                //int reveived = client.Receive(buffer,SocketFlags.None);
                //Console.WriteLine("client received message: " + Encoding.UTF8.GetString(buffer)); 
            };
        }

        public void Step()
        {
            step.Invoke();
        }

        ~Client()
        {
            client.Dispose();
        }
    }
}
