using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Models
{
    public class GetFeedsModel
    {
        public string vehicleIds { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
