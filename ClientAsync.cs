using StereoKit;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr
{
    class ClientAsync
    {
        IPAddress ip;

        IPEndPoint iPEndPoint;
        Func<Task> step;
        Socket client;

        public ClientAsync()
        {
            _ = Init();
        }

        async Task Init()
        {
            //object based communication json or something else
            //serialize deserialize
            //udp only for continuosly sync position or more unimportant repeating data
            //tcp for important events
            //use tcp for everything FIRST
            //biggest ip tricks
            
            ip = IPAddress.Parse("192.168.1.157");
            iPEndPoint = new(ip, 50160);

            client = new(
               iPEndPoint.AddressFamily,
               SocketType.Stream,
               ProtocolType.Tcp
            );
            Task connectedTask = client.ConnectAsync(iPEndPoint);

            Random rnd = new();
            int month = rnd.Next(1, 13);
            Byte[] bytes = Encoding.UTF8.GetBytes("num:" + month.ToString());

            Pose handlePose = new(0, 0, 0, Quat.Identity);
            float scale = .2f;
            Mesh roundedCubeMesh = Mesh.GenerateRoundedCube(Vec3.One * 0.4f, 0.05f);
            Model roundedCubeModel = Model.FromMesh(roundedCubeMesh, Default.Material);

            Mesh sphereMesh = Mesh.GenerateSphere(1, 1);
            Model sphereModel = Model.FromMesh(sphereMesh, Default.Material);

            step = async () =>
            {
                if (!connectedTask.IsCompleted) return;

                await client.SendAsync(bytes, SocketFlags.None);

                byte[] buffer = new byte[1024];
                int received = await client.ReceiveAsync(buffer, SocketFlags.None);

                string msg = Encoding.UTF8.GetString(buffer, 0, received);

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

            };

            while (!connectedTask.IsCompleted)
            {
                await Task.Delay(200);
                Console.WriteLine("Searching for Server!");

            }
        }

        public void Step()
        {
            _ = step?.Invoke();
        }

        ~ClientAsync()
        {
            client.Dispose();
        }
    }
}
