using System;
using System.Linq;

namespace IntraMessenger
{
    public class Subscriber
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

        internal Subscriber(Action<IMessage> callback, Guid unsubKey, Type[] messageTypes = null)
        {
            Callback = callback;
            UnsubscribeKey = unsubKey;
            MessageTypes = messageTypes;

            if (messageTypes == null || messageTypes.Length <= 0)
                SubscriptionSet = SubscriptionSet.All;
            else
                SubscriptionSet = SubscriptionSet.Defined;
        }

        internal Subscriber(Action<IMessage> callback, Guid unsubKey, Type[] messageTypes, SubscriptionSet set)
        {
            Callback = callback;
            UnsubscribeKey = unsubKey;
            MessageTypes = messageTypes;
            SubscriptionSet = set;
        }

        /// <summary>
        /// Initiates the callback if the subscriber has requested to receive the type of the provided message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="skipTypeChecking">Indicates whether to skip the comparison to <see cref="MessageTypes"/> before initiating the callback</param>
        /// <returns>A value indicating whether the callback was run</returns>
        internal bool InitiateCallback<T>(T message, bool skipTypeChecking = false) where T : IMessage
        {
            if (skipTypeChecking)
            {
                Callback.Invoke(message);
                return true;
            }

            switch (SubscriptionSet)
            {
                case SubscriptionSet.All:
                    Callback.Invoke(message);
                    return true;
                case SubscriptionSet.Defined:
                    if (MessageTypes.Contains(typeof(T)))
                    {
                        Callback.Invoke(message);
                        return true;
                    } else
                    {
                        return false;
                    }
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
            return sub.SubscriptionSet.Equals(SubscriptionSet)
                && sub.UnsubscribeKey.Equals(UnsubscribeKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + SubscriptionSet.GetHashCode();
                hash = (hash * 23) + UnsubscribeKey.GetHashCode();
                return hash;
            }
        }

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
        All = 0,

        /// <summary>
        /// Subscribed to a defined subset of message types
        /// </summary>
        Defined = 1
    }
}
