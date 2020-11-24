using NUnit.Framework;
using System;


namespace ComputeCS.Tests
{
    [TestFixture]
    public class TestSecrets
    {           
        [Test]
        public void TestGetSecrets()
        {
            // Load the user class (which loads the secrets on model instantiation)
            UserSettings user = new UserSettings();
            Console.WriteLine($"Username: {user.username}");
            Console.WriteLine($"Password: {user.password}");
            Console.WriteLine($"Host: {user.host}");
            Console.WriteLine($"Auth Host: {user.auth_host}");

            Assert.IsTrue(true, "Assertions in User object creation");
        }
    }
}