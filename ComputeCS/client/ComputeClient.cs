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
        public RESTClient http = new RESTClient();
        public AuthTokens Tokens = new AuthTokens();
        public string auth_host = "";
        public string request_host = "";

        public ComputeClient(string _host = null) {
            http = new RESTClient
            {
                host = _host,
                endPoint = "/auth-jwt/verify/",
                httpMethod = httpVerb.GET
            };
        }

        public ComputeClient (string access_token, string refresh_token) {
            /* Generate a new ComputeClient instance from the tokens.

            NOTE: This allows the JWT tokens to be provided down a pipeline (such as a Grasshopper
            or Dynamo pipeline) in plain-text such as a JSON dictionary.  Then each component may
            access the API in a stateless way.
            */
            Tokens = new AuthTokens() {
                Access = access_token,
                Refresh = refresh_token
            };
            //GetHostsFromTokens();
        }

        public ComputeClient (AuthTokens tokens) {
            /* As above - but tokens provided in a nice data structure
            */
            Tokens = tokens;
            //GetHostsFromTokens();
         }

        public AuthTokens Auth(string username, string password)
        {
            Tokens = http.Request<AuthTokens>(
                $"{auth_host}/auth-jwt/get/", null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"username", username},
                    {"password", password}
                }
            );
            http.token = Tokens.Access;
            return Tokens;
        }

        public static string DecodeTokenToJson(string token)
        {
            token = token.Split(Convert.ToChar("."))[1];
            var missingPadding = token.Length % 4;
            if (missingPadding > 0)
            {
                token += new string('=', (4 - missingPadding));
            }

            token = token.Replace("-", "+");
            token = token.Replace("_", "/");
            return Base64Decode(token);
        }

        public static Dictionary<string, string> DecodeToken(string token) {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(DecodeTokenToJson(token));
        }

        private bool IsTokenExpired()
        {
            if (Tokens.Access == string.Empty)
            {
                throw new MissingFieldException("No access token was found. Please login!");
            }
            var decodedToken = DecodeToken(Tokens.Access);
            // Get the expiry time
            var tokenExpireTime = Convert.ToInt64(decodedToken["exp"]);
            var now = DateTimeOffset.UtcNow;
            // True if now is later than exiry time
            return now.ToUnixTimeSeconds() > tokenExpireTime;
        }

        public string GetAccessToken()
        {
            if (IsTokenExpired())
            {
                Tokens.Access = RefreshAccessToken();
            }
            // Set the token of the underlying RESTClient (http client)
            http.token = Tokens.Access;
            // Return the access token
            return Tokens.Access;
        }

        private string RefreshAccessToken()
        {
            Tokens = http.Request<AuthTokens>(
                $"{auth_host}/auth-jwt/refresh/", null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"refresh", Tokens.Refresh}
                }
            );
            return Tokens.Access;
        }

        private string VerifyAccessToken()
        {
            Tokens = http.Request<AuthTokens>(
                $"{auth_host}/auth-jwt/verify/", null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"access", Tokens.Access}
                }
            );
            return Tokens.Access;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /* 
        Wrappers around the RESTClient.Request methods that checks the token and refreshes
        as required before a call
        */
        public T Request<T>(
            string url, 
            Dictionary<string, object> _query_params = null,
            httpVerb _method = httpVerb.GET, 
            Dictionary<string, object> _payload = null
        ) {
            GetAccessToken();
            return http.Request<T>(url, _query_params, _method, _payload);
        }

        public T Request<T>(Dictionary<string, object> _payload = null) {
            /* Generic request that casts the response to a type provided
            */
            GetAccessToken();
            return http.Request<T>(_payload);
        }
    }
}