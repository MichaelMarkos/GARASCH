



using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IClientRepository : IBaseRepository<Client , long>
    {
        public BaseResponseWithData<GuestProfileDto> Addlanguage(long ClientId , List<int>? languageId , bool updateLanguage = false);
        public BaseResponseWithData<GuestProfileDto> AddAddress(long ClientId , List<AddressDto>? addresslist , long CreatedBy , bool updatAddress = false);
        public BaseResponseWithData<GuestProfileDto> Addinformations(long ClientId , List<ClientinformatinDto>? clientInformation , bool updatAddress = false);
        public ClientInformation GetbyNumberIDUnique(int Num , int Id = 0);
        public BaseResponseWithData<Client> SearchClientbyNationalID(int NationalIDNumber);
        public BaseResponseWithData<GetDetailsClientbyIdViewModel> GetclientbyId(long clientId);
        public BaseResponseWithData<GuestProfileDto> AddNational(long ClientId , int NationalId , bool updateNational = false);
        public BaseResponseWithData<GuestProfileDto> AddClientDetails(long ClientId , int? MaritalStatusId , string? Gender , DateTime? DOB , int nationalityID , long CreatedBy , bool update = false);
        public BaseResponseWithData<GuestProfileDto> AddClientPhone(long ClientId , string Phone , bool update = false);

        public BaseResponseWithData<List<Reservation>> GetReservationsByclentId(long clientId);
        public BaseResponseWithId<long> DeleteClient(long ClientId);

    }
}
