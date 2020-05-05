using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;


namespace TrackService
{
    public class TrackServiceHub : Hub
    {
        //public async Task SendCordinates(string trackingInfo)
        //{
        //    await Clients.All.SendAsync("GetCordinates", trackingInfo);
        //}

    }

}