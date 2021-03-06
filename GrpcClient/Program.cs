﻿using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using GrpcServer.Protos;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Grpc_Client
{
    class Program
    {

        private const string Address = "localhost:5001";

        private static string _token;
        static async Task Main(string[] args)
        {

            //var clientCert = File.ReadAllText(@"ssl/client.crt");
            //var clientKey = File.ReadAllText(@"ssl/client.key");
            //var caCrt = File.ReadAllText(@"ssl/ca.crt");

            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 550 * 1024 * 1024, 
                MaxSendMessageSize = 200 * 1024 * 1024
            });

            Console.WriteLine("Pershendetje \n \n Ju lutem zgjedhni sherbimin te cilin doni ta shftytezoni.");
            while (true)
            {
                try
                {
                    Console.WriteLine(" 1. Autentifikohu \n 2. Shto konsumatore ne vazhdimesi \n 3. Shiko konsumatoret ne kohe reale \n 4. Shiko blerjet ne kohe reale");
                    string choice = Console.ReadLine();
                    if (choice.ToLower().Equals("stop"))
                        break;
                    switch (choice)
                    {
                        case "1": //Authentication service call
                            try
                            {
                                Console.WriteLine("Jepni username: ");
                                string username = Console.ReadLine();
                                Console.WriteLine("Jepni password-in: ");
                                string password = Console.ReadLine();
                                var authClient = new authAdmin.authAdminClient(channel);
                                _token = authClient.StaffAuth(new authInput { Username = username, Password = password }).Token.ToString();
                                if(_token==null)
                                {
                                    Console.WriteLine("Username apo password-i i dhene eshte gabim!");
                                    continue;
                                }
                                channel.Dispose();
                                channel = CreateAuthenticatedChannel($"https://{Address}");
                                Console.WriteLine("Sapo u autentifikuat!");
                            }
                            catch (RpcException e)
                            {
                                Console.WriteLine(e.Status.Detail);
                                Console.WriteLine(e.Status.StatusCode);
                            }
                            break;
                        case "2": //Shto konsumator ne vazhdimsi
                            try
                            {
                                var cClient = new Customer.CustomerClient(channel);
                                using (var call = cClient.AddCustomersStream())
                                {
                                    var response = Task.Run(async () =>
                                    {
                                        while (await call.ResponseStream.MoveNext())
                                        {
                                            Console.WriteLine(call.ResponseStream.Current.Message);     
                                        }
                                    });
                                    
                                    string name="", username="";char mbyll='a';
                                    while (!mbyll.Equals('q'))
                                    {
                                            Console.WriteLine("Shenoni te dhenat per konsumatorin e ri");
                                            Console.WriteLine("Emri: "); name = Console.ReadLine();
                                            Console.WriteLine("Username: "); username = Console.ReadLine();
                                            Console.WriteLine("Mobile Nr"); string mobileNr = Console.ReadLine();
                                        if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(username))
                                        {
                                            try {
                                                bool nr = Int32.TryParse(mobileNr, out int numri);
                                                await call.RequestStream.WriteAsync(new newCustomerData { Name = name, Username = username, MobileNr = nr == true ? numri : 0 });
                                            }
                                            catch
                                            {
                                                try
                                                {
                                                    Status status = call.GetStatus();
                                                    if (status.StatusCode != StatusCode.OK)
                                                        Console.WriteLine($"{status.StatusCode}, {status.Detail}");
                                                }
                                                catch{
                                                    Console.WriteLine("Ka nodhur nje gabim!");
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Emri apo Username nuk eshte plotesuar!");
                                        }
                                            
                                            Console.WriteLine("Shtypni 'q' per te mbyllur, qfardo tasti tjeter per te vazhduar!");
                                            ConsoleKeyInfo ans = Console.ReadKey(true);
                                            mbyll = Char.ToLower(ans.KeyChar);
                                    }
                                    await call.RequestStream.CompleteAsync();
                                }
                            }
                            catch (RpcException ex)
                            {
                                Console.WriteLine(ex.StatusCode);
                                Console.WriteLine(ex.Status.Detail);
                            }
                            break;
                        case "3": //Shiko konsumatoret e regjistruar ne Real Time
                            try
                            {
                                
                                var cClient = new Customer.CustomerClient(channel);
                                using (var call = cClient.GetNewCustomers(new Empty() { }, deadline: DateTime.UtcNow.AddSeconds(500)))
                                {
                                    Console.WriteLine("Konsumatoret e regjistruar");

                                    var response = Task.Run(()=>
                                    {
                                        var line = Console.ReadLine();
                                        call.Dispose();
                                    });

                                    while (await call.ResponseStream.MoveNext())
                                    {
                                        Console.WriteLine($"Name: {call.ResponseStream.Current.Name}, Username: {call.ResponseStream.Current.Username}, Mobile Number: {call.ResponseStream.Current.MobileNr}");
                                    }
                                }

                            }
                            catch (RpcException e)
                            {
                                Console.WriteLine(e.Status.Detail);
                                Console.WriteLine(e.Status.StatusCode);
                            }
                            break;
                        case "4": //Shiko produktet e blera ne Real Time
                            try
                            {
                                var cClient = new sales.salesClient(channel);
                                using (var call = cClient.getSalesInRealTime(new Google.Protobuf.WellKnownTypes.Empty()))
                                {
                                    Console.WriteLine("Produktet e blera REAL TIME");
                                    var response = Task.Run(() =>
                                    {
                                        var line = Console.ReadLine();
                                        call.Dispose();
                                    });

                                    while (await call.ResponseStream.MoveNext())
                                    {
                                        Console.WriteLine($"Customer: {call.ResponseStream.Current.Customer}, Product: {call.ResponseStream.Current.Product}, " +
                                            $"Cmimi: {call.ResponseStream.Current.Price}, Data Blerjes: {call.ResponseStream.Current.Date}");
                                    }
                                } 

                            }
                            catch (RpcException e)
                            {
                                Console.WriteLine(e.Status.Detail);
                                Console.WriteLine(e.Status.StatusCode);
                            }
                            break;
                        default:
                            Console.WriteLine("Ju lutem zgjedhni njerin nga numrat");
                            break;
                    }
                }
                catch (RpcException ex)
                {
                    Console.WriteLine(ex.StatusCode);
                    Console.WriteLine(ex.Status.Detail);
                    Console.WriteLine("Ju lutem zgjedhni njerin nga numrat!");
                }
            }
        }

        
       
        private static GrpcChannel CreateAuthenticatedChannel(string address)
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    metadata.Add("Authorization", $"Bearer {_token}");
                }
                return Task.CompletedTask;
            });

            // SslCredentials is used here because this channel is using TLS.
            // Channels that aren't using TLS should use ChannelCredentials.Insecure instead.
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            return channel;
        }


    }
}
