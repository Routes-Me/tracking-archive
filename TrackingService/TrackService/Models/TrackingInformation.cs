
using Newtonsoft.Json;
using RethinkDb.Driver.Ast;
using System;
using System.Collections.Generic;

namespace TrackService.Models
{
    public class vehicles
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public int vehicleNumber { get; set; }
        public int institutionId { get; set; }
        public DateTime timeStamp { get; set; }
        public int isLive { get; set; }

    }

    public class coordinates
    {
        public string vehicleId { get; set; }
        public int vehicleNumber { get; set; }
        public decimal longitude { get; set; }
        public decimal latitude { get; set; }
        public string timeStamp { get; set; }
    }
}
