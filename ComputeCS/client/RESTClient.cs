using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ComputeCS.utils.Cache;

namespace ComputeCS
{
    public enum httpVerb
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public class RESTClient
    {
        private HttpWebRequest request;
        private HttpWebResponse response = null;

        public string host { get; set; }
        public string endPoint { get; set; }
        public Dictionary<string, object> query_params { get; set; }
        public httpVerb httpMethod { get; set; }
        public Dictionary<string, object> payload { get; set; }
        public string token = null;
        

        public RESTClient()
        {
            host = "";
            endPoint = "";
            query_params = new Dictionary<string, object>();
            httpMethod = httpVerb.GET;
            payload = null;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            bool result = cert.Subject.Contains("procedural.build");
            return result;
        }

        public string QueryString {
            // Serialize the dictionary of query_params
            get { return Utils.DictToQueryString(this.query_params); }
        }

        public string FullUrl {
            // Construct the full URL from provided host, endPoint and stringified query_params
            get { return $"{host}{endPoint}{QueryString}"; }
        }

        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        public T Request<T>() {
            /* Generic request that casts the response to a type provided
            */
            var response = requestToString();
            return JsonConvert.DeserializeObject<T>(response, JsonSettings);
        }

        public T Request<T>(
            string url, 
            Dictionary<string, object> _query_params = null,
            httpVerb _method = httpVerb.GET, 
            Dictionary<string, object> _payload = null
        ) {
            /* Generic request that casts the response to a type provided
            */
            endPoint = url;
            query_params = _query_params != null ? _query_params : new Dictionary<string, object>();
            httpMethod = _method;
            payload = _payload != null ? _payload : payload;
            var response = requestToString();
            return JsonConvert.DeserializeObject<T>(response, JsonSettings);
        }

        public string Request(
            string url,
            string path,
            Dictionary<string, object> _query_params = null,
            httpVerb _method = httpVerb.GET
        )
        {
            /* Generic request that casts the response to a type provided
            */
            endPoint = url;
            query_params = _query_params != null ? _query_params : new Dictionary<string, object>();
            httpMethod = _method;
            var response = RequestToFile(path);

            return response;
        }

        public T Request<T>(Dictionary<string, object> _payload = null) {
            /* Generic request that casts the response to a type provided
            */
            payload = _payload;
            string response = requestToString();
            return JsonConvert.DeserializeObject<T>(response, JsonSettings);
        }

        private void SetPayload() {
            
            if (payload.ContainsKey("file"))
            {
                request.ContentType = "application/octet-stream";
                using (var filePayload = new StreamWriter(request.GetRequestStream()))
                {
                    //var data = (byte[])payload["file"];
                    var data = JsonConvert.SerializeObject(payload, JsonSettings);
                    filePayload.Write(data);
                    filePayload.Close();
                }
            }
            else
            {
                request.ContentType = "application/json";
                using (var swJSONPayload = new StreamWriter(request.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(payload, JsonSettings);
                    swJSONPayload.Write(json);
                    swJSONPayload.Close();
                }
            }

        }

        private string GetResponseString() {
            string responseString = string.Empty;
            System.Net.ServicePointManager.ServerCertificateValidationCallback
                = ((sender, cert, chain, errors) => ValidateRemoteCertificate(sender, cert, chain, errors));

            response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        responseString = reader.ReadToEnd();
                    }
                 }
            }
            return responseString;
        }

        private string WriteResponseToFile(string path) {

            System.Net.ServicePointManager.ServerCertificateValidationCallback
                = ((sender, cert, chain, errors) => ValidateRemoteCertificate(sender, cert, chain, errors));

            response = (HttpWebResponse)request.GetResponse();
            var contentDisposition = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(response.Headers["Content-Disposition"]);
            var filename = contentDisposition.FileName;
            path = Path.Combine(path, filename);

            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var writer = File.OpenWrite(path))
                    {
                        responseStream.CopyTo(writer);
                    }
                }
            }

            return path;
        }

        public string RequestToFile(string path) {
            var responseString = string.Empty;

            // Generate the request object
            request = (HttpWebRequest)WebRequest.Create(FullUrl);
            request.Method = httpMethod.ToString();
            
            if (HasFetched(FullUrl, httpMethod.ToString()))
            {
                return "";
            }
            
            if (token != null)
            {
                request.Headers.Add("Authorization", $"JWT {token}");
            }

            // Get the response object of fallback to error message - then release resources from the request.
            try
            {
                responseString = WriteResponseToFile(path);
            }
            catch (Exception ex)
            {
                responseString = ex.Message.ToString();
            }
            finally
            {
                ((IDisposable) response)?.Dispose();
            }

            return responseString;
        }

        public string requestToString()
        {           
            var responseString = string.Empty;

            // Generate the request object
            request = (HttpWebRequest)WebRequest.Create(FullUrl);
            request.Method = httpMethod.ToString();

            if (HasFetched(FullUrl, httpMethod.ToString()))
            {
                return "";
            }
            
            if (token != null)
            {
                request.Headers.Add("Authorization", $"JWT {token}");
            }

            if ((request.Method == "POST" || request.Method == "PUT"  || request.Method == "PATCH") && payload != null)
            {
                this.SetPayload();
            }

            // Get the response object of fallback to error message - then release resources from the request.
            try
            {
                responseString = GetResponseString();
            }
            catch (Exception ex)
            {
                responseString = "{\"error_messages\":[\"" + ex.Message + "\"],\"errors\":{}}";
            }
            finally
            {
                ((IDisposable) response)?.Dispose();
            }

            return responseString;
        }
        
        private static bool HasFetched(string url, string method, int timeLimit = 100)
        {
            var now = DateTime.Now;
            var cacheKey = $"{method} {url}";
            
            var value = StringCache.getCache(cacheKey);
            if (!string.IsNullOrEmpty(value))
            {
                var lastFetched = DateTime.Parse(value);
                var timeDelta = now - lastFetched;
                if (timeDelta < TimeSpan.FromMilliseconds(timeLimit))
                {
                    return true;
                }
            }
            StringCache.setCache(cacheKey, now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
            return false;
        }
    }
}
