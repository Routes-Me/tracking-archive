using ArchiveTrackService.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Models
{
    public class Response
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ResponseCode responseCode { get; set; }

    }
    public enum ResponseCode
    {
        Success = 200,
        Error = 2,
        InternalServerError = 500,
        MovedPermanently = 301,
        NotFound = 404,
        BadRequest = 400,
        Conflict = 409,
        Created = 201,
        NotAcceptable = 406,
        Unauthorized = 401,
        RequestTimeout = 408,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        Permissionserror = 403,
        Forbidden = 403,
        TokenRequired = 499,
        InvalidToken = 498
    }
    public class feedResponse : Response
    {
        public CoordinateDetails coordinateDetails { get; set; }
    }

    public class vehicleResponse : Response
    {
        public VehicleDetails vehicleDetails { get; set; }
    }

    public class VehicleDetails
    {
        public Pagination pagination { get; set; }
        public List<Archivevehicles> data { get; set; }
    }   

    public class CoordinateDetails
    {
        public Pagination pagination { get; set; }
        public List<Archivecoordinates> data { get; set; }
    }
}
