using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcServer.Repository.DBModels
{
    [Table("userXroles")]
    public partial class UserXroles
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userID")]
        public int UserId { get; set; }
        [Column("roleID")]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(Roles.UserXroles))]
        public virtual Roles Role { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.UserXroles))]
        public virtual Users User { get; set; }
    }
}
