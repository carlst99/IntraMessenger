using System;
using System.Threading.Tasks;
using Xunit;

namespace IntraMessenger.Tests
{
    public class SubscriberTests
    {
        [Fact]
        public void TestConstructor()
        {
            // Test that the unsubId and async flag are property set
            Guid id = Guid.NewGuid();
            Subscriber subscriber = new Subscriber(DoAction, id);
            Assert.Equal(id, subscriber.UnsubscribeKey);
            Assert.False(subscriber.IsAsync);

            subscriber = new Subscriber(DoFunc, Guid.Empty);
            Assert.True(subscriber.IsAsync);
        }

        [Fact]
        public void TestInitiateCallback()
        {
            bool callback = false;
            Subscriber subscriber = new Subscriber((_) => callback = true, Guid.Empty);

            subscriber.InitiateCallback<IMessage>(null);
            Assert.True(callback);
        }

        [Fact]
        public async Task TestInitiateCallbackAsync()
        {
            bool callback = false;
            Subscriber subscriber = new Subscriber(async (_) => callback = true, Guid.Empty);

            Assert.True(subscriber.IsAsync);
            await subscriber.InitiateCallbackAsync<IMessage>(null);
            Assert.True(callback);
        }

        [Fact]
        public void TestInitiateCallBackWithAsyncMethod()
        {
            bool callback = false;
            Subscriber subscriber = new Subscriber(async (_) => callback = true, Guid.Empty);

            Assert.True(subscriber.IsAsync);
            subscriber.InitiateCallback<IMessage>(null);
            Assert.True(callback);
        }

        [Fact]
        public async Task TestInitiateCallBackAsyncWithNonAsyncMethod()
        {
            bool callback = false;
            Subscriber subscriber = new Subscriber((_) => callback = true, Guid.Empty);

            Assert.False(subscriber.IsAsync);
            await subscriber.InitiateCallbackAsync<IMessage>(null);
            Assert.True(callback);
        }

        [Fact]
        public void TestEquals()
        {
            Subscriber s = GetSubscriber(out Guid id);
            Subscriber s2 = GetSubscriber(out _);
            Subscriber s3 = new Subscriber(null, id);

            Assert.NotEqual(s, new object());
            Assert.NotEqual(s, s2);
            Assert.Equal(s, s3);
        }

        [Fact]
        public void TestGetHashCode()
        {
            Subscriber s = GetSubscriber(out Guid id);
            Subscriber s2 = GetSubscriber(out _);
            Subscriber s3 = new Subscriber(null, id);

            Assert.NotEqual(s.GetHashCode(), s2.GetHashCode());
            Assert.Equal(s.GetHashCode(), s3.GetHashCode());
        }

        private Subscriber GetSubscriber(out Guid id)
        {
            id = Guid.NewGuid();
            return new Subscriber(null, id);
        }

        private void DoAction(IMessage message)
        {
        }

        private Task DoFunc(IMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
