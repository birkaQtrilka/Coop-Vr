using System;
using System.Collections.Generic;
using System.Linq;

namespace Coop_Vr.Networking
{
    public class SkObject : ISerializable
    {
        public int ID { get; set; }
        public List<Component> Components;
        readonly List<SkObject> _children = new();

        public PosComponent Transform { get; private set; }

        int _parentID = -1;
        public int ParentID => _parentID;

        //use when just want to pass as serialized data
        public SkObject()
        {
            Components = new();
        }

        public SkObject(int parentID)
        {
            _parentID = parentID;
            Components = new();
        }

        //use when want to place in scene on creation
        public SkObject(int parentID = -1, List<Component> components = null)
        {
            components ??= new List<Component>() { new PosComponent() };

            Components = components;
            _parentID = parentID;
            EventBus<SKObjectCreated>.Publish(new SKObjectCreated(this, parentID));
        }


        public void Init()
        {
            Transform = GetComponent<PosComponent>();
            foreach (Component component in Components)
            {
                component.Init(this);
            }
        }

        public void Start()
        {
            foreach (Component component in Components)
            {
                component.Start();
            }
        }

        public void ForEach(Action<SkObject> method)
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                method(_children[i]);
            }
        }

        public void AddChild(SkObject obj, bool networked = true)
        {
            if (obj.ID == ID || obj == null) return;

            _children.Add(obj);
            if(networked) 
                EventBus<SKObjectAdded>.Publish(new SKObjectAdded(obj, obj._parentID, ID));

            obj.Transform.OnObjAdded();
        }

        public void RemoveChild(SkObject obj, bool networked = true)
        {
            _children.Remove(obj);
            if (networked)
                EventBus<SKObjectRemoved>.Publish(new SKObjectRemoved(obj));
            obj.Transform.OnObjRemoved(this);
        }

        public SkObject GetParent()
        {
            var getter = new SKObjectGetter(_parentID);
            EventBus<SKObjectGetter>.Publish(getter);
            return getter.ReturnedObj();
        }

        public void Deserialize(Packet pPacket)
        {
            ID = pPacket.ReadInt();
            Components = pPacket.ReadComponentsList();

            int count = pPacket.ReadInt();
            for (int i = 0; i < count; i++)
            {
                SkObject obj = new();
                obj.Deserialize(pPacket);
                _children.Add(obj);
                //don't add to scene
            }
            _parentID = pPacket.ReadInt();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.WriteComponentsList(Components);
            pPacket.Write(_children.Count);

            foreach (var obj in _children) obj.Serialize(pPacket);

            pPacket.Write(_parentID);
        }

        public T GetComponent<T>()
        {
            return Components.OfType<T>().First();
        }

        public void Update()
        {
            foreach (var component in Components) if (component.Enabled) component.Update();

            foreach (var child in _children) child.Update();
        }

        public void FixedUpdate()
        {
            foreach (var component in Components) if (component.Enabled) component.FixedUpdate();

            foreach (var child in _children) child.FixedUpdate();
        }
    }
}
