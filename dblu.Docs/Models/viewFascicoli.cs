using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    [Table("vListaFascicoli")]
    public partial class viewFascicoli
    {

        public enum vStati { 
            Attivo=1,
            Chiuso=5,
            Annullato=9
        }

        public viewFascicoli()
        {
            IdFascicolo = Guid.NewGuid();
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            CodiceSoggetto = null;
            NomeSoggetto = null;
            DscFascicolo = "";
            CodCategoria="";
            DscCategoria = "";
            UtenteC = "";
            DataC = DateTime.Now;
            UtenteUM = "";
            DataUM = DateTime.Now;
            Campo1= "";
            Campo2= "";
            Campo3= "";
            Campo4= "";
            Campo5= "";
            Campo6= "";
            Campo7= "";
            Campo8= "";
            Campo9= "";
            Campo10= "";
            Allegati = new HashSet<viewAllegati>();
            Elementi = new HashSet<viewElementi>();
        }

        [ExplicitKey] 
        public Guid IdFascicolo { get; set; }
        public string DscFascicolo { get; set; }
      
        public string Campo1 { get; set; }
        public string Campo2 { get; set; }
        public string Campo3 { get; set; }
        public string Campo4 { get; set; }
        public string Campo5 { get; set; }
        public string Campo6 { get; set; }
        public string Campo7 { get; set; }
        public string Campo8 { get; set; }
        public string Campo9 { get; set; }
        public string Campo10 { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        public string Attributi { get; set; }
        public string CodiceSoggetto { get; set; }
        public string NomeSoggetto { get; set; }
        public string CodCategoria { get; set; }
        public string DscCategoria { get; set; }

      
        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<viewAllegati> Allegati { get; set; }

        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<viewElementi> Elementi { get; set; }

    
    }
}
