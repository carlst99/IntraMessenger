using IntraMessaging.Tests.Context;
using System;
using Xunit;

namespace IntraMessaging.Tests
{
    public class MessageTests
    {
        [Fact]
        public void TestGetSenderType()
        {
            TestMessage m = new TestMessage
            {
                Sender = this,
                Id = Guid.NewGuid(),
                Tag = null
            };

            Assert.Equal(GetType(), m.SenderType);
        }
    }
}
