using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServer.Repository.DBModels
{
    [Table("sales")]
    public partial class Sales
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("customerID")]
        public int? CustomerId { get; set; }
        [Column("productID")]
        public int ProductId { get; set; }
        [Column("saleDate", TypeName = "datetime")]
        public DateTime SaleDate { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [InverseProperty(nameof(Customers.Sales))]
        public virtual Customers Customer { get; set; }
        [ForeignKey(nameof(ProductId))]
        [InverseProperty(nameof(Products.Sales))]
        public virtual Products Product { get; set; }
    }
}
