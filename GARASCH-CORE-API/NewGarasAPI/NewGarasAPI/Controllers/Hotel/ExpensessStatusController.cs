using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Hotel.DTOs;
using NewGaras.Infrastructure.Interfaces.Hotel;

namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensessStatusController : ControllerBase
    {
        private readonly IExpensessStatusRepository _expensess;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ExpensessStatusController(IExpensessStatusRepository expensess , IUnitOfWork unitOfWork
            , IMapper mapper , IRoomRepository roomRepository , IRateRepository rateRepository , IClientRepository clientRepository)
        {
            _expensess=expensess;
            _unitOfWork=unitOfWork;
            _mapper=mapper;

        }
        [HttpGet]
        public async Task<IActionResult> GetAllExpensessStatus()
        {
            BaseResponseWithData<List<ExpensessStatus>> Response = new BaseResponseWithData<List<ExpensessStatus>>();
            Response.Data=new List<ExpensessStatus>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                Response.Data=(List<ExpensessStatus>)await _expensess.GetAllAsync();
                Response.Result=true;
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

        [HttpGet("expensess")]
        public async Task<IActionResult> GetExpensessStatus([FromHeader] int expensessId)
        {
            BaseResponseWithData<ExpensessStatus> Response = new BaseResponseWithData<ExpensessStatus>();
            Response.Data=new ExpensessStatus();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                Response.Data=await _expensess.GetByIdAsync(expensessId);
                Response.Result=true;
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
        //[HttpPost]
        //public IActionResult Addexpensess(List<ExpensessStatusDto> model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    BaseResponseWithData<List<ExpensessStatusDto>> Response = new BaseResponseWithData<List<ExpensessStatusDto>>();
        //    var expensessList = new List<ExpensessStatusDto>();
        //    try
        //    {
        //        foreach (var item in model) 
        //        {
        //            var tempdata =  _expensess.Add(_mapper.Map<ExpensessStatus>(item));
        //            tempdata.TotalCost = tempdata.Qty * tempdata.PriceOfUnit;
        //            expensessList.Add(_mapper.Map<ExpensessStatusDto>(tempdata));
        //        }
        //      //  var newBuilding = _expensess.Add(model);
        //        _unitOfWork.Complete();
        //        Response.Data = expensessList;
        //        Response.Result = true;
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}


        [HttpPost]
        public IActionResult Addexpensess(addexpensess model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            BaseResponseWithData<List<ExpensessStatusDto>> Response = new BaseResponseWithData<List<ExpensessStatusDto>>();
            var expensessList = new List<ExpensessStatusDto>();
            try
            {
                foreach(var item in model.dto)
                {
                    var tempdata = _expensess.Add(_mapper.Map<ExpensessStatus>(item));
                    tempdata.TotalCost=tempdata.Qty*tempdata.PriceOfUnit;
                    expensessList.Add(_mapper.Map<ExpensessStatusDto>(tempdata));
                }
                //  var newBuilding = _expensess.Add(model);
                _unitOfWork.Complete();
                Response.Data=expensessList;
                Response.Result=true;
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
