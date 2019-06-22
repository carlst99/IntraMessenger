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

        }
    }
}
