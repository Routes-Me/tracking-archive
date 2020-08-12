using System;
using System.Collections.Generic;
using System.Text;

namespace TrackService.RethinkDb_Abstractions
{
    public class TrackingStats
    {
        public int vehicleId { get; set; }
        public int institutionId { get; set; }
        public double deviceId { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string timeStamp { get; set; }
        

        public override string ToString()
        {
            return $"vehicleId: {vehicleId}, institutionId: {institutionId}, deviceId: {deviceId}, latitude: {latitude}, longitude: {longitude}, timeStamp: {timeStamp}";
        }
    }

    public class Vehicles
    {
        public double deviceId { get; set; }
        public int vehicleNumber { get; set; }
        public int institutionId { get; set; }
        public DateTime timeStamp { get; set; }
        public int isLive { get; set; }

        public override string ToString()
        {
            return $"vehicleNumber: {vehicleNumber}, institutionId: {institutionId},  deviceId: {deviceId}, timeStamp: {timeStamp}, isLive: {isLive}";
        }
    }

    public class Coordinates
    {
        public int vehicleNumber { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public DateTime timeStamp { get; set; }

        public override string ToString()
        {
            return $"vehicleNumber: {vehicleNumber}, latitude: {latitude}, longitude: {longitude}, timeStamp: {timeStamp}";
        }
    }

    public class IdleModel
    {
        public string status { get; set; }
        public string institutionId { get; set; }
        public string startAt { get; set; }
        public string endAt { get; set; }

    }

    public class VehicleDetails
    {
        public string deviceId { get; set; }
        public string vehicleId { get; set; }
        public string institutionId { get; set; }
        public List<CoordinatesDetail> coordinates { get; set; }

    }

    public class CoordinatesDetail
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timeStamp { get; set; }
    }

    public class ArchiveVehiclesList
    { 
        public List<ArchiveVehicles> archiveVehiclesList { get; set; }
    }
    public class ArchiveCoordinatesList
    {
        public List<ArchiveCoordinates> archiveCoordinatesList { get; set; }
    }
    public class DeleteVehicles
    {
        public double deviceId { get; set; }
        public string vehicleIds { get; set; }
        public string institutionIds { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
    }

    public class DeleteFeeds
    {
        public string vehicleIds { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }

    public class ArchiveVehicles
    {
        public double? deviceId { get; set; }
        public int? vehicleNumber { get; set; }
        public int? institutionId { get; set; }
        public DateTime? timeStamp { get; set; }
        public int? isLive { get; set; }
    }


    public class ArchiveCoordinates
    {
        public int? vehicleNumber { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public DateTime? timeStamp { get; set; }
    }

    public class VehicleJSONResponse
    {
        public string id { get; set;  }
        public double? deviceId { get; set; }
        public int vehicleNumber { get; set; }
        public int institutionId { get; set; }
        public timeStamp timeStamp { get; set; }
        public int isLive { get; set; }
    }

    public class timeStamp
    {
        public string reql_type { get; set; }
        public decimal epoch_time { get; set; }
        public string timezone { get; set; }
    }
}
