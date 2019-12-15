using Grpc.Core;
using GrpcServer.Protos;
using GrpcServer.Repository.DBModels;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Configuration;
using System.Text;

namespace GrpcServer.Services
{
    public class AuthService:authAdmin.authAdminBase
    {
        private DBContext db;

        public AuthService(DBContext _db)
        {
            db = _db;
        }

        public override Task<authOutput> StaffAuth(authInput request, ServerCallContext context)
        {
            var _token = GenerateJwtToken(request);

            return Task.FromResult(new authOutput { Token = _token });
        }

        public string GenerateJwtToken(authInput model)
        {
            
            if (string.IsNullOrEmpty(model.Username))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Username nuk eshte i dhene!"));
            }

            bool auth = Authenticate(model.Username, model.Password);
            if (!auth)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Username apo passwordi i dhene eshte gabim!"));
            
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Username)};
            foreach (var item in rolet(db.Users.SingleOrDefault(x=>x.Username==model.Username).Id))
                claims.Add(new Claim(ClaimTypes.Role,item.ToString()));

            var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("gRPCServer", "Clients", claims, expires: DateTime.Now.AddSeconds(30), signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }

        private bool Authenticate(string username, string password)
        {

            #region ruajtja e pw ne db 
            //byte[] salt;
            //new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            //byte[] hash = pbkdf2.GetBytes(20);

            //byte[] hashBytes = new byte[36];
            //Array.Copy(salt, 0, hashBytes, 0, 16);
            //Array.Copy(hash, 0, hashBytes, 16, 20);

            //string savedPasswordHash = Convert.ToBase64String(hashBytes);

            #endregion
            
                var savedPasswordHash = db.Users.FirstOrDefault(u => u.Username == username);
            if (savedPasswordHash == null)
                return false;
                /* Extract the bytes */
                byte[] hashBytes = Convert.FromBase64String(savedPasswordHash.Password);
                /* Get the salt */
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                /* Compute the hash on the password the user entered */
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                /* Compare the results */
                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                        return false;

                return true;
            
        }

        public List<string> rolet(int userID)
        {
            List<string> rolet = new List<string>();
            
            var roles = (from ur in db.UserXroles
                         join r in db.Roles on ur.RoleId equals r.Id
                         where ur.UserId == userID
                         select new  { r.Name }).ToList();

            foreach (var item in roles)
                rolet.Add(item.Name);
           
            return rolet;
        }

        private readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        private readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes("gRPC-DB-Adriatik-Ademi-Secret-Key-Gen"));
    }
}
