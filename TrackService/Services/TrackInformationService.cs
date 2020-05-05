using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackService.Models;

namespace TrackService.Services
{
    public class TrackInformationService
    {
        HubConnection _connection;
        private HubConnectionState _connectionState;
        private TrackingInformation _trackingInfo;

        public TrackInformationService(TrackingInformation trackingInfo)
        {
            _connection = new HubConnectionBuilder()
              .WithUrl("http://localhost:53353/TrackingServiceHub")
              .Build();


            _trackingInfo = trackingInfo;
        }
        public bool SetTrackingInformation(string trackingInfo)
        {
            //vehicle_id:100;lat:2000303;long:293923923424;date:12-09-2003T14:00:00

            if (_connectionState == HubConnectionState.Connected)
            {
                string[] dataList = trackingInfo.Split(';');

                string vehicleId = dataList[0].ToString();
                _trackingInfo.VehicleId = vehicleId.Substring(vehicleId.IndexOf(":") + 1);


                string latitude = dataList[1].ToString();
                _trackingInfo.Latitude = latitude.Substring(latitude.IndexOf(":") + 1);


                string longitude = dataList[2].ToString();
                _trackingInfo.Longitude = longitude.Substring(longitude.IndexOf(":") + 1);


                string date = dataList[3].ToString();
                _trackingInfo.DateTime = date.Substring(date.IndexOf(":") + 1);
            }
            else
            {
                _connection.StartAsync();
            }
            return true;
        }
    }
}
