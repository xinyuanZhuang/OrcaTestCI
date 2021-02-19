using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orca.Services;
using Orca.Scheduling;
using Orca.Tools;
using Orca.Services.Adapters;

namespace Orca
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SharepointSettings>(Configuration.GetSection("Orca:Sharepoint"));
            // Register the sharepoint manager
            services.AddSingleton<ISharepointManager, SharepointManager>();
            // Register the event aggregator as a service.
            services.AddSingleton<IEventAggregator, EventAggregator>();
            // Register the course catalog and the course catalog updater background task
            services.AddSingleton<SharepointCourseCatalog>(); // directly register as SharepointCourseCatalog for the CourseCatalogUpdater 
            // asking for an ICourseCatalog will give us the same registered SharepointCourseCatalog above
            services.AddSingleton<ICourseCatalog>(serviceFactory => serviceFactory.GetRequiredService<SharepointCourseCatalog>());
            services.AddHostedService<CourseCatalogUpdater>();

            // Register the moodle adapter
            services.AddSingleton<MoodleAdapter>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orca", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orca v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
