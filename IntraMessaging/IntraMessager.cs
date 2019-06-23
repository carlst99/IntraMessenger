using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IntraMessaging.Tests")]

namespace IntraMessaging
{
    public sealed class IntraMessenger : IIntraMessenger
    {
        internal static readonly Type SEND_TO_ALL_TYPE = typeof(IMessage);

        #region Fields

        private readonly Dictionary<Guid, Subscriber> _subscribers;
        private readonly Dictionary<Type, List<Subscriber>> _subscriptions;

        #endregion

        #region Properties

        public static IntraMessenger Instance { get; } = new IntraMessenger();

        /// <summary>
        /// Gets a read only collection of the subscriptions by type to this messenger when using <see cref="Mode.HeavySubscribe"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this messenger is not in the correct mode</exception>
        public IDictionary<Type, ICollection<Subscriber>> Subscriptions
        {
            get
            {
                if (OperationMode != Mode.HeavySubscribe)
                    throw new InvalidOperationException("Can only retrieve this collection in " + nameof(Mode.HeavySubscribe) + " mode");

                var converted = _subscriptions.ToDictionary(k => k.Key, v => (ICollection<Subscriber>)v.Value.AsReadOnly());
                return new ReadOnlyDictionary<Type, ICollection<Subscriber>>(converted);
            }
        }

        /// <summary>
        /// Gets a read only collection of subscribers to this messenger when using <see cref="Mode.HeavyMessaging"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this messenger is not in the correct mode</exception>
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
        public Mode OperationMode { get; private set; }

        #endregion

        #region Constructors

        static IntraMessenger() { }

        /// <summary>
        /// This ctor used internally to setup instance and for testing
        /// </summary>
        internal IntraMessenger()
        {
            _subscribers = new Dictionary<Guid, Subscriber>();
            _subscriptions = new Dictionary<Type, List<Subscriber>>
            {
                { SEND_TO_ALL_TYPE, new List<Subscriber>() }
            };
            OperationMode = Mode.HeavySubscribe;
        }

        #endregion

        /// <summary>
        /// Queues a message to be broadcast to all applicable subscriptions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Send<T>(T message) where T : IMessage, new()
        {
            switch (OperationMode)
            {
                case Mode.HeavyMessaging:
                    foreach (Subscriber subscriber in Subscribers)
                        subscriber.InitiateCallback(message);
                    break;
                case Mode.HeavySubscribe:
                    if (_subscriptions.ContainsKey(typeof(T)))
                        _subscriptions[typeof(T)].ForEach(s => s.InitiateCallback(message, true));
                    _subscriptions[SEND_TO_ALL_TYPE].ForEach(s => s.InitiateCallback(message, true));
                    break;
            }
        }

        /// <summary>
        /// Creates a subscription to the message queue
        /// </summary>
        /// <param name="callback">The callback to invoke when a message is broadcast</param>
        /// <param name="requestedMessageTypes">The types of message to subscribe to</param>
        /// <returns>A GUID which can be used to unsubscribe from the message queue</returns>
        /// <exception cref="ArgumentNullException">Thrown when the callback is null</exception>
        /// <exception cref="ArgumentException">Thrown when the provided message types do not inherit from <see cref="IMessage"/></exception>
        public Guid Subscribe(Action<IMessage> callback, Type[] requestedMessageTypes = null)
        {
            if (callback == null)
                throw new ArgumentNullException("Callback cannot be null");

            if (requestedMessageTypes != null)
            {
                foreach (Type type in requestedMessageTypes)
                {
                    if (!typeof(IMessage).IsAssignableFrom(type))
                        throw new ArgumentException("All requested message types must inherit " + nameof(IMessage));
                }
            }

            Guid unsubKey = Guid.NewGuid();
            Subscriber subscriber = new Subscriber(callback, unsubKey, requestedMessageTypes);

            switch (OperationMode)
            {
                case Mode.HeavyMessaging:
                    _subscribers.Add(unsubKey, subscriber);
                    break;
                case Mode.HeavySubscribe:
                    if (requestedMessageTypes == null)
                    {
                        _subscriptions[SEND_TO_ALL_TYPE].Add(subscriber);
                        break;
                    }

                    foreach (Type type in requestedMessageTypes)
                    {
                        if (!_subscriptions.ContainsKey(type))
                            _subscriptions.Add(type, new List<Subscriber>());

                        _subscriptions[type].Add(subscriber);
                    }
                    break;
            }

            return unsubKey;
        }

        /// <summary>
        /// Removes a subscription from the message queue
        /// </summary>
        /// <param name="unsubscribeKey">The key returned when subscribing the object</param>
        /// <exception cref="ArgumentException">Thrown when an invalid key is provided</exception>
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
                        {
                            element.RemoveAt(index);
                            return;
                        }
                    }
                    throw new ArgumentException("A subscriber with the specified unsubscription key does not exist");
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

            OperationMode = changeTo;
        }

        /// <summary>
        /// Clears any subscriptions and changes to the default mode <see cref="Mode.HeavySubscribe"/>
        /// </summary>
        public void Reset(Mode mode = Mode.HeavySubscribe)
        {
            _subscribers.Clear();
            _subscriptions.Clear();
            _subscriptions.Add(SEND_TO_ALL_TYPE, new List<Subscriber>());
            OperationMode = mode;
        }
    }
}
