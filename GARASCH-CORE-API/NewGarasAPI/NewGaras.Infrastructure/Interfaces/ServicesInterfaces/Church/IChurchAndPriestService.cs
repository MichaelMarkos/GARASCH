using NewGaras.Infrastructure.DTO.ChurchAndPriest;
using NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Church
{
    public interface IChurchAndPriestService
    {
        public BaseResponseWithId<long> AddNewPriest(AddNewPriestDTO dto, long userID);

        public BaseResponseWithId<long> EditPriest(EditPriestDTO dto, long userID);

        public BaseResponseWithDataAndHeader<List<GetPriestsListDTO>> GetPriestList(GetPriestsListFilters filters);
        public BaseResponseWithId<long> AddNewChurch(AddChurchDTO dto);
        public BaseResponseWithId<long> EditChurch(EditChurchDTO dto);
        public BaseResponseWithDataAndHeader<List<GetChurchesListDTO>> GetChurchesList(GetChurchsListFilters filters);

        public BaseResponseWithId<int> AddNewEparchy(AddEparchyDTO dto);

        public SelectDDLResponse GetEparchyDDL();
        public BaseResponseWithData<List<GetHrUserPriestHistoryDTO>> GetHrUserPriestHistory(GetHrUserPriestHistoryFilters filters);
        public BaseResponseWithDataAndHeader<List<GetEparchyWithChurchDTO>> GetEparchyWithChurch(GetEparchyWithChurchFilters filters);
    }
}
