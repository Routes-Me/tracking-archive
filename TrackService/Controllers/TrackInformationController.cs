using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Data;
using System.Threading.Tasks;
using TrackService.Models;
using TrackService.Services;

namespace TrackService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackInformationController : ControllerBase
    {
        private TrackingInformation _trackingInfo;
        private TrackInformationService _trackInformationService;
        public TrackInformationController(TrackInformationService trackInformationService)
        {
            _trackInformationService = trackInformationService;
        }

        [HttpGet]
        public bool SetTrackingInformation(string trackingInfo)
        {
            return _trackInformationService.SetTrackingInformation(trackingInfo);
        }

            
    }
}
