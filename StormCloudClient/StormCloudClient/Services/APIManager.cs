﻿using StormCloudClient.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Services
{
    public class APIResponse
    {
        public HttpStatusCode Status;
        public string Content;
    }
    public class APIManager
    {
        static HttpClient client;
        public static void Initialize()
        {
            client = new HttpClient();
        }
        // ALL THE NEEDED API METHODS

        public static async Task<APIResponse> GetSetupData()
        {
            try
            {
                var url = _GetBaseUrl() + "/api/setup";
                var response = await client.GetAsync(url);
                return new APIResponse() { Content = await response.Content.ReadAsStringAsync(), Status = response.StatusCode };
            }catch(Exception e)
            {
                return new APIResponse() { Content = "", Status = HttpStatusCode.BadRequest };
            }
            
        }

        public static async Task<APIResponse> SendMatches(List<Match> matches)
        {
            try
            {
                var url = _GetBaseUrl() + "/api/submit/data";


                
                var JSONToSubmit = Newtonsoft.Json.JsonConvert.SerializeObject(matches);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("documents", JSONToSubmit)
                });

                var response = await client.PostAsync(url, content);
                return new APIResponse() { Content = await response.Content.ReadAsStringAsync(), Status = response.StatusCode };
            }
            catch (Exception e)
            {
                return new APIResponse() { Content = "", Status = HttpStatusCode.BadRequest };
            }

        }

        static string _GetBaseUrl()
        {
            var stored = DataManagement.GetValue("server_address");
            if (stored == null)
                return "http://localhost";

            var link = (string)stored;
            if(!link.Contains("http"))
                link = "https://" + link;

            return link;
        }
    }
}
