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
        public int Posizione { get; set; }
        public string Processo { get; set; }
        public string RuoliCandidati { get; set; }
        public string UtentiCandidati { get; set; }
        public bool AggregaAElemento { get; set; }

        public bool Abilita { get; set; }

        public string ListaCancellazioni { get; set; }

        [Write(false)]
        [JsonIgnore]
        public IEnumerable<CleanSchedule> _listaCancellazioni
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(ListaCancellazioni)) return new List<CleanSchedule>();
                    return JsonConvert.DeserializeObject<List<CleanSchedule>>(ListaCancellazioni);
                }
                catch (Exception) { return new List<CleanSchedule>(); }
            }

            set
            {
                try
                {
                    ListaCancellazioni = JsonConvert.SerializeObject(value);
                }
                catch (Exception) { ListaCancellazioni = ""; }
            }
        }

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
