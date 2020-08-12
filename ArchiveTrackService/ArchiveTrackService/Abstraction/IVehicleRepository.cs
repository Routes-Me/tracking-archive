using ArchiveTrackService.Models;
using ArchiveTrackService.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Abstraction
{
    public interface IVehicleRepository
    {
        vehicleResponse getVehicles(GetVehiclesModel Model);
        vehicleResponse InsertVehicles(List<Archivevehicles> model);
        vehicleResponse DeleteVehicles(DeleteVehiclesModel model);
    }
}
