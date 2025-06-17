using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services.Hotel;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Hotel.DTOs;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client;
using System.Net;



namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienthotelController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAddressRepository _addressRep;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;


        public ClienthotelController(IClientRepository clientRepository , IUnitOfWork unitOfWork , IMapper mapper , IAddressRepository addressRep , ITenantService tenantService)
        {
            _clientRepository=clientRepository;
            _addressRep=addressRep;
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllclient()
        {
            BaseResponseWithData<List<ClientDto>> Response = new BaseResponseWithData<List<ClientDto>>();
            Response.Data=new List<ClientDto>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var test = await _clientRepository.GetAllAsync();
                    Response.Data=_mapper.Map<List<ClientDto>>(await _clientRepository.GetAllAsync());
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

        [HttpGet("GetAllclientWithPagination")]
        public async Task<IActionResult> GetAllclientWithPagination([FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 20)
        {
            BaseResponseWithDataAndHeader<List<GetClientbyIdViewModel>> Response = new BaseResponseWithDataAndHeader<List<GetClientbyIdViewModel>>();
            Response.Errors=new List<Error>();
            Response.Result=false;


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                    string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");

                    var allClientDb = _unitOfWork.Clients.FindAllQueryable(x=>x.Id > 0);
                    var clientsPagedList = PagedList<Client>.Create(allClientDb,PageNo, NoOfItems);

                    var allClientPhone = _unitOfWork.ClientPhones.FindAll(x=>x.Id > 0);
                    var allClientNationality = _unitOfWork.clientNationals.FindAll(x=>x.Id > 0 ,new []{"National"});
                    var allclientInformations = _unitOfWork.clientInformations.FindAll(x=>x.Type == "PhotoClient");
                    var allClient = clientsPagedList.Select(u => new GetClientbyIdViewModel
                    {
                        Id = u.Id ,
                        Type = u.Type,
                        Name = u.Name,
                        Mobile = allClientPhone.Where(x=>x.ClientId == u.Id).Select(p=>p.Phone).FirstOrDefault(),
                        Nationality = allClientNationality.Where(x=>x.ClientId == u.Id).Select(n=>n.National.Nationality).FirstOrDefault(),
                        ImagePath = allclientInformations.Where(x=>x.ClientId == u.Id).Select(i=>i.Image).FirstOrDefault() != null ?
                                    baseURL + allclientInformations.Where(x=>x.ClientId == u.Id).Select(i=>i.Image).FirstOrDefault() : null,
                    } ).ToList();

                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=clientsPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=clientsPagedList.TotalCount
                    };


                    Response.Data=allClient;
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
        [HttpGet("GetClientbyId")]
        public IActionResult GetClientbyId([FromHeader] long clientId)
        {
            BaseResponseWithData<GetDetailsClientbyIdViewModel> Response = new BaseResponseWithData<GetDetailsClientbyIdViewModel>();
            Response.Data=new GetDetailsClientbyIdViewModel();
            Response.Errors=new List<Error>();
            Response.Result=false;


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var client = _clientRepository.GetclientbyId(clientId);

                    Response=client;
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

        //[HttpPost]
        //public IActionResult AddClient([FromBody] ClientDto client)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    BaseResponseWithData<ClientDto> Response = new BaseResponseWithData<ClientDto>();

        //    try
        //    {
        //        var newclient = _clientRepository.Add(_mapper.Map<Client>(client));
        //        _unitOfWork.Complete();
        //        Response.Data = client;
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
        public IActionResult AddClient([FromForm] GuestProfileDto client , [FromForm] List<AddressDto>? addressDtos , [FromForm] List<ClientinformatinDto>? ClientinformatinDtos)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Data=new GuestProfileDto();
            Response.Errors=new List<Error>();
            Response.Result=false;
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    client.CreatedBy=validation.userID;
                    client.CreationDate=DateTime.Now;
                    var newclient = _clientRepository.Add(_mapper.Map<Client>(client));
                    client.ClientId=newclient.Id;
                    _unitOfWork.Complete();
                    client.ClientId=newclient.Id;
                    if(client.languageeId!=null)
                    {
                        _clientRepository.Addlanguage(newclient.Id , client.languageeId , true);
                    }
                    if(client.NationalityId!=null)
                    {
                        _clientRepository.AddNational(newclient.Id , (int)client.NationalityId , true);
                        _unitOfWork.Complete();

                    }
                    _clientRepository.AddClientDetails(newclient.Id , client.MaritalStatusId , client.Gender , client.DOB , (int)client.NationalityId , client.CreatedBy , true);
                    _unitOfWork.Complete();

                    if(client.Mobile!=null)
                    {
                        _clientRepository.AddClientPhone(newclient.Id , client.Mobile , true);

                    }
                    if(addressDtos.Count()>0)
                    {
                        _clientRepository.AddAddress(newclient.Id , addressDtos , client.CreatedBy);

                    }
                    if(ClientinformatinDtos.Count()>0)
                    {
                        _clientRepository.Addinformations(newclient.Id , ClientinformatinDtos);

                    }
                    _unitOfWork.Complete();
                    Response.Data=client;
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

        //[HttpPost]
        //public IActionResult AddClient([FromBody] ClientDto client)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    BaseResponseWithData<Client> Response = new BaseResponseWithData<Client>();

        //    try
        //    {
        //        var newclient = _clientRepository.Add(_mapper.Map<Client>(client));
        //        _unitOfWork.Complete();
        //        Response.Data = newclient;
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
        [HttpPost("address")]
        public IActionResult adresss([FromBody] AddressDto client)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            BaseResponseWithData<AddressDto> Response = new BaseResponseWithData<AddressDto>();
            Response.Data=new AddressDto();
            Response.Errors=new List<Error>();
            Response.Result=false;
            try
            {

                var newaddress = _addressRep.Add(_mapper.Map<ClientAddress>(client));
                _unitOfWork.Complete();
                Response.Data=client;
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
        [HttpGet("GetClientbyNationalIDOrPassportNumber")]
        public IActionResult GetClientbyNationalID([FromHeader] int NationalID)
        {
            BaseResponseWithData<Client> Response = new BaseResponseWithData<Client>();
            Response.Data=new Client();
            Response.Errors=new List<Error>();
            Response.Result=false;
            try
            {

                var client = _clientRepository.SearchClientbyNationalID(NationalID);
                Response=client;
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

       


        [HttpGet("GetReservationsByclentId")]
        public IActionResult GetReservationsByclentId([FromHeader] long clientId)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data=new List<Reservation>();
            Response.Errors=new List<Error>();
            Response.Result=false;


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var reservations = _clientRepository.GetReservationsByclentId(clientId);

                    Response=reservations;
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







        [HttpPost("update-client")]
        public async Task<IActionResult> UpdateRoom([FromHeader] int ClientId , [FromForm] GuestProfileDto client , [FromForm] List<AddressDto>? addressDtos , [FromForm] List<ClientinformatinDto>? ClientinformatinDtos)
        {
            var response = new BaseResponseWithData<Room>
            {
                Data = new Room(),
                Errors = new List<Error>(),
                Result = false
            };



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors=validation.errors;
                response.Result=validation.result;

                if(response.Result)
                {
                    var oldClient = _clientRepository.GetById(ClientId);
                    if(oldClient!=null)
                    {
                        var newClient = _mapper.Map<Client>(client);
                        newClient.Id=ClientId;

                        _Context.Entry(oldClient).State=EntityState.Detached; 
                        _Context.Clients.Update(newClient);                     
                        _Context.SaveChanges();
                    }

                    if(client.languageeId!=null)
                    {
                        _clientRepository.Addlanguage(ClientId , client.languageeId,true);
                        _Context.SaveChanges();

                    }
                    if(client.NationalityId!=null)
                    {
                        _clientRepository.AddNational(ClientId , (int)client.NationalityId,true);
                        _Context.SaveChanges();


                    }
                    _clientRepository.AddClientDetails(ClientId , client.MaritalStatusId , client.Gender , client.DOB , (int)client.NationalityId , client.CreatedBy);

                    if(client.Mobile!=null)
                    {
                        _clientRepository.AddClientPhone(ClientId , client.Mobile, true);
                        _Context.SaveChanges();
                        

                    }
                    if(addressDtos.Count()>0)
                    {
                        _clientRepository.AddAddress(ClientId , addressDtos , client.CreatedBy,true);
                        _Context.SaveChanges();

                    }
                    if(ClientinformatinDtos.Count()>0)
                    {
                        _clientRepository.Addinformations(ClientId , ClientinformatinDtos, true);
                        _Context.SaveChanges();

                    }
                    _unitOfWork.Complete();
                    response.Result=true;
                    response.Data=null;

                }
                else
                {
                    response.Errors.Add(new Error { ErrorCode="Err11" , ErrorMSG="Room not found." });
                    return Ok(response);
                }
                return Ok(response);



            }
            catch(Exception ex)
            {
                response.Result=false;
                response.Errors.Add(new Error
                {
                    ErrorCode="Err10" ,
                    ErrorMSG=ex.InnerException?.Message??ex.Message
                });

                return BadRequest(response);
            }
        }



        [HttpPost("DeleteClient")]
        public BaseResponseWithId<long> DeleteClient([FromHeader] long ClientId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = _clientRepository.DeleteClient(ClientId);
                if(!data.Result)
                {
                    Response.Result=false;
                    Response.Errors.AddRange(data.Errors);
                    return Response;
                }
                Response=data;

                return Response;
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                ;
                Response.Errors.Add(error);
                return Response;
            }
        }


    }
}
