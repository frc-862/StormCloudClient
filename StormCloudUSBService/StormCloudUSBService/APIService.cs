using System;
using System.Net;
using Microsoft.Extensions.Hosting;

namespace StormCloudUSBService
{
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

