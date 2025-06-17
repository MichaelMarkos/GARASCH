using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGarasAPI.Helper;
using System.Net;
using System.Web;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using NewGaras.Infrastructure.Models.PurchesRequest.Filters;
using NewGarasAPI.Models.User;
using NewGarasAPI.Models.HR;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.Inventory;
using DocumentFormat.OpenXml.Bibliography;
using Azure;
using NewGaras.Infrastructure.Helper;
using Microsoft.IdentityModel.Tokens;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.Inventory.Requests;
using Org.BouncyCastle.Bcpg;

namespace NewGaras.Domain.Services
{
    public class PurchesRequestService : IPurchesRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private GarasTestContext _Context;
        static readonly string key = "SalesGarasPass";
        public PurchesRequestService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, GarasTestContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _Context = context;
        }


        public ManageAssignedPRItemsResponse GetManageAssignedPRItems(ManageAssignedPRItemsFilters filters, string CompName)
        {
            ManageAssignedPRItemsResponse Response = new ManageAssignedPRItemsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AssignedPRItemsList = new List<AssignedPRItems>();
                if (Response.Result)
                {
                    #region old headers
                    ////if (!CommonClass.CheckUserInRole(UserID, 82))
                    ////{
                    ////    return RedirectToAction("NotAuthorized", "Home");
                    ////}
                    //long UserAssignedTo = 0;
                    //if (!string.IsNullOrEmpty(headers["UserAssignedTo"]) && long.TryParse(headers["UserAssignedTo"], out UserAssignedTo))
                    //{
                    //    long.TryParse(headers["UserAssignedTo"], out UserAssignedTo);
                    //}

                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}
                    //bool? IsDirectPR = null;
                    //if (!string.IsNullOrEmpty(headers["IsDirectPR"]))
                    //{
                    //    IsDirectPR = bool.Parse(headers["IsDirectPR"]);
                    //}

                    //long FormInventoryStoreID = 0;
                    //if (!string.IsNullOrEmpty(headers["FormInventoryStoreID"]) && long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID))
                    //{
                    //    long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID);
                    //}
                    //long ProjectID = 0;
                    //if (!string.IsNullOrEmpty(headers["ProjectID"]) && long.TryParse(headers["ProjectID"], out ProjectID))
                    //{
                    //    long.TryParse(headers["ProjectID"], out ProjectID);
                    //}
                    #endregion

                    if (Response.Result)
                    {
                        var PurchaseRequestOBJDB = _unitOfWork.VPurchaseRequestItemsPo.FindAllQueryable(x => x.PritemAssignedTo != null && x.PurchasePoid == null
                                                                                           && x.PurchaseRequestQuantity < x.PurchaseRequestItemQuantity).AsQueryable();
                        if (filters.FormInventoryStoreID != 0 && filters.FormInventoryStoreID != null)
                        {

                            var IDSPurchaseRequest = _unitOfWork.VPurchaseRequests.FindAll(x => x.FromInventoryStoreId == filters.FormInventoryStoreID).Select(x => x.Id).ToList();

                            PurchaseRequestOBJDB = PurchaseRequestOBJDB.Where(x => x.PurchaseRequestId != null ? IDSPurchaseRequest.Contains((long)x.PurchaseRequestId) : false).AsQueryable();
                        }



                        if (filters.UserAssignedTo != 0 && filters.UserAssignedTo != null )
                        {
                            PurchaseRequestOBJDB = PurchaseRequestOBJDB.Where(x => x.PritemAssignedTo == filters.UserAssignedTo);
                        }
                        if (filters.IsDirectPR == true)
                        {
                            PurchaseRequestOBJDB = PurchaseRequestOBJDB.Where(x => x.IsDirectPr == true).AsQueryable();
                        }
                        if (filters.ProjectID != 0 && filters.ProjectID != null)
                        {
                            PurchaseRequestOBJDB = PurchaseRequestOBJDB.Where(x => x.ProjectId == filters.ProjectID).AsQueryable();
                        }
                        if (filters.InventoryItemID != 0 && filters.InventoryItemID != null)
                        {
                            PurchaseRequestOBJDB = PurchaseRequestOBJDB.Where(x => x.InventoryItemId == filters.InventoryItemID);
                        }
                        if (PurchaseRequestOBJDB != null)
                        {
                            var ListOfPurchaseRequestOBJDB = PurchaseRequestOBJDB.ToList();
                            var usersIDs = ListOfPurchaseRequestOBJDB.Select(a => a.PritemAssignedTo).ToList();

                            var usersData = _unitOfWork.Users.FindAll(a => usersIDs.Contains(a.Id));
                            foreach (var items in ListOfPurchaseRequestOBJDB)
                            {
                                //if (items.PurchaseRequestQuantity < items.PurchaseRequestItemQuantity)
                                //{
                                //}
                                AssignedPRItems assignedPRItems = new AssignedPRItems();
                                assignedPRItems.PurchaseRequestID = items.PurchaseRequestId;
                                assignedPRItems.PurchaseRequestItemID = items.PurchaseRequestItemsId;
                                assignedPRItems.InventoryMatrialRequestItemID = items.InventoryMatrialRequestItemId;
                                assignedPRItems.AssginedUserID = items.PritemAssignedTo;
                                if(items.PritemAssignedTo != null)
                                {
                                    var user = usersData.Where(a => a.Id == (long)items.PritemAssignedTo).FirstOrDefault();
                                    var userName = user.FirstName + " " + user.LastName;

                                    assignedPRItems.AssginedUserName = userName;

                                    if(user.PhotoUrl != null)
                                    {
                                        assignedPRItems.AssginedUserImage = Globals.baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(items.PritemAssignedTo.ToString(), key)) + "&type=photo&CompName=" + CompName.ToString().ToLower();
                                    }
                                }
                                
                                //assignedPRItems.AssginedUserName = items.PritemAssignedTo != null ? Common.GetUserName((long)items.PritemAssignedTo).Trim() : "";
                                //if (items.PritemAssignedTo != null)
                                //{
                                //    if (Common.GetUserPhoto((long)items.PritemAssignedTo) != null)
                                //    {
                                //        assignedPRItems.AssginedUserImage = Globals.baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(items.PritemAssignedTo.ToString(), key)) + "&type=photo&CompName=" + CompName.ToString().ToLower();
                                //    }
                                //}
                                assignedPRItems.InventoryItemID = items.InventoryItemId;
                                assignedPRItems.InventoryItemName = items.InventoryItemName.Trim();
                                assignedPRItems.InventoryItemCode = items.InventoryItemCode;
                                assignedPRItems.RecivedQuantity = items.RequestRecivedQuantity;
                                assignedPRItems.InventoryUOMShortName = items.InventoryUomshortName;
                                assignedPRItems.ProjectName = items.ProjectName;
                                assignedPRItems.FabricationOrderNumber = items.FabNumber != null ? items.FabNumber.ToString() : "";
                                assignedPRItems.PurchaseRequestItemQuantity = items.PurchaseRequestItemQuantity;
                                assignedPRItems.PurchaseRequestQuantity = items.PurchaseRequestQuantity;
                                assignedPRItems.RequestRecivedQuantity = items.RequestRecivedQuantity;
                                assignedPRItems.ConvertRateFromPurchasingToRequestionUnit = items.ExchangeFactor;

                                assignedPRItems.PurchasedUOMShortName = items.PurchasedUomshortName;
                                assignedPRItems.RequstionUOMShortName = items.RequstionUomshortName;
                                decimal requestionQTY = items.PurchaseRequestItemQuantity != null ? (decimal)items.PurchaseRequestItemQuantity : 0;
                                decimal factor = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;
                                decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                                assignedPRItems.PurchaseItemQuantity = purchaseQTY;

                                AssignedPRItemsList.Add(assignedPRItems);
                            }
                        }


                    }
                    Response.AssignedPRItemsList = AssignedPRItemsList;


                }
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

        public UserDDLResponse GetPurchasingPersonsList(string SearchKey, string CompName)
        {
            UserDDLResponse Response = new UserDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<UserDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.VGroupUsers.FindAll(x => x.Active == true && x.Name == "PurchasingTeam" && x.UserActive == true).Select(x => x.UserId).ToList();
                    if (!string.IsNullOrEmpty(SearchKey))
                    {

                        
                        var SearchKeyDecoded = HttpUtility.UrlDecode(SearchKey);

                        var IDUserListDB = _unitOfWork.VUserInfos.FindAll(x => x.Active == true && ListDB.Contains(x.Id) && (x.FirstName.Contains(SearchKeyDecoded.ToLower())
                                                                                                                                            || x.MiddleName.Contains(SearchKeyDecoded.ToLower())
                                                                                                                                            || x.LastName.Contains(SearchKeyDecoded.ToLower())
                                                                                                                                            || x.Email.Contains(SearchKeyDecoded.ToLower())
                                                                                                                                            )
                                                                                                                                            ).Select(x => x.Id).ToList();


                        ListDB = IDUserListDB;
                    }
                    var usersData = _unitOfWork.Users.FindAll(x => ListDB.Contains(x.Id));
                    if (ListDB.Count() > 0)
                    {
                        foreach (var User in usersData)
                        {
                            var DLLObj = new UserDDL();
                            DLLObj.ID = User.Id;
                            DLLObj.Name = User.FirstName + " " + User.LastName;
                            if (User.PhotoUrl != null)
                            {
                                DLLObj.Image = Globals.baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(User.Id.ToString(), key)) + "&type=photo&CompName=" + CompName.ToString().ToLower();
                            }

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList.Distinct().ToList();
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

        public BaseResponseWithID AddMatrialDirectPR(AddMatrialDirectPRRequest Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime RequestgDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RequestgDate) || !DateTime.TryParse(Request.RequestgDate, out RequestgDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err37";
                        error.ErrorMSG = "Invalid Reqest Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.DirectPRItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.DirectPRItemList.Count <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int Counter = 0;
                    foreach (var item in Request.DirectPRItemList)
                    {
                        Counter++;
                        if (item.InventoryItemID <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.ReqQTY <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err29";
                            error.ErrorMSG = "Invalid QTY item #" + Counter;
                            Response.Errors.Add(error);
                        }

                    }
                    long PurchaseRequestID = 0;
                    if (Response.Result)
                    {
                        long mrId = new long();
                        //var prId = new long();
                        var MRItemIDSList = new List<long>();
                        // Check if inventory Dirct is not found to add one
                        string StoreName = "DIRECT PR HIDDEN STORE";
                        var InventoryStoreID = _unitOfWork.InventoryStores.Find(a => StoreName.Contains(a.Name))?.Id??0;
                        if (InventoryStoreID == 0)
                        {
                            // Inserty Inventory Store ID
                            //ObjectParameter IDInventoryStore = new ObjectParameter("ID", typeof(int));
                            //var InventoryStore = _Context.proc_InventoryStoreInsert(IDInventoryStore,
                            //                                   StoreName,
                            //                                   true,
                            //                                   null, null,
                            //                                   DateTime.Now,
                            //                                   validation.userID,
                            //                                   DateTime.Now,
                            //                                   validation.userID);

                            var InventoryStore = new InventoryStore()
                            {
                                Name = StoreName,
                                Active = true,
                                CreatedBy = UserID,
                                CreationDate = DateTime.Now,
                                ModifiedBy = UserID,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.InventoryStores.Add(InventoryStore);
                            _unitOfWork.Complete();

                            InventoryStoreID = InventoryStore.Id;
                        }



                        #region Save Matrial Request  --------------

                        // Insertion Matrial Request 
                        //ObjectParameter IDMR = new ObjectParameter("ID", typeof(long));
                        //var MRInsertion = _Context.proc_InventoryMatrialRequestInsert(IDMR,
                        //                                                              validation.userID,
                        //                                                              InventoryStoreID,
                        //                                                              RequestgDate,
                        //                                                              DateTime.Now,
                        //                                                              validation.userID,
                        //                                                              DateTime.Now,
                        //                                                              validation.userID,
                        //                                                              true,
                        //                                                              "Open",
                        //                                                              null,
                        //                                                              "Approved",
                        //                                                              null
                        //                                                              );


                        var MRInsertion = new InventoryMatrialRequest()
                        {
                            FromUserId = UserID,
                            ToInventoryStoreId = InventoryStoreID,
                            RequestDate = RequestgDate,
                            CreatedBy = UserID,
                            CreationDate = DateTime.Now,
                            ModifiedBy = UserID,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            Status = "Open",
                            RequestTypeId = null,
                            ApproveResult = "Approved",
                            ApproveRejectNotes = null
                        };
                        _unitOfWork.InventoryMatrialRequests.Add(MRInsertion);
                        _unitOfWork.Complete();

                        mrId = MRInsertion.Id;
                        MRItemIDSList.Add(mrId);



                        if (mrId != 0)
                        {

                            ////ObjectParameter IDPurchaseRequest = new ObjectParameter("ID", typeof(long));
                            //var PurchaseRequest = _Context.proc_PurchaseRequestInsert(IDPurchaseRequest,
                            //                                                          null,
                            //                                                          InventoryStoreID,
                            //                                                          RequestgDate,
                            //                                                          DateTime.Now,
                            //                                                          validation.userID,
                            //                                                          DateTime.Now,
                            //                                                          validation.userID,
                            //                                                          true,
                            //                                                          "Open",
                            //                                                          mrId,
                            //                                                          true,
                            //                                                          "Approved",
                            //                                                          validation.userID,
                            //                                                          "Automatically Approved",
                            //                                                          DateTime.Now
                            //                                                          //"Waiting For Reply",
                            //                                                          //null, null, null
                            //                                                          );
                            var PurchaseRequest = new PurchaseRequest()
                            {
                                ToUserId = null,
                                FromInventoryStoreId = InventoryStoreID,
                                RequestDate = RequestgDate,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserID,
                                Active = true,
                                Status = "Open",
                                MatrialRequestId = mrId,
                                IsDirectPr = true,
                                ApprovalStatus = "Approved",
                                ApprovalUserId = UserID,
                                ApprovalReplyNotes = "Automatically Approved",
                                ApprovalReplyData = DateTime.Now,
                            };

                            _unitOfWork.PurchaseRequests.Add(PurchaseRequest);
                            _unitOfWork.Complete();

                            PurchaseRequestID = PurchaseRequest.Id;
                            if (PurchaseRequest == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorMSG = "Unable to create Purchase Request, please contact your admin";
                                error.ErrorCode = "Err17";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorMSG = "Unable to create Material Request, please contact your admin";
                            error.ErrorCode = "Err14";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        #region Matrial Request items with Purchase Request Item
                        var inventoryItemsIDs = Request.DirectPRItemList.Select(a => a.InventoryItemID).ToList();

                        var inventoryItemList = _unitOfWork.InventoryItems.FindAll(a => inventoryItemsIDs.Contains(a.Id)).ToList();

                        foreach (var MatrialDataOBJ in Request.DirectPRItemList)
                        {
                            bool FromBom = false;
                            var InventoryItemObjDB = inventoryItemList.Where(a => a.Id == MatrialDataOBJ.InventoryItemID).FirstOrDefault();
                            if (InventoryItemObjDB != null)
                            {
                                //add new 
                                //ObjectParameter IDInventoryMatrialRequestItem = new ObjectParameter("ID", typeof(long));
                                //var MatrialRequestItemInsertion = _Context.Myproc_InventoryMatrialRequestItemsInsert_New(IDInventoryMatrialRequestItem,
                                //                                                                                mrId,
                                //                                                                                MatrialDataOBJ.InventoryItemID,
                                //                                                                                InventoryItemObjDB.RequstionUOMID,
                                //                                                                                null,
                                //                                                                                null,
                                //                                                                                MatrialDataOBJ.Comment,
                                //                                                                                0,
                                //                                                                                MatrialDataOBJ.ReqQTY,
                                //                                                                                MatrialDataOBJ.ReqQTY,
                                //                                                                                FromBom,
                                //                                                                                null,
                                //                                                                                null
                                //                                                                             );


                                var MatrialRequestItemInsertion = new InventoryMatrialRequestItem()
                                {
                                    InventoryMatrialRequestId = mrId,
                                    InventoryItemId = MatrialDataOBJ.InventoryItemID,
                                    Uomid = InventoryItemObjDB.RequstionUomid,
                                    ProjectId = null,
                                    FabricationOrderId = null,
                                    Comments = MatrialDataOBJ.Comment,
                                    RecivedQuantity1 = 0,
                                    ReqQuantity1 = MatrialDataOBJ.ReqQTY,
                                    PurchaseQuantity1 = MatrialDataOBJ.ReqQTY,
                                    FromBom = FromBom,
                                    OfferItemId = null,
                                    IsHold = null

                                };

                                _unitOfWork.InventoryMatrialRequestItems.Add(MatrialRequestItemInsertion);
                                _unitOfWork.Complete();

                                var MRItemID = MatrialRequestItemInsertion.Id;
                                MRItemIDSList.Add(MRItemID);

                                if (PurchaseRequestID != 0)
                                {
                                    //ObjectParameter IDPurchaseRequestItem = new ObjectParameter("ID", typeof(long));
                                    //_Context.Myproc_PurchaseRequestItemsInsert(IDPurchaseRequestItem,
                                    //                                         PurchaseRequestID,
                                    //                                         MatrialDataOBJ.Comment,
                                    //                                         MRItemID,
                                    //                                         MatrialDataOBJ.DirectPrNotes,
                                    //                                         null,
                                    //                                         MatrialDataOBJ.ReqQTY,
                                    //                                         0,
                                    //                                         null
                                    //                                         );

                                    var newPurchaseRequestItems = new PurchaseRequestItem()
                                    {
                                        PurchaseRequestId = PurchaseRequestID,
                                        Comments = MatrialDataOBJ.Comment,
                                        InventoryMatrialRequestItemId = MRItemID,
                                        PurchaseRequestNotes = MatrialDataOBJ.DirectPrNotes,
                                        AssignedTo = null,
                                        Quantity1 = MatrialDataOBJ.ReqQTY,
                                        PurchasedQuantity1 = 0,
                                        RemainQuantity1 = null
                                    };

                                    _unitOfWork.PurchaseRequestItems.Add(newPurchaseRequestItems);
                                    _unitOfWork.Complete();
                                }
                            }

                        }

                        #endregion items

                        #endregion

                        Response.ID = PurchaseRequestID;
                    }


                }
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

        public BaseResponseWithID RemoveAssignedToPRItem(RemoveAssignedToPRItemsRRequest Request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InventoryMatrialRequestItemID <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err39";
                        error.ErrorMSG = "Invalid Inventory Matrial Request Item ID";
                        Response.Errors.Add(error);
                    }


                    if (Request.PurchaseRequestID <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err67";
                        error.ErrorMSG = "Invalid Inventory Matrial Request Item ID";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //// Check Inventory Report Approved and closed or not

                        var RemoveAssignedToPRItem = _unitOfWork.PurchaseRequestItems.Find(a => a.PurchaseRequestId == Request.PurchaseRequestID && a.InventoryMatrialRequestItemId ==  Request.InventoryMatrialRequestItemID, null);
                        if (RemoveAssignedToPRItem != null)
                        {
                            RemoveAssignedToPRItem.AssignedTo = null;
                            _unitOfWork.Complete();
                            Response.ID = RemoveAssignedToPRItem.Id;
                            Response.Result = true;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err68";
                            error.ErrorMSG = "Invalid Item Not found to remove from Assigned To";
                            Response.Errors.Add(error);
                        }

                    }
                }
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

        public SelectPRItemsForAssignResponse GetSelectPRItemsForAssign(long? InventoryItemID)
        {
            SelectPRItemsForAssignResponse Response = new SelectPRItemsForAssignResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SelectPRItemsForAssignList = new List<SelectPRItemsForAssign>();
                if (Response.Result)
                {
                    //if (!CommonClass.CheckUserInRole(UserID, 82))
                    //{
                    //    return RedirectToAction("NotAuthorized", "Home");
                    //}


                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}

                    if (Response.Result)
                    {
                        var PurchaseRequestItemOBJDB = _unitOfWork.VPurchaseRequestItems.FindAllQueryable(x => x.AssignedTo == null && x.ApprovalStatusOfPr == "Approved").AsQueryable();


                        if (InventoryItemID != null)
                        {
                            PurchaseRequestItemOBJDB = PurchaseRequestItemOBJDB.Where(x => x.InventoryItemId == InventoryItemID);
                        }
                        if (PurchaseRequestItemOBJDB != null)
                        {
                            var ListOfPurchaseRequestOBJDB = PurchaseRequestItemOBJDB.ToList();
                            foreach (var items in ListOfPurchaseRequestOBJDB)
                            {
                                if (items.PurchasedQuantity < items.Quantity)
                                {
                                    SelectPRItemsForAssign assignedPRItems = new SelectPRItemsForAssign();
                                    assignedPRItems.PurchaseRequestID = items.PurchaseRequestId;
                                    assignedPRItems.PurchaseRequestItemID = items.Id;
                                    assignedPRItems.ID = items.Id;
                                    assignedPRItems.InventoryMatrialRequestID = items.InventoryMatrialRequestId;
                                    assignedPRItems.InventoryMatrialRequestItemID = items.InventoryMatrialRequestItemId;
                                    assignedPRItems.InventoryItemID = items.InventoryItemId;
                                    assignedPRItems.InventoryItemName = items.ItemName.Trim();
                                    assignedPRItems.InventoryItemCode = items.ItemCode;
                                    assignedPRItems.RecivedQuantity = items.RecivedQuantity;
                                    assignedPRItems.PurchasedUOMShortName = items.UomshortName;
                                    assignedPRItems.ReqQuantity = items.ReqQuantity;
                                    assignedPRItems.RequstionUOMID = items.RequstionUomid;
                                    assignedPRItems.RequstionUOMShortName = items.RequstionUomname;
                                    assignedPRItems.ConvertRateFromPurchasingToRequestionUnit = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;

                                    assignedPRItems.ProjectID = items.ProjectId;
                                    assignedPRItems.ProjectName = items.ProjectName;
                                    assignedPRItems.FabricationOrderID = items.FabricationOrderId;
                                    assignedPRItems.FabricationOrderNumber = items.FabNumber != null ? items.FabNumber.ToString() : "";
                                    assignedPRItems.Comment = items.Comments;

                                    decimal requestionQTY = items.Quantity != null ? (decimal)items.Quantity : 0;
                                    decimal factor = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;
                                    decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                                    assignedPRItems.PurchaseItemQuantity = purchaseQTY;
                                    assignedPRItems.RemainQty = items.Quantity - items.PurchasedQuantity;

                                    SelectPRItemsForAssignList.Add(assignedPRItems);
                                }
                            }
                        }


                    }
                    Response.SelectPRItemsForAssignList = SelectPRItemsForAssignList;


                }
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

        public BaseResponseWithID AddAssignPRItem(AssignPRItemRequest Request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.AssignTo <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err79";
                        error.ErrorMSG = "Invalid Assign To Person";
                        Response.Errors.Add(error);
                    }


                    if (Request.PurchaseRequestItmesList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err78";
                        error.ErrorMSG = "Invalid Inventory Matrial Request Item List";
                        Response.Errors.Add(error);
                    }

                    if (Request.PurchaseRequestItmesList.Count() <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err78";
                        error.ErrorMSG = "Invalid Inventory Matrial Request Item List";
                        Response.Errors.Add(error);
                    }

                    var PurchaseRequestItmesList = _unitOfWork.PurchaseRequestItems.FindAll(a =>  Request.PurchaseRequestItmesList.Contains(a.Id));

                    foreach (var item in PurchaseRequestItmesList)
                    {
                        var PurchaseRequestItem = PurchaseRequestItmesList.Where(a => a.Id == item.Id);
                        if (PurchaseRequestItem == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err77";
                            error.ErrorMSG = "Invalid Inventory Matrial Request Item ID " + item;
                            Response.Errors.Add(error);
                        }
                    }


                    if (Response.Result)
                    {
                        //// Check Inventory Report Approved and closed or not

                        foreach (var item in PurchaseRequestItmesList)
                        {
                            if(item != null)
                            {
                                item.AssignedTo = Request.AssignTo;
                            }
                        }

                        _unitOfWork.Complete();
                    }
                }
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

        public SelectPRItemsForAddPOResponse GetSelectPRItemsForAddPO(long? InventoryItemID, long UserID)
        {
            SelectPRItemsForAddPOResponse Response = new SelectPRItemsForAddPOResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SelectPRItemsForAddPO = new List<SelectPRItemsForAssign>();
                if (Response.Result)
                {
                    //if (!CommonClass.CheckUserInRole(UserID, 82))
                    //{
                    //    return RedirectToAction("NotAuthorized", "Home");
                    //}


                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}

                    if (Response.Result)
                    {
                        var PurchaseRequestItemOBJDB = _unitOfWork.VPurchaseRequestItems.FindAllQueryable(x => x.AssignedTo == UserID && x.PurchasedQuantity < x.Quantity).AsQueryable();


                        if (InventoryItemID != null)
                        {
                            PurchaseRequestItemOBJDB = PurchaseRequestItemOBJDB.Where(x => x.InventoryItemId == InventoryItemID);
                        }
                        if (PurchaseRequestItemOBJDB != null)
                        {
                            var ListOfPurchaseRequestOBJDB = PurchaseRequestItemOBJDB.ToList();
                            foreach (var items in ListOfPurchaseRequestOBJDB)
                            {
                                if (items.PurchasedQuantity < items.Quantity)
                                {
                                    SelectPRItemsForAssign assignedPRItems = new SelectPRItemsForAssign();
                                    assignedPRItems.PurchaseRequestID = items.PurchaseRequestId;
                                    assignedPRItems.ConvertRateFromPurchasingToRequestionUnit = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;

                                    assignedPRItems.PurchaseRequestItemID = items.Id;
                                    assignedPRItems.ID = items.Id;
                                    assignedPRItems.InventoryMatrialRequestID = items.InventoryMatrialRequestId;
                                    assignedPRItems.InventoryMatrialRequestItemID = items.InventoryMatrialRequestItemId;
                                    assignedPRItems.InventoryItemID = items.InventoryItemId;
                                    assignedPRItems.InventoryItemName = items.ItemName.Trim();
                                    assignedPRItems.InventoryItemCode = items.ItemCode;
                                    assignedPRItems.RecivedQuantity = items.RecivedQuantity;
                                    assignedPRItems.PurchasedUOMShortName = items.PurchasingUomname;
                                    assignedPRItems.ReqQuantity = items.ReqQuantity;
                                    assignedPRItems.RequstionUOMID = items.RequstionUomid;
                                    assignedPRItems.RequstionUOMShortName = items.RequstionUomname;

                                    assignedPRItems.ProjectID = items.ProjectId;
                                    assignedPRItems.ProjectName = items.ProjectName;
                                    assignedPRItems.FabricationOrderID = items.FabricationOrderId;
                                    assignedPRItems.FabricationOrderNumber = items.FabNumber != null ? items.FabNumber.ToString() : "";
                                    assignedPRItems.Comment = items.Comments;
                                    assignedPRItems.POType = items.Exported;

                                    decimal requestionQTY = items.Quantity != null ? (decimal)items.Quantity : 0;
                                    decimal factor = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;
                                    decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                                    assignedPRItems.PurchaseItemQuantity = purchaseQTY; // Old to be removed  
                                    assignedPRItems.PRItemQuantityUOP = purchaseQTY;
                                    assignedPRItems.PurchasedQuantity = items.PurchasedQuantity;
                                    assignedPRItems.RemainQty = items.Quantity - items.PurchasedQuantity;

                                    SelectPRItemsForAddPO.Add(assignedPRItems);
                                }
                            }
                        }


                    }
                    Response.SelectPRItemsForAddPOList = SelectPRItemsForAddPO;

                }
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

        public async Task<BaseResponseWithID> AddPurchaseOrder(AddPurchaseOrderRequest Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    long SupplierID = 0;
                    if (Request.SupplierID != 0)
                    {
                        SupplierID = Request.SupplierID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Supplier ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int POTypeID = 0;
                    if (Request.POType != 0)
                    {
                        POTypeID = Request.POType;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err43";
                        error.ErrorMSG = "Invalid PO Type ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime RequestgDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RequestDate) || !DateTime.TryParse(Request.RequestDate, out RequestgDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err37";
                        error.ErrorMSG = "Invalid Reqest Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.PurchaseRequestItmesList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.PurchaseRequestItmesList.Count() <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err78";
                        error.ErrorMSG = "Invalid Inventory Matrial Request Item List";
                        Response.Errors.Add(error);
                    }

                    var PurchaseRequestItmesList = await _unitOfWork.VPurchaseRequestItems.FindAllAsync(a => Request.PurchaseRequestItmesList.Contains(a.Id));


                    foreach (var item in PurchaseRequestItmesList)
                    {
                        //var PurchaseRequestItem = _Context.proc_PurchaseRequestItemsLoadByPrimaryKey(item).Where(x => x.PurchasedQuantity < x.Quantity).FirstOrDefault();
                        var PRItemDB = PurchaseRequestItmesList.Where(x => x.Id == item.Id && x.PurchasedQuantity < x.Quantity).FirstOrDefault();
                        if (PRItemDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err73";
                            error.ErrorMSG = "Invalid Inventory Purchase Request Item ID " + item;
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            if (PRItemDB.InventoryItemId == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err73";
                                error.ErrorMSG = "Inventory Item ID Required" + item;
                                Response.Errors.Add(error);
                            }
                            if (PRItemDB.Uomid == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err73";
                                error.ErrorMSG = "UOMID Required " + item;
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    //long PurchaseRequestID = 0;
                    if (Response.Result)
                    {
                        //// Check Inventory Report Approved and closed or not
                        //var CheckToInventoryReportListDB = _Context.proc_InventoryReportLoadAll().Where(x => x.Active == true && x.Approved == false && x.Closed == false && x.InventoryStoreID == ToInventoryStorID).ToList();
                        //if (CheckToInventoryReportListDB.Count > 0)
                        //{
                        //    foreach (var InventoryRep in CheckToInventoryReportListDB)
                        //    {
                        //        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                        //        {
                        //            string storeName = Common.getInventoryStoreName(ToInventoryStorID);
                        //            string errMsg = "Store " + storeName +
                        //                " is under inventory from " +
                        //                InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
                        //                " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

                        //            Response.Result = false;
                        //            Error error = new Error();
                        //            error.ErrorCode = "Err-44";
                        //            error.ErrorMSG = errMsg;
                        //            Response.Errors.Add(error);
                        //        }
                        //    }
                        //}




                        if (Response.Result)
                        {
                            // Insert Purchase PO 
                            var PurchasePODB = new PurchasePo();
                            PurchasePODB.ToSupplierId = SupplierID;
                            PurchasePODB.RequestDate = RequestgDate;
                            PurchasePODB.CreationDate = DateTime.Now;
                            PurchasePODB.CreatedBy = UserID;
                            PurchasePODB.ModifiedDate = DateTime.Now;
                            PurchasePODB.ModifiedBy = UserID;
                            PurchasePODB.Active = true;
                            PurchasePODB.Status = "Open";
                            PurchasePODB.PotypeId = POTypeID;
                            PurchasePODB.AccountantReplyNotes = "Not Assigned To Accountant";
                            PurchasePODB.AssignedPurchasingPersonId = UserID;

                            _unitOfWork.PurchasePos.Add(PurchasePODB);
                            var Res = _unitOfWork.Complete();

                            //ObjectParameter IDPO = new ObjectParameter("ID", typeof(long));
                            //var OPInsertion = _Context.proc_PurchasePOInsert(IDPO,
                            //                                   SupplierID, RequestgDate,
                            //                                   DateTime.Now, validation.userID, DateTime.Now, validation.userID,
                            //                                   true, "Open", POTypeID,
                            //                                   null, "Not Assigned To Accountant",
                            //                                   null, null, null, null, null, null, null, null, null, null, null, null,
                            //                                   validation.userID,
                            //                                   null, null
                            //                                   );


                            long POID = PurchasePODB.Id;
                            long PurchaseRequestID = 0;


                            #region items


                            if (POID != 0 && Res > 0)
                            {

                                var PurchaseRequestItemsList = _unitOfWork.PurchaseRequestItems.FindAll(a => Request.PurchaseRequestItmesList.Contains(a.Id));
                                foreach (var PurchaseRequestItemID in Request.PurchaseRequestItmesList)
                                {

                                    var PurchaseRequestItem = PurchaseRequestItmesList.Where(x => x.Id == PurchaseRequestItemID).FirstOrDefault();
                                    if (PurchaseRequestItem != null)
                                    {
                                        PurchaseRequestID = PurchaseRequestItem.PurchaseRequestId;
                                        // Insert Purchase PO  Item
                                        var PurchasePOItemDB = new PurchasePoitem();
                                        PurchasePOItemDB.InventoryMatrialRequestItemId = PurchaseRequestItem.InventoryMatrialRequestItemId;
                                        PurchasePOItemDB.InventoryItemId = (long)PurchaseRequestItem.InventoryItemId;
                                        PurchasePOItemDB.Uomid = (int)PurchaseRequestItem.Uomid;
                                        PurchasePOItemDB.ProjectId = PurchaseRequestItem.ProjectId;
                                        PurchasePOItemDB.FabricationOrderId = PurchaseRequestItem.FabricationOrderId;
                                        PurchasePOItemDB.Comments = PurchaseRequestItem.Comments;
                                        PurchasePOItemDB.PurchasePoid = POID;
                                        PurchasePOItemDB.PurchaseRequestItemId = PurchaseRequestItem.Id;
                                        PurchasePOItemDB.ReqQuantity1 = PurchaseRequestItem.ReqQuantity;
                                        PurchasePOItemDB.RecivedQuantity1 = 0;

                                        _unitOfWork.PurchasePOItems.Add(PurchasePOItemDB);
                                        _unitOfWork.Complete();

                                        //    ObjectParameter IDPOItem = new ObjectParameter("ID", typeof(long));
                                        //_Context.Myproc_PurchasePOItemInsert(IDPOItem,
                                        //                                   PurchaseRequestItem.InventoryMatrialRequestItemID,
                                        //                                   PurchaseRequestItem.InventoryItemID,
                                        //                                   PurchaseRequestItem.UOMID,
                                        //                                   PurchaseRequestItem.ProjectID,
                                        //                                   PurchaseRequestItem.FabricationOrderID,
                                        //                                   PurchaseRequestItem.Comments,
                                        //                                   POID,
                                        //                                   PurchaseRequestItem.ID,
                                        //                                   null, null, null, null, null, null, null,
                                        //                                   PurchaseRequestItem.ReqQuantity,
                                        //                                   0,
                                        //                                   null
                                        //                               );


                                        var PurchasedQuantity = PurchaseRequestItem.PurchasedQuantity + PurchaseRequestItem.ReqQuantity;
                                        //_Context.Myproc_PurchaseRequestItemsUpdate_QTY(PurchaseRequestItemID, PurchasedQuantity);
                                        var PRItemObj = PurchaseRequestItemsList.Where(x => x.Id == PurchaseRequestItemID).FirstOrDefault();
                                        PRItemObj.PurchasedQuantity = (double)PurchasedQuantity;

                                        _unitOfWork.Complete();

                                        //var PurchaseRequestItmesList = _Context.proc_PurchaseRequestItemsLoadAll().Where(x => x.PurchaseRequestID == PurchaseRequestItem.PurchaseRequestID)
                                        //                                    .ToList();

                                        //bool found = false;
                                        //if (PurchaseRequestItmesListDB.Count > 0)
                                        //{
                                        //    var TotalQTY = PurchaseRequestItmesListDB.Where(x => x.Quantity != null).Sum(x => x.Quantity);
                                        //    var TotalPurchasedQuantity = PurchaseRequestItmesListDB.Where(x => x.PurchasedQuantity != null).Sum(x => x.PurchasedQuantity);
                                        //    if (TotalQTY != 0 && TotalPurchasedQuantity != null && TotalQTY == TotalPurchasedQuantity)
                                        //        found = true;

                                        //}

                                        //if (found)
                                        //{

                                        //    PRObjDB.Status = "Closed";
                                        //    //_Context.Myproc_PurchaseRequestUpdate_Status(PurchaseRequestItem.PurchaseRequestID, "Closed");
                                        //}



                                    }





                                }
                                var PurchaseRequestItmesListDB = await _unitOfWork.PurchaseRequestItems.FindAllAsync(x => x.PurchaseRequestId == PurchaseRequestID);
                                #region Update PR Status Closed 
                                var PRItemCountDB = PurchaseRequestItmesListDB.Count();
                                var PRItemCountPurchasedDB = PurchaseRequestItmesListDB.Where(x => x.PurchasedQuantity >= x.Quantity).Count();
                                // Check if all Qty is Purchased 
                                if (PRItemCountPurchasedDB >= PRItemCountDB)
                                {
                                    var PRObjDB = await _unitOfWork.PurchaseRequests.FindAsync(x => x.Id == PurchaseRequestID);
                                    if(PRObjDB != null)
                                    {
                                        PRObjDB.Status = "Closed";

                                    }
                                }
                                #endregion
                                _unitOfWork.Complete();


                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err1011";
                                error.ErrorMSG = "Purchase Order Not Created , Please try again";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            #endregion items

                            Response.ID = POID;
                        }


                    }
                }
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

        public InventoryMatrialPurchaseRequestResponse GetMatrialPurchaseRequestList(InventoryMatrialPurchaseFilters filters)
        {
            InventoryMatrialPurchaseRequestResponse Response = new InventoryMatrialPurchaseRequestResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var inventoryMatrialPurchaseRequestByDateList = new List<InventoryMatrialPurchaseRequestByDate>();
                if (Response.Result)
                {
                    #region old filters
                    // filters List InternalBackOrder
                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}

                    //long PurchaseRequestID = 0;
                    //if (!string.IsNullOrEmpty(headers["PurchaseRequestID"]) && long.TryParse(headers["PurchaseRequestID"], out PurchaseRequestID))
                    //{
                    //    long.TryParse(headers["PurchaseRequestID"], out PurchaseRequestID);
                    //}


                    //bool? IsDirectPR = null;
                    //if (!string.IsNullOrEmpty(headers["IsDirectPR"]))
                    //{
                    //    IsDirectPR = bool.Parse(headers["IsDirectPR"]);
                    //}

                    //long FormInventoryStoreID = 0;
                    //if (!string.IsNullOrEmpty(headers["FormInventoryStoreID"]) && long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID))
                    //{
                    //    long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID);
                    //}


                    //long CreatorUserID = 0;
                    //if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                    //{
                    //    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                    //}
                    //string Status = "";
                    //if (!string.IsNullOrEmpty(headers["Status"]))
                    //{
                    //    Status = headers["Status"];
                    //}
                    //string SearchKey = "";
                    //if (!string.IsNullOrEmpty(headers["SearchKey"]))
                    //{
                    //    SearchKey = HttpUtility.UrlDecode(headers["SearchKey"]);
                    //}
                    //int currentpage = 1;
                    //if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out currentpage))
                    //{
                    //    currentpage = int.Parse(headers["CurrentPage"]);
                    //}
                    //int ItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(headers["ItemsPerPage"]) && int.TryParse(headers["ItemsPerPage"], out ItemsPerPage))
                    //{
                    //    ItemsPerPage = int.Parse(headers["ItemsPerPage"]);
                    //}
                    #endregion

                    DateTime? RequestDate = null;
                    DateTime RequestDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(headers["RequestDate"]) && DateTime.TryParse(headers["RequestDate"], out RequestDateTemp))
                    //{
                    //    RequestDateTemp = DateTime.Parse(headers["RequestDate"]);
                    //    RequestDate = RequestDateTemp;
                    //}
                    if (!string.IsNullOrEmpty(filters.RequestDate))
                    {
                        if(!DateTime.TryParse(filters.RequestDate, out RequestDateTemp))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Enter a valid DateFrom";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    var PurchaseRequestOrderQuerable = _unitOfWork.VPurchaseRequests.FindAllQueryable(x => x.Active == true);
                    if (filters.PurchaseRequestID != null )
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.Id == filters.PurchaseRequestID).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.Status))
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.Status.ToLower() == filters.Status.ToLower()).AsQueryable();
                    }
                    if (filters.IsDirectPR == true)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.IsDirectPr == true).AsQueryable();
                    }
                    if (filters.FormInventoryStoreID != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.FromInventoryStoreId == filters.FormInventoryStoreID).AsQueryable();
                    }
                    if (filters.CreatorUserID != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }
                    if (RequestDate != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.RequestDate == RequestDate).AsQueryable();
                    }
                    if (filters.InventoryItemID != null)
                    {
                        var IDPurchaseRequestID = _unitOfWork.VPurchaseRequestItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.PurchaseRequestId).Distinct().ToList();

                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => IDPurchaseRequestID.Contains(x.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {
                        filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                        var IDPurchaseRequestID = _unitOfWork.VPurchaseRequestItems.FindAll(x => x.ItemName.ToLower().Contains(filters.SearchKey) || x.ItemCode.ToLower().Contains(filters.SearchKey)).Select(x => x.PurchaseRequestId).Distinct().ToList();

                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => IDPurchaseRequestID.Contains(x.Id)).AsQueryable();
                    }
                    var PurchaseRequestOrderList = PurchaseRequestOrderQuerable.ToList();
                    var PurchaseRequestFilteredGrouped = PurchaseRequestOrderList.OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();
                    long TotalItemCount = 0;
                    foreach (var PurchaseRequestPerMonth in PurchaseRequestFilteredGrouped)
                    {
                        var PurchaseRequestInfoPList = new List<InventoryMatrialPurchaseRequestInfo>();

                        // var InternalTransferPerMonthList = InternalTransferPerMonth.ToList();
                        var usersIds = PurchaseRequestPerMonth.Select(a => a.CreatedBy).ToList();
                        var usersData = _unitOfWork.Users.FindAll(a => usersIds.Contains(a.Id));

                        TotalItemCount += PurchaseRequestPerMonth.Count();
                        foreach (var Data in PurchaseRequestPerMonth)
                        {
                            var creator = usersData.Where(a => a.Id == Data.CreatedBy).FirstOrDefault();
                            PurchaseRequestInfoPList.Add(new InventoryMatrialPurchaseRequestInfo
                            {
                                PurchaseRequestID = Data.Id.ToString(),
                                FromInventoryStoreName = Data.FromInventoryStoreName,
                                Status = Data.Status,
                                ApprovalStataus = Data.ApprovalStatus,
                                CreatorName = creator.FirstName + " " + creator.LastName,
                                IsDirectPR = Data.IsDirectPr == true ? true : false,
                                RequestDate = Data.RequestDate.ToShortDateString(),
                            });
                        }
                        inventoryMatrialPurchaseRequestByDateList.Add(new InventoryMatrialPurchaseRequestByDate()
                        {
                            DateMonth = Common.GetMonthName(PurchaseRequestPerMonth.Key.month) + " " + PurchaseRequestPerMonth.Key.year.ToString(),
                            //item.Select(x=>x.CreatedDate.Month).ToString(),
                            InventoryMatrialPurchaseRequestInfoList = PurchaseRequestInfoPList,
                        });
                    }

                    Response.InventoryMatrialPurchaseRequestByDateList = inventoryMatrialPurchaseRequestByDateList;
                    Response.TotalCounter = TotalItemCount;
                }
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

        public InventoryMatrialPurchaseRequestResponse2 GetMatrialPurchaseRequestListForWeb(InventoryMatrialPurchaseFilters filters)
        {
            InventoryMatrialPurchaseRequestResponse2 Response = new InventoryMatrialPurchaseRequestResponse2();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var PurchaseRequestInfoPList = new List<InventoryMatrialPurchaseRequestInfo>();
                if (Response.Result)
                {
                    #region old filters
                    // filters List InternalBackOrder
                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}

                    //long PurchaseRequestID = 0;
                    //if (!string.IsNullOrEmpty(headers["PurchaseRequestID"]) && long.TryParse(headers["PurchaseRequestID"], out PurchaseRequestID))
                    //{
                    //    long.TryParse(headers["PurchaseRequestID"], out PurchaseRequestID);
                    //}


                    //bool? IsDirectPR = null;
                    //if (!string.IsNullOrEmpty(headers["IsDirectPR"]))
                    //{
                    //    IsDirectPR = bool.Parse(headers["IsDirectPR"]);
                    //}

                    //long FormInventoryStoreID = 0;
                    //if (!string.IsNullOrEmpty(headers["FormInventoryStoreID"]) && long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID))
                    //{
                    //    long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID);
                    //}


                    //long CreatorUserID = 0;
                    //if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                    //{
                    //    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                    //}
                    //string Status = "";
                    //if (!string.IsNullOrEmpty(headers["Status"]))
                    //{
                    //    Status = headers["Status"];
                    //}
                    //string SearchKey = "";
                    //if (!string.IsNullOrEmpty(headers["SearchKey"]))
                    //{
                    //    SearchKey = HttpUtility.UrlDecode(headers["SearchKey"]);
                    //}
                    //int currentpage = 1;
                    //if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out currentpage))
                    //{
                    //    currentpage = int.Parse(headers["CurrentPage"]);
                    //}
                    //int ItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(headers["ItemsPerPage"]) && int.TryParse(headers["ItemsPerPage"], out ItemsPerPage))
                    //{
                    //    ItemsPerPage = int.Parse(headers["ItemsPerPage"]);
                    //}
                    #endregion

                    DateTime? RequestDate = null;
                    DateTime RequestDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(filters.RequestDate) && DateTime.TryParse(filters.RequestDate, out RequestDateTemp))
                    {
                        RequestDateTemp = DateTime.Parse(filters.RequestDate);
                        RequestDate = RequestDateTemp;
                    }
                    var PurchaseRequestOrderQuerable = _unitOfWork.VPurchaseRequests.FindAllQueryable(x => x.Active == true).AsQueryable();
                    if (filters.PurchaseRequestID != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.Id == filters.PurchaseRequestID).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.Status))
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.Status.ToLower() == filters.Status.ToLower()).AsQueryable();
                    }
                    if (filters.IsDirectPR == true)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.IsDirectPr == true).AsQueryable();
                    }
                    if (filters.FormInventoryStoreID != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.FromInventoryStoreId == filters.FormInventoryStoreID).AsQueryable();
                    }
                    if (filters.CreatorUserID != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }
                    if (RequestDate != null)
                    {
                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => x.RequestDate == RequestDate).AsQueryable();
                    }
                    if (filters.InventoryItemID != null)
                    {
                        var IDPurchaseRequestID = _unitOfWork.VPurchaseRequestItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.PurchaseRequestId).Distinct().ToList();

                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => IDPurchaseRequestID.Contains(x.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {
                        filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                        var IDPurchaseRequestID = _unitOfWork.VPurchaseRequestItems.FindAll(x => x.ItemName.ToLower().Contains(filters.SearchKey) || x.ItemCode.ToLower().Contains(filters.SearchKey)).Select(x => x.PurchaseRequestId).Distinct().ToList();

                        PurchaseRequestOrderQuerable = PurchaseRequestOrderQuerable.Where(x => IDPurchaseRequestID.Contains(x.Id)).AsQueryable();
                    }

                    //var list = PurchaseRequestOrderQuerable.ToList();
                    var PurchaseRequestFilteredGrouped = PurchaseRequestOrderQuerable.OrderByDescending(x => x.CreationDate);
                    long TotalItemCount = 0;
                    var list = PagedList<VPurchaseRequest>.Create(PurchaseRequestFilteredGrouped, filters.currentpage, filters.ItemsPerPage);
                    var creatorsIds = list.Select(a => a.CreatedBy).ToList();
                    var creatorsData = _unitOfWork.Users.FindAll(a => creatorsIds.Contains(a.Id));
                    foreach (var Data in list)
                    {
                        var userData = creatorsData.Where(a => a.Id == Data.CreatedBy).FirstOrDefault();
                        PurchaseRequestInfoPList.Add(new InventoryMatrialPurchaseRequestInfo
                        {
                            PurchaseRequestID = Data.Id.ToString(),
                            FromInventoryStoreName = Data.FromInventoryStoreName,
                            Status = Data.Status,
                            ApprovalStataus = Data.ApprovalStatus,
                            CreatorName = userData.FirstName + " " + userData.LastName,
                            IsDirectPR = Data.IsDirectPr == true ? true : false,
                            RequestDate = Data.RequestDate.ToShortDateString(),
                        });
                        TotalItemCount += 1;
                    }
                    Response.InventoryMatrialPurchaseRequestByDateList = PurchaseRequestInfoPList.ToList();
                    Response.TotalCounter = TotalItemCount;
                    Response.PaginationHeader = new PaginationHeader()
                    {
                        CurrentPage = filters.currentpage,
                        TotalPages = list.TotalPages,
                        ItemsPerPage = filters.ItemsPerPage,
                        TotalItems = list.TotalCount
                    };
                }
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

        public PurchaseRequestWithItemsInfoResponse GetPurchaseRequestWithItemsInfo(long PurchaseRequestID)
        {
            PurchaseRequestWithItemsInfoResponse Response = new PurchaseRequestWithItemsInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryPurchaseRequestWithItemInfoOBJ = new InventoryPurchaseRequestWithItemInfo();
                var PurchaseRequestItemInfoList = new List<PurchaseRequestItemInfo>();
                if (Response.Result)
                {
                   
                    if(PurchaseRequestID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err117";
                        error.ErrorMSG = "Invalid Purchase Request ID";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {
                        var PurchaseRequestOBJDB = _unitOfWork.VPurchaseRequests.Find(x => x.Id == PurchaseRequestID);
                        if (PurchaseRequestOBJDB != null)
                        {
                            InventoryPurchaseRequestWithItemInfoOBJ.PurchaseRequestID = PurchaseRequestID;
                            InventoryPurchaseRequestWithItemInfoOBJ.FromInventoryStoreName = PurchaseRequestOBJDB.FromInventoryStoreName;
                            InventoryPurchaseRequestWithItemInfoOBJ.Status = PurchaseRequestOBJDB.Status;
                            InventoryPurchaseRequestWithItemInfoOBJ.ApprovalStataus = PurchaseRequestOBJDB.ApprovalStatus;
                            InventoryPurchaseRequestWithItemInfoOBJ.IsDirectPR = PurchaseRequestOBJDB.IsDirectPr == true ? true : false;
                            InventoryPurchaseRequestWithItemInfoOBJ.RequestDate = PurchaseRequestOBJDB.RequestDate.ToShortDateString();



                            var ListPurchaseRequestItemListDB = _unitOfWork.PurchaseRequestItems.FindAll(x => x.PurchaseRequestId == PurchaseRequestID, new[] { "InventoryMatrialRequestItem", "InventoryMatrialRequestItem.Project.SalesOffer" , "InventoryMatrialRequestItem.InventoryItem", "PurchasePoitems.PurchasePo.ToSupplier", "InventoryMatrialRequestItem.InventoryItem.RequstionUom", "InventoryMatrialRequestItem.InventoryItem.PurchasingUom" }).ToList();
                            if (ListPurchaseRequestItemListDB != null)
                            {
                                foreach (var item in ListPurchaseRequestItemListDB)
                                {
                                    var MatrialRequestItem = item.InventoryMatrialRequestItem;
                                    var InventoryItem = MatrialRequestItem?.InventoryItem;
                                    var PurchaseRequestItemInfoObj = new PurchaseRequestItemInfo();
                                    PurchaseRequestItemInfoObj.PurchaseRequestItemID = item.Id;
                                    PurchaseRequestItemInfoObj.InventoryItemID = MatrialRequestItem?.InventoryItemId ?? 0; // != null ? (long)item.InventoryItemID : 0;
                                    PurchaseRequestItemInfoObj.ConvertRateFromPurchasingToRequestionUnit = InventoryItem?.ExchangeFactor1 ?? 0;// != null ? (decimal)item.ExchangeFactor : 0;
                                    PurchaseRequestItemInfoObj.ItemName = InventoryItem?.Name; // item.InventoryItemName.Trim();
                                    PurchaseRequestItemInfoObj.ItemCode = InventoryItem?.Code; //item.InventoryItemCode;
                                    PurchaseRequestItemInfoObj.ReqQTY = (decimal)item?.InventoryMatrialRequestItem?.ReqQuantity1; ; // item.RequestQuantity != null ? (decimal)item.RequestQuantity : 0;
                                    PurchaseRequestItemInfoObj.ReqUOM = InventoryItem?.RequstionUom?.ShortName;
                                    PurchaseRequestItemInfoObj.PurchaseUOM = InventoryItem?.PurchasingUom?.ShortName;
                                    PurchaseRequestItemInfoObj.PurchaseQTY = (decimal)item?.PurchasedQuantity; // != null ? (decimal)item.PurchaseRequestQuantity : 0;
                                    PurchaseRequestItemInfoObj.ProjectName = MatrialRequestItem?.Project?.SalesOffer?.ProjectName;
                                    PurchaseRequestItemInfoObj.Comment = item.Comments;
                                    PurchaseRequestItemInfoObj.MRItemComment = item.InventoryMatrialRequestItem.Comments;

                                    // Received Quantity PO
                                    //PurchaseRequestItemInfoObj.PurchasePOItemRecivedQuantity = item.PurchasePOItemRecivedQuantity != null ? (decimal)item.PurchasePOItemRecivedQuantity : 0;
                                    PurchaseRequestItemInfoObj.RecivedQuantityRUOMShortName = PurchaseRequestItemInfoObj.ReqUOM;

                                    var PurchaseOrdersDetailsList = item.PurchasePoitems.Select(POItem => new PurchaseOrdersDetails
                                    {
                                        POID = POItem.PurchasePoid,
                                        PurchaseOrderQTY = POItem.ReqQuantity1 ?? 0,
                                        SupplierName = POItem.PurchasePo?.ToSupplier?.Name
                                    }).ToList();
                                    PurchaseRequestItemInfoObj.PurchaseOrdersDetailsList = PurchaseOrdersDetailsList;
                                    //new List<PurchaseOrdersDetails>();

                                    PurchaseRequestItemInfoList.Add(PurchaseRequestItemInfoObj);
                                }
                            }



                            //var ListPurchaseRequestItemListDB = _Context.V_PurchaseRequestItems_PO.Where(x => x.PurchaseRequestID == PurchaseRequestID).ToList();
                            //if (ListPurchaseRequestItemListDB != null)
                            //{
                            //    foreach (var item in ListPurchaseRequestItemListDB)
                            //    {

                            //        var PurchaseRequestItemInfoObj = new PurchaseRequestItemInfo();
                            //        PurchaseRequestItemInfoObj.PurchaseRequestItemID = item.PurchaseRequestItemsID;
                            //        PurchaseRequestItemInfoObj.InventoryItemID = item.InventoryItemID != null ? (long)item.InventoryItemID : 0;
                            //        PurchaseRequestItemInfoObj.ConvertRateFromPurchasingToRequestionUnit = item.ExchangeFactor != null ? (decimal)item.ExchangeFactor : 0;
                            //        PurchaseRequestItemInfoObj.ItemName = item.InventoryItemName.Trim();
                            //        PurchaseRequestItemInfoObj.ItemCode = item.InventoryItemCode;
                            //        PurchaseRequestItemInfoObj.ReqQTY = item.RequestQuantity != null ? (decimal)item.RequestQuantity : 0;
                            //        PurchaseRequestItemInfoObj.ReqUOM = item.RequstionUOMShortName;
                            //        PurchaseRequestItemInfoObj.PurchaseQTY = item.PurchaseRequestQuantity != null ? (decimal)item.PurchaseRequestQuantity : 0;
                            //        PurchaseRequestItemInfoObj.PurchaseUOM = item.PurchasedUOMShortName;
                            //        PurchaseRequestItemInfoObj.ProjectName = item.ProjectName;
                            //        PurchaseRequestItemInfoObj.Comment = item.Comments;
                            //        PurchaseRequestItemInfoObj.MRItemComment = item.MRItemComments;
                            //        PurchaseRequestItemInfoObj.POID = item.PurchasePOID ?? 0;

                            //        // Received Quantity PO
                            //        PurchaseRequestItemInfoObj.PurchasePOItemRecivedQuantity = item.PurchasePOItemRecivedQuantity != null ? (decimal)item.PurchasePOItemRecivedQuantity : 0;
                            //        PurchaseRequestItemInfoObj.RecivedQuantityRUOMShortName = item.RequstionUOMShortName;
                            //        PurchaseRequestItemInfoList.Add(PurchaseRequestItemInfoObj);
                            //    }
                            //}
                        }




                        //    var ListPurchaseRequestItemListDB = _Context.PurchasePOItems.Where(x=>x.PurchaseRequestItem.PurchaseRequestID == PurchaseRequestID).ToList();
                        //    if (ListPurchaseRequestItemListDB != null)
                        //    {
                        //        foreach (var item in ListPurchaseRequestItemListDB)
                        //        {
                        //            var InvItem = item.InventoryItem;
                        //            var PurchaseRequestItem = item.PurchaseRequestItem;
                        //            var PurchaseRequestItemInfoObj = new PurchaseRequestItemInfo();
                        //            PurchaseRequestItemInfoObj.PurchaseRequestItemID = item.ID;
                        //            PurchaseRequestItemInfoObj.InventoryItemID = item.InventoryMatrialRequestItem?.InventoryItemID ?? 0; // != null ? (long)item.InventoryItemID : 0;
                        //            PurchaseRequestItemInfoObj.ConvertRateFromPurchasingToRequestionUnit = InvItem?.ExchangeFactor ?? 0; // != null ? (decimal)item.ExchangeFactor : 0;
                        //            PurchaseRequestItemInfoObj.ItemName = InvItem?.Name?.Trim(); // item.InventoryItemName.Trim();
                        //            PurchaseRequestItemInfoObj.ItemCode = InvItem?.Code; // item.InventoryItemCode;
                        //            PurchaseRequestItemInfoObj.ReqQTY = PurchaseRequestItem?.Quantity ?? 0; // item.ReqQuantity != null ? (decimal)item.ReqQuantity : 0;
                        //            PurchaseRequestItemInfoObj.ReqUOM = item.InventoryUOM?.Name; //item.RequstionUOMShortName;
                        //            PurchaseRequestItemInfoObj.PurchaseQTY = PurchaseRequestItem?.PurchasedQuantity ?? 0; // != null ? (decimal)item.PurchaseRequestQuantity : 0;
                        //            PurchaseRequestItemInfoObj.PurchaseUOM = item.InventoryUOM?.Name; //.PurchasedUOMShortName;
                        //            PurchaseRequestItemInfoObj.ProjectName = item.Project?.SalesOffer?.ProjectName;
                        //            PurchaseRequestItemInfoObj.Comment = item.Comments;
                        //            PurchaseRequestItemInfoObj.MRItemComment = item.InventoryMatrialRequestItem?.Comments;
                        //            PurchaseRequestItemInfoObj.POID = item.PurchasePOID;
                        //            PurchaseRequestItemInfoObj.SupplierName = item.PurchasePO?.Supplier?.Name;
                        //            PurchaseRequestItemInfoObj.PurchaseOrderQTY = item.ReqQuantity ?? 0; //?.Supplier?.Name;

                        //            // Received Quantity PO
                        //            PurchaseRequestItemInfoObj.PurchasePOItemRecivedQuantity = item.RecivedQuantity ?? 0; // != null ? (decimal)item.PurchasePOItemRecivedQuantity : 0;
                        //            PurchaseRequestItemInfoObj.RecivedQuantityRUOMShortName = item.InventoryUOM?.Name; //item.RequstionUOMShortName;
                        //            PurchaseRequestItemInfoList.Add(PurchaseRequestItemInfoObj);
                        //        }
                        //    }
                        //}

                        InventoryPurchaseRequestWithItemInfoOBJ.PurchaseItemInfoList = PurchaseRequestItemInfoList;

                    }
                    Response.PurchaseRequestWithItemItemInfo = InventoryPurchaseRequestWithItemInfoOBJ;


                }
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

        public GetPurchasePOResponse GetMangePurchasePOList(long ?InventoryItemID, long? CreatorUserID, string RequestDatestr, bool? WithJE)
        {
            GetPurchasePOResponse Response = new GetPurchasePOResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var PurchasePOByDateList = new List<PurchasePOByDate>();
                if (Response.Result)
                {

                    //// filters List InternalBackOrder
                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}



                    //long CreatorUserID = 0;
                    //if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                    //{
                    //    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                    //}


                    //DateTime? RequestDate = null;
                    //DateTime RequestDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(RequestDateStr) && DateTime.TryParse(RequestDateStr, out RequestDateTemp))
                    //{
                    //    RequestDateTemp = DateTime.Parse(RequestDateStr);
                    //    RequestDate = RequestDateTemp;
                    //}
                    DateTime RequestDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(RequestDatestr))
                    { 
                        if(!DateTime.TryParse(RequestDatestr, out RequestDate))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err37";
                            error.ErrorMSG = "Invalid Reqest Date.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    } 

                    // Grouped by DAte as Inquiry 
                    //var InventoryMatrialAddingOrder


                    var PurchasePOQuerable = _unitOfWork.VPurchasePos.FindAllQueryable(x => x.Active == true).AsQueryable();

                    if (CreatorUserID != null)
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.CreatedBy == CreatorUserID).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(RequestDatestr))
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.RequestDate.Date == RequestDate.Date).AsQueryable();
                    }
                    if (InventoryItemID != null)
                    {
                        var IDPurchaseRequestID = _unitOfWork.VPurchasePoItems.FindAll(x => x.InventoryItemId == InventoryItemID).Select(x => x.PurchasePoid).Distinct().ToList();

                        PurchasePOQuerable = PurchasePOQuerable.Where(x => IDPurchaseRequestID.Contains(x.Id)).AsQueryable();
                    }


                    if (WithJE != null)
                    {
                        var ListPurchasePOIDS = _unitOfWork.SupplierAccounts.FindAll(x=>x.Active == true).Select(x => x.PurchasePoid).Distinct().ToList();
                        if (WithJE == true)
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => ListPurchasePOIDS.Contains(x.Id));
                        }
                        else if(WithJE == false) 
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => !ListPurchasePOIDS.Contains(x.Id));
                        }
                    }
                    var PurchasePOList = PurchasePOQuerable.ToList();
                    var PurchasePOFilteredGrouped = PurchasePOList.OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();
                    var usersIds = PurchasePOList.Select(b=> b.CreatedBy).ToList();
                    var usersData = _unitOfWork.Users.FindAll(a => usersIds.Contains(a.Id));
                    foreach (var PurchasePOPerMonth in PurchasePOFilteredGrouped)
                    {
                        var PurchasePOInfoPList = new List<PurchasePO>();

                        // var InternalTransferPerMonthList = InternalTransferPerMonth.ToList();
                        
                        foreach (var Data in PurchasePOPerMonth)
                        {
                            var creator = usersData.Where(a => a.Id == Data.CreatedBy).FirstOrDefault();
                            PurchasePOInfoPList.Add(new PurchasePO
                            {
                                ID = Data.Id,
                                RequestDate = Data.RequestDate.ToShortDateString(),
                                CreationDate = Data.CreationDate.ToShortDateString(),
                                SupplierName = Data.SupplierName,
                                Status = Data.Status,
                                TotalEstimatedCost = Data.TotalEstimatedCost,
                                TechApprovalStatus = Data.TechApprovalStatus,
                                FinalApprovalStatus = Data.FinalApprovalStatus,
                                ApprovalStatus = Data.ApprovalStatus,
                                POTypeID = Data.PotypeId,
                                SentToSupplier = Data.SentToSupplier,
                                AssignedPurchasingPersonID = Data.AssignedPurchasingPersonId,
                                CreatedByID = Data.CreatedBy,
                                CreatedByName = creator.FirstName + "  " + creator.LastName,
                            });
                        }
                        PurchasePOByDateList.Add(new PurchasePOByDate()
                        {
                            DateMonth = Common.GetMonthName(PurchasePOPerMonth.Key.month) + " " + PurchasePOPerMonth.Key.year.ToString(),
                            //item.Select(x=>x.CreatedDate.Month).ToString(),
                            PurchasePOList = PurchasePOInfoPList,
                        });
                    }

                    Response.PurchasePOByDateList = PurchasePOByDateList;

                }
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

        public GetPurchasePOWebResponse GetMangePurchasePOWebList(GetMangePurchasePOWebListFilters filters)
        {
            GetPurchasePOWebResponse Response = new GetPurchasePOWebResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var PurchasePOList = new List<PurchasePO>();
                if (Response.Result)
                {
                    #region old headers
                    //int CurrentPage = 1;
                    //if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    //{
                    //    int.TryParse(headers["CurrentPage"], out CurrentPage);
                    //}

                    //int NumberOfItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    //{
                    //    int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    //}
                    // filters List InternalBackOrder
                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}
                    //long CreatorUserID = 0;
                    //if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                    //{
                    //    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                    //}
                    //long SupplierID = 0;
                    //if (!string.IsNullOrEmpty(headers["SupplierID"]) && long.TryParse(headers["SupplierID"], out SupplierID))
                    //{
                    //    long.TryParse(headers["SupplierID"], out SupplierID);
                    //}


                    //long POID = 0;
                    //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
                    //{
                    //    long.TryParse(headers["POID"], out POID);
                    //}

                    //long POTypeID = 0;
                    //if (!string.IsNullOrEmpty(headers["POTypeID"]) && long.TryParse(headers["POTypeID"], out POTypeID))
                    //{
                    //    long.TryParse(headers["POTypeID"], out POTypeID);
                    //}

                    //DateTime? RequestDate = null;
                    //DateTime RequestDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(headers["RequestDate"]) && DateTime.TryParse(headers["RequestDate"], out RequestDateTemp))
                    //{
                    //    RequestDateTemp = DateTime.Parse(headers["RequestDate"]);
                    //    RequestDate = RequestDateTemp;
                    //}
                    //DateTime? CreationDate = null;
                    //DateTime CreationDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(headers["CreationDate"]) && DateTime.TryParse(headers["CreationDate"], out CreationDateTemp))
                    //{
                    //    CreationDateTemp = DateTime.Parse(headers["CreationDate"]);
                    //    CreationDate = RequestDateTemp;
                    //}
                    #endregion


                    #region date Validation
                    DateTime RequestDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(filters.RequestDate))
                    {
                        if(!DateTime.TryParse(filters.RequestDate, out RequestDate))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-02";
                            error.ErrorMSG = "please Enter a valid Request Date";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    DateTime CreationDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(filters.CreationDate))
                    {
                        if (!DateTime.TryParse(filters.CreationDate, out CreationDate))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-02";
                            error.ErrorMSG = "please Enter a valid Creation Date";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    #endregion

                    var POType = _unitOfWork.PurchasePotypes.GetAll();
                    var PurchasePOQuerable = _unitOfWork.VPurchasePos.FindAllQueryable(x => x.Active == true).AsQueryable();
                    var V_PurchasePoItem = _unitOfWork.PurchasePOItems.FindAllQueryable(a => true);
                    var PurchasePOInvoicesDB = _unitOfWork.PurchasePOInvoices.FindAllQueryable(a => true);


                    if (!string.IsNullOrEmpty(filters.RequestDate))
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.RequestDate == RequestDate).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.CreationDate))
                    {
                        DateTime CreationDateSelected = (DateTime)CreationDate;
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.CreationDate.Year == CreationDateSelected.Year
                                                                        && x.CreationDate.Month == CreationDateSelected.Month
                                                                        && x.CreationDate.Day == CreationDateSelected.Day
                                                                        ).AsQueryable();
                    }
                    if (filters.InventoryItemID != null)
                    {
                        var IDPurchaseRequestID = V_PurchasePoItem.Where(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.PurchasePoid).Distinct().ToList();

                        PurchasePOQuerable = PurchasePOQuerable.Where(x => IDPurchaseRequestID.Contains(x.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {
                        string SearchKey = filters.SearchKey;
                        SearchKey = HttpUtility.UrlDecode(SearchKey);
                        //var IDPurchaseRequestID = V_PurchasePoItem.Where(x => (x.InventoryItem?.Name?.ToLower().Contains(SearchKey.ToLower())
                        //                                                            || x.InventoryItem?.Code?.ToLower().Contains(SearchKey.ToLower())) ??false).Select(x => x.InventoryItemID).Distinct().ToList();


                        //PurchasePOQuerable = PurchasePOQuerable.Where(x => IDPurchaseRequestID.Contains(x.ID)).AsQueryable();
                    }

                    if (!string.IsNullOrEmpty(filters.Status))
                    {
                        string Status = filters.Status;
                        Status = HttpUtility.UrlDecode(Status);
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.Status.ToLower() == Status.ToLower()).AsQueryable();
                    }

                    if (filters.HasAddingOrder != null)
                    {
                        
                        var ListPOID = _unitOfWork.VInventoryAddingOrderItems.FindAll(x => x.Poid != null).Select(x => x.Poid).Distinct().ToList();
                        if (filters.HasAddingOrder == true)
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => ListPOID.Contains(x.Id)).AsQueryable();
                        }
                        else
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => !ListPOID.Contains(x.Id)).AsQueryable();
                        }
                        
                    }
                    bool HasInvoice = false;
                    if (filters.HasInvoice != null)
                    {
                        var ListPOID = _unitOfWork.PurchasePOInvoices.FindAll(x => true).Select(b => b.Poid).Distinct().ToList();
                        if (filters.HasInvoice == true)
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => ListPOID.Contains(x.Id)).AsQueryable();
                        }
                        else
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => !ListPOID.Contains(x.Id)).AsQueryable();
                        }
                        
                    }
                    // Extra Filters 
                    if (!string.IsNullOrWhiteSpace(filters.SupplierInvoiceSerial))
                    {
                        string SupplierInvoiceSerial = filters.SupplierInvoiceSerial;
                        var ListPOID = V_PurchasePoItem.Where(x => x.SupplierInvoiceSerial == SupplierInvoiceSerial).Select(x => x.PurchasePoid).Distinct().ToList();
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => ListPOID.Contains(x.Id)).AsQueryable();
                    }
                    if (filters.IsSentToAcc != null)
                    {
                        
                        var ListPOID = PurchasePOInvoicesDB.Where(x => x.IsSentToAcc == true).Select(x => x.Poid).Distinct().ToList();
                        if (filters.IsSentToAcc == true)
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => ListPOID.Contains(x.Id)).AsQueryable();
                        }
                        else
                        {
                            PurchasePOQuerable = PurchasePOQuerable.Where(x => !ListPOID.Contains(x.Id)).AsQueryable();
                        }
                        
                    }
                    if (filters.CreatorUserID != null)
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.SupplierID != null)
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.ToSupplierId == filters.SupplierID).AsQueryable();
                    }

                    if (filters.POID != null)
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.Id == filters.POID).AsQueryable();
                    }
                    if (filters.POTypeID != null)
                    {
                        PurchasePOQuerable = PurchasePOQuerable.Where(x => x.PotypeId == filters.POTypeID).AsQueryable();
                    }

                    PurchasePOQuerable = PurchasePOQuerable.OrderBy(x => x.CreationDate);
                    if (!string.IsNullOrEmpty(filters.SortByASC))
                    {
                        if (filters.SortByASC.ToLower() == "date")
                        {
                            PurchasePOQuerable = PurchasePOQuerable.OrderBy(x => x.CreationDate);
                        }
                        else if (filters.SortByASC.ToLower() == "poid")
                        {
                            PurchasePOQuerable = PurchasePOQuerable.OrderBy(x => x.Id);
                        }

                    }
                    if (!string.IsNullOrEmpty(filters.SortByDESC))
                    {
                        if (filters.SortByASC.ToLower() == "date")
                        {
                            PurchasePOQuerable = PurchasePOQuerable.OrderByDescending(x => x.CreationDate);
                        }
                        else if (filters.SortByASC.ToLower() == "poid")
                        {
                            PurchasePOQuerable = PurchasePOQuerable.OrderByDescending(x => x.Id);
                        }

                    }

                    var PagingList = PagedList<VPurchasePo>.Create(PurchasePOQuerable.OrderByDescending(x => x.CreationDate), filters.CurrentPage, filters.NumberOfItemsPerPage);
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = PagingList.CurrentPage,
                        TotalPages = PagingList.TotalPages,
                        ItemsPerPage = PagingList.PageSize,
                        TotalItems = PagingList.TotalCount
                    };
                    var usersIds = PagingList.Select(a => a.CreatedBy).ToList();
                    var usersData = _unitOfWork.Users.FindAll(a => usersIds.Contains(a.Id));
                    foreach (var Data in PagingList)
                    {
                        var user = usersData.Where(a => a.Id == Data.CreatedBy).FirstOrDefault();
                        var FirstRejectedOffer = _unitOfWork.PRSupplierOffers.FindAll(a => a.Poid == Data.Id).Select(a => a.Id).FirstOrDefault();

                        var ObjPurchasePO = new PurchasePO();
                        ObjPurchasePO.ID = Data.Id;
                        ObjPurchasePO.IDEnc = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(Data.Id.ToString(), key));
                        ObjPurchasePO.FirstRejectedOffer = FirstRejectedOffer;
                        ObjPurchasePO.RequestDate = Data.RequestDate.ToShortDateString();
                        ObjPurchasePO.CreationDate = Data.CreationDate.ToShortDateString();
                        ObjPurchasePO.SupplierName = Data.SupplierName;
                        ObjPurchasePO.Status = Data.Status;
                        ObjPurchasePO.TotalEstimatedCost = Data.TotalEstimatedCost;
                        ObjPurchasePO.TechApprovalStatus = Data.TechApprovalStatus;
                        ObjPurchasePO.FinalApprovalStatus = Data.FinalApprovalStatus;
                        ObjPurchasePO.ApprovalStatus = Data.ApprovalStatus;
                        ObjPurchasePO.POTypeID = Data.PotypeId;
                        ObjPurchasePO.POTypeName = Data.PotypeId != null ? POType.Where(x => x.Id == Data.PotypeId).Select(x => x.TypeName).FirstOrDefault() : null;
                        ObjPurchasePO.SentToSupplier = Data.SentToSupplier;
                        ObjPurchasePO.AssignedPurchasingPersonID = Data.AssignedPurchasingPersonId;
                        ObjPurchasePO.CreatedByID = Data.CreatedBy;
                        ObjPurchasePO.NoOfRejected = _unitOfWork.PRSupplierOffers.FindAll(x => x.Poid == Data.Id && x.Status == "Rejected").Count();
                        ObjPurchasePO.CreatedByName = user.FirstName + " " + user.LastName;
                        if (HasInvoice)
                        {
                            ObjPurchasePO.HasInvoice = HasInvoice;
                        }
                        var POInvoiceObj = PurchasePOInvoicesDB.Where(x => x.Poid == Data.Id).FirstOrDefault();
                        var SupplierAccount = _unitOfWork.SupplierAccounts.FindAll(x => x.PurchasePoid == Data.Id && x.SupplierId == x.SupplierId);
                        ObjPurchasePO.POCollection = SupplierAccount.Count() > 0 ? SupplierAccount.Sum(x => x.Amount) : 0;
                        if (POInvoiceObj != null)
                        {
                            ObjPurchasePO.TotalInvoice = POInvoiceObj.TotalInvoiceCost;
                            ObjPurchasePO.TotalInvoicePrice = POInvoiceObj.TotalInvoicePrice;
                            ObjPurchasePO.POInvoiceId = POInvoiceObj.Id;
                        }
                        PurchasePOList.Add(ObjPurchasePO);
                    }
                    Response.SumTotalInvoiceCost = PurchasePOInvoicesDB.Where(x => x.TotalInvoiceCost != null).Sum(x => x.TotalInvoiceCost);
                    Response.SumTotalInvoicePrice = PurchasePOInvoicesDB.Where(x => x.TotalInvoiceCost != null).Sum(x => x.TotalInvoicePrice);

                    Response.PurchasePOList = PurchasePOList;

                }
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

        public async Task<ViewPurchaseOrderResponse> ViewPurchaseOrder(long? PoId, string SupplierInvoiceSerial)
        {
            ViewPurchaseOrderResponse Response = new ViewPurchaseOrderResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    // filter By POID or Supplier Invoice Serial

                    long POID = 0;

                    if (PoId != null)
                    {
                        POID = (long)PoId;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(SupplierInvoiceSerial))
                        { 
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err76";
                            error.ErrorMSG = "Invalid PO ID or Supplier Invoice Serial";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }




                    // Grouped by DAte as Inquiry 
                    //var InventoryMatrialAddingOrder


                    var POObjDB = await _unitOfWork.PurchasePOes.FindAsync(x => x.Active == true && x.Id == POID, new[] { "ToSupplier" });
                    if (POObjDB != null)
                    {
                        Response.PONumber = POID;
                        Response.RequestDate = POObjDB.RequestDate.ToShortDateString();
                        Response.CreationDate = POObjDB.CreationDate.ToShortDateString();
                        Response.Status = POObjDB.Status;
                        Response.AccountantApprovalStatus = POObjDB.ApprovalStatus;
                        Response.SupplierName = POObjDB.ToSupplier?.Name;
                        Response.SupplierId = POObjDB.ToSupplierId;

                               Response.POTypeName = "";
                        if (POObjDB.PotypeId == 1)
                        {
                            Response.POTypeName = "Local / Country";
                        }
                        else if (POObjDB.PotypeId == 2)
                        {
                            Response.POTypeName = "Import";
                        }

                    }
                    var POItemList = new List<PurchaseOrderItem>();
                    var POItemListDB = await _unitOfWork.PurchasePOItems.FindAllAsync(x => x.PurchasePoid == POID, new[] { "FabricationOrder", "Uom" , "Project.SalesOffer", "InventoryItem", "InventoryItem.PurchasingUom" });
                    foreach (var Data in POItemListDB)
                    {
                        var POItemObj = new PurchaseOrderItem();


                        string PRItemStoreName = "N/A";
                        var PurchaseRequestitemDB = await _unitOfWork.VPurchaseRequestItems.FindAsync(x => x.Id == Data.PurchaseRequestItemId);
                        if (PurchaseRequestitemDB != null)
                        {
                            var LoadObjDB = await _unitOfWork.VPurchaseRequests.FindAsync(x => x.Id == Data.PurchaseRequestItemId);
                            if (LoadObjDB != null)
                            {
                                if (LoadObjDB.FromInventoryStoreName != null && LoadObjDB.FromInventoryStoreName != "")
                                {
                                    PRItemStoreName = LoadObjDB.FromInventoryStoreName;
                                }
                                else
                                {
                                    PRItemStoreName = "N/A";
                                }
                            }
                        }

                        var InventoyItemObj = Data.InventoryItem;
                        decimal factor = InventoyItemObj?.ExchangeFactor1 != null ? (decimal)Data.InventoryItem?.ExchangeFactor1 : 0; // Data.ExchangeFactor;

                        POItemObj.ID = Data.Id;
                        POItemObj.FromStoreName = PRItemStoreName; // Common.GetPRItemStoreName(Data.PurchaseRequestItemID);
                        POItemObj.Comment = Data.Comments != null && Data.Comments != "" ? Data.Comments : "N/A";
                        POItemObj.InventoryItemID = Data.InventoryItemId;
                        POItemObj.InventoryItemCode = InventoyItemObj?.Code;
                        POItemObj.InventoryItemName = InventoyItemObj?.Name;
                        POItemObj.UOMID = Data.Uomid;
                        POItemObj.UOMShortName = Data.Uom?.ShortName;
                        POItemObj.FabricationOrderID = Data.FabricationOrderId;
                        POItemObj.FabricationOrderNumber = Data.FabricationOrder?.FabNumber?.ToString();  //!= null ? Data.FabNumber.ToString() : "N/A";
                        POItemObj.ProjectID = Data.ProjectId;
                        POItemObj.ProjectName = Data.Project?.SalesOffer?.ProjectName;  // Data.ProjectName != null ? Data.ProjectName.ToString() : "N/A";
                        POItemObj.RecivedQuantity = (decimal?)(Data.RecivedQuantity1 ?? 0);
                        POItemObj.RecivedQuantityUOP = POObjDB.InventoryAddingOrderItems.Where(x => x.InventoryItemId == Data.InventoryItemId && x.Poid == POID).FirstOrDefault()?.RecivedQuantityUop ?? 0;
                        POItemObj.ReqQuantity = Data.ReqQuantity1 ?? 0;
                        POItemObj.RemainQty = POItemObj.ReqQuantity - POItemObj.RecivedQuantity;
                        POItemObj.StockQTY = 0;
                        POItemObj.EstimatedCost = Data.EstimatedCost != null ? Data.EstimatedCost.ToString() : "N/A";
                        POItemObj.CurrencyID = Data.CurrencyId;
                        POItemObj.ConvertRateFromPurchasingToRequestionUnit = factor; // Data.InventoryItem?.ExchangeFactor; // Data.ExchangeFactor;
                        POItemObj.PurchasedUOMShortName = InventoyItemObj?.PurchasingUom.ShortName; // Data.PurchasingUOMShortName;

                        decimal requestionQTY = Data.ReqQuantity1 != null ? (decimal)Data.ReqQuantity1 : 0;
                        decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                        POItemObj.PurchasedQuantity = purchaseQTY;

                        // Get Comment From PRItem Comment 
                        if (PurchaseRequestitemDB != null)
                        {
                            POItemObj.PRItemComment = PurchaseRequestitemDB.Comments;
                        }

                        //Mark Shawky 2023/2/12
                        decimal? RateToEgp = 1;
                        if (Data.RateToEgp != null && Data.RateToEgp != 0)
                        {
                            RateToEgp = Data.RateToEgp;
                        }
                        List<long> InventoryAddingOrderItems =  _unitOfWork.VInventoryAddingOrderItems.FindAll(a => a.Poid == POID && a.InventoryItemId == POItemObj.InventoryItemID).Select(a => a.InventoryAddingOrderId).ToList();
                        POItemObj.MaterialAddingOrdersIds = InventoryAddingOrderItems;
                        POItemObj.InventoryMatrialRequestItemID = Data.InventoryMatrialRequestItemId;
                        POItemObj.InventoryMaterialRequestId = Data.InventoryMatrialRequestItem?.InventoryMatrialRequestId ?? 0; // await _Context.InventoryMatrialRequestItems.Where(a => a.ID == Data.InventoryMatrialRequestItemID).Select(a => a.InventoryMatrialRequestID).FirstOrDefaultAsync();
                        POItemObj.RateToEgp = RateToEgp;
                        POItemObj.PartNumber = InventoyItemObj?.PartNo; // await _Context.InventoryItems.Where(a => a.ID == POItemObj.InventoryItemID).Select(a => a.PartNO).FirstOrDefaultAsync();
                        POItemObj.RequstionUOMID = InventoyItemObj?.RequstionUomid;  //Data.RequstionUOMID;
                        POItemObj.RequstionUOMShortName = InventoyItemObj?.RequstionUom?.ShortName;  // Data.RequstionUOMShortName;
                        POItemObj.PurchasingUOMID = InventoyItemObj?.PurchasingUomid; //Data.PurchasingUOMID;
                        POItemObj.PurchasingUOMShortName = POItemObj.PurchasedUOMShortName;
                        POItemObj.ActualLocalUnitPrice = Data.ActualUnitPrice;
                        POItemObj.ActualUnitPrice = (Data.ActualUnitPrice ?? 0) / RateToEgp;
                        POItemObj.ActualLocalUnitPriceUOR = Data.ActualUnitPrice;
                        POItemObj.ActualUnitPriceUOR = (Data.ActualUnitPrice ?? 0) / RateToEgp;
                        POItemObj.TotalLocalActualPrice = (Data.ActualUnitPrice ?? 0) * ((decimal)(Data.ReqQuantity1 ?? 0));
                        POItemObj.TotalActualPrice = (POItemObj.TotalLocalActualPrice ?? 0) / RateToEgp;
                        POItemObj.FinalLocalUnitCostUOR = Data.FinalUnitCost;
                        POItemObj.FinalUnitCostUOR = (Data.FinalUnitCost ?? 0) / RateToEgp;
                        POItemObj.TotalFinalLocalUnitCost = (Data.FinalUnitCost ?? 0) * ((decimal)(Data.ReqQuantity1 ?? 0));
                        POItemObj.TotalFinalUnitCost = (Data.FinalUnitCost ?? 0) * ((decimal)(Data.ReqQuantity1 ?? 0)) / RateToEgp;
                        POItemObj.TotalActualPrice = Data.TotalActualPrice / RateToEgp;
                        POItemObj.InvoiceComments = Data.InvoiceComments;
                        POItemObj.CurrencyName = Data.Currency?.Name; // Common.GetCurrencyName(Data.CurrencyID ?? 0);
                        POItemObj.ActualUnitPriceUnit = Data.ActualUnitPriceUnit ?? POItemObj.RequstionUOMShortName;
                        POItemObj.IsChecked = Data.IsChecked;
                        POItemObj.SupplierInvoiceSerial = Data.SupplierInvoiceSerial;
                        POItemList.Add(POItemObj);
                    }

                    Response.PurchasePOItemList = POItemList;

                }
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

        public async Task<GetPurchaseItemListResponse> GetPurchaseItemList(string SupplierInvoiceSerial)
        {
            GetPurchaseItemListResponse Response = new GetPurchaseItemListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrWhiteSpace(SupplierInvoiceSerial))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "Invalid Supplier Invoice Serial";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var POItemListDB = await _unitOfWork.PurchasePOItems.FindAllAsync(x => x.SupplierInvoiceSerial == SupplierInvoiceSerial, new[] { "FabricationOrder", "Uom", "Project.SalesOffer", "InventoryItem", "InventoryItem.PurchasingUom" });
                    var POIDLIst = POItemListDB.Select(x => x.PurchasePoid).ToList();
                    var InventoryAddingOrderItemsList = _unitOfWork.InventoryAddingOrderItems.FindAll(x => POIDLIst.Contains(x.Poid ?? 0)).ToList();
                    var POItemList = new List<PurchaseOrderItem>();
                    foreach (var Data in POItemListDB)
                    {
                        var POItemObj = new PurchaseOrderItem();


                        string PRItemStoreName = "N/A";
                        var PurchaseRequestitemDB = await _unitOfWork.VPurchaseRequestItems.FindAsync(x => x.Id == Data.PurchaseRequestItemId);
                        if (PurchaseRequestitemDB != null)
                        {
                            var LoadObjDB = await _unitOfWork.VPurchaseRequests.FindAsync(x => x.Id == Data.PurchaseRequestItemId);
                            if (LoadObjDB != null)
                            {
                                if (LoadObjDB.FromInventoryStoreName != null && LoadObjDB.FromInventoryStoreName != "")
                                {
                                    PRItemStoreName = LoadObjDB.FromInventoryStoreName;
                                }
                                else
                                {
                                    PRItemStoreName = "N/A";
                                }
                            }
                        }

                        var InventoyItemObj = Data.InventoryItem;
                        decimal factor = InventoyItemObj?.ExchangeFactor != null ? (decimal)Data.InventoryItem?.ExchangeFactor : 0; // Data.ExchangeFactor;

                        POItemObj.ID = Data.Id;
                        POItemObj.FromStoreName = PRItemStoreName; // Common.GetPRItemStoreName(Data.PurchaseRequestItemID);
                        POItemObj.Comment = Data.Comments != null && Data.Comments != "" ? Data.Comments : "N/A";
                        POItemObj.InventoryItemID = Data.InventoryItemId;
                        POItemObj.InventoryItemCode = InventoyItemObj?.Code;
                        POItemObj.InventoryItemName = InventoyItemObj?.Name;
                        POItemObj.UOMID = Data.Uomid;
                        POItemObj.UOMShortName = Data.Uom?.ShortName;
                        POItemObj.FabricationOrderID = Data.FabricationOrderId;
                        POItemObj.FabricationOrderNumber = Data.FabricationOrder?.FabNumber?.ToString();  //!= null ? Data.FabNumber.ToString() : "N/A";
                        POItemObj.ProjectID = Data.ProjectId;
                        POItemObj.ProjectName = Data.Project?.SalesOffer?.ProjectName;  // Data.ProjectName != null ? Data.ProjectName.ToString() : "N/A";
                        POItemObj.RecivedQuantity =(Data.RecivedQuantity1 ?? 0);
                        POItemObj.RecivedQuantityUOP = InventoryAddingOrderItemsList.Where(x => x.InventoryItemId == Data.InventoryItemId && x.Poid == Data.PurchasePoid).FirstOrDefault()?.RecivedQuantityUop ?? 0;
                        POItemObj.ReqQuantity = (Data.ReqQuantity1 ?? 0);
                        POItemObj.RemainQty = POItemObj.ReqQuantity - POItemObj.RecivedQuantity;
                        POItemObj.StockQTY = 0;
                        POItemObj.EstimatedCost = Data.EstimatedCost != null ? Data.EstimatedCost.ToString() : "N/A";
                        POItemObj.CurrencyID = Data.CurrencyId;
                        POItemObj.ConvertRateFromPurchasingToRequestionUnit = factor; // Data.InventoryItem?.ExchangeFactor; // Data.ExchangeFactor;
                        POItemObj.PurchasedUOMShortName = InventoyItemObj?.PurchasingUom.ShortName; // Data.PurchasingUOMShortName;

                        decimal requestionQTY = Data.ReqQuantity1 != null ? (decimal)Data.ReqQuantity1 : 0;
                        decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                        POItemObj.PurchasedQuantity = purchaseQTY;

                        // Get Comment From PRItem Comment 
                        if (PurchaseRequestitemDB != null)
                        {
                            POItemObj.PRItemComment = PurchaseRequestitemDB.Comments;
                        }

                        //Mark Shawky 2023/2/12
                        decimal? RateToEgp = 1;
                        if (Data.RateToEgp != null && Data.RateToEgp != 0)
                        {
                            RateToEgp = Data.RateToEgp;
                        }
                        List<long> InventoryAddingOrderItems = InventoryAddingOrderItemsList.Where(a => a.Poid == Data.PurchasePoid && a.InventoryItemId == POItemObj.InventoryItemID).Select(a => a.InventoryAddingOrderId).ToList();
                        POItemObj.MaterialAddingOrdersIds = InventoryAddingOrderItems;
                        POItemObj.InventoryMatrialRequestItemID = Data.InventoryMatrialRequestItemId;
                        POItemObj.InventoryMaterialRequestId = Data.InventoryMatrialRequestItem?.InventoryMatrialRequestId ?? 0; // await _Context.InventoryMatrialRequestItems.Where(a => a.ID == Data.InventoryMatrialRequestItemID).Select(a => a.InventoryMatrialRequestID).FirstOrDefaultAsync();
                        POItemObj.RateToEgp = RateToEgp;
                        POItemObj.PartNumber = InventoyItemObj?.PartNo; // await _Context.InventoryItems.Where(a => a.ID == POItemObj.InventoryItemID).Select(a => a.PartNO).FirstOrDefaultAsync();
                        POItemObj.RequstionUOMID = InventoyItemObj?.RequstionUomid;  //Data.RequstionUOMID;
                        POItemObj.RequstionUOMShortName = InventoyItemObj?.RequstionUom?.ShortName;  // Data.RequstionUOMShortName;
                        POItemObj.PurchasingUOMID = InventoyItemObj?.PurchasingUomid; //Data.PurchasingUOMID;
                        POItemObj.PurchasingUOMShortName = POItemObj.PurchasedUOMShortName;
                        POItemObj.ActualLocalUnitPrice = Data.ActualUnitPrice;
                        POItemObj.ActualUnitPrice = (Data.ActualUnitPrice ?? 0) / RateToEgp;
                        POItemObj.ActualLocalUnitPriceUOR = Data.ActualUnitPrice;
                        POItemObj.ActualUnitPriceUOR = (Data.ActualUnitPrice ?? 0) / RateToEgp;
                        POItemObj.TotalLocalActualPrice = (Data.ActualUnitPrice ?? 0) * ((decimal?)(Data.ReqQuantity1 ?? 0));
                        POItemObj.TotalActualPrice = (POItemObj.TotalLocalActualPrice ?? 0) / RateToEgp;
                        POItemObj.FinalLocalUnitCostUOR = Data.FinalUnitCost;
                        POItemObj.FinalUnitCostUOR = (Data.FinalUnitCost ?? 0) / RateToEgp;
                        POItemObj.TotalFinalLocalUnitCost = (Data.FinalUnitCost ?? 0) * ((decimal?)(Data.ReqQuantity1 ?? 0));
                        POItemObj.TotalFinalUnitCost = (Data.FinalUnitCost ?? 0) * ((decimal?)(Data.ReqQuantity1 ?? 0)) / RateToEgp;
                        POItemObj.TotalActualPrice = Data.TotalActualPrice / RateToEgp;
                        POItemObj.InvoiceComments = Data.InvoiceComments;
                        POItemObj.CurrencyName = Data.Currency?.Name; // Common.GetCurrencyName(Data.CurrencyID ?? 0);
                        POItemObj.ActualUnitPriceUnit = Data.ActualUnitPriceUnit ?? POItemObj.RequstionUOMShortName;
                        POItemObj.IsChecked = Data.IsChecked;
                        POItemObj.SupplierInvoiceSerial = Data.SupplierInvoiceSerial;
                        POItemList.Add(POItemObj);
                    }

                    Response.PurchasePOItemList = POItemList;

                }
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

        public InventoryItemMatrialReleaseInfoResponse GetItemsForCreatePurchaseRequest(long MatrialRequestID, long UserID)
        {
            InventoryItemMatrialReleaseInfoResponse Response = new InventoryItemMatrialReleaseInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var MatrialRequestInfoObj = new InventoryItemMatrialReleaseInfo();
                var MatrialRequestItemsInfoList = new List<MatrialReleaseItemInfo>();
                if (Response.Result)
                {
                    if(MatrialRequestID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err109";
                        error.ErrorMSG = "Invalid Matrial Request Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Response.Result)
                    {


                        var MatrialRequestOBJDB = _unitOfWork.VInventoryMatrialRequests.Find(x => x.Id == MatrialRequestID);

                        if (MatrialRequestOBJDB != null)
                        {
                            if (!CheckISInventoryKeeper(MatrialRequestOBJDB.ToInventoryStoreId, UserID))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err309";
                                error.ErrorMSG = "This User is not store keeper";
                                Response.Errors.Add(error);
                                return Response;
                            }

                            if (MatrialRequestOBJDB.Status == "Open")
                            {
                                MatrialRequestInfoObj.ID = MatrialRequestOBJDB.Id;
                                MatrialRequestInfoObj.InventoryMatrialRequestOrderID = MatrialRequestOBJDB.Id;
                                MatrialRequestInfoObj.ToUserName = MatrialRequestOBJDB.FromUserName;
                                MatrialRequestInfoObj.FromStore = MatrialRequestOBJDB.ToInventoryStoreName;
                                MatrialRequestInfoObj.FromStoreId = MatrialRequestOBJDB.ToInventoryStoreId;
                                MatrialRequestInfoObj.RequestDate = MatrialRequestOBJDB.RequestDate.ToShortDateString();
                                // Get List Of Items
                                var MatrialReleaseItemListDB = _unitOfWork.VInventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialRequestOBJDB.Id).ToList();
                                var IDSInvMatrialRequestItems = MatrialReleaseItemListDB.Select(x => new { x.InventoryItemId, x.ToInventoryStoreId }).ToList();
                                var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSInvMatrialRequestItems.Select(y => y.InventoryItemId).Contains(x.InventoryItemId) && IDSInvMatrialRequestItems.Select(y => y.ToInventoryStoreId).Contains(x.InventoryStoreId)).ToList();
                                if (MatrialReleaseItemListDB.Count() > 0)
                                {
                                    foreach (var item in MatrialReleaseItemListDB)
                                    {

                                        decimal Balance = GetInventoryStoreItemBalance(InventoryStoreItemList, MatrialRequestOBJDB.ToInventoryStoreId, item.InventoryItemId);
                                        decimal ReqQuantity = item.ReqQuantity ?? 0;
                                        decimal RecivedQuantity = item.RecivedQuantity ?? 0;
                                        decimal PurchaseQuantity = item.PurchaseQuantity ?? 0;
                                        decimal Remain = ReqQuantity - RecivedQuantity - PurchaseQuantity;
                                        if (Remain > 0 && ((decimal)Remain) > Balance)
                                        {

                                            var ItemObjDB = new MatrialReleaseItemInfo();
                                            ItemObjDB.ID = 0;
                                            ItemObjDB.Comment = item.Comments;
                                            ItemObjDB.InventoryItemID = item.InventoryItemId;
                                            ItemObjDB.ItemCode = item.ItemCode;
                                            ItemObjDB.ItemName = item.ItemName;
                                            ItemObjDB.ReqQTY = item.ReqQuantity;
                                            ItemObjDB.RecivedQTY = item.RecivedQuantity;
                                            ItemObjDB.UOM = item.RequestedUomshortName;
                                            ItemObjDB.FabOrderName = item.FabNumber.ToString();
                                            ItemObjDB.ProjectName = item.ProjectName;
                                            ItemObjDB.RemQTY = ((((decimal)Remain) - Balance) / (decimal)item.ExchangeFactor); ;
                                            ItemObjDB.MatrialRequestItemID = item.Id;
                                            ItemObjDB.NewComment = "";
                                            //ItemObjDB.NewRecivedQTY = item.NewRecivedQuantity;


                                            //decimal balance = Common.GetInventoryStoreItemBalance(InventoryMatrialRelease.FromInventoryStoreID, item.InventoryItemID);
                                            //ItemObjDB.StockQTY = balance;

                                            MatrialRequestItemsInfoList.Add(ItemObjDB);
                                        }
                                    }
                                }


                                ///////////////////////
                            }
                            else
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err460";
                                error.ErrorMSG = "the Request was already " + MatrialRequestOBJDB.Status;
                                Response.Errors.Add(error);
                                return Response;
                            }


                        }

                        MatrialRequestInfoObj.MatrialReleaseInfoList = MatrialRequestItemsInfoList;

                    }
                    Response.InventoryItemInfo = MatrialRequestInfoObj;


                }
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

        public bool CheckISInventoryKeeper(int InventoryStoreID, long userID)
        {
            bool result = false;
            var InventoryStoreKeeperDB = _unitOfWork.VInventoryStoreKeepers.Find(x => x.InventoryStoreId == InventoryStoreID && x.UserId == userID && x.Active == true && x.StoreActive == true);
            if (InventoryStoreKeeperDB != null)
            {
                result = true;
            }
            return result;
        }

        public static decimal GetInventoryStoreItemBalance(List<InventoryStoreItem> InventoryStoreItemList, int? InventoryStoreID, long? inventoryItemID)
        {
            decimal result = 0;
            if (InventoryStoreID != null && InventoryStoreID != 0 && inventoryItemID != null && inventoryItemID != 0)
            {
                result = (decimal)InventoryStoreItemList.Where(x => x.InventoryStoreId == InventoryStoreID && x.InventoryItemId == inventoryItemID).Select(x => x.Balance).DefaultIfEmpty(0).Sum();
                //result = _Context.proc_InventoryStoreItemLoadAll().Where(x => x.InventoryStoreID == InventoryStoreID && x.InventoryItemID == inventoryItemID).GroupBy(x => x.InventoryItemID).Select(x => x.Sum(a => a.Balance)).FirstOrDefault();
            }
            //InventoryItem item = new InventoryItem();
            //if (item.LoadByPrimaryKey(inventoryItemID))
            //{
            //    GarasERP.InventoryStoreItem StItem = new GarasERP.InventoryStoreItem();
            //    StItem.Where.InventoryStoreID.Value = InventoryStoreID;
            //    StItem.Where.InventoryItemID.Value = inventoryItemID;
            //    StItem.Query.AddGroupBy(GarasERP.InventoryStoreItem.ColumnNames.InventoryItemID);
            //    StItem.Aggregate.Balance.Function = MyGeneration.dOOdads.AggregateParameter.Func.Sum;
            //    if (StItem.Query.Load())
            //    {
            //        if (StItem.DefaultView != null && StItem.DefaultView.Count > 0)
            //        {
            //            if (StItem.s_Balance != "" && item.s_ExchangeFactor != "")
            //            {
            //                result = StItem.Balance /** decimal.Parse(item.ExchangeFactor.ToString())*/;
            //            }
            //        }
            //    }
            //}
            return result;
        }

        public BaseResponseWithID AddInventoryItemPurchaseRequest(AddInventoryItemMatrialReleaseRequest Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    #region Validations
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long MatrialRequestOrderId = 0;
                    if (Request.MatrialRequestOrderId != null)
                    {
                        MatrialRequestOrderId = (long)Request.MatrialRequestOrderId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err325";
                        error.ErrorMSG = "Invalid Matrial Request Order Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime RequestDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RequestDate) || !DateTime.TryParse(Request.RequestDate, out RequestDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Request Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.MatrialReleaseItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-475";
                        error.ErrorMSG = "please insert at least one Purchase Request Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.MatrialReleaseItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-475";
                        error.ErrorMSG = "please insert at least one Purchase Request Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.MatrialReleaseItemList.Where(x => x.NewRecivedQTY == null).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-416";
                        error.ErrorMSG = "New Recived Qty required for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.MatrialReleaseItemList.Where(x => x.NewRecivedQTY > x.RemQTY).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-416";
                        error.ErrorMSG = "New Recived Qty must be less than remain Qty for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.MatrialReleaseItemList.Where(x => x.MatrialRequestItemID == 0).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-417";
                        error.ErrorMSG = "Matrial Request Item ID required for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    #endregion

                    if (Response.Result)
                    {
                        Response.ID = MatrialRequestOrderId;
                        string Status = "";
                        var MatrialRequestOrderDB = _unitOfWork.VInventoryMatrialRequests.Find(x => x.Id == MatrialRequestOrderId);
                        if (MatrialRequestOrderDB != null)
                        {
                            if (MatrialRequestOrderDB.Status == "Closed")
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err328";
                                error.ErrorMSG = "Matrial Request Order Id is already closed.";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            //ObjectParameter IDPR = new ObjectParameter("ID", typeof(long));
                            //var PRInsert = _Context.proc_PurchaseRequestInsert(IDPR,
                            //                                                          null,
                            //                                                          MatrialRequestOrderDB.ToInventoryStoreID,
                            //                                                          RequestDate,
                            //                                                          DateTime.Now,
                            //                                                          validation.userID,
                            //                                                          DateTime.Now,
                            //                                                          validation.userID,
                            //                                                          true,
                            //                                                          "Open",
                            //                                                          MatrialRequestOrderDB.ID,
                            //                                                          null,
                            //                                                          "Waiting For Reply",
                            //                                                          null,
                            //                                                          null,
                            //                                                          null
                            //                                                          );
                            var PRInsert = new PurchaseRequest();
                            PRInsert.FromInventoryStoreId = MatrialRequestOrderDB.ToInventoryStoreId;
                            PRInsert.RequestDate = RequestDate;
                            PRInsert.CreationDate = DateTime.Now;
                            PRInsert.CreatedBy = UserID;
                            PRInsert.ModifiedDate = DateTime.Now;
                            PRInsert.ModifiedBy = UserID;
                            PRInsert.Active = true;
                            PRInsert.Status = "Open";
                            PRInsert.MatrialRequestId = MatrialRequestOrderDB.Id;
                            PRInsert.IsDirectPr = null;
                            PRInsert.ApprovalStatus = "Waiting For Reply";

                            _unitOfWork.PurchaseRequests.Add(PRInsert);
                            _unitOfWork.Complete();

                            if (PRInsert != null)
                            {
                                var PRID = PRInsert.Id;
                                // Create Task


                                // Insert Items
                                foreach (var item in Request.MatrialReleaseItemList)
                                {
                                    var MatrialRequestItemObjDB = _unitOfWork.VInventoryMatrialRequestItems.Find(x => x.Id == item.MatrialRequestItemID);
                                    if (MatrialRequestItemObjDB != null)
                                    {
                                        //ObjectParameter IDItemPR = new ObjectParameter("ID", typeof(long));
                                        //_Context.Myproc_PurchaseRequestItemsInsert(IDItemPR,
                                        //                                           PRID,
                                        //                                           MatrialRequestItemObjDB.Comments,
                                        //                                           MatrialRequestItemObjDB.Id,
                                        //                                           item.NewComment, //PurchaseRequestNotes
                                        //                                           null,
                                        //                                           item.NewRecivedQTY,
                                        //                                           0,
                                        //                                           null
                                        //                                           );

                                        var PRItem = new PurchaseRequestItem();
                                        PRItem.PurchaseRequestId = PRID;
                                        PRItem.Comments = MatrialRequestItemObjDB.Comments;
                                        PRItem.InventoryMatrialRequestItemId = MatrialRequestItemObjDB.Id;
                                        PRItem.PurchaseRequestNotes = item.NewComment;
                                        PRItem.Quantity1 = item.NewRecivedQTY;
                                        PRItem.PurchasedQuantity1 = 0;

                                        _unitOfWork.PurchaseRequestItems.Add(PRItem);
                                        _unitOfWork.Complete();
                                        // Update Purchase QTY for matrial request item
                                        var InventoryMatrialRequestItem = _unitOfWork.InventoryMatrialRequestItems.Find(a => a.Id == MatrialRequestItemObjDB.Id);

                                        if(InventoryMatrialRequestItem != null)
                                        {
                                            InventoryMatrialRequestItem.PurchaseQuantity1 = MatrialRequestItemObjDB.PurchaseQuantity + item.NewRecivedQTY;
                                            _unitOfWork.Complete();
                                        }
                                       // _Context.Myproc_InventoryMatrialRequestItemsUpdate_PurchaseQuantity(MatrialRequestItemObjDB.Id, MatrialRequestItemObjDB.PurchaseQuantity + item.NewRecivedQTY);

                                    }
                                }


                            }


                        }
                        else // Invalid Matrial Request ID
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err325";
                            error.ErrorMSG = "Invalid Matrial Request Order Id.";
                            Response.Errors.Add(error);
                            return Response;
                        }


                    }
                }
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

        public async Task<BaseResponseWithID> ManagePurchaseRequestItem(ManagePurchaseRequestItemRequest Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    Response = await ManagePRItemCall(Request, UserID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        private async Task<BaseResponseWithID> ManagePRItemCall(ManagePurchaseRequestItemRequest Request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();

            //check sent data
            if (Request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err-P12";
                error.ErrorMSG = "please insert a valid data.";
                Response.Errors.Add(error);
                return Response;
            }


            long PRItemID = 0;
            if (Request.Id != 0)
            {
                PRItemID = (long)Request.Id;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err225";
                error.ErrorMSG = "Id is required.";
                Response.Errors.Add(error);
                return Response;
            }
            bool IsDelete = false;
            if (Request.IsDelete != null)
            {
                IsDelete = (bool)Request.IsDelete;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err43";
                error.ErrorMSG = "IsDelete is required.";
                Response.Errors.Add(error);
                return Response;
            }

            // In first must be Validate
            // 
            var PRItemOldDB = await _unitOfWork.PurchaseRequestItems.FindAsync(x => x.Id == PRItemID);
            if (PRItemOldDB == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err433";
                error.ErrorMSG = "Id is not exist.";
                Response.Errors.Add(error);
                return Response;
            }
            // 1- PR Status must be Open
            var CheckPRDB = await _unitOfWork.PurchaseRequests.FindAsync(x => x.Id == PRItemOldDB.PurchaseRequestId);
            if (CheckPRDB != null)
            {
                // if call from manage PO .. not need check is closed or open ,, because it just replace item with same Status
                if (Request.ManagedbyPO != true)
                {
                    if (CheckPRDB.Status == "Closed")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err433";
                        error.ErrorMSG = "Can't manage this PR Item becasue This PR Status is Closed.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err433";
                error.ErrorMSG = "Id is not exist on PR.";
                Response.Errors.Add(error);
                return Response;
            }
            // 2- PO Not Releate by this PR
            var CheckPOItemDB = await _unitOfWork.PurchasePOItems.FindAsync(x => x.PurchaseRequestItemId == PRItemID);
            if (Request.ManagedbyPO != true && Request.InventoryItemId != 0 && Request.InventoryItemId != null)
            {
                if (CheckPOItemDB != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err433";
                    error.ErrorMSG = "Can't manage This PR Item becasue already releated on PO Item.";
                    Response.Errors.Add(error);
                    return Response;
                }
            }

            if (Response.Result)
            {

                long NewMRItemID = 0;
                decimal NewPRItemQTY = 0;
                // Note if PR is Direct must be update changes on Matrial Request Item as PR Item

                var MatrialRequestItemDB = await _unitOfWork.InventoryMatrialRequestItems.FindAsync(x => x.Id == PRItemOldDB.InventoryMatrialRequestItemId);
                if (IsDelete)
                {
                    // Case 1 : Delete PR Item

                    _unitOfWork.PurchaseRequestItems.Delete(PRItemOldDB);
                    if (CheckPRDB.IsDirectPr == true)
                    {
                        // if Direct PR Delete his matrial request item
                        if (MatrialRequestItemDB != null)
                        {
                            _unitOfWork.InventoryMatrialRequestItems.Delete(MatrialRequestItemDB);
                        }
                    }
                }
                else
                {
                    // Case 2 : manage Reason or Qty
                    if (Request.Quantity != null)
                    {
                        PRItemOldDB.Quantity = (double?)Request.Quantity;
                        if (CheckPRDB.IsDirectPr == true)
                        {
                            MatrialRequestItemDB.PurchaseQuantity = (double?)Request.Quantity;
                        }
                    }

                    if (!string.IsNullOrEmpty(Request.Reason))
                    {
                        PRItemOldDB.PurchaseRequestNotes = Request.Reason;   // Change this assign in another apies save notes
                    }

                    // Case 3 : is not direct PR
                    // manage PRItem on Change  InventoryItemID
                    if (Request.InventoryItemId != 0 && Request.InventoryItemId != null)
                    {
                        // check InventoryItem is valid
                        var CheckInvItemDB = await _unitOfWork.InventoryItems.FindAsync(x => x.Id == Request.InventoryItemId);
                        if (CheckInvItemDB != null)
                        {
                            if (CheckPRDB.IsDirectPr == true) // Direct PR
                            {
                                MatrialRequestItemDB.InventoryItemId = (long)Request.InventoryItemId;
                            }
                            else
                            {
                                // 1-add this item to matrial Request item for the same Matrial Request with comment "added by pr item instead of another item (old item)"
                                var MRItemObj = new InventoryMatrialRequestItem();
                                MRItemObj.InventoryMatrialRequestId = MatrialRequestItemDB.InventoryMatrialRequestId;
                                MRItemObj.Uomid = MatrialRequestItemDB.Uomid;
                                MRItemObj.Comments = "Created automatic by PR NO " + PRItemOldDB.PurchaseRequestId +
                                                     " Instead of Requested Item " + CheckInvItemDB.Name + " in MR NO" + MatrialRequestItemDB.InventoryMatrialRequestId;

                                MRItemObj.InventoryItemId = (long)Request.InventoryItemId;
                                MRItemObj.RecivedQuantity = 0;
                                if (Request.Quantity != null)
                                {
                                    MRItemObj.ReqQuantity1 = Request.Quantity;
                                    MRItemObj.PurchaseQuantity1 = Request.Quantity;
                                }
                                else
                                {
                                    MRItemObj.ReqQuantity1 = (decimal?)MatrialRequestItemDB.ReqQuantity;
                                    MRItemObj.PurchaseQuantity1 = (decimal?)MatrialRequestItemDB.PurchaseQuantity;
                                }
                                MRItemObj.FromBom = MatrialRequestItemDB.FromBom;

                                _unitOfWork.InventoryMatrialRequestItems.Add(MRItemObj);
                                NewMRItemID = MRItemObj.Id;
                                //// 2- add new PRItem with new matrial Request Item
                                //DAL.Model.PurchaseRequestItem PRItemObj = new PurchaseRequestItem();
                                //PRItemObj.PurchaseRequestID = PRItemOldDB.PurchaseRequestID;
                                //PRItemObj.Comments = MRItemObj.Comments;
                                //PRItemObj.InventoryMatrialRequestItemID = NewMRItemID;
                                //if (!string.IsNullOrEmpty(Request.Reason))
                                //{
                                //    PRItemObj.PurchaseRequestNotes = Request.Reason;
                                //}
                                //PRItemObj.Quantity = MRItemObj.ReqQuantity;
                                //PRItemObj.PurchasedQuantity = 0;

                                //_Context.PurchaseRequestItems.Add(PRItemObj);


                                // Update Old PR Item with new inventory item with new MR Item
                                PRItemOldDB.Comments = MRItemObj.Comments;
                                PRItemOldDB.InventoryMatrialRequestItemId = NewMRItemID;
                                if (!string.IsNullOrEmpty(Request.Reason))
                                {
                                    PRItemOldDB.PurchaseRequestNotes = Request.Reason;
                                }
                                PRItemOldDB.Quantity = (double?)MRItemObj.ReqQuantity1;
                                PRItemOldDB.PurchasedQuantity = 0;





                                NewPRItemQTY = (decimal?)MRItemObj.ReqQuantity1 ?? 0;
                                //NewPRItemID = PRItemObj.ID;

                                //// if managed from PO to change Item 
                                //// - Delete Old POItem first 
                                //// - Then Delete PRItem 
                                //if (Request.ManagedbyPO == true) 
                                //{

                                //}
                                //// 3- Remove Old PRItem that manage on it
                                //_Context.PurchaseRequestItems.Remove(PRItemOldDB);



                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err130";
                            error.ErrorMSG = "Invalid Inventory Item";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }


                }
                //Part on PR Supplier Offer Item
                // check in first have PRSupplier
                var PRSupplierOfferItemList = await _unitOfWork.PRSupplierOfferItems.FindAllAsync(x => x.PritemId == PRItemID);
                if (PRSupplierOfferItemList != null)
                {
                    if (PRSupplierOfferItemList.Count() > 0)
                    {
                        if (Request.IsDelete == true)
                        {
                            // in Delete PR item .. Remove from PR supplier Offer  

                            _unitOfWork.PRSupplierOfferItems.DeleteRange(PRSupplierOfferItemList);
                        }
                        // in update item in PR Item Remove Old PRItems then add new PRItem for All Supplier
                        else // Delete Old PRItem and Create New PRItem 
                        {
                            if ((Request.InventoryItemId != null && NewMRItemID != 0) || Request.Quantity != null)
                            {
                                foreach (var PRSupplierOfferObjDB in PRSupplierOfferItemList) // New 
                                {
                                    if ((Request.InventoryItemId != null && NewMRItemID != 0))
                                    {
                                        //if called from PO set POItem == null for temp until update on POItem with new POItem
                                        //if (Request.ManagedbyPO == true)
                                        //{
                                        //    PRSupplierOfferObjDB.POItemID = item.POItemID;
                                        //}
                                        PRSupplierOfferObjDB.MritemId = NewMRItemID;
                                        PRSupplierOfferObjDB.ReqQuantity = NewPRItemQTY;
                                        PRSupplierOfferObjDB.RecivedQuantity = 0;
                                        PRSupplierOfferObjDB.EstimatedCost = 0;
                                        PRSupplierOfferObjDB.TotalEstimatedCost = 0;

                                    }
                                    else if (Request.Quantity != null)
                                    {
                                        PRSupplierOfferObjDB.ReqQuantity = (decimal?)Request.Quantity;
                                        PRSupplierOfferObjDB.TotalEstimatedCost = PRSupplierOfferObjDB.EstimatedCost * (decimal)Request.Quantity;
                                    }
                                }

                                //if ((Request.InventoryItemId != null && NewPRItemID != 0 && NewMRItemID != 0))
                                //{
                                //    _Context.PRSupplierOfferItems.RemoveRange(PRSupplierOfferItemList);
                                //}
                            }
                        }
                    }
                }




                var Res =  _unitOfWork.Complete();
                // Check if PR Items is deleted all -> delete PR
                var CheckPRItemListCount = await _unitOfWork.PurchaseRequestItems.CountAsync(x => x.PurchaseRequestId == PRItemOldDB.PurchaseRequestId);
                if (CheckPRItemListCount == 0)
                {
                    _unitOfWork.PurchaseRequests.Delete(CheckPRDB);

                    await UpdatePRStatus(PRItemOldDB.PurchaseRequestId);

                    _unitOfWork.Complete();
                }
                if (Res > 0)
                {
                    Response.ID = PRItemID;
                    Response.Result = true;
                }
                else
                {
                    Response.Result = false;
                }
            }


            return Response;
        }


        public async Task<bool> UpdatePRStatus(long PurchaseRequestID)
        {
            var Res = false;
            if (PurchaseRequestID != 0)
            {
                var PurchaseRequestItmesListDB = await _unitOfWork.PurchaseRequestItems.FindAllAsync(x => x.PurchaseRequestId == PurchaseRequestID);
                #region Update PR Status Closed 
                var PRItemCountDB = PurchaseRequestItmesListDB.Count();
                var PRItemCountPurchasedDB = PurchaseRequestItmesListDB.Where(x => x.PurchasedQuantity >= x.Quantity).Count();
                // Check if all Qty is Purchased 
                if (PRItemCountPurchasedDB >= PRItemCountDB)
                {
                    var PRObjDB = await _unitOfWork.PurchaseRequests.FindAsync(x => x.Id == PurchaseRequestID);
                    PRObjDB.Status = "Closed";
                }
                Res = true;
                #endregion
            }
            return Res;
        }

    }
}
