using IntraMessenger.Tests.Context;
using System;
using Xunit;

namespace IntraMessenger.Tests
{
    public class MessengerTests
    {
        #region Property Tests

        [Fact]
        public void TestPropInstance()
        {
            Assert.NotNull(Messenger.Instance);
        }

        [Fact]
        public void TestPropSubscriptions()
        {
            Messenger messenger = new Messenger();

            messenger.ChangeMode(Mode.HeavySubscribe);
            Assert.True(messenger.Subscriptions.IsReadOnly);

            messenger.ChangeMode(Mode.HeavyMessaging);
            Assert.Throws<InvalidOperationException>(() => messenger.Subscriptions);
        }

        [Fact]
        public void TestPropSubscribers()
        {
            Messenger messenger = new Messenger();

            messenger.ChangeMode(Mode.HeavyMessaging);
            Assert.True(messenger.Subscribers.IsReadOnly);

            messenger.ChangeMode(Mode.HeavySubscribe);
            Assert.Throws<InvalidOperationException>(() => messenger.Subscribers);
        }

        #endregion

        #region Send Tests

        [Fact]
        public void TestSendHeavyMessaging()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.ChangeMode(Mode.HeavyMessaging);
            messenger.Subscribe((_) => callback = true);

            messenger.Send<TestMessage>(null);
            Assert.True(callback);
        }

        [Fact]
        public void TestSendHeavySubscribe()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.ChangeMode(Mode.HeavySubscribe);
            messenger.Subscribe((_) => callback = true);

            messenger.Send<TestMessage>(null);
            Assert.True(callback);

            messenger.Reset(Mode.HeavySubscribe);
            callback = false;
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });

            messenger.Send<UnregisteredMessage>(null);
            Assert.False(callback);

            messenger.Send<TestMessage>(null);
            Assert.True(callback);
        }

        #endregion

        #region Subscribe Tests

        [Fact]
        public void TestSubscribe()
        {
            bool callback = false;

            Messenger messenger = new Messenger();
            Assert.Throws<ArgumentNullException>(() => messenger.Subscribe(null));
            Assert.Throws<ArgumentException>(() => messenger.Subscribe((_) => callback = true, new Type[] { typeof(object) }));

            messenger.ChangeMode(Mode.HeavyMessaging);
            Guid id = messenger.Subscribe((_) => callback = true);
            Assert.NotEmpty(messenger.Subscribers);
            Assert.NotEqual(default, id);
            Assert.NotEqual(Guid.Empty, id);

            messenger.Reset(Mode.HeavySubscribe);
            messenger.Subscribe((_) => callback = true);
            Assert.NotEmpty(messenger.Subscriptions);
            Assert.NotEmpty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);

            messenger.Reset(Mode.HeavySubscribe);
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });
            Assert.NotEmpty(messenger.Subscriptions);
            Assert.Empty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);
            Assert.True(messenger.Subscriptions.ContainsKey(typeof(TestMessage)));

            // This really just removes the build warning
            Assert.False(callback);
        }

        [Fact]
        public void TestUnsubscribe()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.ChangeMode(Mode.HeavyMessaging);

            Guid unsubKey = messenger.Subscribe((_) => callback = true);
            messenger.Unsubscribe(unsubKey);
            Assert.Empty(messenger.Subscribers);
            Assert.Throws<ArgumentException>(() => messenger.Unsubscribe(unsubKey));

            messenger.Reset(Mode.HeavySubscribe);
            unsubKey = messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });

            messenger.Unsubscribe(unsubKey);
            Assert.Empty(messenger.Subscriptions[typeof(TestMessage)]);
            Assert.Throws<ArgumentException>(() => messenger.Unsubscribe(unsubKey));

            Assert.False(callback);
        }

        #endregion

        [Fact]
        public void TestChangeMode()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.ChangeMode(Mode.HeavyMessaging);
            Assert.Equal(Mode.HeavyMessaging, messenger.OperationMode);
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage)});

            messenger.ChangeMode(Mode.HeavySubscribe);
            Assert.True(messenger.Subscriptions.ContainsKey(typeof(TestMessage)));
            Assert.NotEmpty(messenger.Subscriptions[typeof(TestMessage)]);
            Assert.True(messenger.Subscriptions.Count == 2);

            messenger.ChangeMode(Mode.HeavyMessaging);
            Assert.True(messenger.Subscribers.Count == 1);

            messenger.Reset(Mode.HeavyMessaging);
            messenger.Subscribe((_) => callback = false);
            messenger.ChangeMode(Mode.HeavySubscribe);
            Assert.True(messenger.Subscriptions.Count == 1);
            Assert.NotEmpty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);

            Assert.False(callback);
        }

        [Fact]
        public void TestReset()
        {
            bool callback = false;
            Messenger messenger = new Messenger();
            messenger.ChangeMode(Mode.HeavyMessaging);
            messenger.Subscribe((_) => callback = true);

            messenger.Reset(Mode.HeavyMessaging);
            Assert.Empty(messenger.Subscribers);
            Assert.Equal(Mode.HeavyMessaging, messenger.OperationMode);

            messenger.ChangeMode(Mode.HeavySubscribe);
            messenger.Subscribe((_) => callback = true, new Type[] { typeof(TestMessage) });
            messenger.Reset();
            Assert.True(messenger.Subscriptions.Count == 1);
            Assert.Empty(messenger.Subscriptions[Messenger.SEND_TO_ALL_TYPE]);

            Assert.False(callback);
        }
    }
}
