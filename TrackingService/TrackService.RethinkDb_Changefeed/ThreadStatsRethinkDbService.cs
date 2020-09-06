using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Net;
using TrackService.RethinkDb_Abstractions;
using Nancy.Json;
using RethinkDb.Driver;
using System.Net;

namespace TrackService.RethinkDb_Changefeed
{
    public class ThreadStatsRethinkDbService : IThreadStatsChangefeedDbService
    {
        private const string DATABASE_NAME = "trackingdb";
        private const string VEHICLE_TABLE_NAME = "vehicles";
        private const string CORDINATE_TABLE_NAME = "coordinates";

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

        public Task InsertTrackStatsAsync(TrackingStats trackingStats)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(trackingStats.timeStamp)).ToLocalTime();
            ReqlFunction1 filter = expr => expr["vehicleNumber"].Eq(trackingStats.vehicleId);
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicle = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (vehicle.BufferedSize > 0)
            {
                VehicleJSONResponse response = JsonConvert.DeserializeObject<VehicleJSONResponse>(vehicle.BufferedItems[0].ToString());

                _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)
                        .Filter(new { id = response.id })
                        .Update(new { timeStamp = dtDateTime, isLive = 1 }).Run(_rethinkDbConnection);

                _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Insert(new Coordinates
                {
                    vehicleNumber = trackingStats.vehicleId,
                    latitude = trackingStats.latitude,
                    longitude = trackingStats.longitude,
                    timeStamp = dtDateTime
                }
                ).Run(_rethinkDbConnection);
            }

            return Task.CompletedTask;
        }

        public async Task<IChangefeed<Coordinates>> GetTrackStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return new RethinkDbChangefeed<Coordinates>(
                await _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Changes().RunChangesAsync<Coordinates>(_rethinkDbConnection, cancellationToken)
            );
        }

        public VehicleResponse GetAllVehicleDetails(IdleModel IdleModel)
        {
            var keys = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Run(_rethinkDbConnection);
            DateTime startDate;
            DateTime endDate;
            Cursor<object> vehicles;
            VehicleResponse oVehicleResponse = new VehicleResponse();

            ReqlFunction1 filterForinstitutionId = expr => expr["institutionId"].Eq(Convert.ToInt32(IdleModel.institutionId));
            string filterSerializedForinstitutionId = ReqlRaw.ToRawString(filterForinstitutionId);
            var filterExprForinstitutionId = ReqlRaw.FromRawString(filterSerializedForinstitutionId);

            Cursor<object> InstitutionData = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExprForinstitutionId).Run(_rethinkDbConnection);
            if (InstitutionData.BufferedSize == 0)
            {
                oVehicleResponse.Status = false;
                oVehicleResponse.Message = "Institution does not exists in database.";
                oVehicleResponse.responseCode = ResponseCode.NotFound;
                oVehicleResponse.Data = null;
                return oVehicleResponse;
            }

            ReqlFunction1 filterForLive = expr => expr["isLive"].Eq(0);
            string filterSerializedForLive = ReqlRaw.ToRawString(filterForLive);
            var filterExprForLive = ReqlRaw.FromRawString(filterSerializedForLive);

            if (!string.IsNullOrEmpty(Convert.ToString(IdleModel.startAt)) && !string.IsNullOrEmpty(Convert.ToString(IdleModel.endAt)) && DateTime.TryParse(Convert.ToString(IdleModel.startAt), out startDate) && DateTime.TryParse(Convert.ToString(IdleModel.endAt), out endDate))
            {
                ReqlFunction1 filterForStartDate = expr => expr["timeStamp"].Ge(startDate);
                string filterSerializedForStartDate = ReqlRaw.ToRawString(filterForStartDate);
                var filterExprForStartDate = ReqlRaw.FromRawString(filterSerializedForStartDate);

                ReqlFunction1 filterForEndDate = expr => expr["timeStamp"].Le(endDate);
                string filterSerializedForEndDate = ReqlRaw.ToRawString(filterForEndDate);
                var filterExprForEndDate = ReqlRaw.FromRawString(filterSerializedForEndDate);

                vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExprForinstitutionId).Filter(filterExprForLive).Filter(filterForStartDate).Filter(filterForEndDate).Run(_rethinkDbConnection);
            }
            else
            {
                vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExprForinstitutionId).Filter(filterExprForLive).Run(_rethinkDbConnection);
            }

            List<VehicleDetails> listVehicles = new List<VehicleDetails>();
            List<CoordinatesDetail> listCoordinates = new List<CoordinatesDetail>();

            foreach (var vehicle in vehicles)
            {
                string VehicleId = string.Empty, institutionId = string.Empty, DeviceId = string.Empty;
                foreach (var value in JObject.Parse(vehicle.ToString()).Children())
                {

                    if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "id")
                    {
                        ReqlFunction1 cordinatefilter = expr => expr["vehicleId"].Eq(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        string cordinatefilterSerialized = ReqlRaw.ToRawString(cordinatefilter);
                        var cordinatefilterExpr = ReqlRaw.FromRawString(cordinatefilterSerialized);
                        Cursor<object> coordinates = _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Filter(cordinatefilterExpr).Run(_rethinkDbConnection);

                        foreach (var coordinate in coordinates)
                        {
                            string latitude = string.Empty, longitude = string.Empty, timeStamp = string.Empty;
                            foreach (var cordinatevalue in JObject.Parse(coordinate.ToString()).Children())
                            {

                                if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "latitude")
                                {
                                    latitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "longitude")
                                {
                                    longitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "timeStamp")
                                {
                                    timeStamp = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                            }
                            listCoordinates.Add(new CoordinatesDetail
                            {
                                latitude = Convert.ToDouble(latitude),
                                longitude = Convert.ToDouble(longitude),
                                timeStamp = timeStamp
                            });
                        }
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "deviceId")
                    {
                        DeviceId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "vehicleNumber")
                    {
                        VehicleId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "institutionId")
                    {
                        institutionId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                }
                listVehicles.Add(new VehicleDetails
                {
                    deviceId = DeviceId,
                    vehicleId = VehicleId,
                    institutionId = institutionId,
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

        public VehicleResponse GetAllVehicleDetailByInstitutionId(IdleModel IdleModel)
        {
            var keys = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Run(_rethinkDbConnection);
            DateTime startDate;
            DateTime endDate;
            Cursor<object> vehicles;
            VehicleResponse oVehicleResponse = new VehicleResponse();

            ReqlFunction1 filter = expr => expr["isLive"].Eq(0);
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);

            if (!string.IsNullOrEmpty(Convert.ToString(IdleModel.startAt)) && !string.IsNullOrEmpty(Convert.ToString(IdleModel.endAt)) && DateTime.TryParse(Convert.ToString(IdleModel.startAt), out startDate) && DateTime.TryParse(Convert.ToString(IdleModel.endAt), out endDate))
            {
                ReqlFunction1 filterForStartDate = expr => expr["timeStamp"].Ge(startDate);
                string filterSerializedForStartDate = ReqlRaw.ToRawString(filterForStartDate);
                var filterExprForStartDate = ReqlRaw.FromRawString(filterSerializedForStartDate);

                ReqlFunction1 filterForEndDate = expr => expr["timeStamp"].Le(endDate);
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
                string VehicleId = string.Empty, institutionId = string.Empty, DeviceId = string.Empty;
                foreach (var value in JObject.Parse(vehicle.ToString()).Children())
                {

                    if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "id")
                    {
                        ReqlFunction1 cordinatefilter = expr => expr["vehicleId"].Eq(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        string cordinatefilterSerialized = ReqlRaw.ToRawString(cordinatefilter);
                        var cordinatefilterExpr = ReqlRaw.FromRawString(cordinatefilterSerialized);
                        Cursor<object> coordinates = _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Filter(cordinatefilterExpr).Run(_rethinkDbConnection);

                        foreach (var coordinate in coordinates)
                        {
                            string latitude = string.Empty, longitude = string.Empty, timeStamp = string.Empty;
                            foreach (var cordinatevalue in JObject.Parse(coordinate.ToString()).Children())
                            {

                                if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "latitude")
                                {
                                    latitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "longitude")
                                {
                                    longitude = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                                else if (((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Name.ToString() == "timeStamp")
                                {
                                    timeStamp = ((Newtonsoft.Json.Linq.JProperty)cordinatevalue).Value.ToString();
                                }
                            }
                            listCoordinates.Add(new CoordinatesDetail
                            {
                                latitude = Convert.ToDouble(latitude),
                                longitude = Convert.ToDouble(longitude),
                                timeStamp = timeStamp
                            });
                        }
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "deviceId")
                    {
                        DeviceId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "vehicleNumber")
                    {
                        VehicleId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "institutionId")
                    {
                        institutionId = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                    }
                }
                listVehicles.Add(new VehicleDetails
                {
                    deviceId = DeviceId,
                    vehicleId = VehicleId,
                    institutionId = institutionId,
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

        public string GetInstitutionId(string vehicleNumber)
        {
            VehicleJSONResponse response = new VehicleJSONResponse();
            ReqlFunction1 filter = expr => expr["vehicleNumber"].Eq(Convert.ToInt32(vehicleNumber));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicle = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (vehicle.BufferedSize > 0)
            {
                response = JsonConvert.DeserializeObject<VehicleJSONResponse>(vehicle.BufferedItems[0].ToString());
            }

            return Convert.ToString(response.institutionId);
        }

        public string GetDeviceId(string vehicleNumber)
        {
            VehicleJSONResponse response = new VehicleJSONResponse();
            ReqlFunction1 filter = expr => expr["vehicleNumber"].Eq(Convert.ToInt32(vehicleNumber));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicle = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (vehicle.BufferedSize > 0)
            {
                response = JsonConvert.DeserializeObject<VehicleJSONResponse>(vehicle.BufferedItems[0].ToString());
            }

            return Convert.ToString(response.deviceId);
        }

        public bool VehicleExists(string vehicleNumber)
        {
            ReqlFunction1 filter = expr => expr["vehicleNumber"].Eq(Convert.ToInt32(vehicleNumber));
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
            ReqlFunction1 filter = expr => expr["institutionId"].Eq(Convert.ToInt32(institutionId));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> institutions = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (institutions.Count() > 0)
                return true;
            else
                return false;
        }

        // This is called from background service Monitor Vehicle
        public void UpdateTrackStatsAsync()
        {
            ReqlFunction1 filter = expr => expr["timeStamp"].Le(DateTime.UtcNow.AddMinutes(-10));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);

            if (vehicles.BufferedSize > 0)
            {
                foreach (var vehicle in vehicles)
                {
                    VehicleJSONResponse response = JsonConvert.DeserializeObject<VehicleJSONResponse>(vehicle.ToString());

                    _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)
                            .Filter(new { id = response.id })
                            .Update(new { isLive = 0 }).Run(_rethinkDbConnection);
                }
            }
        }

        public void ChangeVehicleStatus(string vehicleId)
        {
            ReqlFunction1 filter = expr => expr["vehicleNumber"].Eq(Convert.ToInt32(vehicleId));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);

            foreach (var vehicle in vehicles)
            {
                var id = JObject.Parse(vehicle.ToString()).Children().Values().LastOrDefault().ToString();

                _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME)
                        .Filter(new { id = id })
                        .Update(new { isLive = 0 }).Run(_rethinkDbConnection);
            }
        }

        public void SyncCoordinatesToArchiveTable()
        {
            try
            {
                ReqlFunction1 filter = expr => expr["timeStamp"].Ge(DateTime.UtcNow.AddHours(-24));
                string filterSerialized = ReqlRaw.ToRawString(filter);
                var filterExpr = ReqlRaw.FromRawString(filterSerialized);
                Cursor<object> coordinates = _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);

                List<ArchiveCoordinates> archiveCoordinates = new List<ArchiveCoordinates>();
                double latitude = 0, longitude = 0;
                int vehicleNumber = 0;
                DateTime? timeStamp = null;
                foreach (var coordinate in coordinates)
                {
                    foreach (var value in JObject.Parse(coordinate.ToString()).Children())
                    {
                        if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "latitude")
                        {
                            latitude = Convert.ToDouble(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        }
                        else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "longitude")
                        {
                            longitude = Convert.ToDouble(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        }
                        else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "timeStamp")
                        {
                            var epocTime = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                            foreach (var timeStampVal in JObject.Parse(epocTime).Children())
                            {
                                if (((Newtonsoft.Json.Linq.JProperty)timeStampVal).Name.ToString() == "epoch_time")
                                {
                                    var UnixTime = ((Newtonsoft.Json.Linq.JProperty)timeStampVal).Value.ToString();

                                    System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                    timeStamp = dateTime.AddSeconds(Convert.ToDouble(UnixTime)).ToLocalTime();
                                    break;
                                }
                            }
                        }
                        else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "vehicleNumber")
                        {
                            vehicleNumber = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                        }
                    }

                    archiveCoordinates.Add(new ArchiveCoordinates
                    {
                        latitude = latitude,
                        longitude = longitude,
                        timeStamp = timeStamp,
                        vehicleNumber = vehicleNumber
                    });
                }

                if (archiveCoordinates.Count > 0 && archiveCoordinates != null)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("http://localhost:56708/api/");
                        var res = client.PostAsync("feeds", new StringContent(new JavaScriptSerializer().Serialize(archiveCoordinates), Encoding.UTF8, "application/json")).Result;

                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            _rethinkDbSingleton.Db(DATABASE_NAME).Table(CORDINATE_TABLE_NAME).Filter(filterExpr).Delete().Run(_rethinkDbConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }
        }

        public void SyncVehiclesToArchiveTable()
        {
            ReqlFunction1 filter = expr => expr["timeStamp"].Ge(DateTime.UtcNow.AddDays(-7));
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicles = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);

            List<ArchiveVehicles> archiveVehiclesList = new List<ArchiveVehicles>();
            int institutionId = 0, isLive = 0, vehicleNumber = 0;
            DateTime? timeStamp = null;
            foreach (var vehicle in vehicles)
            {
                foreach (var value in JObject.Parse(vehicle.ToString()).Children())
                {
                    if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "institutionId")
                    {
                        institutionId = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "isLive")
                    {
                        isLive = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "timeStamp")
                    {
                        var epocTime = ((Newtonsoft.Json.Linq.JProperty)value).Value.ToString();
                        foreach (var timeStampVal in JObject.Parse(epocTime).Children())
                        {
                            if (((Newtonsoft.Json.Linq.JProperty)timeStampVal).Name.ToString() == "epoch_time")
                            {
                                var UnixTime = ((Newtonsoft.Json.Linq.JProperty)timeStampVal).Value.ToString();

                                System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                timeStamp = dateTime.AddSeconds(Convert.ToDouble(UnixTime)).ToLocalTime();
                                break;
                            }
                        }
                    }
                    else if (((Newtonsoft.Json.Linq.JProperty)value).Name.ToString() == "vehicleNumber")
                    {
                        vehicleNumber = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)value).Value.ToString());
                    }
                }

                archiveVehiclesList.Add(new ArchiveVehicles
                {
                    institutionId = Convert.ToInt32(institutionId),
                    timeStamp = timeStamp,
                    vehicleNumber = Convert.ToInt32(vehicleNumber),
                    isLive = Convert.ToInt32(isLive)
                });
            }

            if (archiveVehiclesList.Count > 0 && archiveVehiclesList != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:56708/api/");
                    var res = client.PostAsync("vehicles", new StringContent(new JavaScriptSerializer().Serialize(archiveVehiclesList), Encoding.UTF8, "application/json")).Result;

                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Delete().Run(_rethinkDbConnection);
                    }
                }
            }
        }

        public Task InsertVehicleDataAsync(TrackingStats trackingStats)
        {
            ReqlFunction1 filter = expr => expr["vehicleNumber"].Eq(trackingStats.vehicleId);
            string filterSerialized = ReqlRaw.ToRawString(filter);
            var filterExpr = ReqlRaw.FromRawString(filterSerialized);
            Cursor<object> vehicle = _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Filter(filterExpr).Run(_rethinkDbConnection);
            if (vehicle.BufferedSize == 0)
            {
                _rethinkDbSingleton.Db(DATABASE_NAME).Table(VEHICLE_TABLE_NAME).Insert(new Vehicles
                {
                    deviceId = trackingStats.deviceId,
                    vehicleNumber = trackingStats.vehicleId,
                    institutionId = trackingStats.institutionId,
                    timeStamp = DateTime.UtcNow,
                    isLive = 1
                }
                ).Run(_rethinkDbConnection);
            }

            return Task.CompletedTask;
        }
    }
}
