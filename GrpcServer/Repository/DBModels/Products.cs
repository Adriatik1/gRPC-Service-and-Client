using System;
using System.Collections.Generic;

namespace GrpcServer.Repository.DBModels
{
    public partial class Products
    {
        public Products()
        {
            Sales = new HashSet<Sales>();
        }

        public int Id { get; set; }
        public string Pname { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public virtual ICollection<Sales> Sales { get; set; }
    }
}
