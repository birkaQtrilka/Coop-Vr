
using Coop_Vr.Networking.ClientSide;
using Coop_Vr.Networking.ServerSide.Components;
using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.Scenes
{
    public class AnchorScene : Scene
    {
        public AnchorScene(GameRoom room) : base(room)
        {
        }

        public override void OnStart()
        {
            var point = new SkObject
                (
                    components:
                    new List<Component>()
                    {
                        new PosComponent(),
                        new GraphPoint(),
                        new ModelComponent()
                        {
                            MeshName = "sphere",
                        },
                        new Move()
                    }
                );

            var point2 = new SkObject
                (
                    parentID: point.ID,
                    components:
                    new List<Component>()
                    {
                        new PosComponent(){Position = StereoKit.Vec3.Forward},
                        new GraphPoint(),
                        new ModelComponent()
                        {
                            MeshName = "cube",
                        },
                        new Move()
                    }
                );
            //TO DO:
            //Automatically add to root DONE
            //Send message on add child
        }

        public override void OnStop()
        {

        }
    }
}
