using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntraMessaging
{
    public sealed class IntraMessager : IIntraMessager
    {
        public static IIntraMessager Instance { get; } = new IntraMessager();

        public ICollection<Subscriber> Subscribers { get; }

        static IntraMessager() { }

        private IntraMessager()
        {
            Subscribers = new List<Subscriber>();
        }

        public void Enqueue<T>(T message) where T : IMessage
        {
            foreach (Subscriber subscriber in Subscribers)
                subscriber.InitiateCallback(message);
        }

        public async Task EnqueueAsync<T>(T message) where T : IMessage
        {
            await Task.Factory.StartNew(() => Enqueue(message)).ConfigureAwait(false);
            return;
        }

        public void Subscribe(Action<IMessage> callback, Guid unsubKey, Type[] requestedMessageTypes = null)
        {
            Subscribers.Add(new Subscriber(callback, unsubKey, requestedMessageTypes));
        }

        public void Unsubscribe(Guid unsubKey)
        {
            if (unsubKey == default || unsubKey == Guid.Empty)
                return;

            Subscriber subscriber = Subscribers.FirstOrDefault(s => s.UnsubscribeKey == unsubKey);
            if (!subscriber.Equals(default))
                Subscribers.Remove(subscriber);
        }
    }
}
