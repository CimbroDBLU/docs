using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
//using Syncfusion.Blazor.CircularGauge.Internal;
//using Syncfusion.Blazor.PdfViewerServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Models
{
    public enum Azioni {

        Carica = 0 ,
        CaricaRiepilogo =1 ,
        Salva = 2,
        UnisciPdf = 3 ,  
        Ricarica = 4 ,
        CancellaPagina = 10,
        RuotaPagina90 = 11,
        RuotaPagina270 = 12,
        SpostaPagina = 13 

    }
    public class PdfEditAction
    {
        private string filePdf="";

        public string TipoAllegato { get; set; }
        public string IdAllegato { get; set; }
        public string IdElemento { get; set; }
        public string IdAllegatoAElemento { get; set; }
        public string Descrizione { get; set; }

        // spostato elenco allegati email/zip per evitare doppia lettura
        public IEnumerable<EmailAttachments> FileAllegati { get; set; }

        [JsonIgnore]
        public string TempFolder { get; set; }
        
        [JsonIgnore]
        public string FilePdf { 
            get {
                if (!string.IsNullOrEmpty(TempFolder) && string.IsNullOrEmpty(filePdf)  )
                   filePdf = System.IO.Path.Combine(TempFolder, $"{IdAllegato}.pdf");
                return filePdf; 
            }

            set => filePdf = value; 
        }
        public int Pagina { get; set; }
        public string AggiungiFilePdf{ get; set; }
        public int NuovaPosizione { get; set; }
        //public string Annotazioni { get; set; }
        
        [JsonIgnore]
        public Azioni Azione { get; set; }

        public int iAzione
        {
            get
            {
                return (int)Azione;
            }
            set
            {
                this.Azione = (Azioni)value;
            }
        }

        [JsonIgnore]
        public string FilePdfInModifica
        {
            get
            {
                return $"{FilePdf}.tmp";
            }
        }

        [JsonIgnore]
        public string FilePdfModificato
        {
            get
            {
                return $"{FilePdf}.sav";
            }
        }

        [JsonIgnore]
        public string FileAnnotazioni
        {
            get
            {
                return $"{FilePdf}.json";
            }
        }

        [JsonIgnore]
        public string CacheEntry
        {
            get
            {
                return $"PdfEditAction_{IdAllegato}";
            }
        }

        public PdfEditAction(){

            Azione = Azioni.Carica;
            TipoAllegato = "";
            FileAllegati = new List<EmailAttachments>();

        }
    }
}
