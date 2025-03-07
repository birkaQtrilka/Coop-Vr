using System;

using StereoKit;
using System.Collections.Generic;
using System.Linq;

namespace Coop_Vr.Networking
{
    public class SkObject : ISerializable
    {
        public int ID;
        public List<Component> Components;
        readonly List<SkObject> _children = new();
        
        public PosComponent Transform { get; private set; }

        //use when just want to pass as serialized data
        public SkObject()
        {
            Components = new();
        }

        //use when want to place in scene on creation
        public SkObject(int parentID, List<Component> components)
        {
            Components = components;
            EventBus<SKObjectAdded>.Publish(new SKObjectAdded(this, parentID));
        }
        
        public void RemoveChild(SkObject child)
        {
            _children.Remove(child);
        }

        public void Init()
        {
            Transform = GetComponent<PosComponent>();
            foreach (Component component in Components)
            {
                component.Init(this);

            }
        }

        public void ForEach(Action<SkObject> method)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                method(_children[i]);
            }
        }



        public void AddChild(SkObject obj)
        {
            //TO DO:
            //check if not already child of parent
            //remove from parent
            //set parent as parent

            _children.Add(obj);
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
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(ID);
            pPacket.WriteComponentsList(Components);
            pPacket.Write(_children.Count);

            foreach (var obj in _children) obj.Serialize(pPacket);
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
