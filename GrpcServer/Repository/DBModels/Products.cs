using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServer.Repository.DBModels
{
    [Table("products")]
    public partial class Products
    {
        public Products()
        {
            Sales = new HashSet<Sales>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("pname")]
        [StringLength(255)]
        public string Pname { get; set; }
        [Column("price", TypeName = "decimal(19, 4)")]
        public decimal Price { get; set; }
        [Column("stock")]
        public int Stock { get; set; }

        [InverseProperty("Product")]
        public virtual ICollection<Sales> Sales { get; set; }
    }
}
