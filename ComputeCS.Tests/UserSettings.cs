using NUnit.Framework;
using Microsoft.Extensions.Configuration;


namespace ComputeCS.Tests
{

    public class UserSettings 
    {
        public string username { get; set; }
        public string password { get; set; }
        public string host { get; set; }
        public string auth_host { get; set; }

        public UserSettings() {
            /* Set the user login/pass from secrets. Ensure that you have done:
            dotnet user-secrets set "ComputeAPIUser:Username" "<username>"
            dotnet user-secrets set "ComputeAPIUser:Password" "<password>"
            dotnet user-secrets set "ComputeAPIUser:Host" "<host>"

            Ensure that host is fully qualified including http(s)://

            An optional authentication host may be provided (ie. login.procedural.build).  If not provided
            then the auth_host will be equal to the host.
            */
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<UserSettings>()
                .Build();
            IConfigurationSection _section = configuration.GetSection("ComputeAPIUser");
            username = _section["Username"];
            password = _section["Password"];
            host = _section["Host"];
            auth_host = _section["AuthHost"] != null ? _section["AuthHost"] : host;

            Assert.IsNotNull(username, "Username secret not set. Run: dotnet user-secrets set 'ComputeAPIUser:Username' '<username>'");
            Assert.IsNotNull(password, "Password secret not set. Run: dotnet user-secrets set 'ComputeAPIUser:Password' '<password>'");
            Assert.IsNotNull(host, "Host secret not set. Run: dotnet user-secrets set 'ComputeAPIUser:Host' '<host>'");
        }
    }
}