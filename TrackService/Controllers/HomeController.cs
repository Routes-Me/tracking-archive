using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrackService.RethinkDb_Abstractions;

namespace TrackService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IThreadStatsChangefeedDbService _changeFeed;
        private Timer _timer;

        public HomeController(IThreadStatsChangefeedDbService changeFeed)
        {
            _changeFeed = changeFeed;
        }

        [HttpGet]
        public string Get()
        {
            //_timer = new Timer(DoWork_Frequent, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            //_timer = new Timer(DoWork_Infrequent, null, TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(40));

            //_timer = new Timer(DoWork_CleanData, null, TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(50));

            return "Server start successfully";
        }

        private void DoWork_Frequent(object state)
        {
            _changeFeed.FrequentChanges();
        }
        private void DoWork_Infrequent(object state)
        {
            _changeFeed.InfrequentChanges();
        }
        private void DoWork_CleanData(object state)
        {
            _changeFeed.CleanArchiveData();
        }
    }
}
