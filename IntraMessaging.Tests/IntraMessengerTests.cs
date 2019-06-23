using IntraMessaging.Tests.Context;
using System;
using Xunit;

namespace IntraMessaging.Tests
{
    public class IntraMessengerTests
    {
        #region Property Tests

        [Fact]
        public void TestPropInstance()
        {
            Assert.NotNull(IntraMessenger.Instance);
        }

        [Fact]
        public void TestPropSubscriptions()
        {
            IntraMessenger messenger = new IntraMessenger();

            messenger.ChangeMode(Mode.HeavySubscribe);
            Assert.True(messenger.Subscriptions.IsReadOnly);

            messenger.ChangeMode(Mode.HeavyMessaging);
            Assert.Throws<InvalidOperationException>(() => messenger.Subscriptions);
        }

        [Fact]
        public void TestPropSubscribers()
        {
            IntraMessenger messenger = new IntraMessenger();

            messenger.ChangeMode(Mode.HeavyMessaging);
            Assert.True(messenger.Subscribers.IsReadOnly);

            messenger.ChangeMode(Mode.HeavySubscribe);
            Assert.Throws<InvalidOperationException>(() => messenger.Subscribers);
        }

        #endregion

        [Fact]
        public void TestSendHeavyMessaging()
        {
            bool callback = false;
            IntraMessenger messenger = new IntraMessenger();
            messenger.ChangeMode(Mode.HeavyMessaging);
            messenger.Subscribe((_) => callback = true);

            messenger.Send<TestMessage>(null);
            Assert.True(callback);

            messenger.Reset();
            callback = false;
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });

            messenger.Send<UnregisteredMessage>(null);
            Assert.False(callback);

            messenger.Send<TestMessage>(null);
            Assert.True(callback);
        }
    }
}
