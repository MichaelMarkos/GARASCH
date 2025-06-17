using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client.ClientsCardsStatistics
{
    public class GetClientsCardsStatistics
    {
        int totalClientsCount;
        decimal totalVolume;
        decimal totalRemainCollection;

        bool result;
        List<Error> errors;

        [DataMember]
        public int TotalClientsCount
        {
            get
            {
                return totalClientsCount;
            }

            set
            {
                totalClientsCount = value;
            }
        }

        [DataMember]
        public decimal TotalVolume
        {
            get
            {
                return totalVolume;
            }

            set
            {
                totalVolume = value;
            }
        }

        [DataMember]
        public decimal TotalRemainCollection
        {
            get
            {
                return totalRemainCollection;
            }

            set
            {
                totalRemainCollection = value;
            }
        }

        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }

}
