using IntraMessenger.Tests.Context;
using System;
using Xunit;

namespace IntraMessenger.Tests
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
