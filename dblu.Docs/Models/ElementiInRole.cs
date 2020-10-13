using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    [Table("ElementiInRoles")]
    public partial class ElementiInRoles
    {
        [ExplicitKey]
        public string Tipo { get; set; }
        public string RoleId { get; set; }
    }
}
