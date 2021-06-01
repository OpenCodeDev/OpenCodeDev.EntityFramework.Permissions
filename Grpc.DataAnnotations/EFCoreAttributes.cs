using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.DataAnnotations
{

    [AttributeUsage(AttributeTargets.Method)]
    public class OperationContractRolesAttribute : Attribute
    {
        public string[] Roles;

        public OperationContractRolesAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
