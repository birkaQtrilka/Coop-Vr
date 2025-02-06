using System;

namespace Coop_Vr.Networking
{
    public static class EventBus<T> where T : IEvent
    {
        public static event Action<T> Event;

        public static void Publish(T args)
        {
            Event?.Invoke(args);
        }
    }

    public interface IEvent { }

    public readonly struct SKObjectAdded : IEvent 
    {
        public readonly SkObject Obj;
        public readonly int ParentID; 
        public SKObjectAdded(SkObject obj, int parentID = -1)//-1 is the id of the root
        {
            Obj = obj;
            ParentID = parentID;
        }
    }
}
