using System;
using System.Net;
using Microsoft.Extensions.Hosting;
using static StormCloudUSBService.DataManager;

namespace StormCloudUSBService
{
    public class APIResponse
    {
        public HttpStatusCode Status;
        public string Content;
        public string About;
    }



    public class APIService : IHostedService
    {



        static HttpClient client;
        public static void Initialize()
        {
            client = new HttpClient();
        }















        public async static Task<bool> TestConnection(string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public static async Task<APIResponse> SendMatches(List<Match> matches)
        {
            try
            {
                var url = _GetBaseUrl(preferredUrl) + "/api/submit/data";

                Program.Print("Sending " + matches.Count.ToString() + " to " + preferredUrl, ConsoleColor.Gray);


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

        /*public static async Task<List<APIResponse>> SendPhotos(List<Photo> photos)
        {
            var url = _GetBaseUrl(selectedUrl) + "/api/submit/paper";

            List<APIResponse> responses = new List<APIResponse>();

            foreach (Photo photo in photos)
            {
                try
                {
                    byte[] imageArray = System.IO.File.ReadAllBytes(StorageManagement.GetPath(photo.Path));

                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);
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
                    responses.Add(new APIResponse() { Content = "", Status = HttpStatusCode.BadRequest });

                }

            }
            return responses;


        }*/



        static string _GetBaseUrl(string url)
        {
            if (url == null)
                return "http://localhost";

            var link = (string)url;
            if (!link.Contains("http"))
                link = "https://" + link;

            return link;
        }

        public static string preferredUrl;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Initialize();

            bool res;
            

            Program.Print("Testing Connection to scout.robosmrt.com...", ConsoleColor.Gray);
            res = await APIService.TestConnection("https://scout.robosmrt.com");
            if (res)
                preferredUrl = "https://scout.robosmrt.com";
            Program.Print("Can connect to scout.robosmrt.com: " + res.ToString(), res ? ConsoleColor.Green : ConsoleColor.Red);

            Program.Print("Testing Connection to localhost:8000...", ConsoleColor.Gray);
            res = await APIService.TestConnection("localhost:8000");
            if (res)
                preferredUrl = "localhost:8000";
            Program.Print("Can connect to localhost:8000: " + res.ToString(), res ? ConsoleColor.Green : ConsoleColor.Red);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

