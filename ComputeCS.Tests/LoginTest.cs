using NUnit.Framework;
using ComputeCS;
using System;


namespace ComputeCS.UnitTests.Login
{

    [TestFixture]
    public class ComputeClient_computeClient
    {
        private ComputeClient _client;
        
        [SetUp]
        public void SetUp()
        {
            _client = new ComputeClient();
        }

        [Test]
        public void ComputeClient_GetToken()
        {
            var result = _client.GetAccessToken();

            Assert.IsNotNull(_client.GetAccessToken(), "Access token does not exist");
        }
    }
}