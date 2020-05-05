using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
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
            _connection.StartAsync();
            _trackingInfo = trackingInfo;
        }
        public bool SetTrackingInformation(string trackingInfo)
        {
            //vehicle_id:100;lat:2000303;long:293923923424;date:12-09-2003T14:00:00

            if (_connectionState == HubConnectionState.Connected)
            {
                List<string> dataList = trackingInfo.Split(';').ToList();
                Dictionary<string, string> dict = new Dictionary<string, string>();

                var result = new string[2];

                for (int i = 0; dataList.Count >= i; i++)
                {
                    if (i < 3)
                    {
                        result = dataList.ElementAt(i).Split(':');
                        dict.Add(result[0], result[1]);
                    }
                    else
                    {
                        var dateWord = dataList.ElementAt(3).Substring(0, 4);
                        var dateValue = dataList.ElementAt(3).Substring(5, dataList.ElementAt(3).Length - 5).Replace('T', ' ');
                        dict.Add(dateWord, dateValue);
                    }
                }
            }
            else
            {
                _connection.StartAsync();
            }
            return true;
        }
    }
}
