using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("IntraMessenger.Tests")]

namespace IntraMessenger
{
    public sealed class Messenger : IMessenger
    {
        internal static readonly Type SEND_TO_ALL_TYPE = typeof(IMessage);

        #region Fields

        private readonly Dictionary<Type, List<Subscriber>> _subscriptions;

        #endregion

        #region Properties

        public static Messenger Instance { get; } = new Messenger();

        /// <summary>
        /// Gets a read only collection of the subscriptions by type to this messenger when using <see cref="Mode.HeavySubscribe"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this messenger is not in the correct mode</exception>
        internal IDictionary<Type, IReadOnlyCollection<Subscriber>> Subscriptions
        {
            get
            {
                var converted = _subscriptions.ToDictionary(k => k.Key, v => (IReadOnlyCollection<Subscriber>)v.Value.AsReadOnly());
                return new ReadOnlyDictionary<Type, IReadOnlyCollection<Subscriber>>(converted);
            }
        }

        #endregion

        #region Constructors

        static Messenger() { }

        /// <summary>
        /// This ctor used internally to setup instance and for testing
        /// </summary>
        internal Messenger()
        {
            _subscriptions = new Dictionary<Type, List<Subscriber>>
            {
                { SEND_TO_ALL_TYPE, new List<Subscriber>() }
            };
        }

        #endregion

        /// <summary>
        /// Initiates the callback for any subscriptions depending on the message type being sent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to send</param>
        /// <remarks>This will run any async callbacks synchronously</remarks>
        public void Send<T>(T message) where T : IMessage, new()
        {
            // Send to those who have subscribed to this message
            if (_subscriptions.ContainsKey(typeof(T)))
            {
                foreach (Subscriber s in _subscriptions[typeof(T)])
                    s.InitiateCallback(message);
            }
            // Send to those who want all messages
            foreach (Subscriber s in _subscriptions[SEND_TO_ALL_TYPE])
                s.InitiateCallback(message);
        }

        /// <summary>
        /// Initiates the callback asynchronously for any subscriptions depending on the message type being sent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to send</param>
        public async Task SendAsync<T>(T message) where T : IMessage, new()
        {
            // Send to those who have subscribed to this message
            if (_subscriptions.ContainsKey(typeof(T)))
            {
                foreach (Subscriber s in _subscriptions[typeof(T)])
                    await s.InitiateCallbackAsync(message).ConfigureAwait(false);
            }
            // Send to those who want all messages
            foreach (Subscriber s in _subscriptions[SEND_TO_ALL_TYPE])
                await s.InitiateCallbackAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a subscription to the message queue
        /// </summary>
        /// <param name="action">The callback to invoke when a message is broadcast</param>
        /// <param name="requestedMessageTypes">The types of message to subscribe to</param>
        /// <returns>A GUID which can be used to unsubscribe from the message queue</returns>
        /// <exception cref="ArgumentNullException">Thrown when the callback is null</exception>
        /// <exception cref="ArgumentException">Thrown when the provided message types do not inherit from <see cref="IMessage"/></exception>
        public Guid Subscribe(Action<IMessage> action, Type[] requestedMessageTypes = null)
        {
            if (action == null)
                throw new ArgumentNullException("Callback cannot be null");

            Guid unsubKey = Guid.NewGuid();
            Subscriber subscriber = new Subscriber(action, unsubKey);
            Subscribe(subscriber, requestedMessageTypes);

            return unsubKey;
        }

        /// <summary>
        /// Creates a subscription to the message queue for an asynchronous method
        /// </summary>
        /// <param name="action">The callback to invoke asynchronously when a message is broadcast</param>
        /// <param name="requestedMessageTypes">The types of message to subscribe to</param>
        /// <returns>A GUID which can be used to unsubscribe from the message queue</returns>
        /// <exception cref="ArgumentNullException">Thrown when the callback is null</exception>
        /// <exception cref="ArgumentException">Thrown when the provided message types do not inherit from <see cref="IMessage"/></exception>
        public Guid Subscribe(Func<IMessage, Task> action, Type[] requestedMessageTypes = null)
        {
            if (action == null)
                throw new ArgumentNullException("Callback cannot be null");

            Guid unsubKey = Guid.NewGuid();
            Subscriber subscriber = new Subscriber(action, unsubKey);
            Subscribe(subscriber, requestedMessageTypes);

            return unsubKey;
        }

        /// <summary>
        /// Removes a subscription from the message queue
        /// </summary>
        /// <param name="unsubscribeKey">The key returned when subscribing the object</param>
        /// <exception cref="ArgumentException">Thrown when an invalid key is provided</exception>
        public void Unsubscribe(Guid unsubscribeKey)
        {
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

        /// <summary>
        /// Clears any subscriptions and changes to the default mode <see cref="Mode.HeavySubscribe"/>
        /// </summary>
        public void Reset()
        {
            _subscriptions.Clear();
            _subscriptions.Add(SEND_TO_ALL_TYPE, new List<Subscriber>());
        }

        private void Subscribe(Subscriber subscriber, Type[] types)
        {
            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (!typeof(IMessage).IsAssignableFrom(type))
                        throw new ArgumentException("All requested message types must inherit " + nameof(IMessage));
                }
            }

            if (types == null)
            {
                _subscriptions[SEND_TO_ALL_TYPE].Add(subscriber);
            } else
            {
                foreach (Type type in types)
                {
                    if (!_subscriptions.ContainsKey(type))
                        _subscriptions.Add(type, new List<Subscriber>());

                    _subscriptions[type].Add(subscriber);
                }
            }
        }
    }
}
