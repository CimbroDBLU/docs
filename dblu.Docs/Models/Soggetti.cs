using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using dblu.Docs.Interfacce;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{

    public enum StatoSoggetto
    {
        Attivo = 1,
        Chiuso = 5,
        Annullato = 9
    }


    [Table("Soggetti")]
    public partial class Soggetti : ISoggetti
    {
        [ExplicitKey]
          public string Codice { get; set; }
        
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        //public Guid Id { get; set; }

        public string Nome { get; set; }
        public string Indirizzo { get; set; }
        public string CAP { get; set; }
        public string Localita { get; set; }
        public string Provincia { get; set; }
        public string Nazione { get; set; }
        public string Note { get; set; }
        public StatoSoggetto Stato { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        public string Nomignolo { get; set; }
        public string PartitaIVA { get; set; }
        public string Attributi { get; set; }
        public string NuovoCodice { get; set; }

        [JsonIgnore]
        //[Write(false)]
        private ElencoAttributi _lista;

        //[NotMapped]
        [Write(false)]
        [JsonIgnore]
        public ElencoAttributi elencoAttributi
        {
            get
            {
                return _lista;
            }
            set
            {
                _lista = value;
                if (_lista != null)
                    _lista.ResetValori();
            }
        }

        [Write(false)]
        public virtual ICollection<Fascicoli> Fascicoli { get; set; }

        [Write(false)] 
        public virtual ICollection<EmailSoggetti> Emails { get; set; }

        public Soggetti()
        {
            //Id = Guid.NewGuid();
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            Attributi = "";
            UtenteC = "";
            UtenteUM = "";
            Fascicoli = new HashSet<Fascicoli>();
            Emails = new HashSet<EmailSoggetti>();
        }

        public bool SetAttributo(string Nome, dynamic Valore)
        {
            if (Attributi != null)
            {
                if (elencoAttributi.Assegna(Nome, Valore))
                {
                    return true;
                }
            }
            return false;
        }

        public dynamic GetAttributo(string Nome)
        {
            if (Attributi != null)
            {
                return elencoAttributi.Get(Nome);
            }
            return null;
        }

    }
}
