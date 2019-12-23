using System;
using System.Collections.Generic;

namespace GrpcServer.Repository.DBModels
{
    public partial class Sales
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int ProductId { get; set; }
        public DateTime SaleDate { get; set; }

        public virtual Customers Customer { get; set; }
        public virtual Products Product { get; set; }
    }
}
