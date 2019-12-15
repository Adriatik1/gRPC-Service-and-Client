using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServer.Repository.DBModels
{
    [Table("users")]
    public partial class Users
    {
        public Users()
        {
            UserXroles = new HashSet<UserXroles>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("username")]
        [StringLength(255)]
        public string Username { get; set; }
        [Required]
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<UserXroles> UserXroles { get; set; }
    }
}
