using System;
using System.Collections.Generic;

namespace Coop_Vr.Networking
{
    public abstract class Room<T>
    {
        protected Room(T context)
        {
            this.context = context;
        }
        protected T context;

        readonly List<TcpChanel> _members = new();
        public T Context => context;

        public abstract void ReceiveMessage(IMessage message, TcpChanel sender);
        public abstract void Update();
        public virtual void FixedUpdate() { }
        public abstract void OnEnter();
        public abstract void OnExit();

        public void AddMember(TcpChanel member)
        {
            _members.Add(member); 
        }
        public void RemoveMember(TcpChanel member)
        {
            _members.Remove(member);
        }

        public int MemberCount()
        {
            return _members.Count; 
        }

        public void SafeForEachMember(Action<TcpChanel> method)
        {
            for (int i = _members.Count - 1; i >= 0; i--)
            {
                if (i >= _members.Count) continue;
                method(_members[i]);
            }
        }
    }
}
