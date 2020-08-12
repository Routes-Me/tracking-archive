using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Models
{
    public class DeleteVehiclesModel
    {
        public string vehicleIds { get; set; }
        public string institutionIds { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
    }
}
