using System;
using System.Collections.Generic;

namespace GrpcServer.Repository.DBModels
{
    public partial class UserXroles
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public virtual Roles Role { get; set; }
        public virtual Users User { get; set; }
    }
}
