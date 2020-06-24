using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TrackService.Helper;
using TrackService.RethinkDb_Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace TrackService
{


    public class TrackServiceHub : Hub
    {
        public readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        public readonly static AllVehiclesMapping<string> _allvehicles = new AllVehiclesMapping<string>();
        public readonly static InstitutionsMapping<string> _institutions = new InstitutionsMapping<string>();
        public readonly static SubscribedVehiclesMapping<string> _subscribedvehicles = new SubscribedVehiclesMapping<string>();

        public readonly static AllVehiclesMappingById<string> _allData = new AllVehiclesMappingById<string>();
        public readonly static InstitutionsMappingById<string> _institutionsData = new InstitutionsMappingById<string>();
        public readonly static SubscribedVehiclesMappingById<string> _vehiclesData = new SubscribedVehiclesMappingById<string>();

        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public TrackServiceHub()
        {
        }

        public TrackServiceHub(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        // This function is called from sender(Project) to send vehicle data.
        public async void SendMessage(string VehicleId, string Longitude, string Latitude, string Institution)
        {
            if (string.IsNullOrEmpty(VehicleId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"104\", \"message\":\"VehicleId required!\" }");
                return;
            }

            if (string.IsNullOrEmpty(Institution))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"105\", \"message\":\"InstitutionId required!\" }");
                return;
            }
            
            if (string.IsNullOrEmpty(Longitude))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"107\", \"message\":\"Longitude required!\" }");
                return;
            }
            
            if (string.IsNullOrEmpty(Latitude))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"106\", \"message\":\"Latitude required!\" }");
                return;
            }

            if (!int.TryParse(VehicleId, out int vid))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid VehicleId!\" }");
                return;
            }
            if (!int.TryParse(Institution, out int Iid))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid InstitutionId!\" }");
                return;
            }
            if (!double.TryParse(Longitude, out double longitude))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid Longitude!\" }");
                return;
            }
            if (!double.TryParse(Latitude, out double latitude))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid Latitude!\" }");
                return;
            }
            
            await _threadStatsChangefeedDbService.InsertThreadStatsAsync(new TrackingStats
            {
                VehicleId = Convert.ToInt32(VehicleId),
                InstitutionId = Convert.ToInt32(Institution),
                Longitude = Convert.ToDouble(Longitude),
                Latitude = Convert.ToDouble(Latitude),
                TimeStamp = DateTime.UtcNow.ToString()
            });
        }

        #region Called from Dashboard to Subscribe data
        public void SendAllVehicleData() // Subscribe All Vehicle Data For Admin
        {
            _institutionsData.Remove(Context.ConnectionId);
            _vehiclesData.Remove(Context.ConnectionId);
            _allData.Add(Context.ConnectionId, "ALL");
        }

        public async void SendAllVehicleDataForInstitutionId(string InstitutionId) // Subscribe All Vehicle Data For Particular Institution
        {
            if (string.IsNullOrEmpty(InstitutionId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"105\", \"message\":\"InstitutionId required!\" }");
                return;
            }

            if (!int.TryParse(InstitutionId, out int id))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid InstitutionId!\" }");
                return;
            }

            if (!_threadStatsChangefeedDbService.InstitutionExists(InstitutionId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"101\", \"message\":\"Institution does not exists!\" }");
                return;
            }
            _allData.Remove(Context.ConnectionId);
            _institutionsData.Remove(Context.ConnectionId);
            _institutionsData.Add(Context.ConnectionId, InstitutionId);
            _vehiclesData.Remove(Context.ConnectionId);
        }

        public async void SendSubscribedVehicleData(string VehicleId) // Subscribe All Vehicle Data For Particular VehicleId
        {
            if (string.IsNullOrEmpty(VehicleId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"104\", \"message\":\"VehicleId required!\" }");
                return;
            }

            if (!int.TryParse(VehicleId, out int id))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "Bad request value. Invalid VehicleId!");
                return;
            }

            if (!_threadStatsChangefeedDbService.VehicleExists(VehicleId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"102\", \"message\":\"Vehicle does not exists!\" }");
                return;
            }
            _allData.Remove(Context.ConnectionId);
            _institutionsData.Remove(Context.ConnectionId);
            _vehiclesData.Remove(Context.ConnectionId);
            _vehiclesData.Add(Context.ConnectionId, VehicleId);
        }
        #endregion

        #region Called from Change feed to send data to Dashboard
        public async void SendAllVehicleDataToClient(IHubContext<TrackServiceHub> context, string json) // Send All Vehicle Data For Admin
        {
            var keys = _allData.GetAllData();
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    await context.Clients.Client(key).SendAsync("ReceiveAllVehicleData", json);
                }
            }
            await context.Clients.All.SendAsync("ReceiveAll", json);
        }

        public async void SendAllVehicleDataForInstitutionIdToClient(IHubContext<TrackServiceHub> context, string institutionId, string json) // Send All Vehicle Data For particular Institution
        {
            var keys = _institutionsData.GetAllData(institutionId);
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    await context.Clients.Client(key).SendAsync("ReceiveAllInstitutionVehicleData", json);
                }
            }
        }

        public async void SendSubscribedVehicleDataToClient(IHubContext<TrackServiceHub> context, string VehicleId, string json) // Send All Vehicle Data For particular Vehicle
        {
            var keys = _vehiclesData.GetAllData(VehicleId);
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    await context.Clients.Client(key).SendAsync("ReceiveSubscribedVehicleData", json);
                }
            }
        }

        #endregion

        public void UnsubscribeVehicleData()
        {
            _allData.Remove(Context.ConnectionId);
            _institutionsData.Remove(Context.ConnectionId);
            _vehiclesData.Remove(Context.ConnectionId);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _allData.Remove(Context.ConnectionId);
            _institutionsData.Remove(Context.ConnectionId);
            _vehiclesData.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
    }
}


