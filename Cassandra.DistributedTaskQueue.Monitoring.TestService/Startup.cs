#if NET472
using Microsoft.AspNetCore.Hosting;

using HostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#else
using Microsoft.Extensions.Hosting;

using HostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
#if NET472
            services.AddMvc();
#else
            services.AddControllers().AddNewtonsoftJson();
#endif
            services.AddSingleton<IControllerFactory>(new GroboControllerFactory());
        }

        public void Configure(IApplicationBuilder app, HostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

#if NET472
            app.UseMvc();
#else
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
#endif
        }
    }
}