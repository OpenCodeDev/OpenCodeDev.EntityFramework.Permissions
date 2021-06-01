using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OpenCodeDev.EntityFramework.Permissions.Grpc.Tables
{
    public class TPermissionRole
    {
        [Key]
        public int Id { get; set; }

        [Column]
        public string Name { get; set; }

        public ICollection<TPermissionTable> Permissions { get; set; }
    }
}
