using System;

namespace IntraMessenger
{
    public interface IMessage
    {
        object Sender { get; }
        Type SenderType { get; }
        Guid Id { get; set; }
        object Tag { get; set; }
    }
}
