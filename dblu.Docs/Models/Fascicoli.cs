using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    [Table("Fascicoli")]
    public partial class Fascicoli
    {

        public enum Stati { 
            Attivo=1,
            Chiuso=5,
            Annullato=9
        }

        public Fascicoli()
        {
            Id = Guid.NewGuid();
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            CodiceSoggetto = null;
            Descrizione = "";
            vAllegati = new HashSet<viewAllegati>();
            vElementi = new HashSet<viewElementi>();
            Attributi = "";
            UtenteC = "";
            UtenteUM = "";
            Chiave1 = "";
            Chiave2 = "";
            Chiave3 = "";
            Chiave4 = "";
            Chiave5 = "";
        }

        [ExplicitKey] 
        public Guid Id { get; set; }
        public string Descrizione { get; set; }
        public string Categoria { get; set; }
        public string Chiave1 { get; set; }
        public string Chiave2 { get; set; }
        public string Chiave3 { get; set; }
        public string Chiave4 { get; set; }
        public string Chiave5 { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        public string Attributi { get; set; }
        public string CodiceSoggetto { get; set; }

        //[NotMapped]
        //[Write(false)]
        [JsonIgnore]
        private ElencoAttributi _lista;

        //[NotMapped]
        [Write(false)]
        [JsonIgnore]
        public ElencoAttributi elencoAttributi
        {
            get {
                return _lista;
            }
            set {
                _lista = value;
                if (_lista != null)
                    _lista.ResetValori();
            } 
        }

        [JsonIgnore]
        [Write(false)]
        public virtual Categorie CategoriaNavigation { get; set; }
        
        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<Allegati> Allegati { get; set; }
        
        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<Elementi> Elementi { get; set; }

        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<viewAllegati> vAllegati { get; set; }
    
        [JsonIgnore]
        [Write(false)]
        public virtual ICollection<viewElementi> vElementi { get; set; }
        //[JsonIgnore]
        //public virtual Soggetti CodiceSoggettoNavigation { get; set; }


        public bool SetAttributo(string Nome, dynamic Valore)
        {
            if (elencoAttributi != null)
            {
                if (elencoAttributi.Assegna(Nome, Valore))
                {
                    switch (elencoAttributi.Valori[Nome].Alias)
                    {
                        case "Chiave1":
                            Chiave1 = Valore.ToString();
                            break;
                        case "Chiave2":
                            Chiave2 = Valore.ToString();
                            break;
                        case "Chiave3":
                            Chiave3 = Valore.ToString();
                            break;
                        case "Chiave4":
                            Chiave4 = Valore.ToString();
                            break;
                        case "Chiave5":
                            Chiave5 = Valore.ToString();
                            break;
                        default:
                            break;
                    }
                    return true;
                }
            }
            return false;
        }

        public dynamic GetAttributo(string Nome)
        {
            if (elencoAttributi != null)
            {
                return elencoAttributi.Get(Nome);
            }
            return null;
        }

        public dynamic GetAttributo(string Nome, dynamic dflt)
        {
            dynamic v = GetAttributo(Nome);
            if (v != null)
            {
                return v;
            }
            return dflt;
        }
    }
}
