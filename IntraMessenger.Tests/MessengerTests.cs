using IntraMessenger.Tests.Context;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IntraMessenger.Tests
{
    public class MessengerTests
    {
        [Fact]
        public void TestPropInstance()
        {
            Assert.NotNull(Messenger.Instance);
        }

        [Fact]
        public void TestCtor()
        {
            Messenger messenger = new Messenger();
            Assert.NotEmpty(messenger.Subscriptions);
        }

        [Fact]
        public void TestSend()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
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

        [Fact]
        public async Task TestSendAsync()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.Subscribe(async (_) => callback = true);

            await messenger.SendAsync<TestMessage>(null);
            Assert.True(callback);

            messenger.Reset();
            callback = false;
            messenger.Subscribe(async (_) => callback = true, new Type[] { typeof(TestMessage) });

            await messenger.SendAsync<UnregisteredMessage>(null);
            Assert.False(callback);

            await messenger.SendAsync<TestMessage>(null);
            Assert.True(callback);
        }

        [Fact]
        public void TestSubscribeAction()
        {
            bool callback = false;

            Messenger messenger = new Messenger();
            Action<IMessage> action = null;
            Assert.Throws<ArgumentNullException>(() => messenger.Subscribe(action));
            Assert.Throws<ArgumentException>(() => messenger.Subscribe((_) => callback = true, new Type[] { typeof(object) }));

            Guid id = messenger.Subscribe((_) => callback = true);
            Assert.NotEqual(default, id);
            Assert.NotEqual(Guid.Empty, id);
            Assert.NotEmpty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);

            messenger.Reset();
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });
            Assert.Empty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);
            Assert.True(messenger.Subscriptions.ContainsKey(typeof(TestMessage)));
            Assert.True(messenger.Subscriptions[typeof(TestMessage)].Count == 1);

            Assert.False(callback);
        }

        [Fact]
        public void TestSubscribeFunc()
        {
            bool callback = false;

            Messenger messenger = new Messenger();
            Func<IMessage, Task> func = null;
            Assert.Throws<ArgumentNullException>(() => messenger.Subscribe(func));

            Guid id = messenger.Subscribe(async (_) => callback = true);
            Assert.NotEqual(default, id);
            Assert.NotEqual(Guid.Empty, id);
            Assert.NotEmpty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);

            Assert.False(callback);
        }

        [Fact]
        public void TestUnsubscribe()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            Guid unsubKey = messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });

            messenger.Unsubscribe(unsubKey);
            Assert.Empty(messenger.Subscriptions[typeof(TestMessage)]);
            Assert.Throws<ArgumentException>(() => messenger.Unsubscribe(unsubKey));

            messenger.Send(new TestMessage());
            Assert.False(callback);
        }

        [Fact]
        public void TestReset()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });

            messenger.Reset();
            Assert.True(messenger.Subscriptions.Count == 1);
            Assert.Empty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);
            messenger.Send(new TestMessage());
            Assert.False(callback);
        }
    }
}
