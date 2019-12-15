using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Models
{
    public class salesModel
    {
        public int saleID { get; set; }
        public string customerName { get; set; }
        public string productName { get; set; }
        public string price { get; set; }
        public string date { get; set; }
    }
}
