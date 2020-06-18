
using Newtonsoft.Json;
using RethinkDb.Driver.Ast;
using System;
using System.Collections.Generic;

namespace TrackService.Models
{
    public class vehicles
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public int Vehicle_Num { get; set; }
        public int InstitutionId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int IsLive { get; set; }

    }

    public class coordinates
    {
        public string Vehicle_Id { get; set; }
        public int Vehicle_Num { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string TimeStamp { get; set; }

    }

    public class archive_vehicles : vehicles
    {

    }

    public class archive_coordinates : coordinates
    {

    }
}
