using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Protos;
using GrpcServer.Repository.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
    public class ProductsService:products.productsBase
    {
        private DBContext db;
        public ProductsService(DBContext db)
        {
            this.db = db;
        }

        [Authorize]
        public override async Task<productsRes> listAllProducts(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            productsRes pRmodel = new productsRes();
            
            var query = db.Products.Where(x => x.Stock > 0).AsAsyncEnumerable();
         
            await foreach (var item in query)
            {
                pRmodel.ProductList.Add(new product { Id = item.Id, ProductName = item.Pname, ProductPrice = item.Price.ToString(), Stock = item.Stock});
            }

            return await Task.FromResult(pRmodel);
        }
    }
}
