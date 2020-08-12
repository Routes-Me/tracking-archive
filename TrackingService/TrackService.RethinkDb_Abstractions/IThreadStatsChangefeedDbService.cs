using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrackService.RethinkDb_Abstractions
{
    public interface IThreadStatsChangefeedDbService
    {
        Task EnsureDatabaseCreatedAsync();
        Task InsertTrackStatsAsync(TrackingStats trackingStats);
        void UpdateTrackStatsAsync();
        void ChangeVehicleStatus(string vehicleNumber);
        string GetInstitutionId(string vehicleNumber);
        string GetDeviceId(string vehicleNumber);
        bool VehicleExists(string vehicleNumber);
        bool InstitutionExists(string vehicleNumber);
        Task<IChangefeed<Coordinates>> GetTrackStatsChangefeedAsync(CancellationToken cancellationToken);
        VehicleResponse GetAllVehicleDetails(IdleModel IdleModel);
        VehicleResponse GetAllVehicleDetailByInstitutionId(IdleModel IdleModel);
        void SyncCoordinatesToArchiveTable();
        void SyncVehiclesToArchiveTable();
        Task InsertVehicleDataAsync(TrackingStats trackingStats);
    }
}