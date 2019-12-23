using System;
using System.Collections.Generic;

namespace GrpcServer.Repository.DBModels
{
    public partial class Customers
    {
        public Customers()
        {
            Sales = new HashSet<Sales>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public int? MobileNr { get; set; }

        public virtual ICollection<Sales> Sales { get; set; }
    }
}
