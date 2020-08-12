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
    public class FeedsController : BaseController
    {
        private readonly ICoordinateRepository _coordinateRepository;
        public FeedsController(ICoordinateRepository coordinateRepository)
        {
            _coordinateRepository = coordinateRepository;
        }

        [HttpGet]
        [Route("feeds")]
        public IActionResult Get(string vehicleIds, DateTime? start, DateTime? end, int currentPage = 1, int pageSize = 10)
        {
            feedResponse response = new feedResponse();
            GetFeedsModel Model = new GetFeedsModel();
            Model.vehicleIds = vehicleIds;
            Model.start = start;
            Model.end = end;
            Model.currentPage = currentPage;
            Model.pageSize = pageSize;
            response = _coordinateRepository.getCoordinates(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("feeds")]
        public IActionResult Post(List<Archivecoordinates> Model)
        {
            feedResponse response = new feedResponse();
            if (ModelState.IsValid)
                response = _coordinateRepository.InsertCoordinates(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("feeds")]
        public IActionResult Delete(string vehicleIds, DateTime? start, DateTime? end)
        {
            feedResponse response = new feedResponse();
            DeleteFeedsModel Model = new DeleteFeedsModel();
            Model.vehicleIds = vehicleIds;
            Model.start = start;
            Model.end = end;
            response = _coordinateRepository.DeleteCoordinates(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
