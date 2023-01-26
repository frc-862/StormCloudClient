using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StormCloudUSBService
{
    public class Program
    {

        public static void Print(object contents, ConsoleColor c) {
            Console.ForegroundColor = c;
            Console.WriteLine(contents);

        }


        public static void Main(string[] args)
        {

            DataManager.Initialize();
            


            Host.CreateDefaultBuilder()
                .ConfigureServices((c, s) =>
                {
                    s.AddHostedService<APIService>();
                    s.AddHostedService<USBManager>();
                    
                })
                .Build()
                .Run();
            
        }
    }
}