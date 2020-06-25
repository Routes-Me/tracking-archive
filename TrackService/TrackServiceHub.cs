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
        public readonly static AllVehicleMapping<string> _all = new AllVehicleMapping<string>();
        public readonly static InstitutionsMapping<string> _institutions = new InstitutionsMapping<string>();
        public readonly static VehiclesMapping<string> _vehicles = new VehiclesMapping<string>();

        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public TrackServiceHub()
        {
        }

        public TrackServiceHub(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        // This function is called from sender(Project) to send vehicle data.
        public async void SendLocation(string VehicleId, string Longitude, string Latitude, string Institution)
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

        public async void Subscribe(string InstitutionId, string VehicleId, string All) 
        {
            if (!string.IsNullOrEmpty(InstitutionId))
            {
                if (int.TryParse(InstitutionId, out int id))
                {
                    if (_threadStatsChangefeedDbService.InstitutionExists(InstitutionId))
                    {
                        _institutions.Add(InstitutionId, Context.ConnectionId);
                        return;
                    }
                    else
                    {
                        await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"101\", \"message\":\"Institution does not exists!\" }");
                        return;
                    }
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid InstitutionId!\" }");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(VehicleId))
            {
                if (int.TryParse(VehicleId, out int id))
                {
                    if (_threadStatsChangefeedDbService.VehicleExists(VehicleId))
                    {
                        _vehicles.Add(VehicleId, Context.ConnectionId);
                        return;
                    }
                    else
                    {
                        await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"101\", \"message\":\"Vehicle does not exists!\" }");
                        return;
                    }
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid VehicleId!\" }");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(All) && All.Equals("--all"))
            {
                _all.Add(All, Context.ConnectionId);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"105\", \"message\":\"Please provide atleast one parameter!\" }");
                return;
            }
        }

        public async void Unsubscribe(string InstitutionId, string VehicleId) 
        {
            if (string.IsNullOrEmpty(VehicleId) && string.IsNullOrEmpty(InstitutionId))
            {
                _all.Remove("--all", Context.ConnectionId);
            }

            if (!string.IsNullOrEmpty(InstitutionId))
            {
                if (int.TryParse(InstitutionId, out int id))
                {
                    _institutions.Remove(InstitutionId, Context.ConnectionId);
                    return;
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid InstitutionId!\" }");
                    return;
                }
            }
            
            if (!string.IsNullOrEmpty(VehicleId))
            {
                if (int.TryParse(VehicleId, out int id))
                {
                    _vehicles.Remove(VehicleId, Context.ConnectionId);
                    return;
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ErrorMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid VehicleId!\" }");
                    return;
                }
            }
        }

        public async void SendDataToDashboard(IHubContext<TrackServiceHub> context, string institutionId, string vehicleId, string json)
        {
            // Send data to ReceiveAll screen
            await context.Clients.All.SendAsync("ReceiveAllData", json);

            // Send all vehicle data to Admin.
            foreach (var connectionid in _all.GetAll_ConnectionId("--all"))
            {
                await context.Clients.Client(connectionid).SendAsync("ReceiveAllVehicleData", json);
            }

            // Send all vehicle for institution.
            foreach (var connectionid in _institutions.GetInstitution_ConnectionId(institutionId))
            {
                await context.Clients.Client(connectionid).SendAsync("ReceiveInstitutionData", json);
            }

            // Send vehicle data for vehicle.
            foreach (var connectionid in _vehicles.GetVehicle_ConnectionId(vehicleId))
            {
                await context.Clients.Client(connectionid).SendAsync("ReceiveVehicleData", json);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await base.OnDisconnectedAsync(ex);
        }
    }
}