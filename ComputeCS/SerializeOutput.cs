using System;
using System.Collections.Generic;
using computeCS.types;
using Newtonsoft.Json;

namespace computeCS
{
    public class SerializeOutput
    {
        public AuthTokens Auth = null;
        public Task Task = null;
        public Project Project = null;
        public string Url = null;

        public string ToJson()
        {
            string strAuth = string.Empty;
            string strTask = string.Empty;
            string strProject = string.Empty;
            string strUrl = string.Empty;
            List<string> jsonList = new List<string>();

            if (Auth != null)
            {
                strAuth = JsonConvert.SerializeObject(Auth, Formatting.Indented);
                strAuth = "  \"auth\": " + strAuth;
                jsonList.Add(strAuth);
            }

            if (Task != null)
            {
                strTask = JsonConvert.SerializeObject(Task, Formatting.Indented);
                strTask = "  \"task\": " + strTask;
                jsonList.Add(strTask);
            }

            if (Project != null)
            {
                strProject = JsonConvert.SerializeObject(Project, Formatting.Indented);
                strProject = "  \"project\": " + strProject;
                jsonList.Add(strProject);
            }

            if (Url != null)
            {
                strUrl = "  \"url\": " + Url;
                jsonList.Add(strUrl);
            }
            return "{\n" + String.Join(",\n", jsonList) +  "\n}";
        }
    }


}