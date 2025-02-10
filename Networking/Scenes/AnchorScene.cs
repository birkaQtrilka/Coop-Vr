
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

            var createObjInstruction = new CreateObjectMsg() { NewObj = point, ParentID = -1 };

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                room.SafeForEachMember((m) => m.SendMessage(createObjInstruction));

            });
        }

        public override void OnStop()
        {

        }
    }
}
