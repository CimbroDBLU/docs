using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    [Table("Categorie")] 
    public partial class Categorie
    {
       
        public Categorie()
        {
            Fascicoli = new HashSet<Fascicoli>();
            Attributi = new ElencoAttributi();
            TipiElementi = new HashSet<TipiElementi>();
        }

        [ExplicitKey]
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        //[NotMapped]
        [Write(false)]
        public ElencoAttributi Attributi { get; set; }

        public string ListaAttributi
        {
            get
            {
                return Attributi.GetLista();
            }

            set { 
                if (Attributi == null) { Attributi = new ElencoAttributi(); }
                Attributi.SetLista(value);
            }
        }

        public string ViewAttributi { get; set; }

        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<Fascicoli> Fascicoli { get; set; }

        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<TipiElementi> TipiElementi { get; set; }


        //[NotMapped]
        [Write(false)]
        [JsonIgnore]
        public IEnumerable<Attributo> _listaAttributi
        {
            get
            {
                return Attributi.ToList();
            }

            set
            {
                Attributi.FromList(value);
            }
        }
    }
}
