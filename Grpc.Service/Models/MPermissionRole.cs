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
    public class MPermissionRole
    {
        [ProtoMember(1)]
        [JsonProperty]
        public virtual int Id { get; set; }

        [ProtoMember(2)]
        [JsonProperty]
        public string Name { get; set; }

        [ProtoMember(3)]
        [JsonProperty(IsReference = true, ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore, ItemIsReference = true)]
        public virtual List<MPermission> Permissions { get; set; } = new List<MPermission>();
    }
}
