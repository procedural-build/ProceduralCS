using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS
{
    /* Handles a Generic ViewSet of endpoints with standard CRUD operations
    (LIST, RETRIEVE, CREATE, UPDATE, PARTIAL_UPDATE & DELETE).

    This is a Generic version of the Project and Task models that supports the same
    operations with a more generic interface.  Like this:
    ```
    var viewset = new GenericViewSet<Project>(tokens, "/api/project/");

    // Get a list of Projects like this
    var projects = viewset.List()

    // Get a single Project like this
    var project = viewset.Retrieve("<project_uuid>")

    // Create a single Project like this
    var project = viewset.Create(Dictionary<string, object> data)
    ```
    */
    public class GenericViewSet<ObjectType>
    {
        public ComputeClient client = null;
        private string basePath = "";
        private string host = "";

        public GenericViewSet(ComputeClient _client, string _host, string _basePath)
        {
            basePath = _basePath;
            host = _host;
            client = _client;
            client.http.endPoint = _basePath;
            client.http.httpMethod = httpVerb.GET;
        }

        public GenericViewSet(AuthTokens tokens, string _host, string _basePath)
        {
            client = new ComputeClient(tokens, _host); // The empty string is the host - we need to get this
            basePath = _basePath;
            client.http.endPoint = _basePath;
            client.http.httpMethod = httpVerb.GET;
        }

        public string ObjectPath(string objectId)
        {
            return $"{basePath}{(basePath.EndsWith("/") ? "" : "/")}{(objectId != null ? $"{objectId}/" : "")}";
        }

        public ObjectType GetOrCreate(
            Dictionary<string, object> query_params,
            Dictionary<string, object> create_params = null,
            bool create = false
        )
        {
            /* Try to get an object by filtering using query parameters from a List request. 
            If a single filtered item does not exist then (optionally) create it.
            */

            // Check that at least a name or number is provided
            if (query_params.Keys.Count == 0)
            {
                throw new ArgumentException(@"Please provide some query parameters 
                for filtering the List request at a minimum");
            }

            // Do the Get or Create
            try
            {
                return GetByQueryParams(query_params);
            }
            catch (ArgumentException err)
            {
                if (create)
                {
                    // Merge the query_params with create_params
                    if (create_params == null)
                    {
                        create_params = query_params;
                    }
                    else
                    {
                        create_params = query_params
                            .Union(create_params)
                            .ToDictionary(s => s.Key, s => s.Value);
                    }

                    // Create the object
                    return Create(create_params);
                }

                return JsonConvert.DeserializeObject<ObjectType>("{\"error_messages\":[\"" + err.Message + "\"]}",
                    RESTClient.JsonSettings);
            }
        }

        public ObjectType GetByQueryParams(
            Dictionary<string, object> query_params
        )
        {
            var items = List(query_params);
            if (items.Count > 1)
            {
                throw new ArgumentException(
                    @"Found more than one object that match those query params. 
                    Please provide both a project number and a name to identify an unique project"
                );
            }
            else if (items.Count == 0)
            {
                throw new ArgumentException(
                    "No object found."
                );
            }

            return items.First();
        }

        public List<ObjectType> List(
            Dictionary<string, object> query_params = null
        )
        {
            /* Get a list of all Projects that this user can access 
            Optional query parameters may be provided to filter against name or number
            */
            return client.Request<List<ObjectType>>(
                basePath,
                query_params,
                httpVerb.GET
            );
        }

        public ObjectType Retrieve(
            string objectId,
            Dictionary<string, object> query_params = null
        )
        {
            /* Create a new project with provided name and number 
            */
            return client.Request<ObjectType>(
                ObjectPath(objectId),
                query_params,
                httpVerb.GET
            );
        }

        public string Retrieve(
            string objectId,
            string path,
            Dictionary<string, object> query_params = null
        )
        {
            return client.Request(
                ObjectPath(objectId),
                path,
                query_params,
                httpVerb.GET
            );
        }

        public ObjectType Create(
            Dictionary<string, object> data
        )
        {
            /* Create a new project with provided name and number 
            */
            return client.Request<ObjectType>(
                basePath,
                null,
                httpVerb.POST,
                data
            );
        }

        public ObjectType Update(
            string objectId,
            Dictionary<string, object> data,
            Dictionary<string, object> query_params = null
        )
        {
            /* Create a new project with provided name and number 
            */
            return client.Request<ObjectType>(
                ObjectPath(objectId),
                query_params,
                httpVerb.PUT,
                data
            );
        }

        public ObjectType PartialUpdate(
            string objectId,
            Dictionary<string, object> data,
            Dictionary<string, object> query_params = null
        )
        {
            /* Create a new project with provided name and number 
            */
            return client.Request<ObjectType>(
                ObjectPath(objectId),
                query_params,
                httpVerb.PATCH,
                data
            );
        }

        public ObjectType Delete(
            string objectId,
            Dictionary<string, object> query_params = null
        )
        {
            /* Create a new project with provided name and number 
            */
            return client.Request<ObjectType>(
                ObjectPath(objectId),
                query_params,
                httpVerb.DELETE
            );
        }
    }
}