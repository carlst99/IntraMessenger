using System;
using System.Collections.Generic;

namespace IntraMessaging
{
    public interface IIntraMessager
    {
        ICollection<Subscriber> Subscribers { get; }

        void Enqueue<T>(T message) where T : IMessage;
        void Subscribe(Action<IMessage> callback, Guid unsubKey, Type[] requestedMessageTypes = null);
        void Unsubscribe(Guid unsubKey);
    }
}
