using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS
{
    public class ComputeClient
    {
        public string url = null;
        public AuthTokens Tokens = null;

        public AuthTokens Auth(string username, string password)
        {
            var client = new RESTClient
            {
                endPoint = url + "/auth-jwt/get/",
                httpMethod = httpVerb.POST,
                payload = new Dictionary<string, object>()
                {
                    {"username", username},
                    {"password", password}
                }
            };
            var response = client.makeRequest();
            Tokens = JsonConvert.DeserializeObject<AuthTokens>(response);

            return Tokens;
        }

        private bool IsTokenExpired()
        {
            if (Tokens.Access == string.Empty)
            {
                throw new MissingFieldException("No access token was found. Please login!");
            }

            var token = Tokens.Access;
            token = token.Split(".")[1];
            var missingPadding = token.Length % 4;
            if (missingPadding > 0)
            {
                token += new string('=', (4 - missingPadding));
            }

            token = token.Replace("-", "+");
            token = token.Replace("_", "/");
            var decodedToken = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                Base64Decode(token));
            var tokenExpireTime = Convert.ToInt64(decodedToken["exp"]);
            var now = DateTimeOffset.UtcNow;

            return now.ToUnixTimeSeconds() > tokenExpireTime;
        }

        public string GetAccessToken()
        {
            if (IsTokenExpired())
            {
                Tokens.Access = RefreshAccessToken();
            }

            return Tokens.Access;
        }

        private string RefreshAccessToken()
        {
            var client = new RESTClient
            {
                endPoint = url + "/auth-jwt/refresh/",
                httpMethod = httpVerb.POST,
                payload = new Dictionary<string, object>()
                {
                    {"refresh", Tokens.Refresh}
                }
            };
            var response = client.makeRequest();
            Tokens = JsonConvert.DeserializeObject<AuthTokens>(response);

            return Tokens.Access;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}