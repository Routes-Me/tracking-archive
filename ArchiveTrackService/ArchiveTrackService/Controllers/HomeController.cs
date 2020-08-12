using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArchiveTrackService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "Archive Tracking Service Start Successfully.";
        }
    }
}
