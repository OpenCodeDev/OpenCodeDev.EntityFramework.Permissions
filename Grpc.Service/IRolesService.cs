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
    public interface IRolesService
    {
        [OperationContract]
        Task Create(FetchInput input, CallContext context = default);

        [OperationContract]
        Task Delete(FetchInput input, CallContext context = default);

        [OperationContract]
        Task Fetch(FetchInput input, CallContext context = default);

        [OperationContract]
        Task FetchOne(FetchOneInput input, CallContext context = default);

        [OperationContract]
        Task Update(UpdateInput input, CallContext context = default);
    }
}
