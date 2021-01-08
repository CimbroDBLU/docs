using BPMClient;
using dblu.Docs.Extensions;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.ViewModels
{

    public class MailVM
    {
        public bool isInsideTask { get; set; }
        public Guid id { get; set; }

    }

    public class inArrivoView
    {
       
        //public string Server { get; set; }
        public string Ruolo { get; set; }
    }


    public class EmailAttachments
    {
        public string Id { get; set; }
        public string NomeFile { get; set; }
        public bool Valido { get; set; }
        public bool Incluso { get; set; }

        public string IsIncluso {
            get {
                return Incluso ? "Si" : "No";
            }
            set {
                
            }
        }

        public EmailAttachments() { 
        }

    }

    public class LiteMailViewModel { 
    
        public string IdAllegato { get; set; }
        public string TestoEmail { get; set; }
        public string CodiceSoggetto { get; set; }
        public string NomeSoggetto { get; set; }
        public string IdFascicolo { get; set; }
        public string IdElemento { get; set; }
        public string DescrizioneElemento{ get; set; }
        public string DescrizioneFascicolo { get; set; }
        public StatoElemento Stato { get; set; }
        public IEnumerable<EmailAttachments> FileAllegati { get; set; }
        public IEnumerable<TipiElementi> ListaTipiElementi { get; set; }
        //public IEnumerable<EmailElementi> ListaEmailElementi { get; set; }
    }

    public class MailViewModel
    {
        public string IdTask { get; set; }
        public string IdAllegato { get; set; }
        public Allegati Allegato { get; set; }
        public List<Categorie> ListaCategorie { get; set; }
        //public List<TipiElementi> ListaTipiElemento { get; set; }
        public MimeKit.MimeMessage Messaggio { get; set; }

        public BPMTaskDto task;
        public Dictionary<string, BPMClient.VariableValue> TaskVar;
        public string IdFascicolo {
            get {
                if (Allegato != null && Allegato.IdFascicolo != null) {
                    return Allegato.IdFascicolo.ToString();
                }
                return "";
            }
        }
        public Fascicoli Fascicolo { get; set; }

        public List<TipiElementi> ListaTipiElementi { get; set; }
        public string IdElemento
        {
            get
            {
                if (Allegato != null && Allegato.IdElemento != null)
                {
                    return Allegato.IdElemento.ToString();
                }
                return "";
            }
        }
        public Elementi Elemento { get; set; }


        public string CodiceSoggetto
        {
            get
            {
                try
                {
                    return Allegato.GetAttributo("CodiceSoggetto");
                }
                catch { }
                return "";
            }
        }
        public string NomeSoggetto
        {
            get
            {
                try
                {
                    return Allegato.GetAttributo("NomeSoggetto");
                }
                catch { }
                return "";
            }
        }
        public ISoggetti Soggetto { get; set; }

        public IEnumerable<EmailAttachments> FileAllegati {
            get
            {
                List<EmailAttachments> res = new List<EmailAttachments>();
                if (Messaggio != null)
                {
                    int i = 0;
                    foreach (var attachment in Messaggio.Allegati())
                    {
                        i++;
                        var fileName = attachment.NomeAllegato(i);

                        var incluso = false;
                        switch (System.IO.Path.GetExtension(fileName).ToLower())
                        {
                            case ".pdf":
                            case ".jpg":
                            case ".jpeg":
                            case ".png":
                                incluso = true;
                                break;
                        }
                        var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso };
                        res.Add(a);
                    }
                }
                return res;
            }
        }

        //public IEnumerable<EmailElementi> ListaEmailElementi { get; set; }
    }

    public class AllegaAViewModel {

        public string IdAllegato { get; set; }
        public string IdFascicolo { get; set; }
        public string IdElemento { get; set; }
        public bool allegamail { get; set; }
        public IEnumerable<EmailAttachments> FileAllegati { get; set; }
 
    }

    //public class EmailElementi  {

    //    public Guid Id { get; set; }
    //    public short Revisione { get; set; }
    //    public string Tipo { get; set; }
    //    public string Descrizione { get; set; }
    //    public StatoElemento Stato { get; set; }
    //    public Guid IdFascicolo { get; set; }
    //    public string Chiave1 { get; set; }
    //    public string Chiave2 { get; set; }
    //    public string Chiave3 { get; set; }
    //    public string Chiave4 { get; set; }
    //    public string Chiave5 { get; set; }
    //    public string DescrizioneTipo  { get; set; }
    //    public bool Ultimo { get; set; }
    //    //public Guid IdAllegato { get; set; }
    //    public string IsUltimo
    //    {
    //        get
    //        {
    //            return Ultimo ? "Si" : "";
    //        }
    //        set
    //        {

    //        }
    //    }

    //    public TipoOperazione LastOp { get; set; }
    //}

    public class EmailElementi : viewElementi
    {

        public bool Ultimo { get; set; }
        //public Guid IdAllegato { get; set; }
        public string IsUltimo
        {
            get
            {
                return Ultimo ? "Si" : "";
            }
            set
            {

            }
        }
        public TipoOperazione LastOp { get; set; }

        public string Id
        {
            get
            {
                return base.IdElemento.ToString();
    }

        }
    }


}
