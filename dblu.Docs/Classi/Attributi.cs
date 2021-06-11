using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace dblu.Docs.Classi
{

    public class Attributo
    {
        public Attributo()
        {
            this.Duplicabile = true;
        }

        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public string Alias { get; set; }
        
        [JsonIgnore]
        internal string _tipo { get; set; }
        [JsonIgnore]
        internal Type _type { get; set; }
        
        public string Tipo {
            get {

                return this._tipo;
            }
            set {
                try {
                    if (this._tipo != value) {
                        this._tipo = value;
                        SystemType = System.Type.GetType(value);
                    }
                }
                catch { this._tipo = ""; };
            } 
        }
        [JsonIgnore]
        public System.Type SystemType { 
            get { return _type; }

            set {

                if (_type != value) {
                    _type = value;
                    if (string.IsNullOrEmpty(this._tipo)){
                        this._tipo = value.FullName;
                    }
                }
            } 
        }
        public Visibilita_Attributi Visibilità { get; set; }
        public bool Obbligatorio { get; set; }
        public bool Duplicabile { get; set; }
        public string ValorePredefinito { get; set; }
        public string Sequenza { get; set; }



        //[JsonIgnore]
        public dynamic Valore { get; set; }
    }

    public enum TipiOggetto
    {
        ELEMENTO,
        FASCICOLO,
        ALLEGATO
    }

    public enum Visibilita_Attributi
    {
        EDITABLE,
        HIDDEN,
        VISIBLE
    }

    public class ElencoAttributi 
    {
        public Dictionary<string, Attributo> Valori { get; set; }

        public ElencoAttributi() {
            Valori = new Dictionary<string, Attributo>();
        }
        public int Count() {
            if (Valori == null) { return 0; }
            return Valori.Count;
        }

        public IEnumerable<string> Nomi() {
            return Valori.Keys.ToList();
        }

        public dynamic Get(string Nome)
        {
            try
            {
                if (Valori.ContainsKey(Nome))
                {
                    return Valori[Nome].Valore;
                }
            }
            catch
            {
                
            }
            return null;
        }

        public DateTime? GetDateTime(string Nome)
        {
            try
            {
                if (Valori.ContainsKey(Nome))
                {
                   
                    dynamic v = Valori[Nome].Valore;
                    DateTime? dt = null;
                    if (v != null)
                        if (v.GetType() == typeof(DateTime))
                        {
                            dt = (DateTime)v;
                        }
                        else
                        {
                            DateTime dt1;
                            DateTime.TryParse(v, out dt1);
                            dt = dt1;
                        }
                    return dt;
                }
            }
            catch
            {

            }
            return DateTime.Today;
        }
        public bool? GetBoolean(string Nome)
        {
            try
            {
                if (Valori.ContainsKey(Nome))
                {
                    dynamic v = Valori[Nome].Valore;
                    bool? b = null;
                    if (v!= null && v.GetType() == typeof(bool))
                    {
                        b = (bool)v;
                    }
                    else
                    {
                        b = System.Convert.ToBoolean(v);
                    }
                    return b;
                }
            }
            catch
            {

            }
            return null;
        }

        public bool Add(Attributo a)
        {
            bool bres = false;
            try
            {
                if (! Valori.ContainsKey(a.Nome))
                {
                    Valori.Add(a.Nome, a);
                    bres = true;
                }
            }
            catch
            {
                bres = false;
            }
            return bres;
        }
        public bool Remove(string Nome)
        {
            bool bres = false;
            try
            {
                if (Valori.ContainsKey(Nome))
                {
                    Valori.Remove(Nome);
                    bres = true;
                }
            }
            catch
            {
                bres = false;
            }
            return bres;
        }

        public bool Assegna(string Nome, dynamic valore) {
            bool bres = false;
            try
            {
                if (Valori.ContainsKey(Nome))
                {
                    Attributo a = Valori[Nome];
                    if (valore is null) {
                        if (a.Obbligatorio)
                        {
                            return false;
                        }
                        else {
                            a.Valore = null;
                        }
                    }
                    else { 
                        if (a.SystemType.IsAssignableFrom(valore.GetType()))
                        {
                            a.Valore = valore;
                            bres = true;
                        }
                        else
                        {
                            switch (a.Tipo)
                            {
                                case "System.DateTime":
                                    DateTime dt;
                                    if (DateTime.TryParse(valore.ToString(), out dt))
                                    {
                                        a.Valore = dt;
                                        bres = true;
                                    }
                                    break;
                                case "System.Boolean":
                                    a.Valore = System.Convert.ToBoolean(valore);
                                    break;
                                case "System.Double":
                                    double v = 0;
                                    double.TryParse(valore, NumberStyles.Number, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out v);
                                    a.Valore = v;                                    
                                    break;
                                case "System.Int32":
                                    a.Valore = System.Convert.ToInt32(valore, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                                    break;
                                 case "System.Int64":
                                    a.Valore = System.Convert.ToInt64(valore, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                                    break;
                                case "System.String":
                                    a.Valore = valore.ToString();
                                    break;
                                default:
                                    a.Valore = valore.ToString();
                                    break;
                            }
                        }
                    }
                }
            }
            catch {
                bres = false;
            }
            return bres;
        }


        public bool SetLista(string jAttributi)
        {

            bool bres = true;
            try
            {
                if (!string.IsNullOrEmpty(jAttributi))
                {
                    this.Valori = JsonConvert.DeserializeObject<Dictionary<string, Attributo>>(jAttributi);
                }
                else {
                    this.Valori = new Dictionary<string, Attributo>();
                }
            } 
            catch(Exception ex)
            {
                bres = false;
            }
            return bres;
        }

        public string  GetLista()
        {
            string sres = "";
            try
            {
                sres = JsonConvert.SerializeObject(this.Valori);
            }
            catch
            {
                sres = "";
            }
            return sres;
        }
        public bool SetValori(string jValori) {

            bool bres = true;
            try
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jValori);
                foreach (KeyValuePair<string, dynamic> d in values)
                {
                    try
                    {
                        this.Valori[d.Key].Valore = d.Value;
                    }
                    catch (Exception ex){
                        var Message = ex.Message;
                    }
                }
            }
            catch
            {
                bres = false;
            }
            return bres;
        }

        public string GetValori() {
            string sres = "";
            try
            {
                var values = new Dictionary<string, dynamic>();

                foreach (KeyValuePair<string, Attributo> a in this.Valori)
                {
                    values.Add(a.Key, a.Value.Valore);
                }
                sres = JsonConvert.SerializeObject(values);
            }
            catch
            {
                sres = "";
            }
            return sres;
        }

        public IEnumerable<Attributo> ToList() { 
            return Valori.Values.ToList();
        }
        public bool FromList( IEnumerable<Attributo> values )
        {
            bool bres = true;
            try
            {
                foreach (Attributo d in values)
                {
                    if (!this.Assegna(d.Nome, d))
                    {
                        bres = false;
                    }
                }
            }
            catch
            {
                bres = false;
            }
            return bres;
        }


        public string Descrizione(string Nome)
        {
            var dsc = Nome;
            try
            {
                if (Valori.ContainsKey(Nome))
                    dsc = Valori[Nome].Descrizione;
            }
            catch { 
           
            }
            return dsc;
        }

        public string DescrizioneChiave(string Chiave)
        {
            var dsc = Chiave;
            KeyValuePair<string,Attributo> a = this.Valori.Where(item => item.Value.Alias == Chiave).FirstOrDefault();
            if (a.Value != null) {
                dsc = a.Value.Descrizione;
            }
            return dsc;
        }

        public bool ResetValori()
        {

            bool bres = true;
            try
            {
                foreach (KeyValuePair<string, Attributo> d in this.Valori)
                {
                    var v = this.Valori[d.Key];
                    v.Valore = v.ValorePredefinito;
                }
            }
            catch
            {
                bres = false;
            }
            return bres;
        }

    }
}
