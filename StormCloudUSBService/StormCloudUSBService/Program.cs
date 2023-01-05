using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StormCloudUSBService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder()
                .ConfigureServices((c, s) =>
                {
                    s.AddHostedService<USBManager>();
                })
                .Build()
                .Run();
            
        }
    }
}