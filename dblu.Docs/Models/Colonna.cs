using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    public partial class Colonna
    {

        public Colonna()
        {
            Des = "";
            Visible = false;
            Field = "";
        }

       
        public string Des { get; set; }
        public bool Visible { get; set; }
       
        public string Field { get; set; }
    }
}
