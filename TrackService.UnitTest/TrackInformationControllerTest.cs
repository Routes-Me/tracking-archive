using System;
using System.Collections.Generic;
using System.Text;
using TrackService.Controllers;
using TrackService.IServices;
using TrackService.Models;
using TrackService.Services;
using Xunit;

namespace TrackService.UnitTest
{
    public class TrackInformationControllerTest
    {
        TrackInformationController _controller;
        ITrackInformationService _service;
        TrackingInformation _trackingInfo;

        public TrackInformationControllerTest()
        {
            _service = new TrackInformationService(_trackingInfo);
            _controller = new TrackInformationController(_service);
        }

        //[Fact]
        //public bool SetTrackingInformation_Add(string trackingInfo)
        //{
        //}
    }
}
