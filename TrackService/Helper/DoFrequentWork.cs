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
    internal class DoFrequentWork : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public DoFrequentWork(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoEvent = new AutoResetEvent(false);
            var frequentWorker = new FrequentWorker(_threadStatsChangefeedDbService);
            await Task.Run(() => { new Timer(frequentWorker.DoWorkAsync, autoEvent, TimeSpan.FromMinutes(1), TimeSpan.FromDays(1)); autoEvent.WaitOne(); }).ConfigureAwait(false);
        }
    }

    internal class FrequentWorker
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        public FrequentWorker(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        public async void DoWorkAsync(Object stateInfo)
        {
            while (true)
            {
                if (ThreadStatsRethinkDbService.IsAnotherServiceWorking)
                {
                    //Debug.WriteLine("FrequentWork -- another thread is working.");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                else
                {
                    ThreadStatsRethinkDbService.IsAnotherServiceWorking = true;
                    //Debug.WriteLine("FrequentWork");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    ThreadStatsRethinkDbService.IsAnotherServiceWorking = false;
                    break;
                }
            }
        }
    }
}
