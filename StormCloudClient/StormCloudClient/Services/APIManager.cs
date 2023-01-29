using StormCloudClient.Classes;
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
        public string About;
    }
    public class APIManager
    {
        static HttpClient client;
        public static void Initialize()
        {
            client = new HttpClient();
        }
        // ALL THE NEEDED API METHODS

        public static async Task<bool> ConnectivityTest(string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch(Exception e)
            {
                return false;
            }
        }
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

        public static async Task<APIResponse> GetMatchInformation(int matchNumber)
        {
            try
            {
                var url = _GetBaseUrl() + "/api/request/match?matchNumber="+matchNumber.ToString();
                var response = await client.GetAsync(url);
                return new APIResponse() { Content = await response.Content.ReadAsStringAsync(), Status = response.StatusCode };
            }
            catch (Exception e)
            {
                return new APIResponse() { Content = "", Status = HttpStatusCode.BadRequest };
            }

        }

        public static async Task<APIResponse> GetTeamInformation(int teamNumber)
        {
            try
            {
                var url = _GetBaseUrl() + "/api/request/team?teamNumber=" + teamNumber.ToString();
                var response = await client.GetAsync(url);
                return new APIResponse() { Content = await response.Content.ReadAsStringAsync(), Status = response.StatusCode };
            }
            catch (Exception e)
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

        public static async Task<List<APIResponse>> SendPhotos(List<Photo> photos)
        {
            var url = _GetBaseUrl() + "/api/submit/paper";

            List<APIResponse> responses = new List<APIResponse>();

            foreach (Photo photo in photos)
            {
                try
                {
                    string base64ImageRepresentation = StorageManagement.GetPhotoB64(photo.Path);
                    int l = base64ImageRepresentation.Length;
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("image", base64ImageRepresentation),
                        new KeyValuePair<string, string>("name", photo.Path),
                        new KeyValuePair<string, string>("team", photo.Team.ToString()),
                        new KeyValuePair<string, string>("identifier", photo.Identifier),
                        new KeyValuePair<string, string>("matches", Newtonsoft.Json.JsonConvert.SerializeObject(photo.Matches))
                        
                    });

                    var response = await client.PostAsync(url, content);
                    responses.Add(new APIResponse() { Content = await response.Content.ReadAsStringAsync(), Status = response.StatusCode, About = photo.Path });



                }
                catch (Exception e)
                {
                    responses.Add(new APIResponse() { Content = "", Status = HttpStatusCode.BadRequest } );

                }
                
            }
            return responses;
            

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
