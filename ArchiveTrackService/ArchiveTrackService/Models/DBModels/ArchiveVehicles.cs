using System;
using System.Collections.Generic;

namespace ArchiveTrackService.Models.DBModels
{
    public partial class Archivevehicles
    {
        public int Id { get; set; }
        public int? VehicleNumber { get; set; }
        public int? InstitutionId { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int? IsLive { get; set; }
        public string DeviceId { get; set; }
    }
}
