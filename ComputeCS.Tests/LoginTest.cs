using NUnit.Framework;
using System;

namespace ComputeCS.UnitTests.Login
{
    [TestFixture]
    public class ComputeClient_computeClient
    {
        private UserSettings user = new UserSettings();

        private ComputeClient client;
        
        
        [SetUp]
        public void SetUp()
        {
            client = new ComputeClient(user.host);
        }

        [Test]
        public void ComputeClient_GetToken()
        {
            var tokens = client.Auth(user.username, user.password);

            Console.WriteLine($"Got access token: {tokens.Access}");

            Assert.IsNotNull(tokens.Access, "_value should be true");
        }
    }
}