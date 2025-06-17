using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    [Serializable]
    public class InvoiceLineTaxModel
    {
        string TaxName;
        decimal? TaxPercentage;
        string TaxType;
        decimal? TaxValue;
        string SubTaxName;
        bool? IsPercentage;
        public InvoiceLineTaxModel()
        {
            TaxName = "";
            TaxPercentage = null;
            TaxType = "";
            TaxValue = null;
            SubTaxName = "";
            IsPercentage = null;
        }
        public InvoiceLineTaxModel(string taxName, decimal? taxPercentage, string taxType, decimal? taxValue, string subTaxName, bool? isPercentage)
        {
            TaxName1 = taxName;
            TaxPercentage1 = taxPercentage;
            TaxType1 = taxType;
            TaxValue1 = taxValue;
            SubTaxName1 = subTaxName;
            IsPercentage = isPercentage;
        }

        public string TaxName1 { get => TaxName; set => TaxName = value; }
        public decimal? TaxPercentage1 { get => TaxPercentage; set => TaxPercentage = value; }
        public string TaxType1 { get => TaxType; set => TaxType = value; }
        public decimal? TaxValue1 { get => TaxValue; set => TaxValue = value; }
        public string SubTaxName1 { get => SubTaxName; set => SubTaxName = value; }
        public bool? IsPercentage1 { get => IsPercentage; set => IsPercentage = value; }
    }
}
