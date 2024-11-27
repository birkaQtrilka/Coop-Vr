using Coop_Vr.Networking.ServerSide;
using StereoKit;
using System.Collections.Generic;

namespace Coop_Vr.Networking.ClientSide.StateMachine.States
{
    public class GameView : Room<ClientStateMachine>
    {
        Pose windowPos = Pose.Identity;
        Dictionary<int, SkObject> objects = new();

        public GameView(ClientStateMachine context) : base(context)
        {

        }

        public override void OnEnter()
        {
            
        }

        public override void OnExit()
        {

        }

        public override void ReceiveMessage(IMessage message, TcpChanel sender)
        {
            if(message is CreateObjectResponse createdObject)
            {
                objects.Add(createdObject.NewObj.ID, createdObject.NewObj);
            }
        }

        public override void Update()
        {
            UI.WindowBegin("GameWindow", ref windowPos);
            UI.Label("GameView");
            UI.HSeparator();

            if(UI.Button("Add object sphere"))
            {
                context.SendMessage(new CreateObjectRequest() { 
                    Components = new List<ISerializable>() //really needs to be Icomponent (reference to object, )
                    {   
                        new PosComponent() { pose = new Pose(2,1,1)},
                        new MeshComponent() { MeshName = "sphere"}

                    }

                });
            }
            UI.HSeparator();

            if (UI.Button("Add object cube"))
            {
                context.SendMessage(new CreateObjectRequest()
                {
                    Components = new List<ISerializable>() //really needs to be Icomponent (reference to object, )
                    {
                        new PosComponent() { pose = new Pose(5,2,5)},
                        new MeshComponent() { MeshName = "cube"}

                    }

                });
            }

            UI.WindowEnd();

            foreach (var kv in objects) 
            {
                SkObject obj = kv.Value;
                MeshComponent meshC = obj.GetComponent<MeshComponent>();
                if (meshC.MeshName == "sphere")
                {
                    Model model = Model.FromMesh(Mesh.GenerateSphere(1f),Material.Default);
                    PosComponent pos = obj.GetComponent<PosComponent>();
                    model.Draw(Matrix.TR(pos.pose.position, pos.pose.orientation));
                }
                else if(meshC.MeshName == "cube")
                {
                    Model model = Model.FromMesh(Mesh.GenerateCube(Vec3.One*2), Material.Default);
                    PosComponent pos = obj.GetComponent<PosComponent>();
                    model.Draw(Matrix.TR(pos.pose.position, pos.pose.orientation));
                }
            }
        }
    }
}
