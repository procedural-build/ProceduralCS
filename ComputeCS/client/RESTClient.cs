using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;


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

        public string QueryString {
            // Serialize the dictionary of query_params
            get { return Utils.DictToQueryString(this.query_params); }
        }

        public string FullUrl {
            // Construct the full URL from provided host, endPoint and stringified query_params
            get { return $"{host}{endPoint}{QueryString}"; }
        }

        public T Request<T>() {
            /* Generic request that casts the response to a type provided
            */
            string response = requestToString();
            return JsonConvert.DeserializeObject<T>(response);
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
            string response = requestToString();
            return JsonConvert.DeserializeObject<T>(response);
        }

        public T Request<T>(Dictionary<string, object> _payload = null) {
            /* Generic request that casts the response to a type provided
            */
            payload = _payload;
            string response = requestToString();
            return JsonConvert.DeserializeObject<T>(response);
        }

        private void SetPayload() {
            request.ContentType = "application/json";
            using (StreamWriter swJSONPayload = new StreamWriter(request.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(payload);
                swJSONPayload.Write(json);
                swJSONPayload.Close();
            }
        }

        private string GetResponseString() {
            string responseString = string.Empty;

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

        public string requestToString()
        {           
            string responseString = string.Empty;

            // Generate the request object
            request = (HttpWebRequest)WebRequest.Create(FullUrl);
            request.Method = httpMethod.ToString();

            if (token != null)
            {
                request.Headers.Add("Authorization", $"JWT {token}");
            }

            if (request.Method == "POST" && payload != null)
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
                responseString = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";
            }
            finally
            {
                if (response != null)
                {
                    ((IDisposable)response).Dispose();
                }
            }

            return responseString;
        }
    }
}
