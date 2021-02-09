using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{

    [Table("TipiAllegati")]
    public partial class TipiAllegati
    {
        public TipiAllegati()
        {
            Allegati = new HashSet<Allegati>();
            Attributi = new ElencoAttributi();
        }

        [ExplicitKey]
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string Cartella { get; set; }

        public string Estensione { get; set; }

        //[NotMapped]
        [Write(false)]
        public ElencoAttributi Attributi { get; set; }

        public string ListaAttributi
        {
            get
            {
                return Attributi.GetLista();
            }

            set
            {
                if (Attributi == null) { Attributi = new ElencoAttributi(); }
                Attributi.SetLista(value);
            }
        }
        
        public string ViewAttributi { get; set; }

        [JsonIgnore]
        [Write(false)] 
        public virtual ICollection<Allegati> Allegati { get; set; }

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
                if (value != null)
                {
                    if (Attributi == null) { Attributi = new ElencoAttributi(); }
                    Attributi.FromList(value);
                }
            }
        }
    }
}
