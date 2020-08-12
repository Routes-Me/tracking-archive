﻿using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<IHostedService, TrackStatsChangefeedBackgroundService>();
            services.AddSingleton<IHostedService, MonitorVehicles>();
            return services;
        }
    }
}
