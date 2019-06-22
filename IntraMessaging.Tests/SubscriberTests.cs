using IntraMessaging.Tests.Context;
using System;
using Xunit;

namespace IntraMessaging.Tests
{
    public class SubscriberTests
    {
        [Fact]
        public void TestConstructor()
        {
            Subscriber subscriber = new Subscriber(null, Guid.Empty, null);
            Assert.Equal(SubscriptionSet.All, subscriber.SubscriptionSet);

            subscriber = new Subscriber(null, Guid.Empty, new Type[0]);
            Assert.Equal(SubscriptionSet.All, subscriber.SubscriptionSet);

            Guid id = Guid.NewGuid();
            subscriber = new Subscriber(null, id, new Type[1]);
            Assert.Equal(SubscriptionSet.Defined, subscriber.SubscriptionSet);
            Assert.Equal(id, subscriber.UnsubscribeKey);
        }

        [Fact]
        public void TestInitiateCallbackWithAllTypes()
        {
            bool callback = false;
            Subscriber subscriber = new Subscriber((_) => callback = true, Guid.Empty, null);

            Assert.True(subscriber.InitiateCallback<IMessage>(null));
            Assert.True(callback);
        }

        [Fact]
        public void TestInitiateCallbackWithSpecifiedTypes()
        {
            bool callback = false;
            Type[] subTypes = new Type[] { typeof(TestMessage) };
            Subscriber subscriber = new Subscriber((_) => callback = true, Guid.Empty, subTypes);

            Assert.False(subscriber.InitiateCallback<IMessage>(null));
            Assert.False(callback);

            Assert.True(subscriber.InitiateCallback(new TestMessage()));
            Assert.True(callback);
        }

        [Fact]
        public void TestInitiateCallbackWithInvalidSet()
        {
            const SubscriptionSet set = (SubscriptionSet)255;
            Subscriber s = new Subscriber(null, Guid.Empty, null, set);
            Assert.False(s.InitiateCallback<IMessage>(null));
        }

        [Fact]
        public void TestEquals()
        {
            Subscriber s = GetSubscriber();
            Subscriber s2 = GetSubscriber();
            Subscriber s_GuidChange = new Subscriber(null, Guid.NewGuid());
            Subscriber s_SetValueChange = new Subscriber(null, Guid.Empty, new Type[1]);

            Assert.NotEqual(s, new object());
            Assert.Equal(s, s2);
            Assert.NotEqual(s, s_GuidChange);
            Assert.NotEqual(s, s_SetValueChange);
        }

        [Fact]
        public void TestGetHashCode()
        {
            Subscriber s = GetSubscriber();
            Subscriber s2 = GetSubscriber();
            Subscriber s3 = new Subscriber(null, Guid.NewGuid());

            Assert.Equal(s.GetHashCode(), s2.GetHashCode());
            Assert.NotEqual(s.GetHashCode(), s3.GetHashCode());
        }

        private Subscriber GetSubscriber() => new Subscriber(null, Guid.Empty);
    }
}
