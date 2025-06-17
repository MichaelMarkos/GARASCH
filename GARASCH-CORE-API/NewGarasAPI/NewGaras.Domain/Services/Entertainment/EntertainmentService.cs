using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Medical.MedicalReservation;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Entertainment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services.Entertainment
{
    public class EntertainmentService : IEntertainmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private HearderVaidatorOutput validation;
        private readonly IWebHostEnvironment _host;
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

        public EntertainmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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

            if (data.ParentID != null)
            {
                var parent = _unitOfWork.MedicalReservations.Find(a => a.Id == data.ParentID);
                if (parent == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Medical Reservations with this ID";
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
                    if (data.Type == "Return") newMedicalExamination.Serial = 0;
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
                                InvetoryItem.PurchasingUomid = _unitOfWork.InventoryUoms.FindAllQueryable(x => x.Active).FirstOrDefault().Id;
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

    }
}
