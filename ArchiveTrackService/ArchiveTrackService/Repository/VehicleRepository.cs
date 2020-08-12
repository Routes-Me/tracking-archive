using ArchiveTrackService.Abstraction;
using ArchiveTrackService.Models;
using ArchiveTrackService.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Repository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly archivetrackserviceContext _context;
        public VehicleRepository(archivetrackserviceContext context)
        {
            _context = context;
        }
        public vehicleResponse getVehicles(GetVehiclesModel Model)
        {
            vehicleResponse objVehicleResponse = new vehicleResponse();
            VehicleDetails objVehicleDetails = new VehicleDetails();
            int totalCount = 0;
            try
            {
                List<Archivevehicles> objVehicleList = new List<Archivevehicles>();
                if (!string.IsNullOrEmpty(Model.vehicleIds) && !string.IsNullOrEmpty(Model.institutionIds))
                {
                    string[] VehicleArr = Model.vehicleIds.Split(',');
                    string[] InstitutionArr = Model.institutionIds.Split(',');

                    List<Int32?> VehicleIds = new List<Int32?>();
                    List<Int32?> InstitutionIds = new List<Int32?>();

                    if (VehicleArr.Length > 0)
                    {
                        foreach (var item in VehicleArr)
                        {
                            VehicleIds.Add(Convert.ToInt32(item));
                        }
                    }
                    if (InstitutionArr.Length > 0)
                    {
                        foreach (var item in InstitutionArr)
                        {
                            InstitutionIds.Add(Convert.ToInt32(item));
                        }
                    }

                    if (Model.start != null && Model.end != null)
                    {
                        objVehicleList = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end
                                               && VehicleIds.Contains(x.VehicleNumber) && InstitutionIds.Contains(x.InstitutionId)).
                                               OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                        totalCount = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end
                                               && VehicleIds.Contains(x.VehicleNumber) && InstitutionIds.Contains(x.InstitutionId)).ToList().Count();
                    }
                    else
                    {
                        objVehicleList = (from av in _context.Archivevehicles
                                          where VehicleIds.Contains(Convert.ToInt32(av.VehicleNumber)) && InstitutionIds.Contains(av.InstitutionId)
                                          select av
                        ).OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                        totalCount = (from av in _context.Archivevehicles
                                      where VehicleIds.Contains(Convert.ToInt32(av.VehicleNumber)) && InstitutionIds.Contains(av.InstitutionId)
                                      select av
                        ).ToList().Count();
                    }
                }
                else if (!string.IsNullOrEmpty(Model.vehicleIds) && string.IsNullOrEmpty(Model.institutionIds))
                {
                    List<Int32?> VehicleIds = new List<Int32?>();
                    string[] VehicleArr = Model.vehicleIds.Split(',');
                    if (VehicleArr.Length > 0)
                    {
                        foreach (var item in VehicleArr)
                        {
                            VehicleIds.Add(Convert.ToInt32(item));
                        }
                    }

                    if (VehicleIds != null)
                    {
                        if (Model.start != null && Model.end != null)
                        {
                            objVehicleList = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end
                                                       && VehicleIds.Contains(x.VehicleNumber))
                                                       .OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                            totalCount = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end
                                                       && VehicleIds.Contains(x.VehicleNumber)).ToList().Count();
                        }
                        else
                        {
                            objVehicleList = (from av in _context.Archivevehicles
                                              where VehicleIds.Contains(Convert.ToInt32(av.VehicleNumber))
                                              select av
                            ).OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                            totalCount = (from av in _context.Archivevehicles
                                          where VehicleIds.Contains(Convert.ToInt32(av.VehicleNumber))
                                          select av
                            ).ToList().Count();
                        }
                    }
                }
                else if (string.IsNullOrEmpty(Model.vehicleIds) && !string.IsNullOrEmpty(Model.institutionIds))
                {
                    string[] InstitutionArr = Model.institutionIds.Split(',');
                    List<Int32?> InstitutionIds = new List<Int32?>();

                    if (InstitutionArr.Length > 0)
                    {
                        foreach (var item in InstitutionArr)
                        {
                            InstitutionIds.Add(Convert.ToInt32(item));
                        }
                    }

                    if (InstitutionIds != null)
                    {
                        if (Model.start != null && Model.end != null)
                        {
                            objVehicleList = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end
                            && InstitutionIds.Contains(x.InstitutionId)).OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                            totalCount = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end
                             && InstitutionIds.Contains(x.InstitutionId)).ToList().Count();
                        }
                        else
                        {
                            objVehicleList = (from av in _context.Archivevehicles
                                              where InstitutionIds.Contains(Convert.ToInt32(av.InstitutionId))
                                              select av
                           ).OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                            totalCount = (from av in _context.Archivevehicles
                                         where InstitutionIds.Contains(Convert.ToInt32(av.InstitutionId))
                                         select av
                           ).ToList().Count();
                        }
                    }
                }
                else if (string.IsNullOrEmpty(Model.vehicleIds) && string.IsNullOrEmpty(Model.institutionIds))
                {
                    if (Model.start != null && Model.end != null)
                    {
                        objVehicleList = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                        totalCount = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList().Count();
                    }
                    else
                    {
                        objVehicleList = _context.Archivevehicles.OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                        totalCount = _context.Archivevehicles.ToList().Count();
                    }
                }
                objVehicleDetails.data = objVehicleList;
                if (objVehicleList == null || objVehicleList.ToList().Count == 0)
                {
                    objVehicleResponse.status = false;
                    objVehicleResponse.message = "Vehicle not found.";
                    objVehicleResponse.vehicleDetails = null;
                    objVehicleResponse.responseCode = ResponseCode.NotFound;
                    return objVehicleResponse;
                }
                var page = new Pagination
                {
                    TotalCount = totalCount,
                    CurrentPage = Model.currentPage,
                    PageSize = Model.pageSize,
                    TotalPages = (int)Math.Ceiling(decimal.Divide(totalCount, Model.pageSize)),
                    IndexOne = ((Model.currentPage - 1) * Model.pageSize + 1),
                    IndexTwo = (((Model.currentPage - 1) * Model.pageSize + Model.pageSize) <= totalCount ? ((Model.currentPage - 1) * Model.pageSize + Model.pageSize) : totalCount)
                };
                objVehicleDetails.pagination = page;
                objVehicleResponse.status = true;
                objVehicleResponse.message = "Get all Vehicles successfully.";
                objVehicleResponse.vehicleDetails = objVehicleDetails;
                objVehicleResponse.responseCode = ResponseCode.Success;
                return objVehicleResponse;
            }
            catch (Exception ex)
            {
                objVehicleResponse.status = false;
                objVehicleResponse.message = "Something went wrong while getting Vehicles. Error Message - " + ex.Message;
                objVehicleResponse.vehicleDetails = null;
                objVehicleResponse.responseCode = ResponseCode.InternalServerError;
                return objVehicleResponse;
            }
        }
        public vehicleResponse InsertVehicles(List<Archivevehicles> Model)
        {
            vehicleResponse objVehicleResponse = new vehicleResponse();
            try
            {
                if (Model == null)
                {
                    objVehicleResponse.status = false;
                    objVehicleResponse.message = "Pass valid data in model.";
                    objVehicleResponse.vehicleDetails = null;
                    objVehicleResponse.responseCode = ResponseCode.BadRequest;
                    return objVehicleResponse;
                }
                _context.Archivevehicles.AddRange(Model);
                _context.SaveChanges();
                objVehicleResponse.status = true;
                objVehicleResponse.message = "Vehicles inserted successfully.";
                objVehicleResponse.vehicleDetails = null;
                objVehicleResponse.responseCode = ResponseCode.Success;
                return objVehicleResponse;
            }
            catch (Exception ex)
            {
                objVehicleResponse.status = false;
                objVehicleResponse.message = "Something went wrong while inserting vehicles. Error Message - " + ex.Message;
                objVehicleResponse.vehicleDetails = null;
                objVehicleResponse.responseCode = ResponseCode.InternalServerError;
                return objVehicleResponse;
            }
        }
        public vehicleResponse DeleteVehicles(DeleteVehiclesModel Model)
        {
            vehicleResponse objVehicleResponse = new vehicleResponse();
            try
            {
                List<Archivevehicles> objVehicleList = new List<Archivevehicles>();
                List<Archivevehicles> tempVehicleList;

                if (!string.IsNullOrEmpty(Model.vehicleIds) && !string.IsNullOrEmpty(Model.institutionIds))
                {
                    string[] VehicleArr = Model.vehicleIds.Split(',');
                    string[] InstitutionArr = Model.institutionIds.Split(',');

                    List<Int32?> VehicleIds = new List<Int32?>();
                    List<Int32?> InstitutionIds = new List<Int32?>();

                    if (VehicleArr.Length > 0)
                    {
                        foreach (var item in VehicleArr)
                        {
                            VehicleIds.Add(Convert.ToInt32(item));
                        }
                    }
                    if (InstitutionArr.Length > 0)
                    {
                        foreach (var item in InstitutionArr)
                        {
                            InstitutionIds.Add(Convert.ToInt32(item));
                        }
                    }

                    tempVehicleList = new List<Archivevehicles>();
                    if (Model.start != null && Model.end != null)
                        tempVehicleList = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end && VehicleIds.Contains(x.VehicleNumber) && InstitutionIds.Contains(x.InstitutionId)).ToList();
                    else
                        tempVehicleList = _context.Archivevehicles.Where(x => (x.IsLive == 0 || x.IsLive == 1) && VehicleIds.Contains(x.VehicleNumber) && InstitutionIds.Contains(x.InstitutionId)).ToList();

                    objVehicleList.AddRange(tempVehicleList);
                }
                else if (!string.IsNullOrEmpty(Model.vehicleIds) && string.IsNullOrEmpty(Model.institutionIds))
                {
                    string[] VehicleArr = Model.vehicleIds.Split(',');
                    if (VehicleArr != null)
                    {
                        foreach (var item in VehicleArr)
                        {
                            tempVehicleList = new List<Archivevehicles>();
                            if (Model.start != null && Model.end != null)
                                tempVehicleList = _context.Archivevehicles.Where(x => x.VehicleNumber == Convert.ToInt32(item) && x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList();
                            else
                                tempVehicleList = _context.Archivevehicles.Where(x => x.VehicleNumber == Convert.ToInt32(item)).ToList();

                            objVehicleList.AddRange(tempVehicleList);
                        }
                    }
                }
                else if (string.IsNullOrEmpty(Model.vehicleIds) && !string.IsNullOrEmpty(Model.institutionIds))
                {
                    string[] InstitutionArr = Model.institutionIds.Split(',');
                    if (InstitutionArr != null)
                    {
                        foreach (var item in InstitutionArr)
                        {
                            tempVehicleList = new List<Archivevehicles>();
                            if (Model.start != null && Model.end != null)
                                tempVehicleList = _context.Archivevehicles.Where(x => x.InstitutionId == Convert.ToInt32(item) && x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList();
                            else
                                tempVehicleList = _context.Archivevehicles.Where(x => x.InstitutionId == Convert.ToInt32(item)).ToList();

                            objVehicleList.AddRange(tempVehicleList);
                        }
                    }
                }
                else if (string.IsNullOrEmpty(Model.vehicleIds) && string.IsNullOrEmpty(Model.institutionIds))
                {
                    if (Model.start != null && Model.end != null)
                        objVehicleList = _context.Archivevehicles.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList();
                    else
                        objVehicleList = _context.Archivevehicles.ToList();
                }

                if (objVehicleList == null || objVehicleList.ToList().Count == 0)
                {
                    objVehicleResponse.status = false;
                    objVehicleResponse.message = "Vehicle not found.";
                    objVehicleResponse.vehicleDetails = null;
                    objVehicleResponse.responseCode = ResponseCode.NotFound;
                    return objVehicleResponse;
                }
                if (objVehicleList != null)
                {
                    _context.Archivevehicles.RemoveRange(objVehicleList);
                    _context.SaveChanges();
                }
                objVehicleResponse.status = true;
                objVehicleResponse.message = "Vehicles has been deleted successfully.";
                objVehicleResponse.vehicleDetails= null;
                objVehicleResponse.responseCode = ResponseCode.Success;
                return objVehicleResponse;
            }
            catch (Exception ex)
            {
                objVehicleResponse.status = false;
                objVehicleResponse.message = "Something went wrong while getting Vehicles. Error Message - " + ex.Message;
                objVehicleResponse.vehicleDetails = null;
                objVehicleResponse.responseCode = ResponseCode.InternalServerError;
                return objVehicleResponse;
            }
        }
    }
}
