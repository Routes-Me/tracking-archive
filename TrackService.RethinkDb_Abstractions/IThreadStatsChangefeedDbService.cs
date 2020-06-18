using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrackService.RethinkDb_Abstractions
{
    public interface IThreadStatsChangefeedDbService
    {
        Task EnsureDatabaseCreatedAsync();
        Task InsertThreadStatsAsync(TrackingStats threadStats);
        void UpdateThreadStatsAsync();
        string GetInstitutionId(string vehicleId);
        bool VehicleExists(string vehicleId);
        bool InstitutionExists(string vehicleId);
        Task<IChangefeed<Coordinates>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken);
        VehicleResponse GetVehicleDetail(IdleModel IdleModel);
        VehicleResponse GetVehicleDetailData(IdleModel IdleModel);
        void FrequentChanges();
        void InfrequentChanges();
        void CleanArchiveData();
    }
}