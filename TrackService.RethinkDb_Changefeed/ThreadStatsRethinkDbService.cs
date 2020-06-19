using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Net;
using TrackService.RethinkDb_Abstractions;

namespace TrackService.RethinkDb_Changefeed
{
    public class ThreadStatsRethinkDbService : IThreadStatsChangefeedDbService
    {
        private const string DATABASE_NAME = "trackingdb";
        private const string VEHICLE_TABLE_NAME = "vehicles";
        private const string CORDINATE_TABLE_NAME = "coordinates";
        private const string ARCHIVE_VEHICLE_TABLE_NAME = "archive_vehicles";
        private const string ARCHIVE_CORDINATE_TABLE_NAME = "archive_coordinates";

        public static bool IsAnotherServiceWorking = false;

        private readonly RethinkDb.Driver.RethinkDB _rethinkDbSingleton;
        private readonly Connection _rethinkDbConnection;

        public ThreadStatsRethinkDbService(IRethinkDbSingletonProvider rethinkDbSingletonProvider)
        {
            if (rethinkDbSingletonProvider == null)
            {
                throw new ArgumentNullException(nameof(rethinkDbSingletonProvider));
            }

            _rethinkDbSingleton = rethinkDbSingletonProvider.RethinkDbSingleton;
            _rethinkDbConnection = rethinkDbSingletonProvider.RethinkDbConnection;
        }

        public Task EnsureDatabaseCreatedAsync()
        {
            if (!((string[])_rethinkDbSingleton.DbList().Run(_rethinkDbConnection).ToObject<string[]>()).Contains(DATABASE_NAME))
            {
                _rethinkDbSingleton.DbCreate(DATABASE_NAME).Run(_rethinkDbConnection);
            }

            var database = _rethinkDbSingleton.Db(DATABASE_NAME);
            if (!((string[])database.TableList().Run(_rethinkDbConnection).ToObject<string[]>()).Contains(VEHICLE_TABLE_NAME))
            {
                database.TableCreate(VEHICLE_TABLE_NAME).Run(_rethinkDbConnection);
            }

            if (!((string[])database.TableList().Run(_rethinkDbConnection).ToObject<string[]>()).Contains(CORDINATE_TABLE_NAME))
            {
                database.TableCreate(CORDINATE_TABLE_NAME).Run(_rethinkDbConnection);
            }

            return Task.CompletedTask;
        }

        public Task InsertThreadStatsAsync(TrackingStats trackingStats)
        {
            ReqlFunction1 filter = expr => expr["Vehicle_Num"].Eq(trackingStats.VehicleId);
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicle = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (vehicle.BufferedSize > 0)
            {
                var id = JObject.Parse(vehicle.BufferedItems[0].ToString()).Children().Values().LastOrDefault().ToString();

                _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)
                        .Filter(new { id = id })
                        .Update(new { TimeStamp = DateTime.UtcNow, IsLive = 1 }).Run(_rethinkDbConnection);

                _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Insert(new Coordinates
                {
                    Vehicle_Id = id,
                    Vehicle_Num = trackingStats.VehicleId,
                    Latitude = trackingStats.Latitude,
                    Longitude = trackingStats.Longitude,
                    TimeStamp = DateTime.UtcNow.ToString()
                }
                ).Run(_rethinkDbConnection);
            }
            else
            {
                _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Insert(new Vehicles
                {
                    Vehicle_Num = trackingStats.VehicleId,
                    InstitutionId = trackingStats.InstitutionId,
                    TimeStamp = DateTime.UtcNow,
                    IsLive = 1
                }
                ).Run(_rethinkDbConnection);
                Cursor<object> res = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
                var id = JObject.Parse(res.BufferedItems[0].ToString()).Children().Values().LastOrDefault().ToString();
                _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Insert(new Coordinates
                {
                    Vehicle_Id = id,
                    Vehicle_Num = trackingStats.VehicleId,
                    Latitude = trackingStats.Latitude,
                    Longitude = trackingStats.Longitude,
                    TimeStamp = DateTime.UtcNow.ToString()
                }
                ).Run(_rethinkDbConnection);
            }

            return Task.CompletedTask;
        }

        public async Task<IChangefeed<Coordinates>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return new RethinkDbChangefeed<Coordinates>(
                await _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Changes().RunChangesAsync<Coordinates>(_rethinkDbConnection, cancellationToken)
            );
        }
        public VehicleResponse GetVehicleDetailData(IdleModel IdleModel)
        {
            var keys = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Run(_rethinkDbConnection);
            DateTime startDate;
            DateTime endDate;
            Cursor<object> vehicles;
            VehicleResponse oVehicleResponse = new VehicleResponse();

            ReqlFunction1 filterForInstitutionId = expr => expr["InstitutionId"].Eq(Convert.ToInt32(IdleModel.institution_id));
            string filterSerializedForInstitutionId = ReqlRaw.ToRawString(filterForInstitutionId);
            var filterExprForInstitutionId = ReqlRaw.FromRawString(filterSerializedForInstitutionId);

            Cursor<object> InstitutionData = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExprForInstitutionId).Run(_rethinkDbConnection);
            if (InstitutionData.BufferedSize == 0)
            {
                oVehicleResponse.Status = false;
                oVehicleResponse.Message = "Institution does not exists in database.";
                oVehicleResponse.responseCode = ResponseCode.NotFound;
                oVehicleResponse.Data = null;
                return oVehicleResponse;
            }

            ReqlFunction1 filterForLive = expr => expr["IsLive"].Eq(0);
            string filterSerializedForLive = ReqlRaw.ToRawString(filterForLive);
            var filterExprForLive = ReqlRaw.FromRawString(filterSerializedForLive);

            if (!string.IsNullOrEmpty(Convert.ToString(IdleModel.start_at)) && !string.IsNullOrEmpty(Convert.ToString(IdleModel.end_at)) && DateTime.TryParse(Convert.ToString(IdleModel.start_at), out startDate) && DateTime.TryParse(Convert.ToString(IdleModel.end_at), out endDate))
            {
                ReqlFunction1 filterForStartDate = expr => expr["TimeStamp"].Ge(startDate);
                string filterSerializedForStartDate = ReqlRaw.ToRawString(filterForStartDate);
                var filterExprForStartDate = ReqlRaw.FromRawString(filterSerializedForStartDate);

                ReqlFunction1 filterForEndDate = expr => expr["TimeStamp"].Le(endDate);
                string filterSerializedForEndDate = ReqlRaw.ToRawString(filterForEndDate);
                var filterExprForEndDate = ReqlRaw.FromRawString(filterSerializedForEndDate);

                vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExprForInstitutionId).Filter(filterExprForLive).Filter(filterForStartDate).Filter(filterForEndDate).Run(_rethinkDbConnection);
            }
            else
            {
                vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExprForInstitutionId).Filter(filterExprForLive).Run(_rethinkDbConnection);
            }



            List<VehicleDetails> listVehicles = new List<VehicleDetails>();
            List<CoordinatesDetail> listCoordinates = new List<CoordinatesDetail>();

            foreach (var vehicle in vehicles)
            {
                string VehicleId = string.Empty, InstitutionId = string.Empty;
                foreach (var value in JObject.Parse(vehicle.ToString()).Children())
                {

                    if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "id")
                    {
                        ReqlFunction1 cordinatefilter = expr => expr["Vehicle_Id"].Eq(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        string cordinatefilterSerialized = ReqlRaw.ToRawString(cordinatefilter);
                        var cordinatefilterExpr = ReqlRaw.FromRawString(cordinatefilterSerialized);
                        Cursor<object> coordinates = _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Filter(cordinatefilterExpr).Run(_rethinkDbConnection);

                        foreach (var coordinate in coordinates)
                        {
                            string Latitude = string.Empty, Longitude = string.Empty, TimeStamp = string.Empty;
                            foreach (var cordinatevalue in JObject.Parse(coordinate.ToString()).Children())
                            {

                                if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "Latitude")
                                {
                                    Latitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "Longitude")
                                {
                                    Longitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "TimeStamp")
                                {
                                    TimeStamp = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                            }
                            listCoordinates.Add(new CoordinatesDetail
                            {
                                latitude = Convert.ToDouble(Latitude),
                                longitude = Convert.ToDouble(Longitude),
                                timestamp = TimeStamp
                            });
                        }
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "Vehicle_Num")
                    {
                        VehicleId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "InstitutionId")
                    {
                        InstitutionId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                }
                listVehicles.Add(new VehicleDetails
                {
                    vehicle_id = VehicleId,
                    institution_id = InstitutionId,
                    coordinates = listCoordinates
                });
            }

            if (listVehicles == null || listVehicles.Count == 0)
            {
                oVehicleResponse.Status = false;
                oVehicleResponse.Message = "Vehicle does not exists in database.";
                oVehicleResponse.responseCode = ResponseCode.NotFound;
                oVehicleResponse.Data = null;
                return oVehicleResponse;
            }

            oVehicleResponse.Status = true;
            oVehicleResponse.Message = "Success";
            oVehicleResponse.responseCode = ResponseCode.Success;
            oVehicleResponse.Data = listVehicles;
            return oVehicleResponse;
        }

        public VehicleResponse GetVehicleDetail(IdleModel IdleModel)
        {
            var keys = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Run(_rethinkDbConnection);
            DateTime startDate;
            DateTime endDate;
            Cursor<object> vehicles;
            VehicleResponse oVehicleResponse = new VehicleResponse();

            ReqlFunction1 filter = expr => expr["IsLive"].Eq(0);
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);

            if (!string.IsNullOrEmpty(Convert.ToString(IdleModel.start_at)) && !string.IsNullOrEmpty(Convert.ToString(IdleModel.end_at)) && DateTime.TryParse(Convert.ToString(IdleModel.start_at), out startDate) && DateTime.TryParse(Convert.ToString(IdleModel.end_at), out endDate))
            {
                ReqlFunction1 filterForStartDate = expr => expr["TimeStamp"].Ge(startDate);
                string filterSerializedForStartDate = ReqlRaw.ToRawString(filterForStartDate);
                var filterExprForStartDate = ReqlRaw.FromRawString(filterSerializedForStartDate);

                ReqlFunction1 filterForEndDate = expr => expr["TimeStamp"].Le(endDate);
                string filterSerializedForEndDate = ReqlRaw.ToRawString(filterForEndDate);
                var filterExprForEndDate = ReqlRaw.FromRawString(filterSerializedForEndDate);

                vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Filter(filterForStartDate).Filter(filterForEndDate).Run(_rethinkDbConnection);
            }
            else
            {
                vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            }


            List<VehicleDetails> listVehicles = new List<VehicleDetails>();
            List<CoordinatesDetail> listCoordinates = new List<CoordinatesDetail>();

            foreach (var vehicle in vehicles)
            {
                string VehicleId = string.Empty, InstitutionId = string.Empty;
                foreach (var value in JObject.Parse(vehicle.ToString()).Children())
                {

                    if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "id")
                    {
                        ReqlFunction1 cordinatefilter = expr => expr["Vehicle_Id"].Eq(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        string cordinatefilterSerialized = ReqlRaw.ToRawString(cordinatefilter);
                        var cordinatefilterExpr = ReqlRaw.FromRawString(cordinatefilterSerialized);
                        Cursor<object> coordinates = _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Filter(cordinatefilterExpr).Run(_rethinkDbConnection);

                        foreach (var coordinate in coordinates)
                        {
                            string Latitude = string.Empty, Longitude = string.Empty, TimeStamp = string.Empty;
                            foreach (var cordinatevalue in JObject.Parse(coordinate.ToString()).Children())
                            {

                                if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "Latitude")
                                {
                                    Latitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "Longitude")
                                {
                                    Longitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "TimeStamp")
                                {
                                    TimeStamp = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                            }
                            listCoordinates.Add(new CoordinatesDetail
                            {
                                latitude = Convert.ToDouble(Latitude),
                                longitude = Convert.ToDouble(Longitude),
                                timestamp = TimeStamp
                            });
                        }
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "Vehicle_Num")
                    {
                        VehicleId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "InstitutionId")
                    {
                        InstitutionId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                }
                listVehicles.Add(new VehicleDetails
                {
                    vehicle_id = VehicleId,
                    institution_id = InstitutionId,
                    coordinates = listCoordinates
                });
            }
            if (listVehicles == null || listVehicles.Count == 0)
            {
                oVehicleResponse.Status = false;
                oVehicleResponse.Message = "Vehicle does not exists in database.";
                oVehicleResponse.responseCode = ResponseCode.NotFound;
                oVehicleResponse.Data = null;
                return oVehicleResponse;
            }

            oVehicleResponse.Status = true;
            oVehicleResponse.Message = "Success";
            oVehicleResponse.responseCode = ResponseCode.Success;
            oVehicleResponse.Data = listVehicles;
            return oVehicleResponse;
        }

        public string GetInstitutionId(string vehicleId)
        {
            var keys = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Get(vehicleId).GetField("InstitutionId").Run(_rethinkDbConnection);
            return Convert.ToString(keys);
        }

        public bool VehicleExists(string vehicleId)
        {
            ReqlFunction1 filter = expr => expr["Vehicle_Num"].Eq(Convert.ToInt32(vehicleId));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (vehicles.Count() > 0)
                return true;
            else
                return false;
        }

        public bool InstitutionExists(string institutionId)
        {
            ReqlFunction1 filter = expr => expr["InstitutionId"].Eq(Convert.ToInt32(institutionId));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> institutions = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (institutions.Count() > 0)
                return true;
            else
                return false;
        }

        public void UpdateThreadStatsAsync()
        {
            ReqlFunction1 filter = expr => expr["TimeStamp"].Le(DateTime.UtcNow.AddMinutes(-10));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);

            foreach (var vehicle in vehicles)
            {
                var id = JObject.Parse(vehicle.ToString()).Children().Values().LastOrDefault().ToString();

                _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)
                        .Filter(new { id = id })
                        .Update(new { IsLive = 0 }).Run(_rethinkDbConnection);
            }
        }

        public void FrequentChanges()
        {
            _rethinkDbSingleton.Db(DATABASE_NAME).Table(ARCHIVE_CORDINATE_TABLE_NAME)
                    .Insert(_rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME)).Run(_rethinkDbConnection);

            _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME)
                    .Delete().Run(_rethinkDbConnection);

        }
        public void InfrequentChanges()
        {
            _rethinkDbSingleton.Db(DATABASE_NAME).Table(ARCHIVE_VEHICLE_TABLE_NAME)
                    .Insert(_rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)).Run(_rethinkDbConnection);

            _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)
                    .Delete().Run(_rethinkDbConnection);

        }

        public void CleanArchiveData()
        {
            _rethinkDbSingleton.Db(DATABASE_NAME).Table(ARCHIVE_VEHICLE_TABLE_NAME)
                    .Delete().Run(_rethinkDbConnection);

            _rethinkDbSingleton.Db(DATABASE_NAME).Table(ARCHIVE_CORDINATE_TABLE_NAME)
                    .Delete().Run(_rethinkDbConnection);
        }
    }
}
