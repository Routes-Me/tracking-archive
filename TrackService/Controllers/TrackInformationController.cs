using Microsoft.AspNetCore.Mvc;
using TrackService.IServices;
using TrackService.Models;

namespace TrackService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackInformationController : ControllerBase
    {
        private TrackingInformation _trackingInfo;
        private ITrackInformationService _trackInformationService;
        public TrackInformationController(ITrackInformationService trackInformationService)
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
