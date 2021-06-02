using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using SymmetricSecurityKey = Microsoft.IdentityModel.Tokens.SymmetricSecurityKey;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Extension
{


    public static class PermissionTokenExtension
    {
        private static IConfiguration _config { get; set; }
        public static void UsePermissionBearer(this IApplicationBuilder app, IConfiguration config)
        {
            _config = config;

        }
        
        /// <summary>
        /// Generate a Encrypted Token
        /// </summary>
        /// <param name="identity">Claim Identity</param>
        /// <param name="expiry">Expiry Time in Minutes</param>
        public static async Task<string> GenerateToken(ClaimsIdentity identity, double expiry) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JwtConfig:secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddMinutes(expiry),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate User Auth Token
        /// </summary>
        /// <param name="accid">User Indentifier</param>
        /// <param name="role">Permission Role</param>
        public static async Task<string> GenerateAuthToken(Guid accid, string[] role)
        {

            return await GenerateToken(new ClaimsIdentity(new[]
                {
                    new Claim("identifier", accid.ToString()),
                    new Claim("roles", JObject.FromObject(role).ToString())
                }), double.Parse(_config["JwtConfig:expiration_mins"]));
        }
       
        /// <summary>
        /// Try to Decrypt Given Token
        /// </summary>
        /// <param name="token">Encrypted Token</param>
        public static JwtSecurityToken ValidateToken(this string token) {
            try
            {
                var key = Encoding.ASCII.GetBytes(_config["JwtConfig:secret"]);
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    RequireAudience = false,
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var rawValidatedToken);
                return (JwtSecurityToken)rawValidatedToken;
            }
            catch
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Expired"));
            }
        }
        
        /// <summary>
        /// Decrypt Authentication Token
        /// </summary>
        /// <param name="context">Grpc CallContext</param>
        public static JwtSecurityToken ValidateBearer(this ProtoBuf.Grpc.CallContext context)
        {

            if (context.RequestHeaders.Count(p => p.Key == "authorization") <= 0)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Expired"));
            }
            return context.RequestHeaders.GetValue("authorization").Replace("Bearer ", String.Empty).ValidateToken();
        }
        
        /// <summary>
        /// Decrypt Authentication Token 
        /// </summary>
        /// <param name="context">Asp.Net HttpContext</param>
        /// <returns></returns>
        public static JwtSecurityToken ValidateBearer(this HttpContext context)
        {

            if (context.Request.Headers.Count(p => p.Key == "authorization") <= 0)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Expired"));
            }
            return context.Request.Headers["authorization"].ToString().Replace("Bearer ", String.Empty).ValidateToken();
        }    
        
        /// <summary>
        /// Extract the Role out of decrypted token
        /// </summary>
        /// <param name="validKey">Decrypted token</param>
        public static string[] ExtractRole(this JwtSecurityToken validKey)
        {
            return JObject.Parse(validKey.Claims.First(p => p.Type == "roles").Value).ToObject<string[]>();
        }

        /// <summary>
        /// Extract User ID out of decrypted token
        /// </summary>
        /// <param name="validKey">Decrypted token</param>
        public static Guid ExtractIdentifier(this JwtSecurityToken validKey)
        {
            return Guid.Parse(validKey.Claims.First(p => p.Type == "identifier").Value);
        }

    }
}
