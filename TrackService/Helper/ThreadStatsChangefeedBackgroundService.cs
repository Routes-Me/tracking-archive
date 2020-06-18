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
    internal class ThreadStatsChangefeedBackgroundService : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        private readonly IServerSentEventsService _serverSentEventsService;
        private readonly IHubContext<TrackServiceHub> _hubContext;
        TrackServiceHub trackServiceHub;

        public ThreadStatsChangefeedBackgroundService(IThreadStatsChangefeedDbService threadStatsChangefeedDbService, IServerSentEventsService serverSentEventsService, IHubContext<TrackServiceHub> hubContext)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
            _serverSentEventsService = serverSentEventsService;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _threadStatsChangefeedDbService.EnsureDatabaseCreatedAsync();

            IChangefeed<Coordinates> threadStatsChangefeed = await _threadStatsChangefeedDbService.GetThreadStatsChangefeedAsync(stoppingToken);

            try
            {
                await foreach (Coordinates threadStatsChange in threadStatsChangefeed.FetchFeed(stoppingToken))
                {
                    string InstitutionId = string.Empty;
                    string newThreadStats = threadStatsChange.ToString();
                    string VehicleId = newThreadStats.Split(",")[0].Replace("vehicle_id:", "").Trim();
                    await Task.Run(() => { InstitutionId = _threadStatsChangefeedDbService.GetInstitutionId(VehicleId); }).ConfigureAwait(false);
                    var VehicleNum = newThreadStats.Split(",")[1].Replace("vehicle_num:", "").Trim();
                    var Latitude = newThreadStats.Split(",")[2].Replace("latitude:", "").Trim();
                    var Longitude = newThreadStats.Split(",")[3].Replace("longitude:", "").Trim();
                    var TimeStamp = newThreadStats.Split(",")[4].Replace("timestamp:", "").Trim();
                    var json = "{\"vehicle_id\": \"" + VehicleNum + "\",\"institution_id\": \"" + InstitutionId + "\",\"coordinates\": {\"latitude\": \"" + Latitude + "\", \"longitude\": \"" + Longitude +"\",\"timestamp\": \"" + TimeStamp + "\"}}";
                    trackServiceHub = new TrackServiceHub();
                    await Task.Run(() => { trackServiceHub.SendAllVehicleDataToClient(_hubContext, json); }).ConfigureAwait(true);
                    await Task.Run(() => { trackServiceHub.SendAllVehicleDataForInstitutionIdToClient(_hubContext, InstitutionId, json); }).ConfigureAwait(true);
                    await Task.Run(() => { trackServiceHub.SendSubscribedVehicleDataToClient(_hubContext, VehicleNum, json); }).ConfigureAwait(true);                    //await Task.Run(() => { trackServiceHub.SendAllVehicleDataToClient(_hubContext, VehicleNum, Longitude, Latitude, TimeStamp, InstitutionId); }).ConfigureAwait(true);
                    //await Task.Run(() => { trackServiceHub.SendAllVehicleDataForInstitutionIdToClient(_hubContext, VehicleNum, Longitude, Latitude, TimeStamp, InstitutionId); }).ConfigureAwait(true);
                    //await Task.Run(() => { trackServiceHub.SendSubscribedVehicleDataToClient(_hubContext, VehicleId, VehicleNum, Longitude, Latitude, TimeStamp, InstitutionId); }).ConfigureAwait(true);
                    await Task.WhenAll(
                        _serverSentEventsService.SendEventAsync(newThreadStats)
                    );
                }
            }
            catch (OperationCanceledException)
            { }
        }
    }
}
