﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TrackService.Helper;
using TrackService.RethinkDb_Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using TrackService.Helper.ConnectionMapping;
using TrackService.Models;

namespace TrackService
{
    public class TrackServiceHub : Hub
    {
        public readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        public readonly static AllVehicleMapping<string> _all = new AllVehicleMapping<string>();
        public readonly static InstitutionsMapping<string> _institutions = new InstitutionsMapping<string>();
        public readonly static VehiclesMapping<string> _vehicles = new VehiclesMapping<string>();

        public readonly static InstitutionId<string> _institutionsId = new InstitutionId<string>();
        public readonly static VehicleID<string> _vehiclesId = new VehicleID<string>();
        public readonly static DevicesId<string> _deviceId = new DevicesId<string>();

        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public TrackServiceHub()
        {
        }

        public TrackServiceHub(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        public async void SendLocation(LocationFeed sendLocations)
        {
            foreach (var location in sendLocations.SendLocation)
            {
                try
                {
                    await _threadStatsChangefeedDbService.InsertTrackStatsAsync(new TrackingStats
                    {
                        vehicleId = Convert.ToInt32(_vehiclesId.GetVehicleId(Context.ConnectionId)),
                        longitude = Convert.ToDouble(location.longitude),
                        latitude = Convert.ToDouble(location.latitude),
                        timeStamp = location.timestamp.ToString()
                    });
                    await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"200\", \"message\": Coordinates inserted successfully\"\" }");
                }
                catch (Exception ex)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"103\", \"message\":\"Error:\""+ ex.Message +" }");
                }
            }
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
                        await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"101\", \"message\":\"Institution does not exists!\" }");
                        return;
                    }
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid InstitutionId!\" }");
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
                        await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"101\", \"message\":\"Vehicle does not exists!\" }");
                        return;
                    }
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"103\", \"message\":\"Bad request value. Invalid VehicleId!\" }");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(All) && All.Equals("--all"))
            {
                _all.Add(All, Context.ConnectionId);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("CommonMessage", "{ \"code\":\"105\", \"message\":\"Please provide atleast one parameter!\" }");
                return;
            }
        }

        public async void Unsubscribe() // Subscribe All Vehicle Data For Particular Institution
        {
            _all.RemoveAll(Context.ConnectionId);
            _institutions.RemoveAll(Context.ConnectionId);
            _vehicles.RemoveAll(Context.ConnectionId);
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
            if (Context.GetHttpContext().Request.Query.Keys.Count > 1)
            {
                _deviceId.Add(Context.ConnectionId, Context.GetHttpContext().Request.Query["DeviceId"].ToString());
                _vehiclesId.Add(Context.ConnectionId, Context.GetHttpContext().Request.Query["vehicleId"].ToString());
                _institutionsId.Add(Context.ConnectionId, Context.GetHttpContext().Request.Query["institutionId"].ToString());

                await _threadStatsChangefeedDbService.InsertVehicleDataAsync(new TrackingStats
                {
                    deviceId = Convert.ToDouble(Context.GetHttpContext().Request.Query["DeviceId"].ToString()),
                    vehicleId = Convert.ToInt32(Context.GetHttpContext().Request.Query["vehicleId"].ToString()),
                    institutionId = Convert.ToInt32(Context.GetHttpContext().Request.Query["institutionId"].ToString()),
                    timeStamp = DateTime.UtcNow.ToString()
                });
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _connections.Remove(Context.ConnectionId, Context.GetHttpContext().Request.Query["type"].ToString());
            if (!string.IsNullOrEmpty(Context.GetHttpContext().Request.Query["vehicleId"]))
            {
                _threadStatsChangefeedDbService.ChangeVehicleStatus(Context.GetHttpContext().Request.Query["vehicleId"]);
            }
            _all.RemoveAll(Context.ConnectionId);
            _institutions.RemoveAll(Context.ConnectionId);
            _vehicles.RemoveAll(Context.ConnectionId);
            _deviceId.Remove(Context.ConnectionId);
            _vehiclesId.Remove(Context.ConnectionId);
            _institutionsId.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
    }
}