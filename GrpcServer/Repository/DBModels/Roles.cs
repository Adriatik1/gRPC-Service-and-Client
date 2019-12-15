using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServer.Repository.DBModels
{
    [Table("roles")]
    public partial class Roles
    {
        public Roles()
        {
            UserXroles = new HashSet<UserXroles>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [InverseProperty("Role")]
        public virtual ICollection<UserXroles> UserXroles { get; set; }
    }
}
