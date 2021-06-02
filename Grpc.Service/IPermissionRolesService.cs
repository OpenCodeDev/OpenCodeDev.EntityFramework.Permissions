using OpenCodeDev.EntityFramework.Permissions.Grpc.DataAnnotations;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Models;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Proto.Role;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Service
{
    [ServiceContract]
    public interface IPermissionRolesService
    {
        [OperationContract]
        [OperationContractRoles("Admin")]
        Task<MPermissionRole> Create(CreateInput input, CallContext context = default);

        [OperationContract]
        [OperationContractRoles("Admin")]
        Task Delete(DeleteInput input, CallContext context = default);

        [OperationContract]
        [OperationContractRoles("Admin")]
        Task<List<MPermissionRole>> Fetch(FetchInput input, CallContext context = default);

        [OperationContract]
        [OperationContractRoles("Admin")]
        Task<MPermissionRole> FetchOne(FetchOneInput input, CallContext context = default);

        [OperationContract]
        [OperationContractRoles("Admin")]
        Task<MPermissionRole> Update(UpdateInput input, CallContext context = default);
    }
}
