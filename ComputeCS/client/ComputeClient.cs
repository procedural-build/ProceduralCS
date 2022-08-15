using ComputeCS.types;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputeCS
{
    public class ComputeClient
    {
        public RESTClient http = new RESTClient();
        public AuthTokens Tokens = new AuthTokens();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ComputeClient(string _host = null)
        {
            http = new RESTClient
            {
                host = _host,
                endPoint = "/auth-jwt/verify/",
                httpMethod = httpVerb.GET
            };
        }

        public ComputeClient(string access_token, string refresh_token, string _host)
        {
            /* Generate a new ComputeClient instance from the tokens.

            NOTE: This allows the JWT tokens to be provided down a pipeline (such as a Grasshopper
            or Dynamo pipeline) in plain-text such as a JSON dictionary.  Then each component may
            access the API in a stateless way.
            */
            http.host = _host;
            Tokens = new AuthTokens()
            {
                Access = access_token,
                Refresh = refresh_token
            };
        }

        public ComputeClient(AuthTokens tokens, string _host)
        {
            /* As above - but tokens provided in a nice data structure
            */
            http.host = _host;
            Tokens = tokens;
        }

        public ComputeClient(string jsonString, string _host)
        {
            /* As above - but both tokens are serialized in a JSON string with keys "access" and "refresh"
            */
            http.host = _host;
            Tokens = new AuthTokens().FromJson(jsonString);
        }

        public AuthTokens Auth(string username, string password)
        {
            Tokens = http.Request<AuthTokens>(
                "/auth-jwt/get/", null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"username", username},
                    {"password", password}
                }
            );
            http.token = Tokens.Access;
            Logger.Debug("Got tokens");
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

        public static Dictionary<string, object> DecodeToken(string token)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(DecodeTokenToJson(token));
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
            // True if now is later than expiry time
            Logger.Debug($"Checking token expiry: {tokenExpireTime} and now: {now.ToUnixTimeSeconds()}");
            return now.ToUnixTimeSeconds() > tokenExpireTime;
        }

        public string GetAccessToken()
        {
            if (IsTokenExpired())
            {
                Logger.Debug("Refreshing access token");
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
                "/auth-jwt/refresh/",
                null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"refresh", Tokens.Refresh}
                }
            );
            Logger.Debug($"Got new access token");
            return Tokens.Access;
        }

        private string VerifyAccessToken()
        {
            Tokens = http.Request<AuthTokens>(
                "/auth-jwt/verify/", null,
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
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
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
        )
        {
            GetAccessToken();
            return http.Request<T>(url, _query_params, _method, _payload);
        }

        public T Request<T>(Dictionary<string, object> _payload = null)
        {
            /* Generic request that casts the response to a type provided
            */
            GetAccessToken();
            return http.Request<T>(_payload);
        }

        public byte[] Request(
            string url,
            Dictionary<string, object> _query_params = null,
            httpVerb _method = httpVerb.GET
        )
        {
            GetAccessToken();
            return http.Request(url, _query_params, _method);
        }
    }
}