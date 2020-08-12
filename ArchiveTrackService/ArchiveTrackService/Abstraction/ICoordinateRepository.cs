using ArchiveTrackService.Models;
using ArchiveTrackService.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Abstraction
{
    public interface ICoordinateRepository
    {
        feedResponse getCoordinates(GetFeedsModel Model);
        feedResponse InsertCoordinates(List<Archivecoordinates> model);
        feedResponse DeleteCoordinates(DeleteFeedsModel model);
    }
}
