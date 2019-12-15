using Grpc.Core;
using GrpcServer.Protos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using GrpcServer.Models;

namespace GrpcServer.Services
{
    public class ChattingService : chat.chatBase
    {
        public IConfiguration Configuration { get; }
        public ChattingService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public override async Task Chatting(IAsyncStreamReader<clientMessage> requestStream, IServerStreamWriter<serverMessage> responseStream, ServerCallContext context)
        {
            
                try
                {
                    while (await requestStream.MoveNext())
                    {
                        if (!string.IsNullOrEmpty(requestStream.Current.Message))
                        {
                            if (string.Equals(requestStream.Current.Message, "stop", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                            Console.WriteLine(requestStream.Current);
                            await Task.Delay(1000);
                            await responseStream.WriteAsync(new serverMessage { Message = "Got it!" });
                        }
                    }
                }
                catch (IOException)
                {

                }
            }
        }

    }

