using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackService.Helper;

namespace TrackService.Services
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThreadStats(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, ThreadStatsChangefeedBackgroundService>();
            services.AddSingleton<IHostedService, MonitorVehicles>();
            services.AddSingleton<IHostedService, DoFrequentWork>();
            services.AddSingleton<IHostedService, DoInfrequentWork>();
            services.AddSingleton<IHostedService, ClearData>();
            return services;
        }
    }
}
