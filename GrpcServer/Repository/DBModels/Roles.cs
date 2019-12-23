using System;
using System.Collections.Generic;

namespace GrpcServer.Repository.DBModels
{
    public partial class Roles
    {
        public Roles()
        {
            UserXroles = new HashSet<UserXroles>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserXroles> UserXroles { get; set; }
    }
}
