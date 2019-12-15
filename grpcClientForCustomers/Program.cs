using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer.Protos;
using System;
using System.Threading.Tasks;

namespace grpcClientForCustomers
{
    class Program
    {
        private const string Address = "localhost:5001";
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 50 * 1024 * 1024, //50 MB
                MaxSendMessageSize = 20 * 1024 * 1024 //20 MB
            });

            Console.WriteLine("Pershendetje \n \n Ju lutem zgjedhni sherbimin te cilin doni ta shftytezoni.");
            while (true)
            {
                try
                {
                    Console.WriteLine("1. Listo te gjitha produktet ");
                    string choice = Console.ReadLine();
                    if (choice.ToLower().Equals("stop"))
                        break;
                    switch (choice)
                    {
                        case "1":
                            try
                            {
                                var client = new products.productsClient(channel);
                                var response = client.listAllProducts(new Google.Protobuf.WellKnownTypes.Empty());
                                foreach(var item in response.ProductList)
                                {
                                    Console.WriteLine($"ID: {item.Id}, Emri: {item.ProductName}, Cmimi: {item.ProductPrice}, Stoku: {item.Stock}");
                                }
                                
                            }
                            catch(RpcException ex) 
                            {
                                Console.WriteLine(ex.StatusCode);
                                Console.WriteLine(ex.Status.Detail);
                            }
                            break;
                        default:
                            Console.WriteLine("Ju lutem zgjedhni njerin nga numrat!");
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("Ju lutem zgjedhni njerin nga numrat!");
                }
            }
        }
    }
}
