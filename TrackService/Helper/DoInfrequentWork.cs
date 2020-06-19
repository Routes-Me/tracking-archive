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
    internal class DoInfrequentWork : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public DoInfrequentWork(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoEvent = new AutoResetEvent(false);
            var inFrequentWorker = new InFrequentWorker(_threadStatsChangefeedDbService);
            await Task.Run(() => { new Timer(inFrequentWorker.DoWork, autoEvent, TimeSpan.FromMinutes(2), TimeSpan.FromDays(7)); autoEvent.WaitOne(); }).ConfigureAwait(false);
        }
    }

    internal class InFrequentWorker
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        public InFrequentWorker(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        public async void DoWork(Object stateInfo)
        {
            while (true)
            {
                if (ThreadStatsRethinkDbService.IsAnotherServiceWorking)
                {
                    //Debug.WriteLine("InFrequentWorker -- another thread is working.");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                else
                {
                    ThreadStatsRethinkDbService.IsAnotherServiceWorking = true;
                    //Debug.WriteLine("InFrequentWorker");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    ThreadStatsRethinkDbService.IsAnotherServiceWorking = false;
                    break;
                }
            }
        }
    }
}
