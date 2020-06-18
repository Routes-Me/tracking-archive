using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrackService.Models;
using TrackService.RethinkDb_Abstractions;

namespace TrackService.Helper
{
    internal class MonitorVehicles : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public MonitorVehicles(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoEvent = new AutoResetEvent(false);
            var statusChecker = new StatusChecker(_threadStatsChangefeedDbService);
            await Task.Run(() => { new Timer(statusChecker.CheckStatus, autoEvent, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60)); autoEvent.WaitOne(); }).ConfigureAwait(false);

        }
    }

    internal class StatusChecker
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        public StatusChecker(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        { 
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        public static int count = 0;
        public void CheckStatus(Object stateInfo)
        {
            _threadStatsChangefeedDbService.UpdateThreadStatsAsync();
        }
    }
}
