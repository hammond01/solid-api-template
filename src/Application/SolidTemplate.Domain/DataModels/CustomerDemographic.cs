﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SolidTemplate.Domain.DataModels;

public class CustomerDemographic
{
    [Key]
    [Column("CustomerTypeID")]
    [StringLength(10)]
    public string CustomerTypeId { get; set; } = null!;

    [Column(TypeName = "ntext")]
    public string? CustomerDesc { get; set; }

    [ForeignKey("CustomerTypeId")]
    [InverseProperty("CustomerTypes")]
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
