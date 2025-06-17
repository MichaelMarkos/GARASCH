using NewGaras.Infrastructure.DTO.Medical.MedicalFinance;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical
{
    public interface IMedicalFinancialService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithId<long> AddMedicalOpenBalance(AddOpeningMedicalFinancialDTO dto, long userID);
        public BaseResponseWithId<long> AddMedicalClosingBalance(AddClosingMedicalFinancialDTO dTO);

        public BaseResponseWithData<List<SelectDDL>> GetPosNumerDDL();

        public BaseResponseWithData<List<MedicalDailyTreasuryBalanceDto>> GetAllMedicalDailyTreasuryBalance(int PosNumberId, long CreatedById, string type, DateTime? From, DateTime? To, bool? IsOpeningBalance);

        public BaseResponseWithData<MedicalDailyTreasuryBalanceDto> GetMedicalDailyTreasuryBalance(int PosNumberId, bool IsOpeningBalance, string Type, long? CreatedById);
    }
}
