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
using TrackService.RethinkDb_Changefeed;

namespace TrackService.Helper
{
    internal class ClearData : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public ClearData(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoEvent = new AutoResetEvent(false);
            var clearDataWorker = new ClearDataWorker(_threadStatsChangefeedDbService);
            await Task.Run(() => { new Timer(clearDataWorker.DoWork, autoEvent, TimeSpan.FromMinutes(3), TimeSpan.FromDays(15)); autoEvent.WaitOne(); }).ConfigureAwait(false);
        }
    }

    internal class ClearDataWorker
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        public ClearDataWorker(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        public async void DoWork(Object stateInfo)
        {
            while (true)
            {
                if (ThreadStatsRethinkDbService.IsAnotherServiceWorking)
                {
                    //Debug.WriteLine("ClearData -- another thread is working.");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                else
                {
                    ThreadStatsRethinkDbService.IsAnotherServiceWorking = true;
                    //Debug.WriteLine("ClearData");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    ThreadStatsRethinkDbService.IsAnotherServiceWorking = false;
                    break;
                }
            }
        }
    }
}
