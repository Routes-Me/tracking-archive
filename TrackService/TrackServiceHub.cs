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
        public readonly static AllVehiclesMappingById<string> _allvehiclesbyid = new AllVehiclesMappingById<string>();
        public readonly static InstitutionsMappingById<string> _institutionsbyid = new InstitutionsMappingById<string>();
        public readonly static SubscribedVehiclesMappingById<string> _subscribedvehiclesbyid = new SubscribedVehiclesMappingById<string>();
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public TrackServiceHub()
        {
        }

        public TrackServiceHub(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }


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

        public void SendAllVehicleData()
        {
            _allvehiclesbyid.Remove(Context.ConnectionId);
            _allvehiclesbyid.Add(Context.ConnectionId, "ALL");
            _institutionsbyid.Remove(Context.ConnectionId);
            _subscribedvehiclesbyid.Remove(Context.ConnectionId);
        }

        public async void SendAllVehicleDataForInstitutionId(string InstitutionId)
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
            _allvehiclesbyid.Remove(Context.ConnectionId);
            _institutionsbyid.Remove(Context.ConnectionId);
            _institutionsbyid.Add(Context.ConnectionId, InstitutionId);
            _subscribedvehiclesbyid.Remove(Context.ConnectionId);
        }

        public async void SendSubscribedVehicleData(string VehicleId)
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
            _allvehiclesbyid.Remove(Context.ConnectionId);
            _institutionsbyid.Remove(Context.ConnectionId);
            _subscribedvehiclesbyid.Remove(Context.ConnectionId);
            _subscribedvehiclesbyid.Add(Context.ConnectionId, VehicleId);
        }

        public async void SendAllVehicleDataToClient(IHubContext<TrackServiceHub> context, string json)
        {
            var keys = _allvehiclesbyid.GetAllData();
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    await context.Clients.Client(key).SendAsync("ReceiveAllVehicleData", json);
                }
            }
            await context.Clients.All.SendAsync("ReceiveAll", json);
        }

        public async void SendAllVehicleDataForInstitutionIdToClient(IHubContext<TrackServiceHub> context, string institutionId, string json)
        {
            var keys = _institutionsbyid.GetAllData(institutionId);
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    await context.Clients.Client(key).SendAsync("ReceiveAllInstitutionVehicleData", json);
                }
            }
        }

        public async void SendSubscribedVehicleDataToClient(IHubContext<TrackServiceHub> context, string VehicleId, string json)
        {
            var keys = _subscribedvehiclesbyid.GetAllData(VehicleId);
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    await context.Clients.Client(key).SendAsync("ReceiveSubscribedVehicleData", json);
                }
            }
        }

        public void UnsubscribeVehicleData()
        {
            _allvehiclesbyid.Remove(Context.ConnectionId);
            _institutionsbyid.Remove(Context.ConnectionId);
            _subscribedvehiclesbyid.Remove(Context.ConnectionId);
        }

        public void MapAccountInfo(bool isDashboard, string AccountValue)
        {
            if (isDashboard)
                _connections.Add(AccountValue, Context.ConnectionId);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Clients(Context.ConnectionId).SendAsync("SendAccountInfo");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _allvehiclesbyid.Remove(Context.ConnectionId);
            _institutionsbyid.Remove(Context.ConnectionId);
            _subscribedvehiclesbyid.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
    }

}


