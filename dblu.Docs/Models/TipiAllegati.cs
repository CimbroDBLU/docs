using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace dblu.Docs.Models
{
    public class CleanSchedule
    {
        public string CronExp { get; set; }

        public int State { get; set; }

        public int RetentionDays { get; set; }
    }


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
        public string ListaCancellazioni { get; set; }

        [Write(false)]
        [JsonIgnore]
        public IEnumerable<CleanSchedule> _listaCancellazioni
        {
            get
            {
                try
                {
                    if(string.IsNullOrEmpty(ListaCancellazioni)) return new List<CleanSchedule>();
                    return JsonConvert.DeserializeObject<List<CleanSchedule>>(ListaCancellazioni);
                }
                catch (Exception) { return new List<CleanSchedule>(); }
            }

            set
            {
                try
                {
                    ListaCancellazioni = JsonConvert.SerializeObject(value);
                }catch(Exception) { ListaCancellazioni = ""; }
            }
        }        
       

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
