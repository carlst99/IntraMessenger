using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("IntraMessaging.Tests")]

namespace IntraMessaging
{
    public sealed class IntraMessenger : IIntraMessenger
    {
        private readonly Dictionary<Guid, Subscriber> _subscribers;
        private readonly Dictionary<Type, List<Subscriber>> _subscriptions;

        public static IIntraMessenger Instance { get; } = new IntraMessenger();

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
                    return _subscribers.Values.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Mode"/> of this messenger
        /// </summary>
        public Mode OperationMode { get; }

        static IntraMessenger() { }

        private IntraMessenger()
        {
            _subscribers = new Dictionary<Guid, Subscriber>();
            _subscriptions = new Dictionary<Type, List<Subscriber>>();
            OperationMode = Mode.HeavySubscribe;
        }

        public void Enqueue<T>(T message) where T : IMessage
        {
            foreach (Subscriber subscriber in Subscribers)
                subscriber.InitiateCallback(message);
        }

        public async Task EnqueueAsync<T>(T message) where T : IMessage
        {
            await Task.Factory.StartNew(() => Enqueue(message)).ConfigureAwait(false);
        }

        public Guid Subscribe(Action<IMessage> callback, Type[] requestedMessageTypes = null)
        {
            if (callback == null)
                throw new ArgumentException("Callback cannot be null");

            Guid unsubKey = Guid.NewGuid();
            _subscribers.Add(unsubKey, new Subscriber(callback, unsubKey, requestedMessageTypes));
            return unsubKey;
        }

        public void Unsubscribe(Guid unsubscribeKey)
        {
            switch (OperationMode)
            {
                case Mode.HeavyMessaging:
                    if (_subscribers.ContainsKey(unsubscribeKey))
                        _subscribers.Remove(unsubscribeKey);
                    else
                        throw new ArgumentException("A subscriber with the specified unsubscription key does not exist");
                    break;
                case Mode.HeavySubscribe:
                    foreach (List<Subscriber> element in _subscriptions.Values)
                    {
                        int index = element.FindIndex(s => s.UnsubscribeKey == unsubscribeKey);
                        if (index != -1)
                            element.RemoveAt(index);
                    }
                    break;
            }
        }

        /// <summary>
        /// Changes the <see cref="Mode"/> of this messenger. Note that this can be an intensive operation
        /// </summary>
        /// <param name="changeTo">The mode to change to</param>
        public void ChangeMode(Mode changeTo)
        {
            if (changeTo == OperationMode)
                return;

            switch (changeTo)
            {
                case Mode.HeavyMessaging:
                    _subscribers.Clear();
                    foreach (List<Subscriber> element in _subscriptions.Values)
                    {
                        foreach (Subscriber subscriber in element)
                        {
                            if (!_subscribers.ContainsKey(subscriber.UnsubscribeKey))
                                _subscribers.Add(subscriber.UnsubscribeKey, subscriber);
                        }
                    }
                    break;
                case Mode.HeavySubscribe:
                    _subscriptions.Clear();
                    foreach (Subscriber element in _subscribers.Values)
                    {
                        foreach (Type type in element.MessageTypes)
                        {
                            if (!_subscriptions.ContainsKey(type))
                                _subscriptions.Add(type, new List<Subscriber>());

                            _subscriptions[type].Add(element);
                        }
                    }
                    break;
            }
        }
    }
}
