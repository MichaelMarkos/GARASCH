
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;
using System.Linq;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NewGaras.Domain.Services.Hotel
{

    public class ClientRepository : BaseRepository<Client, long>, IClientRepository
    {
        protected GarasTestContext _context;
        IHostingEnvironment host;
        private readonly IUnitOfWork _unitOfWork;

        public ClientRepository(IUnitOfWork unitOfWork, GarasTestContext context, IHostingEnvironment _host) : base(context)
        {
            _context = context;
            host = _host;
            _unitOfWork = unitOfWork;

        }
        public BaseResponseWithData<GuestProfileDto> Addlanguage(long ClientId, List<int>? languageId, bool updateLanguage = false)
        {
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (ClientId == 0 || languageId is null || languageId.Contains(0))
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid client language";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (updateLanguage == true)
                {
                    foreach (var lang in _context.ClientLanguagees.Where(x => x.ClientId == ClientId))
                    {
                        _context.ClientLanguagees.Remove(lang);
                    }

                }
                foreach (var lang in languageId)
                {
                    _context.ClientLanguagees.Add(new ClientLanguagee { ClientId = ClientId, LanguageeId = lang });
                }

                Response.Result = true;
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

        public BaseResponseWithData<GuestProfileDto> AddNational(long ClientId, int NationalId, bool updateNational = false)
        {
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (ClientId == 0 || NationalId == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid client Nationality";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (updateNational == true)
                {
                    foreach (var item in _context.ClientNationals.Where(x => x.ClientId == ClientId))
                    {
                        _context.ClientNationals.Remove(item);
                    }

                }

                _context.ClientNationals.Add(new ClientNational { ClientId = ClientId, NationalId = NationalId });


                Response.Result = true;
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
        public BaseResponseWithData<GuestProfileDto> AddClientDetails(long ClientId, int? MaritalStatusId, string? Gender, DateTime? DOB, int nationalityID, long CreatedBy, bool update = false)
        {
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (ClientId == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid client";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (update == true)
                {
                    foreach (var item in _context.ClientExtraInfos.Where(x => x.ClientId == ClientId))
                    {
                        _context.ClientExtraInfos.Remove(item);
                    }

                }

                var tempdata = _context.ClientExtraInfos.Add(new ClientExtraInfo
                {
                    ClientId = ClientId,
                    MaritalStatusId = MaritalStatusId,
                    Gender = Gender ?? "لم تحدد",
                    // DateOfBirth = DateOnly.FromDateTime((DateTime)DOB) ?? null
                    DateOfBirth = DOB != null ? Convert.ToDateTime(DOB) : DateTime.Now,
                    CreatedBy = CreatedBy,
                    ModifiedBy = CreatedBy,
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now,
                    NationalityId = nationalityID,
                    IdentityNumber = 1

                });


                Response.Result = true;
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
        public BaseResponseWithData<GuestProfileDto> AddClientPhone(long ClientId, string Phone, bool update = false)
        {
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (ClientId == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid client";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (update == true)
                {
                    foreach (var item in _context.ClientPhones.Where(x => x.ClientId == ClientId))
                    {
                        _context.ClientPhones.Remove(item);
                    }

                }
                _context.ClientPhones.Add(new ClientPhone
                {
                    ClientId = ClientId,
                    Phone = Phone,
                    CreatedBy = 1,
                    CreationDate = DateTime.Now,

                });


                Response.Result = true;
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
        public BaseResponseWithData<GuestProfileDto> AddAddress(long ClientId, List<AddressDto>? addresslist, long CreatedBy, bool updatAddress = false)
        {
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (addresslist.Count < 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid client Address";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (updatAddress == true)
                {
                    foreach (var lang in _context.ClientAddresses.Where(x => x.ClientId == ClientId))
                    {
                        _context.ClientAddresses.Remove(lang);
                    }

                }


                foreach (var item in addresslist)
                {
                    _context.ClientAddresses.Add(new ClientAddress
                    {
                        ClientId = ClientId,
                        CountryId = item.CountryId,
                        GovernorateId = item.GovernorateId,
                        AreaId = item.AreaId,
                        Address = item.Address,
                        BuildingNumber = item.BuildingNumber,
                        Floor = item.Floor,
                        Description = item.Description,
                        Active = (bool)item.Active,
                        CreationDate = DateTime.Now,
                        CreatedBy = CreatedBy
                    });
                }

                Response.Result = true;
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


        public BaseResponseWithData<GuestProfileDto> Addinformations(long ClientId, List<ClientinformatinDto>? clientInformation, bool updatAddress = false)
        {
            BaseResponseWithData<GuestProfileDto> Response = new BaseResponseWithData<GuestProfileDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {


                if (clientInformation.Count < 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid client information";
                    Response.Errors.Add(error);
                    return Response;
                }

                var AllclientInformations = _context.ClientInformations.Where(x => x.ClientId == ClientId);

                if (updatAddress == true)
                {
                    foreach (var lang in AllclientInformations)
                    {
                        _context.ClientInformations.Remove(lang);
                    }

                }



                for (int i = 0; clientInformation.Count > i; i++)
                {
                    if (clientInformation[i].Number != null) 
                    {
                        var testNationalID = GetbyNumberIDUnique((int)clientInformation[i].Number);
                        if (testNationalID != null)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "NationalID is Used";
                            Response.Errors.Add(error);
                            return Response;
                        }

                    }

                    if (clientInformation[i].File != null)
                    {
                        string fileName = string.Empty;
                        string update = string.Empty;
                        string fullpath = string.Empty;
                        if (clientInformation[i].Type == "NationalID")
                        {
                            var PhotoClientpath = "hotel/NationalID/";
                            string SaveIMGPathCert = Path.Combine(host.WebRootPath, PhotoClientpath);



                            if (!System.IO.Directory.Exists(SaveIMGPathCert))
                            {
                                System.IO.Directory.CreateDirectory(SaveIMGPathCert); //Create directory if it doesn't exist
                            }
                            if (AllclientInformations.Where(x=>x.Type == "NationalID").Any())
                            {
                                string fullPath = SaveIMGPathCert + ( AllclientInformations.Where(x => x.Type == "NationalID").Select(p=>p.Image).FirstOrDefault() );
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            update = Path.Combine(host.WebRootPath, PhotoClientpath);

                            fileName = clientInformation[i].File.FileName;
                            fullpath = Path.Combine(update, fileName);
                            clientInformation[i].File.CopyTo(new FileStream(fullpath, FileMode.Create));

                            clientInformation[i].Image = PhotoClientpath + "/" + fileName;
                        }
                        else if (clientInformation[i].Type == "Passport")
                        {
                            var PhotoClientpath = "hotel/Passport/";
                            string SaveIMGPathCert = Path.Combine(host.WebRootPath, PhotoClientpath);
                            if (!System.IO.Directory.Exists(SaveIMGPathCert))
                            {
                                System.IO.Directory.CreateDirectory(SaveIMGPathCert); //Create directory if it doesn't exist
                            }
                            if (AllclientInformations.Where(x => x.Type == "Passport").Any())
                            {
                                string fullPath = SaveIMGPathCert + (AllclientInformations.Where(x => x.Type == "Passport").Select(p => p.Image).FirstOrDefault());
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            update = Path.Combine(host.WebRootPath, PhotoClientpath);

                            fileName = clientInformation[i].File.FileName;
                            fullpath = Path.Combine(update, fileName);
                            clientInformation[i].File.CopyTo(new FileStream(fullpath, FileMode.Create));

                            clientInformation[i].Image = PhotoClientpath + "/" + fileName;
                        }
                        else if (clientInformation[i].Type == "PhotoClient")
                        {
                             var PhotoClientpath = "hotel/PhotoClient/";
                            string SaveIMGPathCert = Path.Combine(host.WebRootPath, PhotoClientpath);
                            if (!System.IO.Directory.Exists(SaveIMGPathCert))
                            {
                                System.IO.Directory.CreateDirectory(SaveIMGPathCert); //Create directory if it doesn't exist
                            }
                            if (AllclientInformations.Where(x => x.Type == "PhotoClient").Any())
                            {
                                string fullPath = SaveIMGPathCert + (AllclientInformations.Where(x => x.Type == "PhotoClient").Select(p => p.Image).FirstOrDefault());
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            update = Path.Combine(host.WebRootPath, PhotoClientpath);

                            fileName = clientInformation[i].File.FileName;
                            fullpath = Path.Combine(update, fileName);
                            clientInformation[i].File.CopyTo(new FileStream(fullpath, FileMode.Create));

                            clientInformation[i].Image = PhotoClientpath + "/" + fileName;
                        }
                        else if (clientInformation[i].Type == "Other")
                        {
                            var PhotoClientpath = "hotel/Other/";
                            string SaveIMGPathCert = Path.Combine(host.WebRootPath, PhotoClientpath);
                            if (!System.IO.Directory.Exists(SaveIMGPathCert))
                            {
                                System.IO.Directory.CreateDirectory(SaveIMGPathCert); //Create directory if it doesn't exist
                            }
                            if (AllclientInformations.Where(x => x.Type == "Other").Any())
                            {
                                string fullPath = SaveIMGPathCert + (AllclientInformations.Where(x => x.Type == "Other").Select(p => p.Image).FirstOrDefault());
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            update = Path.Combine(host.WebRootPath, "Other");
                            fileName = clientInformation[i].File.FileName;
                            fullpath = Path.Combine(update, fileName);
                            clientInformation[i].File.CopyTo(new FileStream(fullpath, FileMode.Create));

                            clientInformation[i].Image = PhotoClientpath + "/" + fileName;
                        }

                    }

                    _context.ClientInformations.Add(new ClientInformation
                    {

                        ClientId = ClientId,
                        Type = clientInformation[i].Type,
                        CreationDate = clientInformation[i].CreationDate,
                        Number = clientInformation[i].Number,
                        Image = clientInformation[i].Image
                    });
                }

                Response.Result = true;
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


        public ClientInformation GetbyNumberIDUnique(int Num, int Id = 0)
        {
            return _context.ClientInformations.FirstOrDefault(s => s.Number == Num && s.Id != Id);

        }

        public BaseResponseWithData<Client> SearchClientbyNationalID(int NationalIDNumber)
        {
            BaseResponseWithData<Client> Response = new BaseResponseWithData<Client>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                var clientId = _context.ClientInformations.FirstOrDefault(x => x.Number == NationalIDNumber).ClientId;
                var client = _context.Clients.FirstOrDefault(x => x.Id == clientId);
                Response.Data = client;
                Response.Result = true;
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

        public BaseResponseWithData<GetDetailsClientbyIdViewModel> GetclientbyId(long clientId)
        {
            BaseResponseWithData<GetDetailsClientbyIdViewModel> Response = new BaseResponseWithData<GetDetailsClientbyIdViewModel>();
            Response.Errors = new List<Error>();
            Response.Result = false;


            try
            {
                var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");

                var clientViewmodel = new GetDetailsClientbyIdViewModel();
                var tempdata = _context.Clients.Include(c => c.ClientNationals).FirstOrDefault(x => x.Id == clientId);
                var allclientInformations = _context.ClientInformations.Where(x => x.ClientId == clientId);

                if (tempdata != null)
                {
                    var GeneralInformation = _context.ClientExtraInfos.FirstOrDefault(x => x.ClientId == clientId);
                    var countryDB = _context.Countries.Where(x=>x.Id > 0);
                    var GovernorateDB = _context.Governorates.Where(x=>x.Id > 0);
                    var areaDB = _context.Areas.Where(x=>x.Id > 0);
                    var adressClient = _context.ClientAddresses.Where(x => x.ClientId == clientId);

                    clientViewmodel.Id = tempdata.Id;
                    clientViewmodel.Type = tempdata.Type;
                    clientViewmodel.Name = tempdata.Name;
                    clientViewmodel.Email = tempdata.Email;
                    clientViewmodel.Mobile = _context.ClientPhones.Where(x => x.ClientId == clientId).Select(y => y.Phone).FirstOrDefault();
                    clientViewmodel.Nationality = _context.ClientNationals.Include(n => n.National).Where(x => x.ClientId == clientId).Select(y => y.National.Nationality).FirstOrDefault();
                    clientViewmodel.languages = _context.ClientLanguagees.Include(l => l.Languagee).Where(x => x.ClientId == clientId).Select(y => y.Languagee.Value).ToList();
                    clientViewmodel.MaritalStatus = _context.MaritalStatuses.Where(x => x.Id == GeneralInformation.MaritalStatusId).Select(m => m.Name).FirstOrDefault();
                    clientViewmodel.DOB = GeneralInformation.DateOfBirth.ToString();
                    clientViewmodel.CreationDate = tempdata.CreationDate.ToString();
                    clientViewmodel.CreatedBy = tempdata.CreatedBy;
                    clientViewmodel.Gender = GeneralInformation.Gender;
                    clientViewmodel.ImagePath = Globals.baseURL + (allclientInformations.Where(p => p.Type == "PhotoClient")).Select(i => i.Image).FirstOrDefault();
                    clientViewmodel.addressDtoByIds = _context.ClientAddresses.Where(x => x.ClientId == clientId).Select(a => new AddressDtoById 
                    {
                        Country = countryDB.Where(x=>x.Id == a.CountryId).Select(c=>c.Name).FirstOrDefault(),
                        Governorate = GovernorateDB.Where(x => x.Id == a.GovernorateId).Select(c => c.Name).FirstOrDefault(),
                        Area = areaDB.Where(x => x.Id == a.AreaId).Select(c => c.Name).FirstOrDefault(),
                        Address = a.Address,
                        BuildingNumber = a.BuildingNumber,
                        Floor = a.Floor,
                        Description = a.Description,
                        Active = a.Active

                    }).ToList();
                    clientViewmodel.Images = allclientInformations.Where(x=>x.Type != "PhotoClient").Select(a=>new GetClientImages 
                    {
                        TypeImage = a.Type,
                        Image = a.Image != null ?  baseURL+ a.Image : null
                    }).ToList();

                }
              //  var tempdata2 = _context.ClientInformations.FirstOrDefault(x => x.ClientId == clientId);
                //if (tempdata2 != null)
                //{
                //    //clientViewmodel.TypeImage = tempdata2.Type;
                //    //clientViewmodel.Image = Globals.baseURL + tempdata2.Image;

                //}
                Response.Result = true;
                Response.Data = clientViewmodel;
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

        public BaseResponseWithData<List<Reservation>> GetReservationsByclentId(long clientId)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Errors = new List<Error>();
            Response.Result = false;


            try
            {
                var reservations = _context.Reservations.Where(x => x.ClientId == clientId).ToList();
                Response.Result = true;
                Response.Data = reservations;
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



        public BaseResponseWithId<long> DeleteClient(long ClientId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (Response.Result)
                {
                    if (ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "client Id Is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var client = _unitOfWork.Clients.FindAll(x=>x.Id == ClientId).FirstOrDefault();
                    if (client == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Client not found";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var langsOfClient = _context.ClientLanguagees.Where(x => x.ClientId == ClientId);
                    if (langsOfClient != null) 
                    {
                        foreach (var lang in langsOfClient)
                        {
                            _context.ClientLanguagees.Remove(lang);
                        }
                    }
                    var ClientNationals = _context.ClientNationals.Where(x => x.ClientId == ClientId);
                    if (ClientNationals != null)
                    {
                        foreach (var item in ClientNationals)
                        {
                            _context.ClientNationals.Remove(item);
                        }

                    }


                    var ClientExtraInfos = _context.ClientExtraInfos.Where(x => x.ClientId == ClientId);
                    if (ClientNationals != null)
                    {
                        foreach (var item in ClientExtraInfos)
                        {
                            _context.ClientExtraInfos.Remove(item);
                        }

                    }



                    var ClientPhones = _context.ClientPhones.Where(x => x.ClientId == ClientId);
                    if (ClientNationals != null)
                    {
                        foreach (var item in ClientPhones)
                        {
                            _context.ClientPhones.Remove(item);
                        }

                    }

                    var ClientAddresses = _context.ClientAddresses.Where(x => x.ClientId == ClientId);
                    if (ClientAddresses != null)
                    {
                        foreach (var lang in ClientAddresses)
                        {
                            _context.ClientAddresses.Remove(lang);
                        }
                    }

                    var ClientInformations = _context.ClientInformations.Where(x => x.ClientId == ClientId);
                    if (ClientAddresses != null)
                    {
                        foreach (var lang in ClientInformations)
                        {
                            _context.ClientInformations.Remove(lang);
                        }
                    }


                    _unitOfWork.Clients.Delete(client);
                    _unitOfWork.Complete();
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

    }

}
