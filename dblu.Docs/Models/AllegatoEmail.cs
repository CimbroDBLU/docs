using dblu.Docs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    public class AllegatoEmail : Allegati
    {
  
        public string Mittente { get; set; }

        public string Destinatario { get; set; }
 

    }
}
