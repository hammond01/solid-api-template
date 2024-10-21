﻿namespace SolidTemplate.Domain.DataModels;

[Index("CompanyName", Name = "CompanyName")]
[Index("PostalCode", Name = "PostalCode")]
public class Supplier
{
    [Key]
    [Column("SupplierID")]
    public int SupplierId { get; set; }

    [StringLength(40)]
    public string CompanyName { get; set; } = null!;

    [StringLength(30)]
    public string? ContactName { get; set; }

    [StringLength(30)]
    public string? ContactTitle { get; set; }

    [StringLength(60)]
    public string? Address { get; set; }

    [StringLength(15)]
    public string? City { get; set; }

    [StringLength(15)]
    public string? Region { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    [StringLength(15)]
    public string? Country { get; set; }

    [StringLength(24)]
    public string? Phone { get; set; }

    [StringLength(24)]
    public string? Fax { get; set; }

    [Column(TypeName = "ntext")]
    public string? HomePage { get; set; }

    [InverseProperty("Supplier")]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
