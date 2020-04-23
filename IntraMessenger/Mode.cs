namespace IntraMessenger
{
    /// <summary>
    /// Defines a set of messaging modes
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Default mode. Use this mode if you have a large number of subscribers and/or are sending a large number of messages
        /// </summary>
        HeavySubscribe = 0,

        /// <summary>
        /// Use this mode if you are frequently subscribing/unsubscribing. Otherwise use <see cref="HeavySubscribe"/>
        /// </summary>
        HeavyMessaging = 1
    }
}
