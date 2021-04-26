using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService
{
    public static class ExchangeServiceEntryPoint
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build()
                .Run();
        }
    }
}