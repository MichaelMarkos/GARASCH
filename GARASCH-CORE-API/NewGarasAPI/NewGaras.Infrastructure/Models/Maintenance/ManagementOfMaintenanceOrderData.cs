using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class ManagementOfMaintenanceOrderData
    {
        public long ID { get; set; }
        public long MaintenanceOfferID { get; set; }
        public long MaintenanceForID { get; set; }
        public long ProjectID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public IFormFile WarrentyCertificateAttachmentContent { get; set; }
        public string WarrentyCertificateAttachmentName { get; set; }
        public string WarrentyCertificateAttachmentExtension { get; set; }

        public IFormFile ContractAttachementContent { get; set; }
        public string ContractAttachementName { get; set; }
        public string ContractAttachementExtension { get; set; }

        public string ProjectName { get; set; }
        public int NumberOfVisits { get; set; }
        public decimal? ContractPrice { get; set; }
        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public string Building { get; set; }
        public string Floor { get; set; }
        public string Street { get; set; }
        public string Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool Active { get; set; }
        public decimal? RateToLocalCu { get; set; }
        public string ContractAttachement { get; set; }
        public string WarrentyCertificateAttachment { get; set; }
        public string ContractStatus { get; set; }
        public string ClosingReason { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string ContractType { get; set; }
        public string ClosingContractType { get; set; }
        public decimal? ClosingMileageCounter { get; set; }
        public decimal? CurrentMileageCounter { get; set; }
        public string ContractNumber { get; set; }
        public int NumberOfCheques { get; set; } = 0;

        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public long? AreaId { get; set; }


        public string? CountryName { get; set; }
        public string? CityName { get; set; }
        public string? AreaName { get; set; }

    }
}
