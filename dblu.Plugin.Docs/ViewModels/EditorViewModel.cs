using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.ViewModels;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class EditorFascicoloViewModel
    {

        public Fascicoli Oggetto { get; set; }
    
    }


    public class EditorElementoViewModel: BaseViewModel
    {
   
        public string UrlRefer { get; set; } // 

        public bool IsInsideTask { get; set; }
        public Elementi Oggetto { get; set; }

   
        
    }

    public class EditorAttributiViewModel
    {

        public string Tipo { get; set; }
        public ElencoAttributi Lista { get; set; }

    }

    public class EditorAllegatoViewModel : BaseViewModel
    {

        public string UrlRefer { get; set; } // 

        public bool IsInsideTask { get; set; }
        public Allegati Oggetto { get; set; }



    }
}
