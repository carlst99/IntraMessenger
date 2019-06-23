using System;
using Xunit;

namespace IntraMessaging.Tests
{
    public class IntraMessengerTests
    {
        [Fact]
        public void TestPropInstance()
        {
            Assert.NotNull(IntraMessenger.Instance);
        }

        [Fact]
        public void TestPropSubscriptions()
        {
            IntraMessenger i = new IntraMessenger();

            i.ChangeMode(Mode.HeavySubscribe);
            Assert.True(i.Subscriptions.IsReadOnly);

            i.ChangeMode(Mode.HeavyMessaging);
            Assert.Throws<InvalidOperationException>(() => i.Subscriptions);
        }

        [Fact]
        public void TestPropSubscribers()
        {
            IntraMessenger i = new IntraMessenger();

            i.ChangeMode(Mode.HeavyMessaging);
            Assert.True(i.Subscribers.IsReadOnly);

            i.ChangeMode(Mode.HeavySubscribe);
            Assert.Throws<InvalidOperationException>(() => i.Subscribers);
        }
    }
}
