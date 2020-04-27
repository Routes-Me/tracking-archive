using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackService
{
    public class TrackServiceHub : Hub
    {
        //public async Task SendCordinates(string VehicleId, string latitude, string longitude, string datetime)
        //{
        //    await Clients.All.SendAsync("GetCordinates", VehicleId, latitude, longitude, datetime);
        //}
    }
}
