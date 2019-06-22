using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("IntraMessaging.Tests")]

namespace IntraMessaging
{
    public sealed class IntraMessager : IIntraMessager
    {
        private readonly List<Subscriber> _subscribers;
        private readonly Dictionary<Type, List<Subscriber>> _subscriptions;

        public static IIntraMessager Instance { get; } = new IntraMessager();

        /// <summary>
        /// Gets a read only collection of the subscriptions by type to this messenger when using <see cref="Mode.HeavySubscribe"/>
        /// </summary>
        public IDictionary<Type, ICollection<Subscriber>> Subscriptions
        {
            get
            {
                if (OperationMode != Mode.HeavySubscribe)
                    throw new InvalidOperationException("Can only retrieve this collection in " + nameof(Mode.HeavySubscribe) + " mode");

                return _subscriptions
                    .Cast<dynamic>()
                    .ToDictionary(k => (Type)k.Key, v => (ICollection<Subscriber>)v.Value.AsReadOnly());
            }
        }

        /// <summary>
        /// Gets a read only collection of subscribers to this messenger when using <see cref="Mode.HeavyMessaging"/>
        /// </summary>
        public ICollection<Subscriber> Subscribers
        {
            get
            {
                if (OperationMode != Mode.HeavyMessaging)
                    throw new InvalidOperationException("Can only retrieve this collection in " + nameof(Mode.HeavyMessaging) + " mode");
                else
                    return _subscribers.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Mode"/> of this messenger
        /// </summary>
        public Mode OperationMode { get; }

        static IntraMessager() { }

        private IntraMessager()
        {
            _subscribers = new List<Subscriber>();
            OperationMode = Mode.HeavySubscribe;
            _subscriptions = new Dictionary<Type, List<Subscriber>>();
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

        public Guid Subscribe(Action<IMessage> callback, Type[] requestedMessageTypes = null)
        {
            if (callback == null)
                throw new ArgumentException("Callback cannot be null");

            Guid unsubKey = Guid.NewGuid();
            _subscribers.Add(new Subscriber(callback, unsubKey, requestedMessageTypes));
            return unsubKey;
        }

        public void Unsubscribe(Guid unsubKey)
        {
            if (unsubKey == default || unsubKey == Guid.Empty)
                throw new ArgumentException("The unsubscribe key cannot be empty or default");

            Subscriber subscriber = _subscribers.Find(s => s.UnsubscribeKey == unsubKey);
            if (!subscriber.Equals(default))
                _subscribers.Remove(subscriber);
        }
    }
}
