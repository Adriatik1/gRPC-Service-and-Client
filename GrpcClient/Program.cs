using Grpc.Core;
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

            bool useAuth = true;

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
                                            if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(username))
                                            {
                                                await call.RequestStream.WriteAsync(new newCustomerData { Name = name, Username = username });
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
                            catch
                            {
                                Console.WriteLine("Lidhja tashme eshte mbyllur!");
                            }
                            break;
                        case "3": //Shiko konsumatoret e regjistruar ne Real Time
                            try
                            {
                                Console.WriteLine("Konsumatoret e regjistruar");
                                var cClient = new Customer.CustomerClient(channel);
                                using (var call = cClient.GetNewCustomers(new Empty() { }))
                                {
                                    var response = Task.Run(async () =>
                                    {
                                        while (await call.ResponseStream.MoveNext())
                                        {
                                            
                                                Console.WriteLine($"Name: {call.ResponseStream.Current.Name}, Username: {call.ResponseStream.Current.Username}");
                                            
                                        }
                                    });

                                    var line = Console.ReadLine();

                                    if (line.ToLower().Equals("stop"))
                                    {
                                        call.Dispose();
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
                                Console.WriteLine("Produktet e blera REAL TIME");
                                var cClient = new sales.salesClient(channel);
                                using (var call = cClient.getSalesInRealTime(new Google.Protobuf.WellKnownTypes.Empty()))
                                {
                                    var response = Task.Run(async () =>
                                    {
                                        while (await call.ResponseStream.MoveNext())
                                        {
                                            Console.WriteLine($"Customer: {call.ResponseStream.Current.Customer}, Product: {call.ResponseStream.Current.Product}, " +
                                                $"Cmimi: {call.ResponseStream.Current.Price}, Data Blerjes: {call.ResponseStream.Current.Date}");
                                        }
                                    });

                                    var line = Console.ReadLine();

                                    if (line.ToLower().Equals("stop"))
                                    {
                                        call.Dispose();
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
                catch
                {
                    Console.WriteLine("Ju lutem zgjedhni njerin nga numrat!");
                }
            }


            if (useAuth)
            {
                try
                {
                    var authClient = new authAdmin.authAdminClient(channel);
                    _token = authClient.StaffAuth(new authInput { Username = "adriatik", Password = "adriatik" }).Token.ToString();
                    channel.Dispose();
                    channel = CreateAuthenticatedChannel($"https://{Address}");
                }
                catch (RpcException e)
                {
                    Console.WriteLine(e.Status.Detail);
                    Console.WriteLine(e.Status.StatusCode);
                }
            }

            var customerClient = new Customer.CustomerClient(channel);
            using (var call = customerClient.GetNewCustomers(new Empty()))
            {
                var response = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        Console.WriteLine($"Name: {call.ResponseStream.Current.Name}, Username: {call.ResponseStream.Current.Username}");

                    }

                });
                
                    var line =  Console.ReadLine();
                    
                    if (line.ToLower().Equals("stop"))
                    {
                        call.Dispose();
                        //channel.Dispose();
                        Console.WriteLine("Disconnected from server!");
                        Console.ReadKey();
                    }
                
            }
            var greetClient = new Greeter.GreeterClient(channel);

            var greetRes = greetClient.SayHello(new HelloRequest { Name = "hej njeri" });
            Console.WriteLine(greetRes.Message);

                Console.ReadKey();

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
