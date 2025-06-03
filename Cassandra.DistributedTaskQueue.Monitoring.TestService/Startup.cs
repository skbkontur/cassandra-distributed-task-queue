using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new LongToStringConverter()));
        services.AddSingleton<IControllerFactory>(new GroboControllerFactory());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}