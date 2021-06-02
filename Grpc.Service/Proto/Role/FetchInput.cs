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
    public class FetchInput
    {
        /// <summary>
        /// Limit of rows (Minimum: 10, Maximum: 1000)
        /// </summary>
        [ProtoMember(1)]
        [Range(10, 1000, ErrorMessage = "This field is must be valid int and cannot be negative")]
        [Required(ErrorMessage = "This field is required")]
        public int Limit { get; set; }

        /// <summary>
        /// Page from 1 to 10000. If not found, 
        /// </summary>
        [ProtoMember(2)]
        [Range(1, 10000, ErrorMessage = "Page must be between 1 to 10,000.")]
        [Required(ErrorMessage = "This field is required")]
        public int Page { get; set; } = 1;

    }
}
