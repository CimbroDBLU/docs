using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    public enum StatoAllegato
    {
        DaSmistare = -1,
        Attivo = 1,
        Elaborato = 2,
        Stampato = 3,
        Chiuso = 5,
        Spedito = 7,
        Annullato = 9
    }

    [Table("Allegati")]
    public partial class Allegati
    {
        
        [ExplicitKey]
        public Guid Id { get; set; }
        public string Descrizione { get; set; }
        public string NomeFile { get; set; }
        public string Tipo { get; set; }
        public StatoAllegato Stato { get; set; }
        public Guid? IdFascicolo { get; set; }
        public Guid? IdElemento { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        public string Attributi { get; set; }
        public string Note { get; set; }
        public string Chiave1 { get; set; }
        public string Chiave2 { get; set; }
        public string Chiave3 { get; set; }
        public string Chiave4 { get; set; }
        public string Chiave5 { get; set; }
        public string Origine { get; set; }
        public string Testo { get; set; }

        //[NotMapped]
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
                if (_lista!=null)
                    _lista.ResetValori();
            }
        }

        //[NotMapped]
        [Write(false)]
        public JToken jNote { 
            get { return string.IsNullOrEmpty(Note) ? null : JsonConvert.DeserializeObject<JToken>(Note); }
            set { Note = value==null ? "" : JsonConvert.SerializeObject(value); }
        }

        [JsonIgnore]
        [Write(false)]
        public virtual Fascicoli IdFascicoloNavigation { get; set; }
        
        [JsonIgnore]
        [Write(false)]
        public virtual TipiAllegati TipoNavigation { get; set; }

        [Write(false)]
        public TipoOperazione LastOp { get; set; }
        
        public Allegati()
        {
            Id = Guid.NewGuid();
            Descrizione = "";
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            IdFascicolo = null;
            IdElemento = null;
            Attributi = "";
            Note = "";
            UtenteC = "";
            UtenteUM = "";
            Chiave1 = "";
            Chiave2 = "";
            Chiave3 = "";
            Chiave4 = "";
            Chiave5 = "";
            Origine = "";
            LastOp = TipoOperazione.Nessuna;
        }
                
        public bool SetAttributo(string Nome, dynamic Valore) {
            if (elencoAttributi != null) {
                if (elencoAttributi.Assegna(Nome, Valore)) {
                    string s = Valore.ToString();
                    if (s.Length > 255)
                        s = s.Substring(0, 255);
                    switch (elencoAttributi.Valori[Nome].Alias)
                    {
                        case "Chiave1":
                            Chiave1 = s;
                            break;
                        case "Chiave2":
                            Chiave2 = s;
                            break;
                        case "Chiave3":
                            Chiave3 = s;
                            break;
                        case "Chiave4":
                            Chiave4 = s;
                            break;
                        case "Chiave5":
                            Chiave5 = s;
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
            if (Attributi != null)
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

    }




}
