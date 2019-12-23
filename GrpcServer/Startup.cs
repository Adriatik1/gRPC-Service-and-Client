using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrpcServer.Repository.DBModels;
using GrpcServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Grpc.Core;

namespace GrpcServer
{
    public class Startup
    {
        //private DBContext db;
        //// This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
      //  private DBContext db;
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options => {
                options.MaxReceiveMessageSize = 200 * 1024 * 1024; // 200 MB
                options.MaxSendMessageSize = 550 * 1024 * 1024; // 550 MB
            });


            services.AddDbContext<DBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("grpcDBConn")));

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.Name);
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes("gRPC-DB-Adriatik-Ademi-Secret-Key-Gen"))
                };
                });

            services.AddHttpContextAccessor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DBContext _db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == 401)
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "Nuk keni autorizim!"));
                else
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "Ka ndodhur nje gabim!"));
            });

            app.Use(async (context, next) =>
            {
                await next();

                // If response is unauthorized and the endpoint is a gRPC method then
                // return grpc-status permission denied instead
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "Nuk keni autorizim!"));
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<CustomerService>();
                endpoints.MapGrpcService<ChattingService>();
                endpoints.MapGrpcService<AuthService>();
                endpoints.MapGrpcService<SalesService>();
                endpoints.MapGrpcService<ProductsService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Komunikimi me endpoints te gRPC duhet te behete me ane te nje gRPC klienti!"/*"Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"*/);
                });

                endpoints.MapGet("/generateJwtToken", context =>
                {
                    return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]));
                });

            });
        }

        private string GenerateJwtToken(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("Name is not specified.");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.Role, "admin") };

            var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("gRPCServer", "Clients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }

        private readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        private readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());
        
        

    }
}
