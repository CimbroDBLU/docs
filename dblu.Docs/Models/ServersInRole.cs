using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    [Table("ServersInRole")]
    public partial class ServersInRole
    {
        [ExplicitKey]
        public string idServer { get; set; }
        public string RoleId { get; set; }
    }

   
}
