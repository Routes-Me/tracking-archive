using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiveTrackService.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArchiveTrackService.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult GetActionResult(Response response)
        {
            switch (response.responseCode)
            {
                case ResponseCode.Success:
                    return Ok(response);
                case ResponseCode.Error:
                    return BadRequest(response);
                case ResponseCode.NotFound:
                    return NotFound(response);
                case ResponseCode.BadRequest:
                    return BadRequest(response);
                case ResponseCode.Conflict:
                    return Conflict(response);
                case ResponseCode.InternalServerError:
                    return BadRequest(response);
                default:
                    return StatusCode((int)response.responseCode, response.message);
            }
        }
    }
}
