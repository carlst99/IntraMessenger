using System;
using System.Threading.Tasks;

namespace IntraMessenger
{
    internal class Subscriber
    {
        #region Properties

        public bool IsAsync { get; }

        /// <summary>
        /// Gets the callback invoked when a message is queued
        /// </summary>
        internal Action<IMessage> Callback { get; }

        /// <summary>
        /// Gets the callback invoked asynchronously when a message is queued
        /// </summary>
        internal Func<IMessage, Task> AsyncCallback { get; }

        /// <summary>
        /// Gets the key used to unsubscribe this subscriber
        /// </summary>
        public Guid UnsubscribeKey { get; }

        #endregion

        #region Constructors

        internal Subscriber(Action<IMessage> callback, Guid unsubKey)
            : this(unsubKey)
        {
            Callback = callback;
            IsAsync = false;
        }

        internal Subscriber(Func<IMessage, Task> callback, Guid unsubKey)
            : this(unsubKey)
        {
            AsyncCallback = callback;
            IsAsync = true;
        }

        private Subscriber(Guid unsubKey)
        {
            UnsubscribeKey = unsubKey;
        }

        #endregion

        /// <summary>
        /// Initiates the callback if the subscriber has requested to receive the type of the provided message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="skipTypeChecking">Indicates whether to skip the comparison to <see cref="MessageTypes"/> before initiating the callback</param>
        /// <returns>A value indicating whether the callback was run</returns>
        internal async Task InitiateCallbackAsync<T>(T message) where T : IMessage
        {
            if (IsAsync)
                await AsyncCallback(message).ConfigureAwait(false);
            else
                await Task.Run(() => Callback(message)).ConfigureAwait(false);
        }

        internal void InitiateCallback<T>(T message) where T : IMessage
        {
            if (IsAsync)
                AsyncCallback(message).Wait();
            else
                Callback(message);
        }

        #region Equality Overrides

        public override bool Equals(object obj)
        {
            return obj is Subscriber sub
                && sub.UnsubscribeKey.Equals(UnsubscribeKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (17 * 23) + UnsubscribeKey.GetHashCode();
            }
        }

        #endregion
    }
}
