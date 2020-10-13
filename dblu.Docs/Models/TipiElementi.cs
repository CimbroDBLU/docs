using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{

    [Table("TipiElementi")]
    public partial class TipiElementi
    {
        public TipiElementi()
        {
            Elementi = new HashSet<Elementi>();
            Attributi = new ElencoAttributi();
        }

        [ExplicitKey]
        public string Codice { get; set; }
        public string Descrizione { get; set; }

        public string Processo { get; set; }
        public bool AggregaAElemento { get; set; }

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
        public virtual ICollection<Elementi> Elementi { get; set; }

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
                if (value!=null)
                {
                    if (Attributi == null) { Attributi = new ElencoAttributi(); }
                    Attributi.FromList(value);
                }

            }
        }

        public string Categoria { get; set; }

        [Write(false)]
        [JsonIgnore]
        public virtual Categorie CategoriaNavigation { get; set; }

    }
}
