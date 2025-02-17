using System;

namespace Coop_Vr.Networking.ClientSide.StateMachine
{
    public class MessageSender
    {
        public readonly int ID;
        readonly Action<IMessage> _message;

        public MessageSender(Action<IMessage> method, int id)
        {
            _message = method;
            ID = id;
        }

        public void SendMessage(IMessage msg)
        {
            _message?.Invoke(msg);
        }
    }
}
