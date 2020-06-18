using System;
using System.Collections.Generic;
using System.Text;

namespace TrackService.RethinkDb_Abstractions
{
    public class TrackingStats
    {
        public int VehicleId { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string TimeStamp { get; set; }
        public int InstitutionId { get; set; }

        public override string ToString()
        {
            return $"vehicle_id: {VehicleId}, institution_id: {InstitutionId}, latitude: {Latitude}, longitude: {Longitude}, timestamp: {TimeStamp}";
        }
    }

    public class Vehicles
    {
        public int Vehicle_Num { get; set; }
        public int InstitutionId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int IsLive { get; set; }

        public override string ToString()
        {
            return $"vehicle_num: {Vehicle_Num}, institution_id: {InstitutionId}, timestamp: {TimeStamp}, islive: {IsLive}";
        }
    }

    public class Coordinates
    {
        public string Vehicle_Id { get; set; }
        public int Vehicle_Num { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string TimeStamp { get; set; }

        public override string ToString()
        {
            return $"vehicle_id: {Vehicle_Id}, vehicle_num: {Vehicle_Num}, latitude: {Latitude}, longitude: {Longitude}, timestamp: {TimeStamp}";
        }
    }

    public class IdleModel
    {
        public string status { get; set; }
        public string institution_id { get; set; }
        public string start_at { get; set; }
        public string end_at { get; set; }

    }

    public class VehicleDetails
    {
        public string vehicle_id { get; set; }
        public string institution_id { get; set; }
        public List<CoordinatesDetail> coordinates { get; set; }

    }

    public class CoordinatesDetail
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timestamp { get; set; }
    }

}
