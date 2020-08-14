using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class AdvertisementsRepository : IAdvertisementsRepository
    {
        private readonly advertisementserviceContext _context;
        public AdvertisementsRepository(advertisementserviceContext context)
        {
            _context = context;
        }

        public AdvertisementsResponse DeleteAdvertisements(int id)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            try
            {
                var advertisementsData = _context.Advertisements.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementsData == null)
                {
                    response.status = false;
                    response.message = "Advertisement not found.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                }

                var advertisementsIntervals = _context.Advertisementsintervals.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementsIntervals != null)
                    _context.Advertisementsintervals.Remove(advertisementsIntervals);

                var advertisementscampaigns = _context.Advertisementscampaigns.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementscampaigns != null)
                    _context.Advertisementscampaigns.Remove(advertisementscampaigns);

                _context.Advertisements.Remove(advertisementsData);
                _context.SaveChanges();
                response.status = true;
                response.message = "Advertisements deleted successfully.";
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Advertisement. Error Message - " + ex.Message;
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsResponse GetAdvertisements(GetAdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            AdvertisementsDetails objAdvertisementsDetails = new AdvertisementsDetails();
            int totalCount = 0;
            try
            {
                List<AdvertisementsModel> objAdvertisementsModelList = new List<AdvertisementsModel>();
                AdvertisementsDetailsData AdvertisementsDetailsData = new AdvertisementsDetailsData();

                if (model.AdvertisementId == 0)
                {
                    objAdvertisementsModelList = (from advertisement in _context.Advertisements
                                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                                  join adinterval in _context.Advertisementsintervals on advertisement.AdvertisementId equals adinterval.AdvertisementId
                                                  join interval in _context.Intervals on adinterval.IntervalId equals interval.IntervalId
                                                  select new AdvertisementsModel()
                                                  {
                                                      AdvertisementId = advertisement.AdvertisementId,
                                                      CreatedAt = advertisement.CreatedAt,
                                                      InstitutionId = advertisement.InstitutionId,
                                                      MediaId = advertisement.MediaId,
                                                      MediaType = media.MediaType,
                                                      Url = media.Url,
                                                      Size = metadata.Size,
                                                      Duration = metadata.Duration,
                                                      IntervalTitle = interval.Title
                                                  }).OrderBy(a => a.AdvertisementId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                    totalCount = (from advertisement in _context.Advertisements
                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  join adinterval in _context.Advertisementsintervals on advertisement.AdvertisementId equals adinterval.AdvertisementId
                                  join interval in _context.Intervals on adinterval.IntervalId equals interval.IntervalId
                                  select new AdvertisementsModel()
                                  {
                                      AdvertisementId = advertisement.AdvertisementId,
                                      CreatedAt = advertisement.CreatedAt,
                                      InstitutionId = advertisement.InstitutionId,
                                      MediaId = advertisement.MediaId,
                                      MediaType = media.MediaType,
                                      Url = media.Url,
                                      Size = metadata.Size,
                                      Duration = metadata.Duration,
                                      IntervalTitle = interval.Title
                                  }).ToList().Count();
                }
                else
                {
                    objAdvertisementsModelList = (from advertisement in _context.Advertisements
                                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                                  join adinterval in _context.Advertisementsintervals on advertisement.AdvertisementId equals adinterval.AdvertisementId
                                                  join interval in _context.Intervals on adinterval.IntervalId equals interval.IntervalId
                                                  where advertisement.AdvertisementId == model.AdvertisementId
                                                  select new AdvertisementsModel()
                                                  {
                                                      AdvertisementId = advertisement.AdvertisementId,
                                                      CreatedAt = advertisement.CreatedAt,
                                                      InstitutionId = advertisement.InstitutionId,
                                                      MediaId = advertisement.MediaId,
                                                      MediaType = media.MediaType,
                                                      Url = media.Url,
                                                      Size = metadata.Size,
                                                      Duration = metadata.Duration,
                                                      IntervalTitle = interval.Title
                                                  }).OrderBy(a => a.AdvertisementId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                    totalCount = (from advertisement in _context.Advertisements
                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  join adinterval in _context.Advertisementsintervals on advertisement.AdvertisementId equals adinterval.AdvertisementId
                                  join interval in _context.Intervals on adinterval.IntervalId equals interval.IntervalId
                                  where advertisement.AdvertisementId == model.AdvertisementId
                                  select new AdvertisementsModel()
                                  {
                                      AdvertisementId = advertisement.AdvertisementId,
                                      CreatedAt = advertisement.CreatedAt,
                                      InstitutionId = advertisement.InstitutionId,
                                      MediaId = advertisement.MediaId,
                                      MediaType = media.MediaType,
                                      Url = media.Url,
                                      Size = metadata.Size,
                                      Duration = metadata.Duration,
                                      IntervalTitle = interval.Title
                                  }).ToList().Count();
                }

                if (objAdvertisementsModelList == null || objAdvertisementsModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Advertisement not found.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var page = new Pagination
                {
                    TotalCount = totalCount,
                    CurrentPage = model.currentPage,
                    PageSize = model.pageSize,
                    TotalPages = (int)Math.Ceiling(decimal.Divide(totalCount, model.pageSize)),
                    IndexOne = ((model.currentPage - 1) * model.pageSize + 1),
                    IndexTwo = (((model.currentPage - 1) * model.pageSize + model.pageSize) <= totalCount ? ((model.currentPage - 1) * model.pageSize + model.pageSize) : totalCount)
                };

                AdvertisementsDetailsData.advertisements  = objAdvertisementsModelList;
                objAdvertisementsDetails.data = AdvertisementsDetailsData;
                objAdvertisementsDetails.pagination = page;

                response.message = "Advertisements data retrived successfully.";
                response.status = true;
                response.advertisementsDetails = objAdvertisementsDetails;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching data. Error Message - " + ex.Message;
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsResponse InsertAdvertisements(AdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var media = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (media == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                Advertisements objAdvertisements = new Advertisements()
                {
                    CreatedAt = model.CreatedAt,
                    InstitutionId = model.InstitutionId,
                    MediaId = model.MediaId
                };
                _context.Advertisements.Add(objAdvertisements);
                _context.SaveChanges();

                response.status = true;
                response.message = "Advertisements inserted successfully.";
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Advertisement. Error Message - " + ex.Message;
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsResponse UpdateAdvertisements(AdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var advertisementData = _context.Advertisements.Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
                if (advertisementData == null)
                {
                    response.status = false;
                    response.message = "Advertisement not found.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var media = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (media == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.advertisementsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                advertisementData.InstitutionId = model.InstitutionId;
                advertisementData.MediaId = model.MediaId;
                _context.Advertisements.Update(advertisementData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Advertisement updated successfully.";
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Advertisement. Error Message - " + ex.Message;
                response.advertisementsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
