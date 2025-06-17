using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services.Hotel;
using System.Security.Claims;
using NewGaras.Infrastructure.Models.Hotel;
using NewGaras.Infrastructure.Models;

namespace NewGarasAPI.Controllers.Hotel
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaidController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public PaidController(IRoomViewRepository roomViewRepository , IUnitOfWork unitOfWork , ITenantService tenantService)
        {
            _unitOfWork=unitOfWork;
            _helper=new Helper.Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }
        private long ApplicationUserId
        {
            get
            {
                string userIdString = User.FindFirstValue("uid");

                if(long.TryParse(userIdString , out long userId))
                {
                    return userId;
                }
                else
                {
                    throw new InvalidOperationException("Invalid user ID.");
                }
            }
        }


        [HttpPost("paid")]
        public IActionResult AddRoomView(Paid paid)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<RoomView> Response = new BaseResponseWithData<RoomView>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var newPaid = new ReservationInvoice
                    {
                        ClientId = paid.ClientId,
                        ReservationId = paid.ReservationId,
                        InvoiceDate = DateTime.Now ,
                        Amount = paid.Amount,
                        Serial = paid.Serial,
                        CreateBy= ApplicationUserId ,
                        CreateDate= DateTime.Now ,
                        InvoiceTypeId= paid.InvoiceTypeId,
                        CurrencyId = 1 

                    };

                     _unitOfWork.ReservationInvoice.Add(newPaid);
                    var reservation = _unitOfWork.Reservations.FindAll(x=>x.Id == paid.ReservationId).FirstOrDefault();
                    var remine = reservation.TotalCost - (reservation.TotalPaid + paid.Amount);
                    if(remine == 0)
                    {
                        reservation.IsFinished = true;
                        _unitOfWork.Reservations.Update(reservation);
                    }
                    _unitOfWork.Complete();
                    //Response.Data=newRoomView;
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpPost("InvoiceType")]
        public IActionResult InvoiceType(InvoiceType invoiceTpe)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponse Response = new BaseResponse();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                   

                    _unitOfWork.InvoiceTypes.Add(invoiceTpe);
                    Response.Result=true;
                   
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }




    }
}
