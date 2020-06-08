using System;
using System.Collections.Generic;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS
{
    public static class SerializeIO
    {
        public static string OutputToJson(
            Inputs inputs = null,
            AuthTokens auth = null,
            string url = null,
            Project project = null,
            Task task = null,
            Mesh mesh = null,
            CFDSolution solution = null
            )
        {
            if (inputs == null)
            {
                inputs = new Inputs();
            }

            if (auth != null)
            {
                inputs.Auth = auth;
            }

            if (url != null)
            {
                inputs.Url = url;
            }

            if (project != null)
            {
                inputs.Project = project;
            }

            if (task != null)
            {
                inputs.Task = task;
            }
            if (mesh != null)
            {
                inputs.Mesh = mesh;
            }
            if (solution != null)
            {
                inputs.CFDSolution = solution;
            }

            return JsonConvert.SerializeObject(inputs, Formatting.Indented);
        }

        public static Inputs InputsFromJson(string inputData)
        {
            return JsonConvert.DeserializeObject<Inputs>(inputData);
        }
    }
}