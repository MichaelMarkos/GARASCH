﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("TransportationVehicleRoute")]
public partial class TransportationVehicleRoute
{
    [Key]
    public long Id { get; set; }

    [Column("transportationLineId")]
    public int TransportationLineId { get; set; }

    [Column("supplierId")]
    public long? SupplierId { get; set; }

    [Column("supplierContactPersonId")]
    public long? SupplierContactPersonId { get; set; }

    [Column("branchScheduleId")]
    public long? BranchScheduleId { get; set; }

    [Required]
    [Column("serial")]
    [StringLength(100)]
    public string Serial { get; set; }

    [Column("periodFrom", TypeName = "datetime")]
    public DateTime PeriodFrom { get; set; }

    [Column("periodTo", TypeName = "datetime")]
    public DateTime? PeriodTo { get; set; }

    [Column("active")]
    public bool Active { get; set; }

    [Column("creationDate", TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column("creationBy")]
    public long CreationBy { get; set; }

    [Column("modifiedDate", TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [Column("modifiedBy")]
    public long ModifiedBy { get; set; }

    public long? SupervisorId { get; set; }

    [ForeignKey("BranchScheduleId")]
    [InverseProperty("TransportationVehicleRoutes")]
    public virtual BranchSchedule BranchSchedule { get; set; }

    [ForeignKey("CreationBy")]
    [InverseProperty("TransportationVehicleRouteCreationByNavigations")]
    public virtual User CreationByNavigation { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("TransportationVehicleRouteModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [ForeignKey("SupervisorId")]
    [InverseProperty("TransportationVehicleRouteSupervisors")]
    public virtual User Supervisor { get; set; }

    [ForeignKey("SupplierId")]
    [InverseProperty("TransportationVehicleRoutes")]
    public virtual Supplier Supplier { get; set; }

    [ForeignKey("SupplierContactPersonId")]
    [InverseProperty("TransportationVehicleRoutes")]
    public virtual SupplierContactPerson SupplierContactPerson { get; set; }

    [ForeignKey("TransportationLineId")]
    [InverseProperty("TransportationVehicleRoutes")]
    public virtual TransportationLine TransportationLine { get; set; }

    [InverseProperty("TransportationVehicleRoute")]
    public virtual ICollection<TransportationVehicleRouteAccount> TransportationVehicleRouteAccounts { get; set; } = new List<TransportationVehicleRouteAccount>();

    [InverseProperty("TransportationVehicleRoute")]
    public virtual ICollection<TransportationVehicleRouteDeduction> TransportationVehicleRouteDeductions { get; set; } = new List<TransportationVehicleRouteDeduction>();

    [InverseProperty("TransportationVehicleRoute")]
    public virtual ICollection<TransportationVehicleRouteDirection> TransportationVehicleRouteDirections { get; set; } = new List<TransportationVehicleRouteDirection>();

    [InverseProperty("TransportationVehicleRoute")]
    public virtual ICollection<TransportationVehicleRouteEmployeeException> TransportationVehicleRouteEmployeeExceptions { get; set; } = new List<TransportationVehicleRouteEmployeeException>();

    [InverseProperty("TransportationVehicleRoute")]
    public virtual ICollection<TransportationVehicleRouteEmployee> TransportationVehicleRouteEmployees { get; set; } = new List<TransportationVehicleRouteEmployee>();
}
