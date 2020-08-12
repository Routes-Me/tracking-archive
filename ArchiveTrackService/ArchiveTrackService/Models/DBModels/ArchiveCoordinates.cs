using System;
using System.Collections.Generic;

namespace ArchiveTrackService.Models.DBModels
{
    public partial class Archivecoordinates
    {
        public int Id { get; set; }
        public int? VehicleNumber { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public DateTime? TimeStamp { get; set; }
    }
}
