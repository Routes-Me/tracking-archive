using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiveTrackService.Abstraction;
using ArchiveTrackService.Models;
using ArchiveTrackService.Models.DBModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArchiveTrackService.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclesController : BaseController
    {
        private readonly IVehicleRepository _vehicleRepository;
        public VehiclesController(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        [HttpGet]
        [Route("vehicles")]
        public IActionResult Get(string vehicleIds, string institutionIds, DateTime? start, DateTime? end, int currentPage = 1, int pageSize = 10)
        {
            vehicleResponse response = new vehicleResponse();
            GetVehiclesModel Model = new GetVehiclesModel();
            Model.vehicleIds = vehicleIds;
            Model.institutionIds = institutionIds;
            Model.start = start;
            Model.end = end;
            Model.currentPage = currentPage;
            Model.pageSize = pageSize;
            response = _vehicleRepository.getVehicles(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("vehicles")]
        public IActionResult Post(List<Archivevehicles> Model)
        {
            vehicleResponse response = new vehicleResponse();
            if (ModelState.IsValid)
                response = _vehicleRepository.InsertVehicles(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("vehicles")]
        public IActionResult Delete(string vehicleIds, string institutionIds, DateTime? start, DateTime? end)
        {
            vehicleResponse response = new vehicleResponse();
            DeleteVehiclesModel Model = new DeleteVehiclesModel();
            Model.vehicleIds = vehicleIds;
            Model.institutionIds = institutionIds;
            Model.start = start;
            Model.end = end;
            response = _vehicleRepository.DeleteVehicles(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
