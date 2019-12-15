using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServer.Repository.DBModels
{
    [Table("customers")]
    public partial class Customers
    {
        public Customers()
        {
            Sales = new HashSet<Sales>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }
        [Required]
        [Column("username")]
        [StringLength(255)]
        public string Username { get; set; }

        [InverseProperty("Customer")]
        public virtual ICollection<Sales> Sales { get; set; }
    }
}
