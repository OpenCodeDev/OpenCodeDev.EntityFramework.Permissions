using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Extension;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Middleware;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Service;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Models;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Proto.Role;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Tables;
using OpenCodeDev.Forms.Grpc.Extensions;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Grpc.Api
{
    public class ApiPermission : IPermissionRolesService
    {        
        public async Task<MPermissionRole> Create(CreateInput input, CallContext context = default)
        {
            input.ValidateForm();
            IServiceProvider _serviceProvider = context.ServerCallContext.GetHttpContext().RequestServices;
            var db = (DbContext)_serviceProvider.GetRequiredService(MiddlewareOptions.Db);
            var roles = db.Set<TPermissionRole>();
            if (roles.Count(p => p.Name.Equals(input.Name)) > 0) { throw new RpcException(new Status(StatusCode.AlreadyExists, "Entry already exist.")); }
            var role = new TPermissionRole() { Name = input.Name };
            roles.Add(role);
            await db.SaveChangesAsync();
            var publicRole = roles.Where(p=>p.Name.Equals("Public")).Include(p=>p.Permissions).First();
            foreach (var perm in publicRole.Permissions)
            {
                var newPermission = new TPermissionTable()
                {
                    Allow = perm.Allow,
                    Method = perm.Method,
                    Namespace = perm.Namespace,
                    Role = role,
                    Table = perm.Table
                };
                role.Permissions.Add(newPermission);
            }
            await db.SaveChangesAsync();
            return role.ToProtoMessage<MPermissionRole>();
        }

        public async Task Delete(DeleteInput input, CallContext context = default)
        {
            IServiceProvider _serviceProvider = context.ServerCallContext.GetHttpContext().RequestServices;
            var db = (DbContext)_serviceProvider.GetRequiredService(MiddlewareOptions.Db);
            var roles = db.Set<TPermissionRole>();
            var role = roles.Where(p => p.Id.Equals(input.Id)).Include(p => p.Permissions).FirstOrDefault();
            if (role == null || role.Name.Equals("Admin") || role.Name.Equals("Authenticated") || role.Name.Equals("Public")) 
            { throw new RpcException(new Status(StatusCode.NotFound, "Entry doesn't exist.")); }

            roles.Remove(role);
            await db.SaveChangesAsync();
        }

        public async Task<List<MPermissionRole>> Fetch(FetchInput input, CallContext context = default)
        {
            IServiceProvider _serviceProvider = context.ServerCallContext.GetHttpContext().RequestServices;
            var db = (DbContext)_serviceProvider.GetRequiredService(MiddlewareOptions.Db);
            var roleTable = db.Set<TPermissionRole>();
            var userDecrypted = context.ValidateBearer();
            int skip = input.Page - 1 < 0 ? 0 : input.Page - 1 * input.Limit;
            var roles = roleTable.OrderBy(p => p.Name).Select(p=>p.ToProtoMessage<MPermissionRole>()).Skip(skip).Take(input.Limit);

            return roles.ToList();
        }

        public async Task<MPermissionRole> FetchOne(FetchOneInput input, CallContext context = default)
        {
            IServiceProvider _serviceProvider = context.ServerCallContext.GetHttpContext().RequestServices;
            var db = (DbContext)_serviceProvider.GetRequiredService(MiddlewareOptions.Db);
            var roleTable = db.Set<TPermissionRole>();
            var role = roleTable.Single(p => p.Id.Equals(input.Id));

            return role.ToProtoMessage<MPermissionRole>();
        }

        public async Task<MPermissionRole> Update(UpdateInput input, CallContext context = default)
        {
            throw new NotImplementedException();
        }
    }
}
