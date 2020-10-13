using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    [Table("AllegatiInRoles")]
    public partial class AllegatiInRoles
    {
        [ExplicitKey]
        public string Tipo { get; set; }
        public string RoleId { get; set; }
    }
}
