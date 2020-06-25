using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackService.Helper;
using TrackService.Models;
using TrackService.RethinkDb_Abstractions;
using IdleModel = TrackService.RethinkDb_Abstractions.IdleModel;
using VehicleDetails = TrackService.RethinkDb_Abstractions.VehicleDetails;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TrackService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        public readonly static AllVehicleMapping<string> _allvehicles = new AllVehicleMapping<string>();
        public readonly static InstitutionsMapping<string> _institutions = new InstitutionsMapping<string>();
        public readonly static VehiclesMapping<string> _subscribedvehicles = new VehiclesMapping<string>();
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        public VehicleController(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        [HttpGet]
        [Route("VehicleStatus")]
        public async Task<IActionResult> VehicleStatus(IdleModel IdleModel)
        {
            VehicleResponse oVehicleResponse = new VehicleResponse();
            try
            {
                if (ModelState.IsValid)
                {
                    List<VehicleDetails> vehicleDetails = new List<VehicleDetails>();

                    if (string.IsNullOrEmpty(Convert.ToString(IdleModel.institution_id)))
                        await Task.Run(() => { oVehicleResponse = _threadStatsChangefeedDbService.GetAllVehicleDetails(IdleModel); }).ConfigureAwait(false);
                    else
                        await Task.Run(() => { oVehicleResponse = _threadStatsChangefeedDbService.GetAllVehicleDetailByInstitutionId(IdleModel); }).ConfigureAwait(false);

                    if (oVehicleResponse.responseCode != ResponseCode.Success)
                        return NotFound(oVehicleResponse);
                    else
                        return Ok(oVehicleResponse);
                }
                else
                {
                    oVehicleResponse.Status = false;
                    oVehicleResponse.Message = "Validation error.";
                    oVehicleResponse.responseCode = ResponseCode.Forbidden;
                    oVehicleResponse.Data = null;
                    return BadRequest(oVehicleResponse);
                }
            }
            catch (Exception ex)
            {
                oVehicleResponse.Status = false;
                oVehicleResponse.Message = "Something went wrong while getting vehicle data. Error - " + ex.Message;
                oVehicleResponse.responseCode = ResponseCode.InternalServerError;
                oVehicleResponse.Data = null;
                return BadRequest(oVehicleResponse);
            }
        }
    }
}
