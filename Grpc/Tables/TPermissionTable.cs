using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Tables
{
    public class TPermissionTable
    {
        [Key]
        public int Id { get; set; }
        [Column]
        public string Namespace { get; set; }
        [Column]
        public string Table { get; set; }
        [Column]
        public string Method { get; set; }
        [Column]
        public bool Allow { get; set; }

        [Column]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public TPermissionRole Role { get; set; }
    }
}
