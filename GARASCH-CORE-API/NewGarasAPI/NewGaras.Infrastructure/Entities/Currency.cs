using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Currency")]
public partial class Currency
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; }

    [Required]
    [StringLength(5)]
    public string ShortName { get; set; }

    public bool Active { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Required]
    [StringLength(250)]
    public string CreatedBy { get; set; }

    [StringLength(250)]
    public string ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Modified { get; set; }

    public bool? IsLocal { get; set; }

    [InverseProperty("Currency")]
    public virtual ICollection<AccountOfJournalEntryOtherCurrency> AccountOfJournalEntryOtherCurrencies { get; set; } = new List<AccountOfJournalEntryOtherCurrency>();

    [InverseProperty("Currency")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [InverseProperty("Currency")]
    public virtual ICollection<AllowncesType> AllowncesTypes { get; set; } = new List<AllowncesType>();

    [InverseProperty("OpeningBalanceCurrency")]
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    [InverseProperty("Currency")]
    public virtual ICollection<DailyReportExpense> DailyReportExpenses { get; set; } = new List<DailyReportExpense>();

    [InverseProperty("FromCurrency")]
    public virtual ICollection<ExchangeRate> ExchangeRateFromCurrencies { get; set; } = new List<ExchangeRate>();

    [InverseProperty("ToCurrency")]
    public virtual ICollection<ExchangeRate> ExchangeRateToCurrencies { get; set; } = new List<ExchangeRate>();

    [InverseProperty("DefaultCurrency")]
    public virtual ICollection<ExtraCostLibrary> ExtraCostLibraries { get; set; } = new List<ExtraCostLibrary>();

    [InverseProperty("CurrencyNavigation")]
    public virtual ICollection<JobTitle> JobTitles { get; set; } = new List<JobTitle>();

    [InverseProperty("Currency")]
    public virtual ICollection<MaintenanceReportExpense> MaintenanceReportExpenses { get; set; } = new List<MaintenanceReportExpense>();

    [InverseProperty("Currency")]
    public virtual ICollection<ManagementOfMaintenanceOrder> ManagementOfMaintenanceOrders { get; set; } = new List<ManagementOfMaintenanceOrder>();

    [InverseProperty("Currency")]
    public virtual ICollection<PricingBom> PricingBoms { get; set; } = new List<PricingBom>();

    [InverseProperty("Currency")]
    public virtual ICollection<PricingExtraCost> PricingExtraCostCurrencies { get; set; } = new List<PricingExtraCost>();

    [InverseProperty("LocalCurrency")]
    public virtual ICollection<PricingExtraCost> PricingExtraCostLocalCurrencies { get; set; } = new List<PricingExtraCost>();

    [InverseProperty("Currency")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    [InverseProperty("Currency")]
    public virtual ICollection<PrsupplierOfferItem> PrsupplierOfferItems { get; set; } = new List<PrsupplierOfferItem>();

    [InverseProperty("Currency")]
    public virtual ICollection<PurchasePoitem> PurchasePoitems { get; set; } = new List<PurchasePoitem>();

    [InverseProperty("Currency")]
    public virtual ICollection<ReservationInvoice> ReservationInvoices { get; set; } = new List<ReservationInvoice>();

    [InverseProperty("Currency")]
    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();

    [InverseProperty("Currency")]
    public virtual ICollection<SalesBranchProductTarget> SalesBranchProductTargets { get; set; } = new List<SalesBranchProductTarget>();

    [InverseProperty("Currency")]
    public virtual ICollection<SalesBranchTarget> SalesBranchTargets { get; set; } = new List<SalesBranchTarget>();

    [InverseProperty("Currency")]
    public virtual ICollection<SalesBranchUserProductTarget> SalesBranchUserProductTargets { get; set; } = new List<SalesBranchUserProductTarget>();

    [InverseProperty("Currency")]
    public virtual ICollection<SalesBranchUserTarget> SalesBranchUserTargets { get; set; } = new List<SalesBranchUserTarget>();
}
