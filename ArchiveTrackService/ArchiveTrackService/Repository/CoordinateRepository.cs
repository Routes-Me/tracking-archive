using ArchiveTrackService.Abstraction;
using ArchiveTrackService.Models;
using ArchiveTrackService.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveTrackService.Repository
{
    public class CoordinateRepository : ICoordinateRepository
    {
        private readonly archivetrackserviceContext _context;
        public CoordinateRepository(archivetrackserviceContext context)
        {
            _context = context;
        }
        public feedResponse getCoordinates(GetFeedsModel Model)
        {
            feedResponse objFeedResponse = new feedResponse();
            CoordinateDetails objCoordinateDetails = new CoordinateDetails();
            int totalCount = 0;
            try
            {
                List<Archivecoordinates> objCoordinateList = new List<Archivecoordinates>();
                if (!string.IsNullOrEmpty(Model.vehicleIds))
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
                            objCoordinateList = _context.Archivecoordinates.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end && VehicleIds.Contains(x.VehicleNumber))
                           .OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                            totalCount = _context.Archivecoordinates.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end && VehicleIds.Contains(x.VehicleNumber)).ToList().Count();
                        }
                        else
                        {
                            objCoordinateList = (from ac in _context.Archivecoordinates
                                                 where VehicleIds.Contains(Convert.ToInt32(ac.VehicleNumber))
                                                 select ac
                                           ).OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                            totalCount = (from ac in _context.Archivecoordinates
                                          where VehicleIds.Contains(Convert.ToInt32(ac.VehicleNumber))
                                          select ac
                                           ).ToList().Count();
                        }
                    }
                }
                else
                {
                    if (Model.start != null && Model.end != null)
                    {
                        objCoordinateList = _context.Archivecoordinates.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end)
                            .OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                        totalCount = _context.Archivecoordinates.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList().Count();
                    }
                    else
                    {
                        objCoordinateList = _context.Archivecoordinates.OrderBy(a => a.Id).Skip((Model.currentPage - 1) * Model.pageSize).Take(Model.pageSize).ToList();

                        totalCount = _context.Archivecoordinates.ToList().Count();
                    }
                }
                if (objCoordinateList == null || objCoordinateList.Count == 0)
                {
                    objFeedResponse.status = false;
                    objFeedResponse.message = "Feeds not found.";
                    objFeedResponse.coordinateDetails = null;
                    objFeedResponse.responseCode = ResponseCode.NotFound;
                    return objFeedResponse;
                }
                objCoordinateDetails.data = objCoordinateList;
                var page = new Pagination
                {
                    TotalCount = totalCount,
                    CurrentPage = Model.currentPage,
                    PageSize = Model.pageSize,
                    TotalPages = (int)Math.Ceiling(decimal.Divide(totalCount, Model.pageSize)),
                    IndexOne = ((Model.currentPage - 1) * Model.pageSize + 1),
                    IndexTwo = (((Model.currentPage - 1) * Model.pageSize + Model.pageSize) <= totalCount ? ((Model.currentPage - 1) * Model.pageSize + Model.pageSize) : totalCount)
                };
                objCoordinateDetails.pagination = page;
                objFeedResponse.status = true;
                objFeedResponse.message = "Get all feeds successfully.";
                objFeedResponse.coordinateDetails = objCoordinateDetails;
                objFeedResponse.responseCode = ResponseCode.Success;
                return objFeedResponse;
            }
            catch (Exception ex)
            {
                objFeedResponse.status = false;
                objFeedResponse.message = "Something went wrong while getting feeds. Error Message - " + ex.Message;
                objFeedResponse.coordinateDetails = null;
                objFeedResponse.responseCode = ResponseCode.InternalServerError;
                return objFeedResponse;
            }
        }
        public feedResponse InsertCoordinates(List<Archivecoordinates> Model)
        {
            feedResponse objFeedResponse = new feedResponse();
            try
            {
                if (Model == null)
                {
                    objFeedResponse.status = false;
                    objFeedResponse.message = "Pass valid data in model.";
                    objFeedResponse.coordinateDetails = null;
                    objFeedResponse.responseCode = ResponseCode.BadRequest;
                    return objFeedResponse;
                }
                _context.Archivecoordinates.AddRange(Model);
                _context.SaveChanges();
                objFeedResponse.status = true;
                objFeedResponse.message = "Coordinates inserted successfully.";
                objFeedResponse.coordinateDetails = null;
                objFeedResponse.responseCode = ResponseCode.Success;
                return objFeedResponse;
            }
            catch (Exception ex)
            {
                objFeedResponse.status = false;
                objFeedResponse.message = "Something went wrong while inserting feeds. Error Message - " + ex.Message;
                objFeedResponse.coordinateDetails = null;
                objFeedResponse.responseCode = ResponseCode.InternalServerError;
                return objFeedResponse;
            }
        }
        public feedResponse DeleteCoordinates(DeleteFeedsModel Model)
        {
            feedResponse objFeedResponse = new feedResponse();
            try
            {
                List<Archivecoordinates> objCoordinateList = new List<Archivecoordinates>();
                List<Archivecoordinates> tempCoordinateList;
                if (!string.IsNullOrEmpty(Model.vehicleIds))
                {
                    string[] VehicleArr = Model.vehicleIds.Split(',');
                    if (VehicleArr.Length > 0)
                    {
                        foreach (var item in VehicleArr)
                        {
                            tempCoordinateList = new List<Archivecoordinates>();
                            if (Model.start != null && Model.end != null)
                                tempCoordinateList = _context.Archivecoordinates.Where(x => x.VehicleNumber == Convert.ToInt32(item) && x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList();
                            else
                                tempCoordinateList = _context.Archivecoordinates.Where(x => x.VehicleNumber == Convert.ToInt32(item)).ToList();

                            objCoordinateList.AddRange(tempCoordinateList);
                        }
                    }
                }
                else
                {
                    if (Model.start != null && Model.end != null)
                        objCoordinateList = _context.Archivecoordinates.Where(x => x.TimeStamp >= Model.start && x.TimeStamp <= Model.end).ToList();
                    else
                        objCoordinateList = _context.Archivecoordinates.ToList();
                }
                if (objCoordinateList == null || objCoordinateList.ToList().Count == 0)
                {
                    objFeedResponse.status = false;
                    objFeedResponse.message = "Feeds not found.";
                    objFeedResponse.coordinateDetails = null;
                    objFeedResponse.responseCode = ResponseCode.NotFound;
                    return objFeedResponse;
                }
                if (objCoordinateList != null)
                {
                    _context.Archivecoordinates.RemoveRange(objCoordinateList);
                    _context.SaveChanges();
                }
                objFeedResponse.status = true;
                objFeedResponse.message = "Feeds has been deleted successfully.";
                objFeedResponse.coordinateDetails = null;
                objFeedResponse.responseCode = ResponseCode.Success;
                return objFeedResponse;
            }
            catch (Exception ex)
            {
                objFeedResponse.status = false;
                objFeedResponse.message = "Something went wrong while deleting feeds. Error Message - " + ex.Message;
                objFeedResponse.coordinateDetails = null;
                objFeedResponse.responseCode = ResponseCode.InternalServerError;
                return objFeedResponse;
            }
        }
    }
}
