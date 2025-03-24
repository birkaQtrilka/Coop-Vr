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

    public readonly struct SKObjectCreated : IEvent 
    {
        public readonly SkObject Obj;
        public readonly int ParentID; 
        public readonly int SenderID; //in case the object is created by a client
        public SKObjectCreated(SkObject obj, int parentID = -1, int senderID = MySettings.SERVER_ID)//-1 is the id of the root
        {
            Obj = obj;
            ParentID = parentID;
            SenderID = senderID;
        }
    }

    public readonly struct SKObjectAdded : IEvent
    {
        public readonly SkObject AddedObj;
        public readonly int NewParentID;
        public readonly int OldParentID; 
        public SKObjectAdded(SkObject obj, int oldParentID, int newParentId)//-1 is the id of the root
        {
            AddedObj = obj;
            OldParentID = oldParentID;
            NewParentID = newParentId;
        }
    }

    public readonly struct SKObjectRemoved : IEvent
    {
        public readonly SkObject RemovedObj;
        public SKObjectRemoved(SkObject obj)//-1 is the id of the root
        {
            RemovedObj = obj;
        }
    }

    public readonly struct SKObjectDestroyed : IEvent
    {
        public readonly SkObject DestroyedObj;

        public SKObjectDestroyed(SkObject obj)//-1 is the id of the root
        {
            DestroyedObj = obj;
        }
    }

    public class SKObjectGetter : IEvent
    {
        public readonly int ID;
        public Func<SkObject> ReturnedObj;

        public SKObjectGetter(int id)//-1 is the id of the root
        {
            ID = id;
        }
    }
}
