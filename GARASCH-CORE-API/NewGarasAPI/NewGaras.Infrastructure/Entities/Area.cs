﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Area")]
public partial class Area
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    public long? ModifiedBy { get; set; }

    [Column("DistrictID")]
    public long? DistrictId { get; set; }

    [InverseProperty("Area")]
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    [InverseProperty("Area")]
    public virtual ICollection<ClientAddress> ClientAddresses { get; set; } = new List<ClientAddress>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("AreaCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("DistrictId")]
    [InverseProperty("Areas")]
    public virtual District District { get; set; }

    [InverseProperty("Area")]
    public virtual ICollection<HrUserAddress> HrUserAddresses { get; set; } = new List<HrUserAddress>();

    [ForeignKey("ModifiedBy")]
    [InverseProperty("AreaModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [InverseProperty("Area")]
    public virtual ICollection<ProjectContactPerson> ProjectContactPeople { get; set; } = new List<ProjectContactPerson>();

    [InverseProperty("Area")]
    public virtual ICollection<SalesOfferLocation> SalesOfferLocations { get; set; } = new List<SalesOfferLocation>();

    [InverseProperty("Area")]
    public virtual ICollection<SupplierAddress> SupplierAddresses { get; set; } = new List<SupplierAddress>();

    [InverseProperty("Area")]
    public virtual ICollection<UserPatient> UserPatients { get; set; } = new List<UserPatient>();
}
