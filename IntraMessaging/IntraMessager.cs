﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("IntraMessaging.Tests")]

namespace IntraMessaging
{
    public sealed class IntraMessager : IIntraMessager
    {
        private readonly Dictionary<Guid, Subscriber> _subscribers;
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
                    return _subscribers.Values.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Mode"/> of this messenger
        /// </summary>
        public Mode OperationMode { get; }

        static IntraMessager() { }

        private IntraMessager()
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

        public void Unsubscribe(Guid unsubKey)
        {
            if (_subscribers.ContainsKey(unsubKey))
                _subscribers.Remove(unsubKey);
            else
                throw new ArgumentException("A subscriber with the specified unsubscription key does not exist");
        }
    }
}
