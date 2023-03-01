using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WipelotTask.SignalRHub.Hubs;
using WipelotTask.SignalRHub.Services.RabbitMQ;

namespace WipelotTask.SignalRHub
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddDefaultPolicy(policy =>
                            policy.AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .SetIsOriginAllowed(origin => true)
            ));
            services.AddSingleton<IRabbitMQService, RabbitMQService>();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<WipelotHub>("/wipelothub");
            });
            lifetime.ApplicationStarted.Register(() => RegisterSignalRWithRabbitMQ(app.ApplicationServices));
        }

        public void RegisterSignalRWithRabbitMQ(IServiceProvider serviceProvider)
        {
            var rabbitMQService = (IRabbitMQService)serviceProvider.GetService(typeof(IRabbitMQService));
            rabbitMQService.Connect();
        }
    }
}