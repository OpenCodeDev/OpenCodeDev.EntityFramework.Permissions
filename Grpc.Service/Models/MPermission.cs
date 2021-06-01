using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Models
{
    [ProtoContract]
    [Serializable]
    public class MPermission
    {
        [ProtoMember(1)]
        [JsonProperty]
        public virtual int Id { get; set; }

        [ProtoMember(2)]
        [JsonProperty]
        public virtual string Namespace { get; set; }

        [ProtoMember(3)]
        [JsonProperty]
        public virtual string Table { get; set; }

        [ProtoMember(4)]
        [JsonProperty]
        public virtual string Method { get; set; }

        [ProtoMember(5)]
        [JsonProperty]
        public virtual bool Allow { get; set; }

        [ProtoMember(6)]
        [JsonProperty]
        public virtual MPermission Permission { get; set; }
    }
}
