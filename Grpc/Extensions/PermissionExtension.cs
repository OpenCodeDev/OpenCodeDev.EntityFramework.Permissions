using Microsoft.EntityFrameworkCore;
using OpenCodeDev.EntityFramework.Permissions.Grpc.DataAnnotations;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Tables;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Extension
{
    internal sealed class PermissionTransit{
        public string[] Roles { get; set; }
        public bool Forced { get; set; }

        public string Method { get; set; }
        public string Table { get; set; }
        public string Namespace { get; set; }
    }
    public static class PermissionExtension
    {
        /// <summary>
        /// Search and list all the Grpc Actions linked to the assembly. Any new OperationContract will be taken.
        /// </summary>
        private static List<PermissionTransit> GeneratePermissions(bool codeFirst = false)
        {
            var list = new List<PermissionTransit>();
            foreach (var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                var loaded = Assembly.Load(assembly);
                foreach (var classType in loaded.GetTypes())
                {
                    foreach (var serviceContractAttr in classType.GetCustomAttributes(typeof(ServiceContractAttribute)))
                    {
                        foreach (var member in classType.GetMembers())
                        {
                            foreach (var opContractAttr in member.GetCustomAttributes(typeof(OperationContractAttribute)))
                            {
                                if (member.MemberType == MemberTypes.Method)
                                {
                                    var permisionRoles = (OperationContractRolesAttribute) classType.GetCustomAttribute(typeof(OperationContractRolesAttribute)) ;
                                  
                                    list.Add(new PermissionTransit() {
                                        Forced = codeFirst,
                                        Roles = permisionRoles != null ? permisionRoles.Roles : null,
                                        Method = member.Name,
                                        Table = classType.Name,
                                        Namespace = classType.Namespace
                                });
                                }

                            }
                        }



                        //foreach (var item in collection)
                        //{

                        //}

                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Ensured to create inexistant permission and delete unrequired ones. <br />
        /// Note: Overload is not supported. <br/>
        /// When creating a ServiceContract, You cannot create 2 OperationContract with the same name (overload) or unexpected permission issue will occur.<br/>
        /// Overloads may result to security breach due to permission confusion.
        /// </summary>
        /// <param name="db">Database instance where Permissions should be created</param>
        /// <param name="_defaultRoles">Admin, Authenticate, Public and Self are Mandatory and will be automatically added if not defined</param>
        public static void EnsurePermissionsCreated(this DbContext db, string[] _defaultRoles = null )
        {
            var setPR = db.Set<TPermissionRole>();
            List<string> defaultRoles = _defaultRoles != null ? _defaultRoles.ToList() : new List<string>();
            if (!defaultRoles.Contains("Admin")) { defaultRoles.Add("Admin"); }
            if (!defaultRoles.Contains("Authenticated")) { defaultRoles.Add("Authenticated"); }
            if (!defaultRoles.Contains("Public")) { defaultRoles.Add("Public"); }



            // Create Default Unremovable roles
            foreach (var role in defaultRoles)
            {
                var dbRole = setPR.Where(p => p.Name.Equals(role)).FirstOrDefault();
                if (dbRole == null)
                {
                    var nRole = new TPermissionRole() { Name = role };
                    setPR.Add(nRole);                    
                }
            }

            db.SaveChanges();
            // Load All Available Permissions
            var permissions = GeneratePermissions();
            var dbRoles = setPR.Select(p => p).Include(p => p.Permissions);
            // For Each Roles, Including the one added by admin
            foreach (var role in dbRoles.ToList())
            {
                // Select permission to remove since no longer in use
                var toDelete = role.Permissions.ToList()
                .Where(p => permissions.Count(b => b.Namespace.Equals(p.Namespace) && b.Method.Equals(p.Method) && b.Table.Equals(p.Table)) <= 0)
                .ToList();

                toDelete.ForEach(p => {
                    Console.WriteLine($"{p.Table}->{p.Method}-> Removed from {role.Name}");
                    role.Permissions.Remove(p);

                });

                db.SaveChanges();

                var toAdd = permissions
                .Where(x =>
                role.Permissions.ToList().Count(p => p.Namespace.Equals(x.Namespace) && p.Method.Equals(x.Method) && p.Table.Equals(x.Table)) <= 0)
                .ToList();
                toAdd.ForEach(p => {
                    var perm = new TPermissionTable()
                    {
                        Method = p.Method,
                        Role = role,
                        Namespace = p.Namespace,
                        Table = p.Table,
                        Allow = role.Name.Equals("Admin") ? true : p.Roles != null ? p.Roles.Contains(role.Name) : false
                    };

                    Console.WriteLine($"{perm.Table}->{perm.Method}-> Added to {role.Name}");
                    role.Permissions.Add(perm);
                });
                

                if (!role.Name.Equals("Admin"))
                {
                    permissions.ForEach(x=> {
                        var perm = role.Permissions.Where(p => p.Namespace.Equals(x.Namespace) && p.Method.Equals(x.Method) && p.Table.Equals(x.Table)).FirstOrDefault();
                        if (perm != null && x.Forced)
                        {                            perm.Allow = x.Roles != null ? x.Roles.Contains(role.Name) : false;
                        }
                        });

                }
                db.SaveChanges();
            }
            
            dbRoles.ToList().ForEach(p => { Console.WriteLine($"{p.Name} has {p.Permissions.Count} Permissions"); });
        }
       
        /// <summary>
        /// Context Builder for Permissions
        /// </summary>
        /// <param name="builder">ModelBuilder Instance</param>
        public static void BuildPermissionModel(this ModelBuilder builder)
        {
            builder.Entity<TPermissionRole>().HasMany(p => p.Permissions).WithOne(p => p.Role);
            builder.Entity<TPermissionRole>().HasIndex(u => u.Id).IsUnique();
            builder.Entity<TPermissionTable>().HasIndex(u => u.Id).IsUnique();
        }

        public static void ThrowFailedPermission(this CallContext context){
            
        }
    }
}
