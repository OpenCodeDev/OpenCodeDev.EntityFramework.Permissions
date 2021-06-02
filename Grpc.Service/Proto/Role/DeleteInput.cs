using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Service.Proto.Role
{
    [ProtoContract]
    public class DeleteInput
    {
        [ProtoMember(1)]
        [Range(0, int.MaxValue, ErrorMessage = "This field is must be valid int and cannot be negative")]
        [Required(ErrorMessage = "This field is required")]
        public int Id { get; set; }
    }
}
