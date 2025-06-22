using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.Family.Filters;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IFamilyService
    {
        public BaseResponseWithId<int> AddFamilyStatus(AddFamilyStatusDTO dto);

        public SelectDDLResponse GetFamilyStatusDDL();

        public BaseResponseWithId<long> AddFamily(AddFamilyDTO dto);

        public BaseResponseWithData<List<GetFamiliesListDTO>> GetFamiliesList(GetFamiliesListFilters filters);

        public BaseResponseWithData<GetFamiliesListDTO> GetFamilyByID(long familyID);

        public BaseResponseWithId<long> AddHrUserFamily(AddHrUserFamilyDTO dto, long userId);
        public BaseResponseWithDataAndHeader<List<GetHrUserFamiliesListDTO>> GetHrUserFamiliesList(GetHrUserFamiliesListFilters filters);
        public BaseResponseWithData<GetHrUserFamiliesListDTO> GetHrUserFamilyByID(long HrUserfamilyID);
        public BaseResponseWithId<long> EditFamily(EditFamilyDTO dto);
        public BaseResponseWithId<long> EditHrUserFamily(EditHrUserFamilyDTO dto, long userId);
        public BaseResponseWithId<long> AddNewFamilyWithMemebers(AddNewFamilyWithMembersDTO dto, long userID);
        public BaseResponseWithDataAndHeader<List<GetFamilyCardsDTO>> GetFamilyCards(GetFamilyCardsFilters filters);
        public BaseResponseWithId<long> DeleteHrUserFromFamily(DeleteHrUserFromFamilyDTO dto);
        public BaseResponseWithId<long> EditHrUserFamilyActive(EditHrUserFamilyActiveDTO dto);
        public BaseResponseWithId<long> EditTheHeadOfFamily(EditTheHeadOfFamilyDTO dto);
    }
}
