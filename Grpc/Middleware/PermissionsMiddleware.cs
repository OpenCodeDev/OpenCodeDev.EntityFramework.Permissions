using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Grpc.AspNetCore.Web.Internal;
using Grpc.AspNetCore.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Tables;
using Microsoft.AspNetCore.Mvc.Controllers;
using OpenCodeDev.EntityFramework.Permissions.Grpc.DataAnnotations;
using System.Reflection;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Extension;
using Microsoft.AspNetCore.Builder;
using Permissions.Grpc.Api;
using Microsoft.AspNetCore.Routing;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Middleware
{
    internal static class MiddlewareOptions
    {
        public static Type Db { get; set; }
        public static bool IsCodeFirst { get; set; }
    }
    public static class PermExt
    {
        /// <summary>
        /// Create a Middleware for the use of Grpc Permission System.
        /// </summary>
        /// <param name="db">Database where permission tables are stored.</param>
        /// <param name="CodeFirst"> True = Permission are available in the Database as read only.<br/>
        ///     When permission check, system will return permission initially set in [GrpcPermissionRoles("")].<br/>
        ///     False = Default, All permission will be created once according to [GrpcPermissionRoles("")] but if edited in database, database predominate the code. (use if intend to handle permission thru dashboard)
        /// </param>
        public static void UseGrpcPermissions(this IApplicationBuilder app, Type db, bool CodeFirst = false)
        {
            if (db == default)
            {
                throw new Exception("You must set a valid DatabaseContext as db.");
            }
            MiddlewareOptions.Db = db;
            MiddlewareOptions.IsCodeFirst = CodeFirst;
            app.UseMiddleware<PermissionsMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ApiPermission>().EnableGrpcWeb().RequireCors("AllowAll");
            });
        }
        public static void UseGrpcPermissionsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            if (MiddlewareOptions.Db == default)
            {
                throw new Exception("You must call UseGrpcPermissions() before endpoints configuration.");
            }
            endpoints.MapGrpcService<ApiPermission>().EnableGrpcWeb().RequireCors("AllowAll");
        }
    }

    public class PermissionsMiddleware
    {
        private RequestDelegate _next;
        public PermissionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string[] role = new string[] { "Public" }; // Default Role
            bool authenticated = false;
            //TODO: Decrypt Account and Extract Roles from Token.
            var methodMeta = (GrpcMethodMetadata)context.GetEndpoint().Metadata.GetMetadata<GrpcMethodMetadata>();
            if (methodMeta == null) { await _next(context); }

            var method = methodMeta.ServiceType.GetMembers().SingleOrDefault(p => p.Name.Equals(methodMeta.Method.Name));
            var validOperationContracts = method.DeclaringType.GetInterfaces()
            .Where(p => p.GetMethod(methodMeta.Method.Name) != null).Select(p => p.GetMethod(methodMeta.Method.Name))
            .Where(p => p.GetCustomAttribute(typeof(OperationContractAttribute)) != null).FirstOrDefault();
            OperationContractRolesAttribute RequiredCheck = (OperationContractRolesAttribute)(method.GetCustomAttribute(typeof(OperationContractRolesAttribute)));

            // No check required, move on.
            if (RequiredCheck != null) {
                if (validOperationContracts != null)
                {
                    OperationContractRolesAttribute permissionAttr = (OperationContractRolesAttribute)(validOperationContracts.GetCustomAttribute(typeof(OperationContractRolesAttribute)));
                    if (permissionAttr != null)
                    {
                        if (MiddlewareOptions.IsCodeFirst)
                        {
                            bool pass = (role.Contains("Admin") || (permissionAttr.Roles.Contains("Autehnticated") && authenticated)
                            || role.Count(p => permissionAttr.Roles.Contains(p)) > 0);

                            if (pass)
                            {
                                await _next(context);
                            }
                            else
                            {
                                context.Response.StatusCode = 403;
                            }
                        }
                        else
                        {
                            var serviceScopeFactory = context.RequestServices.GetRequiredService<IServiceScopeFactory>();
                            using (var serviceScope = serviceScopeFactory.CreateScope())
                            {
                                var db = (DbContext)serviceScope.ServiceProvider.GetService(MiddlewareOptions.Db);

                                var setPR = db.Set<TPermissionRole>();
                                var setPT = db.Set<TPermissionTable>();
                                var roles = setPR.Where(p => role.Contains(p.Name)
                                    && (p.Permissions.Count(b => b.Allow && b.Method.Equals(methodMeta.Method.Name)
                                    && b.Namespace.Equals(validOperationContracts.DeclaringType.Namespace)
                                    && b.Table.Equals(validOperationContracts.DeclaringType.Name)) > 0)).Include(p => p.Permissions).ToList();

                                //var permissons = setPR.Where(p => p.Permissions.Where(b => b.Allow ));
                                bool pass = roles.Count > 1;
                                if (pass)
                                {
                                    await _next(context);
                                }
                                else
                                {
                                    context.Response.StatusCode = 403;
                                }
                            }
                        }

                    }
                    else
                    {
                        context.Response.StatusCode = 403;
                    }


                }
                else
                {
                    context.Response.StatusCode = 403;
                }
            }
            else
            {
                context.Response.StatusCode = 403;

            }
        }
    }
}
