using System;
using System.Collections.Generic;

namespace IntraMessaging
{
    public interface IIntraMessenger
    {
        ICollection<Subscriber> Subscribers { get; }
        Mode OperationMode { get; }

        void Send<T>(T message) where T : IMessage;
        Guid Subscribe(Action<IMessage> callback, Type[] requestedMessageTypes = null);
        void Unsubscribe(Guid unsubKey);
        void ChangeMode(Mode changeTo);
        void Reset();
    }
}
