using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    public enum StatoElemento
    {
        Attivo = 1,
        Elaborato = 2,
        Chiuso = 5,
        Annullato = 9
    }
    [Table("Elementi")]
    public partial class Elementi
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public short Revisione { get; set; }
        public string Tipo { get; set; }
        public string Descrizione { get; set; }
        public StatoElemento Stato { get; set; }
        public Guid IdFascicolo { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        [JsonIgnore]        
        public string Attributi { get; set; }
        public string Chiave1 { get; set; }
        public string Chiave2 { get; set; }
        public string Chiave3 { get; set; }
        public string Chiave4 { get; set; }
        public string Chiave5 { get; set; }

        public Guid ElemRif { get; set; }

        //[NotMapped]
        //[Write(false)]
        [JsonIgnore]
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

        //[NotMapped]
        [Write(false)]
        [JsonProperty("AttributiJSON")]
        public JObject jAttributi
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(Attributi);
            }

            set { Attributi = JsonConvert.SerializeObject(value); }
        }


        [JsonIgnore]
        [Write(false)]
        public virtual Fascicoli IdFascicoloNavigation { get; set; }


        [JsonIgnore]
        [Write(false)]
        public virtual TipiElementi TipoNavigation { get; set; }


        public Elementi()
        {
            Id = Guid.NewGuid();
            Revisione = 0;
            Descrizione = "";
            Stato = StatoElemento.Attivo;
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            UtenteC = "";
            UtenteUM = "";
            Chiave1 = "";
            Chiave2 = "";
            Chiave3 = "";
            Chiave4 = "";
            Chiave5 = "";
            Attributi = "";
        }

        public bool SetAttributo(string Nome, dynamic Valore)
        {
            if (elencoAttributi != null)
            {
                if (elencoAttributi.Assegna(Nome, Valore)) {
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

        public DateTime GetDateTime(string Nome, DateTime dflt)
        {
            if (elencoAttributi != null)
            {
                DateTime? v = elencoAttributi.GetDateTime(Nome);
                if (v != null)
                {
                    return (DateTime)v;
                }
            }
            return dflt;
        }

        public bool GetBoolean(string Nome, bool dflt)
        {
            if (elencoAttributi != null)
            {
                bool? v = elencoAttributi.GetBoolean(Nome);
                if (v != null)
                {
                    return (bool)v;
                }
            }
            return dflt;
        }
    }
}
