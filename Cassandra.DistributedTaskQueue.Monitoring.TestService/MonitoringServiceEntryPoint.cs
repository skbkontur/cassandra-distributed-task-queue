#if NET472
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

#else
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

#endif

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    public static class MonitoringServiceEntryPoint
    {
        public static void Main(string[] args)
        {
#if NET472
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .Build()
                   .Run();
#else
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build()
                .Run();
#endif
        }
    }
}