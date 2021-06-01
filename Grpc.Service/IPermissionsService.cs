using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ProtoBuf.Grpc;
using OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Proto.Permission;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Service
{
    [ServiceContract]
    public interface IPermissionsService
    {

        [OperationContract]
        Task Fetch (FetchInput input, CallContext context = default);

        [OperationContract]
        Task FetchOne (FetchOneInput input, CallContext context = default);

        [OperationContract]
        Task Update (UpdateInput input, CallContext context = default);

    }
}
