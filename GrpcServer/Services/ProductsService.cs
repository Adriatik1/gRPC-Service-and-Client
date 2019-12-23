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

        public override async Task<buyResponseModel> buyProduct(buyRequestModel request, ServerCallContext context)
        {
            var customerDB = db.Customers.Where(x => x.Username == request.CustomerUsername && x.MobileNr == request.MobileNumber).SingleOrDefault();
            buyResponseModel buyResult = new buyResponseModel();
            try
            {
                if (request.ProductID == 0)
                    return await Task.FromResult(new buyResponseModel { Message = "Ju lutem shenoni ID e produktit te cilit doni ta blini. Shfaqni listen e produkteve per te gjetur ID e tyre!" });
                Sales newSale = new Sales();
                newSale.ProductId = request.ProductID;
                newSale.SaleDate = DateTime.UtcNow;
                
                newSale.CustomerId = customerDB==null ? 1244 : customerDB.Id; //1244 osht customer Guest
                await db.Sales.AddAsync(newSale);
                await db.SaveChangesAsync();
                return await Task.FromResult(new buyResponseModel { Message = "Blerja eshte kryer me sukses!" });
            }
            catch { }
            return await Task.FromResult(new buyResponseModel { Message = "Ka nodhur nje gabim!" });
        }
    }
}
