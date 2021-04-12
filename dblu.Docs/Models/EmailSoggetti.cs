using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

namespace dblu.Docs.Models
{
    [Table("EmailSoggetti")]
    public partial class EmailSoggetti
    {
        //[ExplicitKey]
        public string Email { get; set; }
        public string CodiceSoggetto { get; set; }

/*
        [JsonIgnore]
        public virtual Soggetti CodiceSoggettoNavigation { get; set; }

*/
    }
}
