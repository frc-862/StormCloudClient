using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudUSBService
{
    public class USBManager : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            USBListener l = new USBListener();
            l.BeginListening();
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
