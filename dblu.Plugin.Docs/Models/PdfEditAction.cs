using Newtonsoft.Json;
using Syncfusion.Blazor.PdfViewerServer;
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
        CancellaPagina = 10,
        RuotaPagina90 = 11,
        RuotaPagina270 = 12,
        SpostaPagina = 13 

    }
    public class PdfEditAction
    {

        public string TipoAllegato { get; set; }
        public string IdAllegato { get; set; }
        public string IdElemento { get; set; }
        public string FilePdf { get; set; }
        public int Pagina { get; set; }
        public string AggiungiFilePdf{ get; set; }
        public int NuovaPosizione { get; set; }
        
        [JsonIgnore]
        public Azioni Azione { get; set; }

        public int iAzione {
            get {
                return (int)Azione;
            }
            set
            {
                this.Azione = (Azioni)value;
            }
        }
    }
}
