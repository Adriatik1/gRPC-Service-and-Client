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
    public class CustomerService : Customer.CustomerBase
    {
        private readonly ILogger<CustomerService> _logger;
        private DBContext db;
        public IConfiguration Configuration { get; }
        List<customersModel> customers;
        customersModel lastCustomer = new customersModel();
        public CustomerService(ILogger<CustomerService> logger, DBContext db, IConfiguration configuration)
        {
            _logger = logger;
            this.db = db;
            Configuration = configuration;
            customers = new List<customersModel>();
        }

        public override Task<responseList> GetCustomer(Empty request, ServerCallContext context)
        {
           
            customerResponseModel model = new customerResponseModel();
            customerResponseModel model1 = new customerResponseModel();

            responseList ls = new responseList();

            var customers = db.Customers.ToList();

            model.Id = 1;
            model.Username = "Adriatik123";
            //  model.Emri = "Adriatik Ademi";

            model1.Id = 2;
            model1.Username = "Tiki123";
            //model1.Emri = "Tiki";

            for (int i = 0; i < 1109900; i++)
            {
                ls.ResList.Add(new customerResponseModel { Id = 3, Name = "Test1", Username = "Testttt" });
            }

            ls.ResList.Add(model);
            ls.ResList.Add(model1);


            return Task.FromResult(ls);

        }

        [Authorize(Roles ="customer")]
        public override async Task GetNewCustomers(Empty request, IServerStreamWriter<customerResponseModel> responseStream, ServerCallContext context)
        {
            SqlDependency.Start(Configuration.GetConnectionString("grpcDBConn"));
            var connection = new SqlConnection(Configuration.GetConnectionString("grpcDBConn"));
            connection.Open();

            using (SqlCommand command = new SqlCommand(@"Select [id], [name], [username], [mobileNr] from [dbo].[customers]", connection))
            {
                command.Notification = null;
                SqlDependency dependency = new SqlDependency(command);

                OnChangeEventHandler handler = (sender, args) =>
                {
                    customers.Add(new customersModel { id = 324, name = "qwe", username = "Wqeqw" });                
                };

                dependency.OnChange += new OnChangeEventHandler((sender, e) => streamON(sender, e));

                SqlDataReader reader = await command.ExecuteReaderAsync();
                foreach (var item in reader)
                {
                    customers.Add(new customersModel
                    {
                        id = int.Parse(reader["id"].ToString()),
                        username = reader["username"].ToString(),
                        name = reader["name"].ToString(),
                        mobileNr = reader["mobileNr"].ToString()
                    });
                }
                try
                {

                    while (true)
                    {
                        if (customers.Count > 0)
                        {
                            foreach (var item in customers)
                            {
                                await responseStream.WriteAsync(new customerResponseModel { Id = item.id, Name = item.name, Username = item.username, MobileNr =item.mobileNr });
                            }
                            lastCustomer = customers.Where(x => x.id == customers.Max(x => x.id)).SingleOrDefault();
                            customers.Clear();
                            dependency.OnChange -= new OnChangeEventHandler((sender, e) => streamON(sender, e));
                        }
                        await Task.Delay(3000);
                    }
                }
                catch
                {
                    
                }
            }
        }

        private void streamON(object sender, SqlNotificationEventArgs e)
        {
            var connection = new SqlConnection(Configuration.GetConnectionString("grpcDBConn"));
            connection.Open();

            using (SqlCommand command = new SqlCommand(@"Select [id], [name], [username], [mobileNr] from [dbo].[customers] where id > @lastID", connection))
            {
                command.Notification = null;
                command.Parameters.AddWithValue("@lastID", lastCustomer.id);
                SqlDependency dependency = new SqlDependency(command);
                dependency.OnChange += new OnChangeEventHandler((sender, e) => streamON(sender, e));
                SqlDataReader reader = command.ExecuteReader();
                foreach (var item in reader)
                {
                    customers.Add(new customersModel
                    {
                        id = int.Parse(reader["id"].ToString()),
                        name = reader["name"].ToString(),
                        username = reader["username"].ToString(),
                        mobileNr = reader["mobileNr"].ToString()
                    });
                }
            }
        }

        [Authorize]
        public override async Task AddCustomersStream(IAsyncStreamReader<newCustomerData> requestStream, IServerStreamWriter<newCustomerResponse> responseStream, ServerCallContext context)
        {
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    if (requestStream.Current!=null)
                    {
                        if (string.Equals(requestStream.Current.Name, "stop", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        try
                        {
                            if (db.Customers.Where(x => x.Username == requestStream.Current.Username).Any())
                                await responseStream.WriteAsync(new newCustomerResponse { Message = "Ekziston!" });
                            else if (db.Customers.Where(x=>x.MobileNr==requestStream.Current.MobileNr).Any())
                                await responseStream.WriteAsync(new newCustomerResponse { Message = "Ekziston nje konsumator me kete numer!" }); 
                            else if (String.IsNullOrEmpty(requestStream.Current.Name) || String.IsNullOrEmpty(requestStream.Current.Username))
                                    await responseStream.WriteAsync(new newCustomerResponse { Message = "Ju lutem plotesoni mire te dhenat!" });
                            else
                            {
                                    Customers cs = new Customers();
                                    cs.Name = requestStream.Current.Name;
                                    cs.Username = requestStream.Current.Username;
                                if (requestStream.Current.MobileNr != 0)
                                    cs.MobileNr = requestStream.Current.MobileNr;   
                                    db.Entry(cs).State = EntityState.Added;
                                    db.SaveChanges();
                                    await responseStream.WriteAsync(new newCustomerResponse { Message = "Konsumatori u shtua me sukses!" });
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new RpcException(new Status(StatusCode.Unknown, "Ka ndodhur nje gabim! "+ ex.Message));    
                        }
                    }
                }
            }
            catch (IOException)
            {

            }
        }

    }
}
