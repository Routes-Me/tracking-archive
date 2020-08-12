using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrackService.IServices;
using TrackService.RethinkDb_Abstractions;

namespace TrackService.Helper
{
    internal class TrackStatsChangefeedBackgroundService : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        private readonly IHubContext<TrackServiceHub> _hubContext;
        TrackServiceHub trackServiceHub;

        public TrackStatsChangefeedBackgroundService(IThreadStatsChangefeedDbService threadStatsChangefeedDbService, IHubContext<TrackServiceHub> hubContext)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _threadStatsChangefeedDbService.EnsureDatabaseCreatedAsync();

            IChangefeed<Coordinates> threadStatsChangefeed = await _threadStatsChangefeedDbService.GetTrackStatsChangefeedAsync(stoppingToken);

            try
            {
                await foreach (Coordinates threadStatsChange in threadStatsChangefeed.FetchFeed(stoppingToken))
                {
                    if (threadStatsChange != null)
                    {
                        string InstitutionId = string.Empty, DeviceId = string.Empty;
                        string newThreadStats = threadStatsChange.ToString();
                        var VehicleNum = newThreadStats.Split(",")[0].Replace("vehicleNumber:", "").Trim();
                        await Task.Run(() => { InstitutionId = _threadStatsChangefeedDbService.GetInstitutionId(VehicleNum); }).ConfigureAwait(false);
                        await Task.Run(() => { DeviceId = _threadStatsChangefeedDbService.GetDeviceId(VehicleNum); }).ConfigureAwait(false);
                        var Latitude = newThreadStats.Split(",")[1].Replace("latitude:", "").Trim();
                        var Longitude = newThreadStats.Split(",")[2].Replace("longitude:", "").Trim();
                        var TimeStamp = newThreadStats.Split(",")[3].Replace("timeStamp:", "").Trim();
                        var json = "{\"vehicleId\": \"" + VehicleNum + "\",\"institutionId\": \"" + InstitutionId + "\",\"deviceId\": \"" + DeviceId + "\",\"coordinates\": {\"latitude\": \"" + Latitude + "\", \"longitude\": \"" + Longitude + "\",\"timeStamp\": \"" + TimeStamp + "\"}}";
                        trackServiceHub = new TrackServiceHub();
                        await Task.Run(() => { trackServiceHub.SendDataToDashboard(_hubContext, InstitutionId, VehicleNum, json); }).ConfigureAwait(true); // To send data to all subscribe vehicled for admin
                    }
                }
            }
            catch (OperationCanceledException)
            { }
        }
    }
}
