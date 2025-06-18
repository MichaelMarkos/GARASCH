using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("HrUserAddress")]
public partial class HrUserAddress
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    [Column("CountryID")]
    public int CountryId { get; set; }

    [Column("GovernorateID")]
    public int GovernorateId { get; set; }

    [Column("CityID")]
    public int? CityId { get; set; }

    [Column("DistrictID")]
    public long? DistrictId { get; set; }

    [Column("AreaID")]
    public long? AreaId { get; set; }

    [Required]
    public string Address { get; set; }

    [StringLength(250)]
    public string ZipCode { get; set; }

    [Column(TypeName = "decimal(18, 8)")]
    public decimal? Longitude { get; set; }

    [Column(TypeName = "decimal(18, 8)")]
    public decimal? Latitude { get; set; }

    [Column("GeographicalNameID")]
    public long? GeographicalNameId { get; set; }

    [StringLength(250)]
    public string Description { get; set; }

    [StringLength(250)]
    public string Street { get; set; }

    public int? HouseNumber { get; set; }

    public int? FloorNumber { get; set; }

    public int? ApartmentNumber { get; set; }

    [ForeignKey("AreaId")]
    [InverseProperty("HrUserAddresses")]
    public virtual Area Area { get; set; }

    [ForeignKey("CityId")]
    [InverseProperty("HrUserAddresses")]
    public virtual City City { get; set; }

    [ForeignKey("CountryId")]
    [InverseProperty("HrUserAddresses")]
    public virtual Country Country { get; set; }

    [ForeignKey("DistrictId")]
    [InverseProperty("HrUserAddresses")]
    public virtual District District { get; set; }

    [ForeignKey("GeographicalNameId")]
    [InverseProperty("HrUserAddresses")]
    public virtual GeographicalName GeographicalName { get; set; }

    [ForeignKey("GovernorateId")]
    [InverseProperty("HrUserAddresses")]
    public virtual Governorate Governorate { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserAddresses")]
    public virtual HrUser HrUser { get; set; }
}
