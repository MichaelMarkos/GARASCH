using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.ProjectFabrication.Headers;
using NewGarasAPI.Models.ProjectFabrication.Responses;
using NewGarasAPI.Models.ProjectFabrication.UsedInResponses;
using System.Collections.Generic;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.Project
{
    [Route("/[controller]")]
    [ApiController]
    public class ProjectFabricationController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly ITenantService _tenantService;
        public ProjectFabricationController(ITenantService tenantService)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
        }

        [HttpGet("GetFabricationOrdersCards")]
        public FabticationOrderCardsResponse GetFabricationOrdersCards([FromHeader] GetInstallatioAndFabricationOrdersCardsHeader headers)
        {

            FabticationOrderCardsResponse Response = new FabticationOrderCardsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            Response.Errors = validation.errors;
            Response.Result = validation.result;
            #endregion

            try
            {
                if (Response.Result)
                {
                    var FabOrdersQuery = _Context.VProjectFabricationProjectOffers.AsQueryable();

                    var TotalWorkingHoursList = _Context.VProjectFabricationTotalHours.ToList();

                    #region dealing with headers
                    if (headers.ProjectID != 0)
                    {
                        FabOrdersQuery = FabOrdersQuery.Where(a => a.ProjectId == headers.ProjectID);
                    }
                    else
                    {
                        FabOrdersQuery = FabOrdersQuery.Where(a => a.StartDate >= headers.DateFrom && a.EndDate <= headers.DateTo);
                    }
                    #endregion

                    var pagedFabOrdersList = PagedList<VProjectFabricationProjectOffer>.Create(FabOrdersQuery, headers.CurrentPage, headers.NumberOfItemsPerPage);


                    List<MiniFabticationOrderCard> fabticationOrderCardslist = new List<MiniFabticationOrderCard>();

                    foreach (var card in pagedFabOrdersList)
                    {
                        MiniFabticationOrderCard fabOrderCard = new MiniFabticationOrderCard();

                        fabOrderCard.ProjectId = card.ProjectId.ToString();
                        fabOrderCard.ProjectName = card.ProjectName;
                        fabOrderCard.ClientName = card.ClientName;
                        fabOrderCard.ProjectManagerID = card.ProjectManagerId;

                        if (card.Id != null)
                        {
                            fabOrderCard.FabricationOrderId = card.Id.ToString();
                        }
                        if (card.FabNumber != null)
                        {
                            fabOrderCard.FabricationOrderNumber = (long)card.FabNumber;
                        }
                        if (card.EndDate != null)
                        {
                            fabOrderCard.EndDate = card.EndDate?.ToShortDateString();
                            fabOrderCard.RemainingDays = (long)((DateTime)card.EndDate - DateTime.Now).TotalDays;
                        }
                        fabOrderCard.ReceivingDate = card.StartDate?.ToShortDateString();
                        fabOrderCard.RequireApprovalFeedBack = card.RequireApprovalFeedBack;
                        if (card.Progress != null)
                        {
                            fabOrderCard.FabricationProgress = card.Progress.ToString();
                        }
                        if (card.PassQc != null)
                        {
                            fabOrderCard.QualityInspection = card.PassQc.ToString();
                        }
                        if (card.Revision != null)
                        {
                            fabOrderCard.Revesion = card.Revision.ToString();
                        }
                        //fabOrderCard.RequireFinFeedBack = card.RequireFinFeedBack??false;
                        //fabOrderCard.FinFeedBackResult = card.FinFeedBackResult;
                        //fabOrderCard.RequireApprovalFeedBack = card.RequireApprovalFeedBack;
                        //fabOrderCard.ApprovalFeedBackResult = card.ApprovalFeedBackCooment;

                        //string SerailQR = card.FabOrderSerial;

                        //if(SerailQR != null)
                        //{
                        //    QRCodeGenerator _qrCode2 = new QRCodeGenerator();
                        //    QRCodeData _qrCodeData2 = _qrCode2.CreateQrCode(SerailQR, QRCodeGenerator.ECCLevel.Q);
                        //    QRCode qrCode2 = new QRCode(_qrCodeData2);
                        //    Bitmap qrCodeImage2 = qrCode2.GetGraphic(20);

                        //    var ButmapQRCodeIMGFabSerial = Common.BitmapToBytesCode(qrCodeImage2);

                        //    fabOrderCard.SerailQR = ButmapQRCodeIMGFabSerial;
                        //}
                        //fabOrderCard.ProjectSerial = card.ProjectSerial;

                        if (TotalWorkingHoursList.Where(a => a.ProjectFabId == card.Id).FirstOrDefault() != null)
                        {
                            fabOrderCard.TotalWorkingHours = TotalWorkingHoursList.Where(a => a.ProjectFabId == card.Id).Select(b => b.TotalHours).FirstOrDefault();
                        }
                        else
                        {
                            fabOrderCard.TotalWorkingHours = 0;
                        }

                        if (card.PassQc == true)
                        {
                            fabOrderCard.PassQC = "Passed";
                        }
                        else
                        {
                            fabOrderCard.PassQC = "Not Passed";
                        }




                        if (card.RequireFinFeedBack == true)
                        {
                            if (card.FinFeedBackResult != null)
                            {
                                if (card.FinFeedBackResult == "Waiting for reply")
                                {
                                    fabOrderCard.FinFeedBackResult = "Pending";

                                }
                                else
                                {
                                    fabOrderCard.FinFeedBackResult = card.FinFeedBackResult;
                                }

                            }
                            else
                            {
                                card.FinFeedBackResult = "Pending";
                            }
                            fabOrderCard.FinFeedBackResultApproved = "Yes";
                        }
                        else
                        {
                            fabOrderCard.FinFeedBackResultApproved = "No";
                        }


                        if (card.RequireSalesPersonFeedBack == false)
                        {
                            if (card.SalesPersonFeedBackResult != null)
                            {
                                if (card.SalesPersonFeedBackResult == "Waiting for reply")
                                {
                                    fabOrderCard.SalesPersonFeedBackResult = "Pending";

                                }
                                else
                                {
                                    fabOrderCard.SalesPersonFeedBackResult = card.SalesPersonFeedBackResult;
                                }

                            }
                            else
                            {
                                fabOrderCard.SalesPersonFeedBackResult = "Pending";
                            }
                            fabOrderCard.SalesPersonFeedBackResultApproved = "Yes";
                        }
                        else
                        {
                            fabOrderCard.SalesPersonFeedBackResultApproved = "No";
                        }


                        fabticationOrderCardslist.Add(fabOrderCard);
                    }

                    Response.FabticationOrderCards = fabticationOrderCardslist;
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


    }

}
