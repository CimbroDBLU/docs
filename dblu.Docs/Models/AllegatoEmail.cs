using dblu.Docs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    public class AllegatoEmail : Allegati
    {

        private string _mittente;

        private string _destinatario;

        private string _oggetto { get; set; }

        private string _messageId { get; set; }

        private DateTime?  _data { get; set; }

        public string Mittente {  
            get {
                if (string.IsNullOrEmpty(_mittente) && elencoAttributi != null) {
                    _mittente = elencoAttributi.Get("Mittente");
                }
                return _mittente;
            } 
            set {
                _mittente = value;
                if (elencoAttributi!=null)
                    base.SetAttributo("Mittente",value);
            } 
        }

        public string Destinatario
        {
            get
            {
                if (string.IsNullOrEmpty(_destinatario) && elencoAttributi != null)
                {
                    _destinatario = elencoAttributi.Get("Destinatario");
                }
                return _destinatario;
            }
            set
            {
                _destinatario = value;
                if (elencoAttributi != null)
                    base.SetAttributo("Destinatario", value);
            }
        }

        public string Oggetto
        {
            get
            {
                if (string.IsNullOrEmpty(_oggetto) && elencoAttributi != null)
                {
                    _oggetto = elencoAttributi.Get("Oggetto");
                }
                return _oggetto;
            }
            set
            {
                _oggetto = value;
                if (elencoAttributi != null)
                    base.SetAttributo("Oggetto", value);
            }
        }
        public string MessageId 
       {
            get
            {
                if (string.IsNullOrEmpty(_messageId) && elencoAttributi != null)
                {
                    _messageId = elencoAttributi.Get("MessageId");
                }
                return _messageId;
            }
            set
            {
                _messageId = value;
                if (elencoAttributi != null)
                    base.SetAttributo("MessageId", value);
            }
        }

        public DateTime? Data
        {
            get
            {
                if (string.IsNullOrEmpty(_messageId) && elencoAttributi != null)
                {
                    _data = elencoAttributi.GetDateTime("Data");
                }
                return _data;
            }
            set
            {
                _data = value;
                if (elencoAttributi != null)
                    base.SetAttributo("Data", value);
            }
        }

        public static  string SqlAttributi( string prefissoTabella="")
        {
                List<string> nomi = new List<string>{ "Mittente", "Destinatario", "Oggetto", "MessageId", "Data" };
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(prefissoTabella))
                    prefissoTabella = prefissoTabella + ".";
                else
                    prefissoTabella = "";
                foreach (var nome in nomi) {
                    sb.Append($",  JSON_VALUE({prefissoTabella}Attributi,'$.{nome}') {nome}");
                }
                return sb.ToString().Substring(1);
        }

    }
}
