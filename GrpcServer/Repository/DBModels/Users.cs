using System;
using System.Collections.Generic;

namespace GrpcServer.Repository.DBModels
{
    public partial class Users
    {
        public Users()
        {
            UserXroles = new HashSet<UserXroles>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public virtual ICollection<UserXroles> UserXroles { get; set; }
    }
}
