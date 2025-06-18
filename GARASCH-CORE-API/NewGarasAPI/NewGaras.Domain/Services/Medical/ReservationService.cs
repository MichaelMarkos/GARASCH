using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Medical.DoctorSchedule;
using NewGaras.Infrastructure.DTO.Medical.MedicalReservation;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.Medical.Filters;
using NewGaras.Infrastructure.Models.Medical.UsedInResponse;
using NewGarasAPI.Models.Inventory;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services.Medical
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private HearderVaidatorOutput validation;
        private readonly IMapper _mapper;
        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public ReservationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public BaseResponseWithId<long> AddMedicalExaminationOffer(AddMedicalExaminationOfferDTO offer)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            var currentDate = DateTime.Now;

            if (offer.EndDate.Date < currentDate)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "can not make an offer with already passed date";
                response.Errors.Add(err);
                return response;
            }

            if (offer.DoctorID != null)
            {

                var doctor = _unitOfWork.HrUsers.Find(a => a.Id == offer.DoctorID, new[] { "MedicalExaminationOffers" });
                if (doctor == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Doctor with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                if (doctor.MedicalExaminationOffers.Count() > 0 && doctor.MedicalExaminationOffers.Any(a => a.EndDate.Date > currentDate.Date))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "this Doctor alraedy has an active offer ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (offer.SpecialityID != null)
            {
                var Speciality = _unitOfWork.Teams.GetById(offer.SpecialityID ?? 0);
                if (Speciality == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Speciality with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (offer.SpecialityID != null && offer.DoctorID != null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Enter only one from Doctor ID or Speciality ID ";
                response.Errors.Add(err);
                return response;
            }

            if (offer.Percentage != null && offer.Amount != null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "can not enter discount Percentage and discount amount at the same recored(only one is allowed)";
                response.Errors.Add(err);
                return response;
            }

            if (offer.Percentage == null && offer.Amount == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "please enter discount Percentage or discount amount (only one is allowed)";
                response.Errors.Add(err);
                return response;
            }



            try
            {
                if (offer.DoctorID != null)
                {

                    var newOffer = _mapper.Map<MedicalExaminationOffer>(offer);

                    newOffer.CreationDate = DateTime.Now;
                    newOffer.CreatedBy = validation.userID;
                    newOffer.ModifiedBy = validation.userID;
                    newOffer.ModificationDate = DateTime.Now;

                    _unitOfWork.MedicalExaminationOffers.Add(newOffer);
                    _unitOfWork.Complete();

                    response.ID = newOffer.Id;
                }

                if (offer.SpecialityID != null)
                {

                    //var doctorsList = _unitOfWork.HrUsers.FindAll(a => a.TeamId == offer.SpecialityID);

                    var offersList = new List<MedicalExaminationOffer>();

                    //foreach (var DocOffer in doctorsList)
                    //{
                    //    var newOffer = _mapper.Map<MedicalExaminationOffer>(offer);

                    //    newOffer.DoctorId = DocOffer.Id;
                    //    newOffer.CreationDate = DateTime.Now;
                    //    newOffer.CreatedBy = validation.userID;
                    //    newOffer.ModifiedBy = validation.userID;
                    //    newOffer.ModificationDate = DateTime.Now;

                    //    offersList.Add(newOffer);
                    //}

                    //_unitOfWork.MedicalExaminationOffers.AddRange(offersList);
                    //_unitOfWork.Complete();
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> EditMedicalExaminationOffer(EditMedicalExaminationOfferDTO offer)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            if (offer.ID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Medical Examination Offer ID is mandatory";
                response.Errors.Add(err);
                return response;
            }

            if (offer.DoctorID != null)
            {
                var doctor = _unitOfWork.MedicalExaminationOffers.GetById(offer.DoctorID ?? 0);
                if (doctor == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Medical Examination Offer with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            try
            {
                var MedicalExamination = _unitOfWork.MedicalExaminationOffers.GetById(offer.ID);
                if (MedicalExamination == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Medical Examination with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                if (offer.DoctorID != null) MedicalExamination.DoctorId = offer.DoctorID ?? 0;
                if (offer.OfferName != null) MedicalExamination.OfferName = MedicalExamination.OfferName;
                if (offer.Percentage != null) MedicalExamination.Percentage = MedicalExamination.Percentage ?? 0;
                if (offer.Amount != null) MedicalExamination.Amount = MedicalExamination.Amount ?? 0;
                if (offer.StartDate != null) MedicalExamination.StartDate = offer.StartDate ?? DateTime.Now;
                if (offer.EndDate != null) MedicalExamination.EndDate = offer.EndDate ?? DateTime.Now;
                if (offer.Active != null) MedicalExamination.Active = offer.Active ?? false;

                MedicalExamination.ModifiedBy = validation.userID;
                MedicalExamination.ModificationDate = DateTime.Now;


                _unitOfWork.Complete();


                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<GetMedicalExaminationOfferList>> GetMedicalExaminationOfferList()
        {
            var response = new BaseResponseWithData<List<GetMedicalExaminationOfferList>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var listDB = _unitOfWork.MedicalExaminationOffers.GetAll();

                var offersList = new List<GetMedicalExaminationOfferList>();

                foreach (var offer in listDB)
                {
                    var currentOffer = new GetMedicalExaminationOfferList();

                    currentOffer.Id = offer.Id;
                    currentOffer.DoctorId = offer.DoctorId;
                    currentOffer.offerName = offer.OfferName;
                    currentOffer.Percentage = offer.Percentage;
                    currentOffer.Amount = offer.Amount;
                    currentOffer.StartDate = offer.StartDate.ToString();
                    currentOffer.EndDate = offer.EndDate.ToString();
                    currentOffer.Active = offer.Active;

                    offersList.Add(currentOffer);

                }
                response.Data = offersList;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> AddMedicalReservation(AddMedicalReservationDTO data)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var date = DateTime.Now;
            if (!DateTime.TryParse(data.ReservationDate, out date))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "please Enter a valid Reservation Date";
                response.Errors.Add(err);
                return response;
            }

            if (date.Date < DateTime.Now.Date)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Can not make a Reservation with date already passed";
                response.Errors.Add(err);
                return response;
            }
            var parent = new MedicalReservation();
            if (data.ParentID != null)
            {
                parent = _unitOfWork.MedicalReservations.Find(a => a.Id == data.ParentID);
                if (parent == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Medical Reservations with this ID (parent ID)";
                    response.Errors.Add(err);
                    return response;
                }

            }

            var doctor = _unitOfWork.HrUsers.Find(a => a.Id == data.DoctorID, new[] { "MedicalExaminationOffers" });
            if (doctor == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Doctor with this ID";
                response.Errors.Add(err);
                return response;
            }

            var patient = _unitOfWork.Clients.GetById(data.PatientID);
            if (patient == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Patient with this ID";
                response.Errors.Add(err);
                return response;
            }

            var DoctorSchedule = _unitOfWork.DoctorSchedules.GetById(data.DoctorScheduleID);
            if (DoctorSchedule == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Schedule with this ID";
                response.Errors.Add(err);
                return response;
            }

            //var room = _unitOfWork.DoctorRooms.GetById(data);
            //if (team == null)
            //{
            //    response.Result = false;
            //    Error err = new Error();
            //    err.ErrorCode = "E101";
            //    err.ErrorMSG = "No specialty with this ID";
            //    response.Errors.Add(err);
            //    return response;
            //}

            var PatientType = _unitOfWork.MedicalPatientTypes.GetById(data.PatientTypeID);
            if (PatientType == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Patient Type with this ID";
                response.Errors.Add(err);
                return response;
            }


            var paymentMethod = _unitOfWork.PaymentMethods.GetById(data.PaymentMethodId);
            if (paymentMethod == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Payment Methods with this ID";
                response.Errors.Add(err);
                return response;
            }

            if (string.IsNullOrEmpty(data.Type))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Please enter Type";
                response.Errors.Add(err);
                return response;
            }

            if (data.Type != "Examination" && data.Type != "Consultation" && data.Type != "Return" && data.Type != "Addons")
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Please enter a valid Type from (Examination,Consultation,Return,Addons)";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            #region Serial repeated check
            var reservationInSameDayListDB = _unitOfWork.MedicalReservations.FindAll(a => a.ReservationDate.Date == date.Date && a.DoctorScheduleId == data.DoctorScheduleID).ToList();
            if(reservationInSameDayListDB.Count() > 0)
            {
                var reparedSerial = reservationInSameDayListDB.Where(a => a.Serial ==  data.Serial).FirstOrDefault();
                if(reparedSerial != null) 
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "This serial is already exsists";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            #region InventoryItem Validation
            //var inventoryItemsIDs = data.InventoryItemAndCategoryList.Select(a => a.InventoryItemId);
            //var inventoryItemsData = _unitOfWork.InventoryItems.FindAll(a => inventoryItemsIDs.Contains(a.Id)).ToList();

            //var inventoryItemCateroiesIDs = data.InventoryItemAndCategoryList.Select(a => a.InventoryItemCategoryID);
            //var inventoryItemCateroiesData = _unitOfWork.InventoryItemCategories.FindAll(a => inventoryItemCateroiesIDs.Contains(a.Id)).ToList();

            List<Infrastructure.Entities.InventoryItem> inventoryItemsData = new List<Infrastructure.Entities.InventoryItem>();
            if (data.InventoryItemAndCategoryList != null)
            {
                var inventoryItemsIDs = data.InventoryItemAndCategoryList.Select(a => a.InventoryItemId);
                inventoryItemsData = _unitOfWork.InventoryItems.FindAll(a => inventoryItemsIDs.Contains(a.Id)).ToList();

                var inventoryItemCateroiesIDs = data.InventoryItemAndCategoryList.Select(a => a.InventoryItemCategoryID);
                var inventoryItemCateroiesData = _unitOfWork.InventoryItemCategories.FindAll(a => inventoryItemCateroiesIDs.Contains(a.Id)).ToList();

                int index = 1;
                foreach (var item in data.InventoryItemAndCategoryList)
                {
                    var inventoryItem = inventoryItemsData.Where(a => a.Id == item.InventoryItemId).FirstOrDefault();
                    if (inventoryItem == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = $"No Inventory Item with this ID:{item.InventoryItemId} at index : {index}";
                        response.Errors.Add(err);
                        return response;
                    }

                    var inventoryItemCategory = inventoryItemCateroiesData.Where(a => a.Id == item.InventoryItemCategoryID).FirstOrDefault();
                    if (inventoryItem == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = $"No Inventory Item Category with this ID:{item.InventoryItemId} at index : {index}";
                        response.Errors.Add(err);
                        return response;
                    }
                    index++;
                }
            }
            #endregion

            try
            {
                var examinationPrice = DoctorSchedule.ExaminationPrice;
                var consultationPrice = DoctorSchedule.ConsultationPrice;
                //var doctorOffer = doctor.MedicalExaminationOffers.Where(a => a.EndDate.Date > date).Sum(b => b.Percentage);
                decimal? finalExaminationPrice = 0;
                decimal? finalConsultationPrice = 0;
                decimal? finalPercentagediscount = null;
                decimal? finalAmountdiscount = null;

                finalPercentagediscount = PatientType?.Percentage;

                if (doctor?.MedicalExaminationOffers.FirstOrDefault()?.Percentage != null)
                {
                    //var test = doctor.MedicalExaminationOffers.Where(a => a.EndDate.Date > date).Sum(b => b.Percentage) ?? 0;
                    finalPercentagediscount += doctor.MedicalExaminationOffers.Where(a => a.EndDate.Date > date).Sum(b => b.Percentage) ?? 0;
                    finalExaminationPrice = examinationPrice - (examinationPrice * ((finalPercentagediscount) / 100));
                    finalConsultationPrice = consultationPrice - (consultationPrice * ((finalPercentagediscount) / 100));
                }

                else if (doctor?.MedicalExaminationOffers.FirstOrDefault()?.Amount != null)
                {
                    finalAmountdiscount = doctor.MedicalExaminationOffers.Where(a => a.EndDate.Date > date).Sum(b => b.Amount) ?? 0;
                    finalExaminationPrice = examinationPrice - finalAmountdiscount;
                    finalConsultationPrice = consultationPrice - finalAmountdiscount;
                }

                if (finalExaminationPrice == 0)
                {
                    if (finalPercentagediscount == null && finalAmountdiscount == null)
                    {
                        finalExaminationPrice = examinationPrice;
                        finalConsultationPrice = consultationPrice;
                    }
                    else
                    {
                        finalExaminationPrice = examinationPrice - (examinationPrice * (finalPercentagediscount / 100));
                        finalConsultationPrice = consultationPrice - (consultationPrice * (finalPercentagediscount / 100));
                    }
                }



                var newMedicalExamination = _mapper.Map<MedicalReservation>(data);

                newMedicalExamination.ReservationDate = date;
                newMedicalExamination.CreationDate = DateTime.Now;
                newMedicalExamination.CreatedBy = validation.userID;
                newMedicalExamination.ModifiedBy = validation.userID;
                newMedicalExamination.ModificationDate = DateTime.Now;
                newMedicalExamination.IntervalFrom = DoctorSchedule.IntervalFrom;
                newMedicalExamination.IntervalTo = DoctorSchedule.IntervalTo;
                newMedicalExamination.Capacity = DoctorSchedule.Capacity;
                newMedicalExamination.RoomId = DoctorSchedule.RoomId ?? 0;
                newMedicalExamination.PaymentMethodId = data.PaymentMethodId;
                newMedicalExamination.CardNumber = data.CardNumber;
                newMedicalExamination.TeamId = DoctorSchedule.DoctorSpecialityId;
                newMedicalExamination.Type = data.Type;
                newMedicalExamination.ConsultationPrice = DoctorSchedule.ConsultationPrice;
                newMedicalExamination.ExaminationPrice = DoctorSchedule.ExaminationPrice;
                

                var NewSalesOfferInsert = new SalesOffer()
                {
                    OfferSerial = newMedicalExamination.Serial.ToString(),
                    StartDate = DateOnly.FromDateTime(newMedicalExamination.ReservationDate),
                    EndDate = DateOnly.FromDateTime(newMedicalExamination.ReservationDate),
                    SalesPersonId = _unitOfWork.HrUsers.GetById(newMedicalExamination.DoctorId)?.UserId ?? 0,
                    CreatedBy = newMedicalExamination.CreatedBy,
                    CreationDate = newMedicalExamination.CreationDate,
                    ModifiedBy = newMedicalExamination.ModifiedBy,
                    Modified = newMedicalExamination.ModificationDate,
                    Active = true,
                    Status = "Closed",
                    Completed = true,
                    OfferAmount = newMedicalExamination.FinalAmount,
                    SendingOfferDate = newMedicalExamination.ReservationDate,
                    ClientApprovalDate = newMedicalExamination.ReservationDate,
                    ClientId = newMedicalExamination.PatientId,
                    BranchId = _unitOfWork.Branches.FindAll(a => true).Select(a => a.Id).FirstOrDefault(),
                    OfferType = "ExtClinics",
                    //FinalOfferPrice = newMedicalExamination.FinalAmount,
                };
                if (data.Type == "Return") NewSalesOfferInsert.OfferType = "ExtClinics Return";
                var newSalesOffer = _unitOfWork.SalesOffers.Add(NewSalesOfferInsert);
                var SalesOfferInsert = _unitOfWork.Complete();

                if (data.ParentID != null)
                {
                    var parentHasChildDB = _unitOfWork.MedicalReservations.Find(a => a.ParentId == data.ParentID);
                    if (parentHasChildDB != null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "This Medical Examination already have Consultation record";
                        response.Errors.Add(err);
                        return response;
                    }
                    newMedicalExamination.ParentId = data.ParentID;
                }

                if (data.ParentID != null)
                {
                    newMedicalExamination.ConsultationPrice = finalConsultationPrice ?? 0;
                    newMedicalExamination.ExaminationPrice = finalExaminationPrice ?? 0;
                    newMedicalExamination.FinalAmount = finalConsultationPrice ?? 0;
                    //newMedicalExamination.Type = "Consultation"; 
                    if (data.Type == "Return") 
                    {
                        newMedicalExamination.Serial = 0;
                        newMedicalExamination.FinalAmount = parent.FinalAmount;
                    }
                }
                else
                {
                    newMedicalExamination.FinalAmount = finalExaminationPrice ?? 0;
                    //newMedicalExamination.Type = "Examination";
                }


                newMedicalExamination.OfferId = newSalesOffer.Id;
                newSalesOffer.FinalOfferPrice = newMedicalExamination.FinalAmount;
                _unitOfWork.MedicalReservations.Add(newMedicalExamination);
                _unitOfWork.Complete();





                if (data.Type == "Examination" || data.Type == "Consultation" || data.Type == "Return")
                {

                    long? InventoryItemId = null;
                    int? InventoryItemCategoryId = null;

                    if (data.Type == "Examination" || data.Type == "Consultation")
                    {

                        InventoryItemId = _unitOfWork.InventoryItems.Find(a => a.Name == data.Type)?.Id;
                        if (InventoryItemId == null)
                        {

                            InventoryItemCategoryId = _unitOfWork.InventoryItemCategories.Find(a => a.Name == data.Type)?.Id;
                            if (InventoryItemCategoryId == null)
                            {
                                var InventoryItemCategory = new InventoryItemCategory();
                                InventoryItemCategory.Name = data.Type;
                                InventoryItemCategory.Description = data.Type;
                                InventoryItemCategory.Active = true;
                                InventoryItemCategory.CreatedBy = validation.userID;
                                InventoryItemCategory.ModifiedBy = validation.userID;
                                InventoryItemCategory.CreationDate = DateTime.Now;
                                InventoryItemCategory.ModifiedDate = DateTime.Now;
                                InventoryItemCategory.HaveItem = true;

                                var ItemCategoryInsertion = _unitOfWork.InventoryItemCategories.Add(InventoryItemCategory);
                                _unitOfWork.Complete();

                                if (ItemCategoryInsertion.Id != 0)
                                {
                                    InventoryItemCategoryId = ItemCategoryInsertion.Id;

                                }
                            }

                            if (InventoryItemCategoryId != null)
                            {
                                var InvetoryItem = new Infrastructure.Entities.InventoryItem();
                                InvetoryItem.Code = "0";
                                InvetoryItem.Name = data.Type;
                                InvetoryItem.Description = data.Type;
                                InvetoryItem.Active = true;
                                InvetoryItem.InventoryItemCategoryId = (int)InventoryItemCategoryId;
                                InvetoryItem.CreatedBy = validation.userID;
                                InvetoryItem.ModifiedBy = validation.userID;
                                InvetoryItem.CreationDate = DateTime.Now;
                                InvetoryItem.ModifiedDate = DateTime.Now;
                                InvetoryItem.PurchasingUomid = _unitOfWork.InventoryUoms.FindAllQueryable(x=>x.Active).FirstOrDefault().Id;
                                InvetoryItem.RequstionUomid = InvetoryItem.PurchasingUomid;
                                InvetoryItem.CalculationType = _unitOfWork.CategoryTypes.FindAllQueryable(x => x.Active).FirstOrDefault().Id;
                                InvetoryItem.CustomeUnitPrice = 0;
                                InvetoryItem.AverageUnitPrice = 0;
                                InvetoryItem.LastUnitPrice = 0;
                                InvetoryItem.MaxBalance = 0;

                                var ItemInsertion = _unitOfWork.InventoryItems.Add(InvetoryItem);
                                _unitOfWork.Complete();

                                if (ItemInsertion.Id != 0)
                                {
                                    InventoryItemId = ItemInsertion.Id;
                                }
                            }
                        }

                    }

                    var NewSalesOfferProductInsert = new SalesOfferProduct()
                    {
                        CreatedBy = newMedicalExamination.CreatedBy,
                        CreationDate = newMedicalExamination.CreationDate,
                        ModifiedBy = newMedicalExamination.ModifiedBy,
                        Modified = newMedicalExamination.ModificationDate,
                        Active = true,
                        OfferId = NewSalesOfferInsert.Id,
                        Quantity = 1,
                        InventoryItemId = InventoryItemId,
                        InventoryItemCategoryId = InventoryItemCategoryId,
                        ItemPrice = newMedicalExamination.FinalAmount,
                        //ItemPricingComment = ItemComment,
                        FinalPrice = newMedicalExamination.FinalAmount,

                    };

                    _unitOfWork.SalesOfferProducts.Add(NewSalesOfferProductInsert);
                    _unitOfWork.Complete();
                }
                if (data.Type == "Addons")
                {
                    var listOfProducts = new List<SalesOfferProduct>();
                    decimal totalAmount = 0;
                    foreach (var product in data.InventoryItemAndCategoryList)
                    {
                        var currentInventoryItem = inventoryItemsData.Where(a => a.Id == product.InventoryItemId).FirstOrDefault();
                        var newSalesOfferProduct = new SalesOfferProduct()
                        {
                            CreatedBy = newMedicalExamination.CreatedBy,
                            CreationDate = newMedicalExamination.CreationDate,
                            ModifiedBy = newMedicalExamination.ModifiedBy,
                            Modified = newMedicalExamination.ModificationDate,
                            Active = true,
                            OfferId = NewSalesOfferInsert.Id,
                            Quantity = 1,
                            ItemPrice = product.ItemPrice,
                            //ItemPricingComment = ItemComment,
                            FinalPrice = product.Quantity * product.ItemPrice,
                            InventoryItemId = product.InventoryItemId,
                            InventoryItemCategoryId = product.InventoryItemCategoryID,
                            ItemPricingComment = currentInventoryItem.Name
                        };
                        listOfProducts.Add(newSalesOfferProduct);
                        totalAmount += product.ItemPrice * product.Quantity;
                    }
                    var productsAdded = _unitOfWork.SalesOfferProducts.AddRange(listOfProducts);
                    newSalesOffer.FinalOfferPrice = totalAmount;
                    newMedicalExamination.FinalAmount = totalAmount;
                    _unitOfWork.Complete();
                }

                response.ID = newMedicalExamination.Id;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetMedicalReservationList> GetMedicalReservationList(GetGetMedicalReservationFilters filters)
        {
            var response = new BaseResponseWithData<GetMedicalReservationList>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region date validation
            var startDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.StartDate))
            {
                if (!DateTime.TryParse(filters.StartDate, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Data From format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.EndDate))
            {
                if (!DateTime.TryParse(filters.EndDate, out EndDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Data To format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            try
            {
                var MedicalReservationsListQueryable = _unitOfWork.MedicalReservations.FindAllQueryable(a => true, new[] { "Doctor", "PatientType", "Team", "DoctorSchedule", "DoctorSchedule.WeekDay", "Patient", "PaymentMethod", "Parent" });

                if (filters.DoctorID != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.DoctorId == filters.DoctorID).AsQueryable();
                }

                if (filters.Serial != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.Serial == filters.Serial).AsQueryable();
                }

                if (!string.IsNullOrEmpty(filters.StartDate))
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.ReservationDate.Date >= startDate.Date).AsQueryable();
                }

                if (!string.IsNullOrEmpty(filters.EndDate))
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.ReservationDate.Date <= EndDate.Date).AsQueryable();
                }

                if (filters.PatientID != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.PatientId <= filters.PatientID).AsQueryable();
                }

                if (filters.SpecialtyID != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.TeamId <= filters.SpecialtyID).AsQueryable();
                }

                if (filters.PatientTypeID != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.PatientTypeId <= filters.PatientTypeID).AsQueryable();
                }

                if (filters.Room != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.RoomId == filters.Room).AsQueryable();
                }

                if (filters.ParentID != null)
                {
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => a.ParentId == filters.ParentID).AsQueryable();
                }

                if (!string.IsNullOrEmpty(filters.ReservationTyps))
                {
                    var types = filters.ReservationTyps.Split(',').ToList();
                    MedicalReservationsListQueryable = MedicalReservationsListQueryable.Where(a => types.Contains(a.Type)).AsQueryable();
                }

                var doctorSchedulePagedList = PagedList<MedicalReservation>.Create(MedicalReservationsListQueryable, filters.currentPage, filters.numberOfItemsPerPage);

                var doctorScheduleParentIDs = doctorSchedulePagedList.Select(a => a.Id).ToList();       //to search database for childs for this parents
                var doctorScheduleChildsListDB = _unitOfWork.MedicalReservations.FindAll(a => doctorScheduleParentIDs.Contains(a.Id));  //childs data


                var doctorScheduleList = new GetMedicalReservationList();
                var doctorSchedule = new List<GetMedicalReservation>();

                foreach (var reserv in doctorSchedulePagedList)
                {
                    var newMedicalReservation = new GetMedicalReservation();

                    newMedicalReservation.ID = reserv.Id;
                    newMedicalReservation.DoctorID = reserv.DoctorId;
                    newMedicalReservation.DoctorName = reserv.Doctor.FirstName + " " + reserv.Doctor.LastName;
                    newMedicalReservation.Serial = reserv.Serial;
                    newMedicalReservation.ReservationDate = reserv.ReservationDate.ToShortDateString();
                    newMedicalReservation.PatientID = reserv.PatientId;
                    newMedicalReservation.PatientName = reserv.Patient.Name;
                    newMedicalReservation.PatientTypeID = reserv.PatientTypeId;
                    newMedicalReservation.PatientTypeName = reserv.PatientType.Type;
                    newMedicalReservation.DoctorScheduleID = reserv.DoctorScheduleId;
                    newMedicalReservation.SpecialtyID = reserv.TeamId;
                    newMedicalReservation.SpecialtyName = reserv.Team.Name;
                    newMedicalReservation.Room = reserv.RoomId;
                    newMedicalReservation.ExaminationPrice = reserv.ExaminationPrice;
                    newMedicalReservation.consultationPrice = reserv.ConsultationPrice;
                    newMedicalReservation.DoctorIntervalFrom = reserv.IntervalFrom.ToString();
                    newMedicalReservation.DoctorIntervalTo = reserv.IntervalTo.ToString();
                    newMedicalReservation.DayID = reserv.DoctorSchedule.WeekDayId;
                    newMedicalReservation.DayName = reserv.DoctorSchedule.WeekDay.Day;
                    newMedicalReservation.Type = reserv.Type;
                    newMedicalReservation.FinalAmount = reserv.FinalAmount;
                    newMedicalReservation.ParentID = reserv.ParentId;
                    newMedicalReservation.paymentMethodId = reserv.PaymentMethodId;
                    newMedicalReservation.paymentMethodName = reserv.PaymentMethod.Name;

                    if (reserv.ParentId != null) newMedicalReservation.ParentSalesOfferID = reserv.Parent.OfferId;

                    var childReservation = _unitOfWork.MedicalReservations.Find(a => a.ParentId == reserv.Id);
                    if (childReservation != null)
                    {
                        newMedicalReservation.ChildReservationID = childReservation.Id;
                        newMedicalReservation.ChildSalesofferID = childReservation.Parent.OfferId;
                    }

                    newMedicalReservation.ChildsList = new List<ReservationChildDTO>();
                    var childsReservations = doctorScheduleChildsListDB.Where(a => a.ParentId == reserv.Id).FirstOrDefault();
                    if(childsReservations != null)
                    {
                        var currentChild = new ReservationChildDTO
                        {
                            ID = childsReservations.Id,
                            Type = reserv.Type
                        };
                        newMedicalReservation.ChildsList.Add(currentChild);
                    }
                    doctorSchedule.Add(newMedicalReservation);
                }

                doctorScheduleList.MedicalReservationList = doctorSchedule;
                response.Data = doctorScheduleList;

                var PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = filters.currentPage,
                    ItemsPerPage = filters.numberOfItemsPerPage,
                    TotalItems = doctorSchedulePagedList.TotalCount,
                    TotalPages = doctorSchedulePagedList.TotalPages,
                };

                response.Data.PaginationHeader = PaginationHeader;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> MoveReservationsToAnotherDoctor(MoveReservationsToAnotherDoctorDTO data, long userID)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var startDate = DateTime.Now;
            if (!string.IsNullOrEmpty(data.From))
            {
                if (!DateTime.TryParse(data.From, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Date From format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(data.To))
            {
                if (!DateTime.TryParse(data.To, out EndDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Date To format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var DocSchedule = _unitOfWork.DoctorSchedules.GetById(data.DoctorScheduleId);
            if (DocSchedule == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "No Doctor schedule with this ID";
                response.Errors.Add(err);
                return response;
            }

            var oldDoctor = _unitOfWork.HrUsers.GetById(data.OldDoctoreID);
            if (oldDoctor == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "No Doctor (old doctor ) with this ID";
                response.Errors.Add(err);
                return response;
            }

            var NewDoctor = _unitOfWork.HrUsers.GetById(data.OldDoctoreID);
            if (NewDoctor == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "No Doctor (old doctor ) with this ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion


            try
            {
                // close the old Doctor schedule and open new one for the new doctor
                DocSchedule.EndDate = startDate.AddDays(-1);
                DocSchedule.ModificationDate = DateTime.Now;
                DocSchedule.ModifiedBy = userID;

                var newDocSchedule = new DoctorSchedule();

                newDocSchedule.DoctorId = data.NewDoctorID;
                newDocSchedule.Capacity = DocSchedule.Capacity;
                newDocSchedule.IntervalFrom = DocSchedule.IntervalFrom;
                newDocSchedule.IntervalTo = DocSchedule.IntervalTo;
                newDocSchedule.ConsultationPrice = DocSchedule.ConsultationPrice;
                newDocSchedule.StatusId = DocSchedule.StatusId;
                newDocSchedule.RoomId = DocSchedule.RoomId;
                newDocSchedule.StartDate = startDate;
                newDocSchedule.EndDate = EndDate;
                newDocSchedule.PercentageTypeId = DocSchedule.PercentageTypeId;
                newDocSchedule.DoctorSpecialityId = DocSchedule.DoctorSpecialityId;
                newDocSchedule.ExaminationPrice = DocSchedule.ExaminationPrice;
                newDocSchedule.CreatedBy = userID;
                newDocSchedule.CreationDate = DateTime.Now;
                newDocSchedule.ModifiedBy = userID;
                newDocSchedule.ModificationDate = DateTime.Now;
                newDocSchedule.WeekDayId = DocSchedule.WeekDayId;
                //newDocSchedule.HoldQuantity = DocSchedule.HoldQuantity;


                var newSchedule = _unitOfWork.DoctorSchedules.Add(newDocSchedule);
                _unitOfWork.Complete();

                //get all reservations for old Doctor in the interval and move them to the new doctor and new schedule
                var oldReservationList = _unitOfWork.MedicalReservations.FindAll(a => a.DoctorScheduleId == data.DoctorScheduleId && a.DoctorId == data.OldDoctoreID
                                                                                      && a.ReservationDate >= startDate && a.ReservationDate <= EndDate).ToList();

                foreach (var res in oldReservationList)
                {
                    res.DoctorId = data.NewDoctorID;
                    res.DoctorScheduleId = newSchedule.Id;
                }


                //make a new schedule for the old doctor after the interval
                var oldDocNewSchedule = new DoctorSchedule();

                oldDocNewSchedule.DoctorId = data.OldDoctoreID;
                oldDocNewSchedule.Capacity = DocSchedule.Capacity;
                oldDocNewSchedule.IntervalFrom = DocSchedule.IntervalFrom;
                oldDocNewSchedule.IntervalTo = DocSchedule.IntervalTo;
                oldDocNewSchedule.ConsultationPrice = DocSchedule.ConsultationPrice;
                oldDocNewSchedule.StatusId = DocSchedule.StatusId;
                oldDocNewSchedule.RoomId = DocSchedule.RoomId;
                oldDocNewSchedule.StartDate = EndDate.AddDays(1); //start from end date + 1 of the interval 
                oldDocNewSchedule.EndDate = null;
                oldDocNewSchedule.PercentageTypeId = DocSchedule.PercentageTypeId;
                oldDocNewSchedule.DoctorSpecialityId = DocSchedule.DoctorSpecialityId;
                oldDocNewSchedule.ExaminationPrice = DocSchedule.ExaminationPrice;
                oldDocNewSchedule.CreatedBy = userID;
                oldDocNewSchedule.CreationDate = DateTime.Now;
                oldDocNewSchedule.ModifiedBy = userID;
                oldDocNewSchedule.ModificationDate = DateTime.Now;
                oldDocNewSchedule.WeekDayId = DocSchedule.WeekDayId;


                _unitOfWork.DoctorSchedules.Add(oldDocNewSchedule);
                _unitOfWork.Complete();

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> AddClientPatientInfo(AddClientPatientInfoDTO data, long userID)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region validation
                var salesPerson = _unitOfWork.Users.GetById(data.salesPersonID);
                if (salesPerson == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "No sales Person with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                var nationality = _unitOfWork.Nationalities.GetById(data.NationalityId);
                if (nationality == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "No Nationality with this ID";
                    response.Errors.Add(err);
                    return response;
                }


                var dateOfBirth = DateTime.Now;
                if (!DateTime.TryParse(data.BirthDate, out dateOfBirth))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "please Enter a valid Birth Date";
                    response.Errors.Add(err);
                    return response;
                }

                if (data.IdentityNumber == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "please Enter a valid Identity Number";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                var newClient = new Client();
                newClient.Name = data.ClientName;
                newClient.Type = "Individual";
                newClient.Email = data.Email;
                newClient.SalesPersonId = data.salesPersonID;
                newClient.CreatedBy = userID;
                newClient.CreationDate = DateTime.Now;
                newClient.FollowUpPeriod = 3;

                var newClientData = _unitOfWork.Clients.Add(newClient);
                _unitOfWork.Complete();


                var newClientExtra = new ClientExtraInfo();
                newClientExtra.ClientId = newClientData.Id;
                newClientExtra.IdentityNumber = data.IdentityNumber;
                newClientExtra.NationalityId = data.NationalityId;
                newClientExtra.Gender = data.Gender;
                newClientExtra.DateOfBirth = dateOfBirth;
                newClientExtra.CreatedBy = userID;
                newClientExtra.CreationDate = DateTime.Now;
                newClientExtra.ModifiedBy = userID;
                newClientExtra.ModificationDate = DateTime.Now;

                var newData = _unitOfWork.ClientExtraInfos.Add(newClientExtra);


                var newClientMobile = new ClientMobile();
                newClientMobile.ClientId = newClientData.Id;
                newClientMobile.Mobile = data.ClientMobile;
                newClientMobile.CreatedBy = userID;
                newClientMobile.CreationDate = DateTime.Now;
                newClientMobile.ModifiedBy = userID;
                newClientMobile.Modified = DateTime.Now;
                newClientMobile.Active = true;

                var newClientobledata = _unitOfWork.ClientMobiles.Add(newClientMobile);

                _unitOfWork.Complete();

                response.ID = newClientData.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetClientPatientInfoDTO> GetClientPatientInfo(long ClientID)
        {
            var response = new BaseResponseWithData<GetClientPatientInfoDTO>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var clientDataQueryable = _unitOfWork.ClientExtraInfos.FindAllQueryable(a => a.ClientId == ClientID, new[] { "Client", "Client.ClientMobiles", "Nationality", "Client.SalesPerson" });


                var clientData = clientDataQueryable.ToList().FirstOrDefault();

                if (clientData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "No Client with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                var clientMobile = clientData.Client.ClientMobiles.FirstOrDefault();

                var clientDataResponse = new GetClientPatientInfoDTO()
                {
                    Id = clientData.ClientId,
                    ClientName = clientData.Client.Name,
                    ClientMobile = clientMobile.Mobile,
                    ClientType = clientData.Client.Type,
                    Email = clientData.Client.Email,
                    salesPersonID = clientData.Client.SalesPersonId,
                    salesPersonName = clientData.Client.SalesPerson.FirstName + " " + clientData.Client.SalesPerson.LastName,
                    FollowUpPeriod = clientData.Client.FollowUpPeriod,
                    IdentityNumber = clientData.IdentityNumber,
                    NationalityId = clientData.NationalityId,
                    NationalityName = clientData.Nationality.Nationality1,
                    Gender = clientData.Gender,
                    BirthDate = clientData.DateOfBirth.ToShortDateString(),
                };

                response.Data = clientDataResponse;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> EditClientPatientInfo(EditClientPatientInfoDTO data, long userID)
        {

            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region validation
                var client = _unitOfWork.Clients.GetById(data.Id);
                if (client == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "No Client with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                if (data.salesPersonID != null)
                {
                    var salesPerson = _unitOfWork.Users.GetById(data.salesPersonID ?? 0);
                    if (salesPerson == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E10";
                        err.ErrorMSG = "No sales Person with this ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                if (data.NationalityId != null)
                {

                    var nationality = _unitOfWork.Nationalities.GetById(data.NationalityId ?? 0);
                    if (nationality == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E10";
                        err.ErrorMSG = "No Nationality with this ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                var dateOfBirth = DateTime.Now;
                if (!string.IsNullOrEmpty(data.BirthDate))
                {
                    if (!DateTime.TryParse(data.BirthDate, out dateOfBirth))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "please Enter a valid Birth Date";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                #endregion

                //var newClient = new Client();
                if (!string.IsNullOrEmpty(data.ClientName)) client.Name = data.ClientName;
                if (!string.IsNullOrEmpty(data.ClientType)) client.Type = data.ClientType;
                if (!string.IsNullOrEmpty(data.Email)) client.Email = data.Email;
                if (data.salesPersonID != null) client.SalesPersonId = data.salesPersonID ?? 0;
                if (data.FollowUpPeriod != null) client.FollowUpPeriod = data.FollowUpPeriod ?? 0;




                //var newClientExtra = new ClientExtraInfo();
                var clientExtraInfo = _unitOfWork.ClientExtraInfos.Find(a => a.ClientId == data.Id);

                if (clientExtraInfo != null)                                                                         //ever client(patient) must have Extra info this just for safety
                {
                    if (data.FollowUpPeriod != null) clientExtraInfo.IdentityNumber = data.IdentityNumber ?? 0;
                    if (data.NationalityId != null) clientExtraInfo.NationalityId = data.NationalityId ?? 0;
                    if (!string.IsNullOrEmpty(data.Gender)) clientExtraInfo.Gender = data.Gender;
                    if (!string.IsNullOrEmpty(data.BirthDate)) clientExtraInfo.DateOfBirth = dateOfBirth;
                    clientExtraInfo.ModifiedBy = userID;
                    clientExtraInfo.ModificationDate = DateTime.Now;
                }

                //var newData = _unitOfWork.ClientExtraInfos.Add(newClientExtra);


                //var newClientMobile = new ClientMobile();
                var ClientMobile = _unitOfWork.ClientMobiles.Find(a => a.ClientId == data.Id);

                if (ClientMobile != null)
                {
                    if (!string.IsNullOrEmpty(data.ClientMobile)) ClientMobile.Mobile = data.ClientMobile;
                    ClientMobile.ModifiedBy = userID;
                    ClientMobile.Modified = DateTime.Now;
                }



                _unitOfWork.Complete();

                response.ID = client.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public SelectDDLResponse GetPaymentMethods()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.PaymentMethods.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Name;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<int>> GetListOfSerialReserved(GetListOfSerialReservedFilters filters)
        {
            var response = new BaseResponseWithData<List<int>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var date = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.DayDate))
            {
                if (!DateTime.TryParse(filters.DayDate, out date))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Date To format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (filters.DoctorScheduleID != null)
            {
                var DocSchedule = _unitOfWork.DoctorSchedules.GetById(filters.DoctorScheduleID ?? 0);
                if (DocSchedule == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "No Doctor schedule with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (filters.DoctorID == 0)
            {
                var Doctor = _unitOfWork.HrUsers.GetById(filters.DoctorID ?? 0);
                if (Doctor == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "No Doctor with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion


            try
            {
                var dayName = date.ToString("dddd");
                var reservationData = _unitOfWork.MedicalReservations.FindAllQueryable(a => a.ReservationDate.Date == date.Date);

                if (filters.DoctorID != null)
                {
                    reservationData = reservationData.Where(a => a.DoctorId == filters.DoctorID).AsQueryable();
                }
                if (filters.DoctorScheduleID != null)
                {
                    reservationData = reservationData.Where(a => a.DoctorScheduleId == filters.DoctorScheduleID).AsQueryable();
                }

                var serialList = reservationData.Select(a => a.Serial).ToList();

                response.Data = serialList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public SelectDDLResponse GetPatientTypeDDl()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.MedicalPatientTypes.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Type;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> MoveReservationListToAnotherSchedule(MoveReservationListToAnotherSchedule dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {
                #region validation
                var reservationsIDs = dto.ReservationsWithSerialList.Select(a => a.ReservationID);
                var reservationListDB = _unitOfWork.MedicalReservations.FindAll(a => reservationsIDs.Contains(a.Id));

                var index = 1;
                foreach (var res in reservationsIDs)
                {
                    var currentReservation = reservationListDB.Where(a => a.Id == res).FirstOrDefault();
                    if(currentReservation == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E10";
                        err.ErrorMSG = $"No reservation with this ID ({res})";
                        response.Errors.Add(err);
                        return response;
                    }
                }


                var doctorScheduleIDs = new List<long>
                {
                    dto.NewScheduleID,
                    dto.OldScheduleID
                };

                var scheduleListDB = _unitOfWork.DoctorSchedules.FindAll(a => doctorScheduleIDs.Contains(a.Id));

                var oldDocScheduleDB = scheduleListDB.Where(a => a.Id == dto.OldScheduleID).FirstOrDefault();
                if (oldDocScheduleDB == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = $"No Doc. Schedule with this ID ({dto.OldScheduleID}, old Doc. Schedule)";
                    response.Errors.Add(err);
                    return response;
                }

                var newDocScheduleDB = scheduleListDB.Where(a => a.Id == dto.NewScheduleID).FirstOrDefault();
                if (newDocScheduleDB == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = $"No Doc. Schedule with this ID ({dto.NewScheduleID}, New Doc. Schedule)";
                    response.Errors.Add(err);
                    return response;
                }

                
                if (string.IsNullOrEmpty(dto.NewReservationDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = $"please , Add a valid Reservation Date (can not be Empty)";
                    response.Errors.Add(err);
                    return response;
                }

                var resDate = DateTime.Now;
                if (!string.IsNullOrEmpty(dto.NewReservationDate))
                {
                    if (!DateTime.TryParse(dto.NewReservationDate, out resDate))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "please Enter a valid Birth Date";
                        response.Errors.Add(err);
                        return response;
                    }
                    if(resDate.Date < DateTime.Now.Date)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "Can not add reservation in date already passed!";
                        response.Errors.Add(err);
                        return response;
                    }
                }

               
                #endregion

                var lastSerialInNewReservation = _unitOfWork.MedicalReservations.FindAll(a => a.ReservationDate.Date == resDate.Date).LastOrDefault()?.Serial;
                if (lastSerialInNewReservation == null) lastSerialInNewReservation = 1;
                foreach (var reser in reservationListDB)
                {
                    reser.DoctorScheduleId = dto.NewScheduleID;
                    reser.ReservationDate = resDate;
                    reser.DoctorId = newDocScheduleDB.DoctorId;
                    reser.Serial = dto.ReservationsWithSerialList.Where(a => a.ReservationID == reser.Id).Select(b => b.Serial).FirstOrDefault();

                    lastSerialInNewReservation++;
                }
                _unitOfWork.Complete();

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }


        public BaseResponseWithData<GetMedicalReservationDTO> GetMedicalReservationById(long Id)
        {
            var response = new BaseResponseWithData<GetMedicalReservationDTO>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (Id <= 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Invalid Id!";
                    response.Errors.Add(err);
                    return response;
                }
                var reservation = _unitOfWork.MedicalReservations.FindAll(a => a.Id == Id, new[] { 
                    "Doctor", "PatientType", "Team", "DoctorSchedule", "DoctorSchedule.WeekDay", "Patient", "PaymentMethod","Offer.SalesOfferProducts.InventoryItem" }).FirstOrDefault();

                if(reservation == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.ErrorMSG = "Reservation Not Found!";
                    response.Errors.Add(err);
                    return response;
                }
                var getDTo = new GetMedicalReservationDTO()
                {
                    ID = reservation.Id,
                    DoctorID = reservation.DoctorId,
                    DoctorName = reservation.Doctor.FirstName + " " + reservation.Doctor.LastName,
                    Serial = reservation.Serial,
                    ReservationDate = reservation.ReservationDate.ToShortDateString(),
                    PatientID = reservation.PatientId,
                    PatientName = reservation.Patient.Name,
                    PatientTypeID = reservation.PatientTypeId,
                    PatientTypeName = reservation.PatientType.Type,
                    DoctorScheduleID = reservation.DoctorScheduleId,
                    SpecialtyID = reservation.TeamId,
                    SpecialtyName = reservation.Team.Name,
                    Room = reservation.RoomId,
                    ExaminationPrice = reservation.ExaminationPrice,
                    consultationPrice = reservation.ConsultationPrice,
                    DoctorIntervalFrom = reservation.IntervalFrom.ToString(),
                    DoctorIntervalTo = reservation.IntervalTo.ToString(),
                    DayID = reservation.DoctorSchedule.WeekDayId,
                    DayName = reservation.DoctorSchedule.WeekDay.Day,
                    Type = reservation.Type,
                    FinalAmount = reservation.FinalAmount,
                    ParentID = reservation.ParentId,
                    paymentMethodId = reservation.PaymentMethodId,
                    paymentMethodName = reservation.PaymentMethod.Name,
                    Addons = reservation.Offer.SalesOfferProducts.Select(a=>new ReservationAddonDTO()
                    {
                        ItemName = a.InventoryItem.Name,
                        Cost = a.ItemPrice??0,
                        Image = a.InventoryItem.ImageUrl!=null? Globals.baseURL +"/"+a.InventoryItem.ImageUrl:null,
                        Qty = a.Quantity??0
                    }).ToList(),
                };
                response.Data = getDTo;
                return response;
            }
            catch (Exception ex) 
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
    }
}
