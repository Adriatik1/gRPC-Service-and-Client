using Grpc.Core;
using GrpcServer.Models;
using GrpcServer.Protos;
using GrpcServer.Repository;
using GrpcServer.Repository.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
    public class SalesService : sales.salesBase
    {
        private DBContext db;
        public IConfiguration Configuration { get; }
        private readonly ILogger<salesModel> _logger;
        List<salesModel> sales;
        salesModel lastSale = new salesModel();
        public SalesService(ILogger<salesModel> logger, DBContext db, IConfiguration configuration)
        {
            this.db = db;
            sales = new List<salesModel>();
            Configuration = configuration;
            _logger = logger; 
        }

        private void streamON(object sender, SqlNotificationEventArgs e)
        {
            var connection = new SqlConnection(Configuration.GetConnectionString("grpcDBConn"));
            connection.Open();

            using (SqlCommand command = new SqlCommand(@"Select [id] from [dbo].[sales] where id > @saleID", connection))
            {
                command.Notification = null;
                command.Parameters.AddWithValue("@saleID", lastSale.saleID);
                SqlDependency dependency = new SqlDependency(command);
                SqlDataReader reader = command.ExecuteReader();
                reader.Close();
                dependency.OnChange += new OnChangeEventHandler((sender, e) => streamON(sender, e));
                var queryEF = (from s in db.Sales
                               join c in db.Customers on s.CustomerId equals c.Id
                               join p in db.Products on s.ProductId equals p.Id
                               where s.Id > lastSale.saleID
                               select new
                               {
                                   ID = s.Id,
                                   cName = c.Name,
                                   pName = p.Pname,
                                   Date = s.SaleDate,
                                   Price = p.Price
                               }).ToList();
                //while(await reader.ReadAsync())
                foreach (var item in queryEF)
                {
                    sales.Add(new salesModel
                    {
                        saleID = item.ID,
                        customerName = item.cName,
                        productName = item.pName,
                        date = item.Date.ToString(),
                        price = item.Price.ToString()
                    });
                }
            }
        }

        [Authorize]
        public override async Task getSalesInRealTime(Google.Protobuf.WellKnownTypes.Empty request, IServerStreamWriter<salesResponse> responseStream, ServerCallContext context)
        {
            SqlDependency.Start(Configuration.GetConnectionString("grpcDBConn"));
            var connection = new SqlConnection(Configuration.GetConnectionString("grpcDBConn"));
            connection.Open();

            string query = @"Select [id] from [dbo].[sales]";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Notification = null;
                SqlDependency dependency = new SqlDependency(command);

                dependency.OnChange += new OnChangeEventHandler((sender, e) => streamON(sender, e));

                SqlDataReader reader = await command.ExecuteReaderAsync();
                await reader.CloseAsync();
                var queryEF = (from s in db.Sales join c in db.Customers on s.CustomerId equals c.Id
                               join p in db.Products on s.ProductId equals p.Id select new { 
                               ID = s.Id, 
                               cName = c.Name,
                               pName = p.Pname,
                               Date = s.SaleDate,
                               Price = p.Price
                               }).ToList();

                foreach(var item in queryEF)
                {
                    sales.Add(new salesModel
                    {
                     saleID=item.ID,
                     customerName = item.cName,
                     productName = item.pName,
                     date = item.Date.ToString(),
                     price = item.Price.ToString()
                    });
                }
                try
                {

                    while (true)
                    {
                        if (sales.Count > 0)
                        {
                            foreach (var item in sales.ToList())
                            {
                                await responseStream.WriteAsync(new salesResponse { Customer = item.customerName, Product = item.productName, Price = item.price, Date = item.date});
                            }
                            lastSale = sales.Where(x => x.saleID == sales.Max(x => x.saleID)).FirstOrDefault();
                            sales.Clear();
                            dependency.OnChange -= new OnChangeEventHandler((sender, e) => streamON(sender, e));
                        }
                        await Task.Delay(3000);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        
    }
}
