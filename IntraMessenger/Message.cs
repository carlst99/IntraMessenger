using System;

namespace IntraMessenger
{
    public abstract class Message : IMessage
    {
        public object Sender { get; set; }
        public Guid Id { get; set; }
        public object Tag { get; set; }

        public Type SenderType => Sender.GetType();
    }
}
