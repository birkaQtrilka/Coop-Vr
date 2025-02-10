using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking
{
    public abstract class Scene
    {
        protected GameRoom room {  get; }

        public Scene(GameRoom room)
        {
            this.room = room;
        }

        public abstract void OnStart();
        public abstract void OnStop();
    }
}
