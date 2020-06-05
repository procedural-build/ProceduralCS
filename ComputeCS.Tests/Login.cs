using System;
using computeCS;
using NUnit.Framework;
using computeCS.types;

namespace ComputeCS.Tests
{
    [TestFixture]
    public class LoginTest
    {
        [TestCase]
        public void TestLoginSuccess()
        {
            const string username = "admin";
            const string password = "0358d7bb";
            const string url = "http://localhost:8001";
            
            var client = new ComputeClient
            {
                url = url
            };

            var tokens = client.Auth(username, password);

            var output = new SerializeOutput
            {
                Auth = tokens,
                Url = url
            };

            string strOutput = output.ToJson();
            Assert.That(strOutput, Does.Contain("Access"));
            Assert.That(strOutput, Does.Contain("Refresh"));
            Assert.That(strOutput, Does.Contain("url"));
        }
        
        [TestCase]
        public void TestLoginFailure()
        {
            const string username = "admin";
            const string password = "dafdafasd";
            const string url = "http://localhost:8001";
            
            var client = new ComputeClient
            {
                url = url
            };

            var tokens = client.Auth(username, password);

            Assert.That(tokens.Access, Is.Null);
            Assert.That(tokens.Refresh, Is.Null);
        }
    }
}