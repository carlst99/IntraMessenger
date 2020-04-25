using System;
using System.Threading.Tasks;

namespace IntraMessenger
{
    public interface IMessenger
    {
        /// <summary>
        /// Initiates the callback for any subscriptions depending on the message type being sent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to send</param>
        void Send<T>(T message) where T : IMessage, new();

        /// <summary>
        /// Initiates the callback asynchronously for any subscriptions depending on the message type being sent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to send</param>
        Task SendAsync<T>(T message) where T : IMessage, new();

        /// <summary>
        /// Creates a subscription to the message queue
        /// </summary>
        /// <param name="action">The callback to invoke when a message is broadcast</param>
        /// <param name="requestedMessageTypes">The types of message to subscribe to</param>
        /// <returns>A GUID which can be used to unsubscribe from the message queue</returns>
        Guid Subscribe(Action<IMessage> action, Type[] requestedMessageTypes = null);

        /// <summary>
        /// Creates a subscription to the message queue for an asynchronous method
        /// </summary>
        /// <param name="action">The callback to invoke asynchronously when a message is broadcast</param>
        /// <param name="requestedMessageTypes">The types of message to subscribe to</param>
        /// <returns>A GUID which can be used to unsubscribe from the message queue</returns>
        Guid Subscribe(Func<IMessage, Task> action, Type[] requestedMessageTypes = null);

        /// <summary>
        /// Removes a subscription from the message queue
        /// </summary>
        /// <param name="unsubscribeKey">The key returned when subscribing the object</param>
        /// <exception cref="ArgumentException">Thrown when an invalid key is provided</exception>
        void Unsubscribe(Guid unsubKey);

        /// <summary>
        /// Clears any subscriptions
        /// </summary>
        void Reset();
    }
}
