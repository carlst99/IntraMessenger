using System;
using System.Linq;

namespace IntraMessaging
{
    public struct Subscriber
    {
        /// <summary>
        /// Gets the type of subscription that this subscriber is requesting
        /// </summary>
        internal SubscriptionSet SubscriptionSet { get; }

        /// <summary>
        /// Gets the callback invoked when a message is queued
        /// </summary>
        public Action<IMessage> Callback { get; }

        /// <summary>
        /// Gets the types of message that the subscriber wants to receive, provided <see cref="SubscriptionSet.Defined"/> is flagged
        /// </summary>
        public Type[] MessageTypes { get; }

        /// <summary>
        /// Gets the key used to unsubscribe this subscriber
        /// </summary>
        public Guid UnsubscribeKey { get; }

        internal Subscriber(Action<IMessage> callback, Guid unsubKey, Type[] subTypes = null)
        {
            Callback = callback;
            UnsubscribeKey = unsubKey;
            MessageTypes = subTypes;

            if (subTypes == null)
                SubscriptionSet = SubscriptionSet.All;
            else
                SubscriptionSet = SubscriptionSet.Defined;
        }

        /// <summary>
        /// Initiates the callback if the subscriber has requested to receive the type of the provided message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender">The sender of the message</param>
        /// <param name="message">The message to send</param>
        /// <returns>A value indicating whether the callback was run</returns>
        internal bool InitiateCallback<T>(T message) where T : IMessage
        {
            switch (SubscriptionSet)
            {
                case SubscriptionSet.All:
                    Callback.Invoke(message);
                    return true;
                case SubscriptionSet.Defined:
                    if (MessageTypes.Contains(typeof(T)))
                        Callback.Invoke(message);
                    return true;
                default:
                    return false;
            }
        }

        #region Equality Overrides

        public override bool Equals(object obj)
        {
            return obj is Subscriber sub
                && Equals(sub);
        }

        public bool Equals(Subscriber sub)
        {
            return sub.SubscriptionSet == SubscriptionSet
                && sub.Callback.Equals(Callback)
                && sub.MessageTypes.SequenceEqual(MessageTypes)
                && sub.UnsubscribeKey == UnsubscribeKey;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + SubscriptionSet.GetHashCode();
                hash = (hash * 23) + Callback.GetHashCode();
                hash = (hash * 23) + MessageTypes.GetHashCode();
                hash = (hash * 23) + UnsubscribeKey.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Subscriber sub1, Subscriber sub2) => sub1.Equals(sub2);

        public static bool operator !=(Subscriber sub1, Subscriber sub2) => !sub1.Equals(sub2);

        #endregion
    }

    /// <summary>
    /// Defines types of subscriptions that a subscriber can request
    /// </summary>
    internal enum SubscriptionSet
    {
        /// <summary>
        /// Subscribed to all message types
        /// </summary>
        All,

        /// <summary>
        /// Subscribed to a defined subset of message types
        /// </summary>
        Defined
    }
}
