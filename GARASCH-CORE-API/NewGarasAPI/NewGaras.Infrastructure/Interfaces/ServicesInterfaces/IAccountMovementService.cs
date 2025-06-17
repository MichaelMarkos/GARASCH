using NewGaras.Infrastructure.DBContext;
using NewGarasAPI.Models.AccountAndFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IAccountMovementService
    {
        public List<AccountOfMovement> GetAccountMovementList_WithListAccountIds(string AccountIdSTRr, bool CalcWithoutPrivate, bool OrderByCreationDatee, DateTime? DateFrom, DateTime? DateTo, long ClientIdd, long SupplierIdd, long BranchIdd);
    }
}
